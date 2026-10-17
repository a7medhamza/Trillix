using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Trillix.Modules.Identity;

public class JwtAccessTokenService(IOptions<JwtOptions> options) : IJwtAccessTokenService
{
    private readonly JwtOptions _jwt = options.Value;

    public (string Token, int ExpiresInSeconds) CreateAccessToken(Admins admin)
    {
        var expiresIn = _jwt.AccessTokenMinutes * 60;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, admin.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, admin.Email),
            new Claim(ClaimTypes.Name, admin.FullName),
            new Claim(ClaimTypes.Role, AppRoles.Admin)
        };

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes),
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresIn);
    }
}
