using Bookazone.Application.Interfaces.Services.Profile;
using Google.Apis.Auth;

namespace Bookazone.Infrastructure.Services.Profile;

public class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly IConfiguration _config;
    private readonly ILogger<GoogleTokenValidator> _logger;

    public GoogleTokenValidator(IConfiguration config, ILogger<GoogleTokenValidator> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<GoogleTokenPayload> ValidateIdTokenAsync(string idToken, CancellationToken ct = default)
    {
        idToken = idToken?.Trim() ?? "";
        
        var clientIds = _config.GetSection("Google:ClientIds").Get<string[]>() ?? Array.Empty<string>();
        _logger.LogInformation("Google ClientIds loaded: {ClientIds}", string.Join(" | ", clientIds));
        
        try
        {
            var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ReadJwtToken(idToken);
            var azp = jwt.Claims.FirstOrDefault(c => c.Type == "azp")?.Value;
            _logger.LogInformation("Token aud: {Aud} | azp: {Azp} | iss: {Iss}",
                jwt.Audiences.FirstOrDefault(),
                azp,
                jwt.Issuer
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to decode JWT for debugging. Token may be malformed.");
        }

        if (clientIds.Length == 0)
            throw new Exception("Google ClientIds config is empty. Check appsettings / environment variables.");

        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = clientIds,
            // Clock = SystemClock.Instance // Ensures time-based validation uses correct time
        };

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            return new GoogleTokenPayload(
                Subject: payload.Subject,
                Email: payload.Email ?? "",
                EmailVerified: payload.EmailVerified,
                GivenName: payload.GivenName,
                FamilyName: payload.FamilyName,
                PictureUrl: payload.Picture
            );
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogError(ex, 
                "JWT signature validation failed. Ensure: 1) Token is from one of the configured ClientIds, " +
                "2) Frontend is using correct Google OAuth ClientId, 3) Token hasn't expired");
            throw;
        }
    }
}