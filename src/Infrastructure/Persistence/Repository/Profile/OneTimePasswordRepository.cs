using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Bookazone.Api.Controllers.V1.Config.Request;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs;
using Bookazone.Application.Interfaces;
using Bookazone.Application.Interfaces.Repository;
using Bookazone.Application.Interfaces.Repository.Profile;
using Bookazone.Domain.Entities.Profile;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Services;
using Bookazone.Infrastructure.Services.JWT;
using Functions = Bookazone.Application.Common.Shared.Utils.Functions;

namespace Bookazone.Infrastructure.Persistence.Repository.Profile;

public class OneTimePasswordRepository(BookazoneDbContext BookazoneDbContext, IUserRepository userRepository, IEmailService emailService, IJwtService tokenService, IOptions<JwtOptions> options, IDeviceRepository deviceRepository ) :ITransactionalRepository, IOneTimePasswordRepository
{
    private static readonly Random Random = new Random();
    private readonly JwtOptions _jwtOptions = options.Value;

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await BookazoneDbContext.Database.BeginTransactionAsync();
    }

    
    public async Task<OneTimePassword?> GetLatestForUserAsync(Guid userId, string? purpose = null)
    {
        return await BookazoneDbContext.OneTimePassword!
            .Where(o => o.FkUser != null && o.FkUser.Id == userId && o.Purpose != null && o.Purpose == purpose && o.Deleted != true)
            .OrderByDescending(o => o.DateCreated)
            .FirstOrDefaultAsync();
    }

    public async Task DeactivateAllForUserPurposeAsync(Guid userId, string purpose)
    {
        var activeOtps = await BookazoneDbContext.OneTimePassword!
            .Where(o => o.FkUser!.Id == userId && o.Purpose == purpose && o.Active)
            .ToListAsync();

        foreach (var otp in activeOtps)
        {
            otp.Active = false;
            otp.Used = true;
            otp.UsedTimeDate = DateTime.UtcNow;
        }

        BookazoneDbContext.OneTimePassword.UpdateRange(activeOtps);
        await BookazoneDbContext.SaveChangesAsync();
    }

    
    public async Task InvalidateAllForUserAsync(Guid userId)
    {
        var list = await BookazoneDbContext.OneTimePassword!
            .Where(o => o.FkUser != null && o.FkUser.Id == userId && o.Deleted != true && o.Used != true)
            .ToListAsync();

        if (!list.Any()) return;

        foreach (var o in list)
        {
            o.Used = true;
            o.TransmitStatus = "INVALIDATED_BY_RESEND";
            o.ExpiredDate = DateTime.UtcNow;
            o.Active = false;
        }

        BookazoneDbContext.OneTimePassword!.UpdateRange(list);
        await BookazoneDbContext.SaveChangesAsync();
    }

    
    
    
    
    public async Task<OneTimePassword?> GetByTokenAsync(string token)
    {
        return await BookazoneDbContext.OneTimePassword
            .Include(o => o.FkUser)
           // .ThenInclude(u => u.FkTenant)
            .FirstOrDefaultAsync(o => o.Token == token && o.Deleted != true && o.Active == true && o.Used != true && o.FkUser != null);
    }

    public async Task UpdateAsync(OneTimePassword otp)
    {
        otp.DateUpdated = DateTime.UtcNow;
        BookazoneDbContext.OneTimePassword.Update(otp);
        await BookazoneDbContext.SaveChangesAsync();
    }
    
    public List<OneTimePassword> All()
    {
        return (BookazoneDbContext.OneTimePassword ?? throw new Except(ErrorHttp.DbQueryRunFailed)).Where(a => a.Deleted == false || a.Deleted == null).ToListAsync().Result;
    }

    public OneTimePassword? Find(Guid? id)
    {
        if (BookazoneDbContext.OneTimePassword == null) throw new Except(ErrorHttp.DbQueryRunFailed);
        return BookazoneDbContext.OneTimePassword.FindAsync(id).Result;
    }
    
    public OneTimePassword? FindByOtp(string? otp)
    {
        if (BookazoneDbContext.OneTimePassword == null) throw new Except(ErrorHttp.DbQueryRunFailed);
        return BookazoneDbContext.OneTimePassword.FirstOrDefault(a => a.Otp == otp && a.Deleted != true);
    }
    
    public OneTimePassword? FindByReference(string? reference)
    {
        if (BookazoneDbContext.OneTimePassword == null) throw new Except(ErrorHttp.DbQueryRunFailed);
        return BookazoneDbContext.OneTimePassword.FirstOrDefault(a => a.Reference == reference && a.Deleted != true);
    }
    
    public OneTimePassword? FindToValidate(string? otp, string? reference)
    {
        if (BookazoneDbContext.OneTimePassword == null) throw new Except(ErrorHttp.DbQueryRunFailed);
        return BookazoneDbContext.OneTimePassword.FirstOrDefault(a => 
            a.Otp == otp && 
            a.Reference == reference && 
            a.Used != true && 
            a.Active == true && 
            a.Deleted != true
            );
    }
    
    
    public ApiResult Validate(BookazoneOtpValidate model)
    {
        try
        {
            if (model.Reference == null || model.Otp == null)
                return  ApiResponse.Error(ErrorHttp.Error);
            var otp = FindToValidate(model.Otp, model.Reference);
            if (otp == null)
                return  ApiResponse.Error(ErrorHttp.NotFound);
            
            if (otp.ExpiredDate != null && otp.ExpiredDate.Value <= DateTime.UtcNow)
                return  ApiResponse.Error(ErrorHttp.OtpExpired);

            otp.Used = true;
            otp.UsedTimeDate = DateTime.UtcNow;
            Update(otp);
            return  ApiResponse.Success(new { Message = $"OTP is valid {model.Otp}", ValidationTime = otp.UsedTimeDate });
        }
        catch (Exception e)
        {
            return ApiResponse.Error(ErrorHttp.Error, e);
        }
    }
    
    

    public bool Create(OneTimePassword entity)
    {
        entity.Active = true;
        entity.Deleted = false;
        entity.DateCreated = DateTime.UtcNow;
        if (BookazoneDbContext.OneTimePassword == null) throw new Except(ErrorHttp.DbCreateError);
        BookazoneDbContext.OneTimePassword.Add(entity);
        var result = BookazoneDbContext.SaveChangesAsync().Result; return result > 0;
    }

    public bool Update(OneTimePassword entity)
    {
        entity.DateUpdated = DateTime.UtcNow;
        BookazoneDbContext.Entry(Find(entity.Id) ?? throw new Except(ErrorHttp.DbUpdateError)).CurrentValues.SetValues(entity);
        var result = BookazoneDbContext.SaveChangesAsync().Result; return result > 0;
    }

    public bool Delete(Guid? id)
    {
        var entity = this.Find(id);
        if (id == null || entity == null) throw new Except(ErrorHttp.NotFound);
        entity.Active = false;
        entity.Deleted = true;
        entity.DateDeleted = DateTime.UtcNow;
        BookazoneDbContext.Entry(Find(entity.Id) ?? throw new Except(ErrorHttp.DbUpdateError)).CurrentValues.SetValues(entity);
        var result =BookazoneDbContext.SaveChangesAsync().Result; return result > 0;
    }

    public bool Status(Guid? id, bool active)
    {
        var entity = this.Find(id);
        if (id == null || entity == null) throw new Except(ErrorHttp.NotFound);
        entity.Active = active;
        entity.DateUpdated = DateTime.UtcNow;
        BookazoneDbContext.Entry(Find(entity.Id) ?? throw new Except(ErrorHttp.DbUpdateError)).CurrentValues.SetValues(entity);
        var result =BookazoneDbContext.SaveChangesAsync().Result; return result > 0;
    }
    
    public async Task<BookazoneOtpResponse?> GenerateOtp(BookazoneOtpGenerate apiCoreOtpGenerate)
    {
        var user = userRepository.FindByEmailOrPhoneOrUsername(apiCoreOtpGenerate.Email ?? apiCoreOtpGenerate.Phone)!.FirstOrDefault();
        if (user == null) throw new Except(ErrorHttp.UserNotFound);
        var generatedOtp = Functions.Generate(6);
        string? rawToken = null;
        var otpEntity = new OneTimePassword
        {
            FkUser = user,
            Email = apiCoreOtpGenerate.Email,
            Phone = apiCoreOtpGenerate.Phone,
            Purpose = apiCoreOtpGenerate.Purpose,
            Active = true,
            DateCreated = DateTime.UtcNow,
            ExpiredDate = DateTime.UtcNow.AddMinutes(15),
            Reference = Functions.Generate(10),
            CreatedBy = user.Username ?? "system",
            IsTokenBased = apiCoreOtpGenerate.IsTokenBased
        };
        if (otpEntity.IsTokenBased)
        {
            var claims = new List<Claim>
            {
                new Claim("purpose", otpEntity.Purpose ?? "verification"),
                new Claim("user_id", user.Id.ToString()),
                new Claim("reference", otpEntity.Reference ?? ""),
                new Claim("email", user.Email ?? ""),
            };
            if (!string.IsNullOrEmpty(apiCoreOtpGenerate.DeviceRequest?.Identifier))
                claims.Add(new Claim("device_identifier", apiCoreOtpGenerate.DeviceRequest.Identifier));
            if (apiCoreOtpGenerate.ExtraClaims != null)
            {
                foreach (var kvp in apiCoreOtpGenerate.ExtraClaims)
                {
                    if (!string.IsNullOrWhiteSpace(kvp.Value))
                        claims.Add(new Claim(kvp.Key, kvp.Value));
                }
            }
            rawToken = tokenService.GenerateJwt(claims, expiresInMinutes: 30);
            otpEntity.Token = CryptoService.HashToken(rawToken);
            otpEntity.Otp = null;
        }
        else
        {
            otpEntity.Otp = generatedOtp;
        }

        BookazoneDbContext.OneTimePassword?.Add(otpEntity);
        await BookazoneDbContext.SaveChangesAsync();

        return new BookazoneOtpResponse
        {
            Reference = otpEntity.Reference,
            Token = rawToken,
            Otp = otpEntity.Otp,
            Email = otpEntity.Email,
            ExpiredDate = otpEntity.ExpiredDate
        };
    }

 
 
 
 
 
 
public Task<bool?> OtpValidate(BookazoneOtpValidate apiCoreOtpValidate)
    {
        try
        {
            var validateInput = new BookazoneOtpValidate
            {
                Reference = apiCoreOtpValidate.Reference,
                Otp = apiCoreOtpValidate.Otp
            };
            var result = Validate(validateInput);
            return Task.FromResult<bool?>(result.Status == 1);
        }
        catch (Exception)
        {
            return Task.FromResult<bool?>(false);
        }
    }
    
    public OneTimePassword Generate(string? email, string? phone, string? purpose, DeviceRequest? deviceRequest)
    {
        var generatedOtp = UniqueOtp();
        var otpEntity = new OneTimePassword
        {
            Otp = generatedOtp,
            Email = email,
            Phone = phone,
            Purpose = purpose,
            Active = true,
            DateCreated = DateTime.UtcNow,
            ExpiredDate = DateTime.UtcNow.AddMinutes(15),
        };
        var referenceFound = false;
        while (referenceFound == false)
        {
            otpEntity.Reference = Functions.Generate(10);
            var checkTrx = FindByReference(otpEntity.Reference);
            if (checkTrx == null) referenceFound = true;
        }
        BookazoneDbContext.OneTimePassword?.Add(otpEntity);
        BookazoneDbContext.SaveChanges();
        
        if (!string.IsNullOrEmpty(email))
        {
            if (otpEntity?.Reference != null)
                emailService.SendOtpEmailAsync(email, generatedOtp, purpose ?? "Verification", otpEntity.Reference);
        }

        if (otpEntity != null && otpEntity.Otp != null)
        {
            return otpEntity;
        }
        else
        {
            return  null!;
        }
    }


    public BookazoneOtpResponse GenerateOtpForRegistration(OtpGenerateRequest otpRequest)
    {
        var generatedOtp = UniqueOtp();
        var otpEntity = new OneTimePassword
        {
            Otp = generatedOtp,
            Email = otpRequest.Email,
            Phone = otpRequest.Phone,
            Purpose = otpRequest.Purpose,
            Active = true,
            DateCreated = DateTime.UtcNow,
            ExpiredDate = DateTime.UtcNow.AddMinutes(15),
        };

        bool referenceFound = false;
        while (!referenceFound)
        {
            otpEntity.Reference = Functions.Generate(10);
            var checkTrx = FindByReference(otpEntity.Reference);
            if (checkTrx == null)
            {
                referenceFound = true;
            }
        }

        BookazoneDbContext.OneTimePassword?.Add(otpEntity);
        BookazoneDbContext.SaveChanges();

        if (!string.IsNullOrEmpty(otpRequest.Email))
        {
            if (otpEntity.Reference != null)
                emailService.SendOtpEmailAsync(otpRequest.Email, generatedOtp, "Verification", otpEntity.Reference);
        }

        
        return new BookazoneOtpResponse
        {
            Reference = otpEntity.Reference,
            Email = otpEntity.Email,
            Phone = otpEntity.Phone
        };
    }


    public bool IsOtpAlreadyUsed(string otp)
    {
        return BookazoneDbContext.OneTimePassword?.Any(o => o.Otp == otp && o.Used != true && o.Active == true) ?? false;
    }
    
    public string GenerateToken(Users user, string deviceIdentifier)
    {
        var tokenPayload = new
        {
            sub = user.Id,
            email = user.Email,
            device = deviceIdentifier,
            purpose = "OTP_VERIFICATION",
            exp = DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds()
        };

        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var creds = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var jwtToken = new JwtSecurityToken(
            issuer: "BookazoneAuthServer",
            audience: "BookazoneClients",
            claims: new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim("device_id", deviceIdentifier),
                new Claim("purpose", "OTP_VERIFICATION")
            },
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }

    
    private string UniqueOtp()
    {
        string otp;
        do
        {
            otp = Random.Next(100000, 1000000).ToString("D6");
        } while (IsOtpAlreadyUsed(otp));

        return otp;
    }
}