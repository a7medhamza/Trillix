using Microsoft.EntityFrameworkCore;
using Trillix.Data;

namespace Trillix.Modules.Identity;

public static class AdminAuthEndpoints
{
    public static void MapAdminAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/admin/auth");

        auth.MapPost("/login", async (
            LoginRequest body,
            AppDbContext db,
            IPasswordService hasher,
            IJwtAccessTokenService accessTokens,
            IRefreshTokenService refreshTokens,
            CancellationToken ct) =>
        {
            var email = body.Email?.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(body.Password))
                return Results.Unauthorized();

            var admin = await db.Admins.FirstOrDefaultAsync(a => a.Email == email, ct);
            if (admin is null || !admin.IsActive || !hasher.Verify(body.Password, admin.PasswordHash))
                return Results.Unauthorized();

            var (accessToken, expiresIn) = accessTokens.CreateAccessToken(admin);
            var refreshToken = await refreshTokens.IssueAsync(admin, ct);
            return Results.Ok(new TokenResponse(accessToken, refreshToken, expiresIn));
        });

        auth.MapPost("/refresh", async (
            RefreshRequest body,
            IJwtAccessTokenService accessTokens,
            IRefreshTokenService refreshTokens,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(body.RefreshToken))
                return Results.Unauthorized();

            var rotated = await refreshTokens.RotateAsync(body.RefreshToken, ct);
            if (rotated is null)
                return Results.Unauthorized();

            var (accessToken, expiresIn) = accessTokens.CreateAccessToken(rotated.Value.Admin);
            return Results.Ok(new TokenResponse(accessToken, rotated.Value.NewRefreshToken, expiresIn));
        });

        auth.MapPost("/logout", async (
            RefreshRequest body,
            IRefreshTokenService refreshTokens,
            CancellationToken ct) =>
        {
            if (!string.IsNullOrWhiteSpace(body.RefreshToken))
                await refreshTokens.RevokeAsync(body.RefreshToken, ct);
            return Results.NoContent();
        });
    }

    private sealed record LoginRequest(string? Email, string? Password);
    private sealed record RefreshRequest(string? RefreshToken);
    private sealed record TokenResponse(string AccessToken, string RefreshToken, int ExpiresIn);
}
