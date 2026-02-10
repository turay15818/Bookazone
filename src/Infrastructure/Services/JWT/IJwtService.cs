using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Bookazone.Application.Common.Shared.Utils;

namespace Bookazone.Infrastructure.Services.JWT;

public interface IJwtService
{
    string GenerateJwt(IEnumerable<Claim> claims, int expiresInMinutes);
    IDictionary<string, object> DecodeJwt(string token);
}


public class JwtService(IOptions<JwtOptions> options) : IJwtService
{
    private readonly JwtOptions _jwtOptions = options.Value;

    public string GenerateJwt(IEnumerable<Claim> claims, int expiresInMinutes)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(expiresInMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public IDictionary<string, object> DecodeJwt(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        if (string.IsNullOrEmpty(token) || !handler.CanReadToken(token)) return null;
        var jwt = handler.ReadJwtToken(token);
        return jwt.Claims.ToDictionary(c => c.Type, c => (object)c.Value);
}
}
