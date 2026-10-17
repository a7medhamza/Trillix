namespace Trillix.Modules.Identity;

public interface IJwtAccessTokenService
{
    (string Token, int ExpiresInSeconds) CreateAccessToken(Admins admin);
}
