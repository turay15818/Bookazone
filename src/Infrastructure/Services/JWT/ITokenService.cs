namespace Bookazone.Infrastructure.Services.JWT;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

public interface ITokenService
{
    string CreateVerificationToken(Guid userId, Guid otpId, string fingerprintHash, TimeSpan ttl);
    ClaimsPrincipal? ValidateToken(string token, bool validateLifetime = true);
}

public class JwtVerificationOptions
{
    public string Secret { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int ExpiryMinutes { get; set; }
}

public class JwtTokenService : ITokenService
{
    private readonly JwtVerificationOptions _opts;
    private readonly byte[] _key;

    public JwtTokenService(IOptions<JwtVerificationOptions> opts)
    {
        _opts = opts.Value;
        _key = Encoding.UTF8.GetBytes(_opts.Secret);
    }

    public string CreateVerificationToken(Guid userId, Guid otpId, string fingerprintHash, TimeSpan ttl)
    {
        var now = DateTime.UtcNow;

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("otpId", otpId.ToString()),
            new Claim("fp", fingerprintHash),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var creds = new SigningCredentials(new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha512);

        var jwt = new JwtSecurityToken(
            issuer: _opts.Issuer,
            audience: _opts.Audience,
            claims: claims,
            notBefore: now,
            expires: now.Add(ttl),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    public ClaimsPrincipal? ValidateToken(string token, bool validateLifetime = true)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = !string.IsNullOrWhiteSpace(_opts.Issuer),
            ValidIssuer = _opts.Issuer,
            ValidateAudience = !string.IsNullOrWhiteSpace(_opts.Audience),
            ValidAudience = _opts.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(_key),
            ValidateLifetime = validateLifetime,
            ClockSkew = TimeSpan.FromMinutes(2)
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, parameters, out var validatedToken);
            return principal;
        }
        catch
        {
            return null;
        }
    }
    
    
    
    
    
}
