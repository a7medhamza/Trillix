using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Trillix.Data;

namespace Trillix.Modules.Identity;

public class RefreshTokenService(AppDbContext db, IOptions<JwtOptions> options) : IRefreshTokenService
{
    private readonly JwtOptions _jwt = options.Value;

    public async Task<string> IssueAsync(Admins admin, CancellationToken ct = default)
    {
        var token = GenerateToken();
        db.AdminRefreshTokens.Add(new AdminRefreshToken
        {
            AdminId = admin.Id,
            TokenHash = Hash(token),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays),
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);
        return token;
    }

    public async Task<(Admins Admin, string NewRefreshToken)?> RotateAsync(string refreshToken, CancellationToken ct = default)
    {
        var hash = Hash(refreshToken);
        var row = await db.AdminRefreshTokens
            .Include(t => t.Admin)
            .FirstOrDefaultAsync(t => t.TokenHash == hash && t.RevokedAt == null, ct);

        if (row is null || row.ExpiresAt <= DateTime.UtcNow || !row.Admin.IsActive)
            return null;

        row.RevokedAt = DateTime.UtcNow;
        var newToken = GenerateToken();
        db.AdminRefreshTokens.Add(new AdminRefreshToken
        {
            AdminId = row.AdminId,
            TokenHash = Hash(newToken),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays),
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);
        return (row.Admin, newToken);
    }

    public async Task RevokeAsync(string refreshToken, CancellationToken ct = default)
    {
        var hash = Hash(refreshToken);
        var row = await db.AdminRefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == hash && t.RevokedAt == null, ct);
        if (row is null) return;

        row.RevokedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }

    private static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
