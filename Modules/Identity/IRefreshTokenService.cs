namespace Trillix.Modules.Identity;

public interface IRefreshTokenService
{
    Task<string> IssueAsync(Admins admin, CancellationToken ct = default);
    Task<(Admins Admin, string NewRefreshToken)?> RotateAsync(string refreshToken, CancellationToken ct = default);
    Task RevokeAsync(string refreshToken, CancellationToken ct = default);
}
