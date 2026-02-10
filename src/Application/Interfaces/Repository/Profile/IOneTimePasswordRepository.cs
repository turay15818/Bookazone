using Microsoft.EntityFrameworkCore.Storage;
using Bookazone.Api.Controllers.V1.Config.Request;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs;
using Bookazone.Domain.Entities.Profile;

namespace Bookazone.Application.Interfaces.Repository.Profile;

public interface IOneTimePasswordRepository
{
    List<OneTimePassword> All();
    OneTimePassword? Find(Guid? id);
    OneTimePassword? FindByOtp(string? otp);
    OneTimePassword? FindByReference(string? reference);
    OneTimePassword? FindToValidate(string? otp, string? reference);
    ApiResult Validate(BookazoneOtpValidate model);
    Task<BookazoneOtpResponse?> GenerateOtp(BookazoneOtpGenerate otpGenerate);
    Task<bool?> OtpValidate(BookazoneOtpValidate BookazoneOtpValidate);
    bool Create(OneTimePassword entity);
    bool Update(OneTimePassword entity);
    bool Delete(Guid? id);
    bool Status(Guid? id, bool active);
    OneTimePassword Generate(string? email, string? phone, string? purpose, DeviceRequest? deviceRequest);
    BookazoneOtpResponse GenerateOtpForRegistration(OtpGenerateRequest otpRequest);
    Task DeactivateAllForUserPurposeAsync(Guid userId, string purpose);

    Task<OneTimePassword?> GetByTokenAsync(string token);
    Task UpdateAsync(OneTimePassword otp);
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task<OneTimePassword?> GetLatestForUserAsync(Guid userId, string? purpose);
    Task InvalidateAllForUserAsync(Guid userId);
    public string GenerateToken(Users user, string deviceIdentifier);
}

