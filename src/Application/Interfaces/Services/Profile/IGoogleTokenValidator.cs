namespace Bookazone.Application.Interfaces.Services.Profile;

public record GoogleTokenPayload(
    string Subject,
    string Email,
    bool EmailVerified,
    string? GivenName,
    string? FamilyName,
    string? PictureUrl
);

public interface IGoogleTokenValidator
{
    Task<GoogleTokenPayload> ValidateIdTokenAsync(string idToken, CancellationToken ct = default);
}