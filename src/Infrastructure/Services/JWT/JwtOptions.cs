namespace Bookazone.Infrastructure.Services.JWT;

public class JwtOptions
{
    public JwtOptions()
    {

    }

    public JwtOptions(string? issuer, string? audience, string? authority, string? secretKey)
    {
        Issuer = issuer;
        Audience = audience;
        Authority = authority;
        SecretKey = secretKey;
    }

    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    public string? Authority { get; set; }
    public string? SecretKey { get; set; }
    public int? ExpiryMinutes { get; set; }
}