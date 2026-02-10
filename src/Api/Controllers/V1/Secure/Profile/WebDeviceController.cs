using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Api.Controllers.V1.Config.Request;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs;
using Bookazone.Application.DTOs.Converter;
using Bookazone.Application.Interfaces.Repository.Profile;
using Bookazone.Domain.Entities.Profile;
using Bookazone.Infrastructure.Services;
using Functions = Bookazone.Application.Common.Shared.Utils.Functions;
using OtpRequest = Bookazone.Api.Controllers.V1.Config.Request.OtpRequest;

namespace Bookazone.Api.Controllers.V1.Secure.Profile;

[Authorize]
[ApiController]
[ApiExplorerSettings(GroupName = SwaggerConfig.SwaggerDocName.WebVersion1.Slug)]
[Route(RouteWebVersion1.Secure.Device.Base)]
public class WebDeviceController(
    ILogger<WebDeviceController> logger,
    IDeviceRepository deviceRepository,
    Functions functions,
    IAuthenticateService authenticateService,
    IUserRepository userRepository,
    FirebaseNotificationService firebaseNotificationService,
    IOneTimePasswordRepository oneTimePasswordRepository
)
    : ControllerBase
{
    [Authorize]
    [HttpPost]
    [Route(RouteWebVersion1.Secure.Device.EmailPhoneCheck)]
    public async Task<ApiResult> EmailPhoneCheck(EmailPhoneRequest emailPhoneRequest)
    {
        try
        {
            if (
                string.IsNullOrEmpty(emailPhoneRequest.DeviceRequest.Identifier)
            ) return await Task.FromResult(ApiResponse.Error(ErrorHttp.BadRequest));
            var user = functions.GetUser(User.Identity?.Name);
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return await Task.FromResult(ApiResponse.Error(ErrorHttp.Error));
            }

            logger.LogInformation($"{User?.Identity?.Name}  :  This is theAuthenticated user");
            if (user == null) return await Task.FromResult(ApiResponse.Error(ErrorHttp.TokenExpired));
            var device = deviceRepository.FindByIdentifierAndUser(emailPhoneRequest.DeviceRequest.Identifier, user);
            if (device == null)
            {
                device = new Device
                {
                    Slug = Functions.GenerateUid(),
                    Name = emailPhoneRequest.DeviceRequest.Name,
                    Identifier = emailPhoneRequest.DeviceRequest.Identifier,
                    Version = emailPhoneRequest.DeviceRequest.Version,
                    System = emailPhoneRequest.DeviceRequest.System,
                    AppVersion = emailPhoneRequest.DeviceRequest.AppVersion,
                    Os = emailPhoneRequest.DeviceRequest.Os,
                    FkUser = user
                };
                device = deviceRepository.Create(device);
            }

            emailPhoneRequest.Phone = Functions.PhoneLocalFormat(emailPhoneRequest.Phone);
            if (device == null) return await Task.FromResult(ApiResponse.Error(ErrorHttp.DeviceNotFound));
            if (user.PhoneVerified != true || user.Phone != emailPhoneRequest.Phone)
            {
                user.Phone = emailPhoneRequest.Phone;
                user.PhoneVerified = false;
                var otpResponse = oneTimePasswordRepository.GenerateOtp(new BookazoneOtpGenerate
                    { Phone = user.Phone, Purpose = device.Name }).Result;
                if (otpResponse == null)
                    return await Task.FromResult(ApiResponse.Error(ErrorHttp.DeviceVerifyNotAvailable));
                device.OtpPhoneReference = otpResponse.Reference;
            }

            if (user.EmailVerified != true || user.Email != emailPhoneRequest.Email)
            {
                user.Email = emailPhoneRequest.Email;
                user.EmailVerified = false;
                var otpResponse = oneTimePasswordRepository.GenerateOtp(new BookazoneOtpGenerate
                    { Email = user.Email, Purpose = device.Name }).Result;
                if (otpResponse == null)
                    return await Task.FromResult(ApiResponse.Error(ErrorHttp.DeviceVerifyNotAvailable));
                device.OtpEmailReference = otpResponse.Reference;
            }

            userRepository.Update(user);
            deviceRepository.Update(device);
            return await Task.FromResult(ApiResponse.Success(new
            {
                DeviceVerified = false,
                user.EmailVerified,
                user.PhoneVerified
            }));
        }
        catch (Exception e)
        {
            logger.LogError("error");
            return await Task.FromResult(ApiResponse.Error(ErrorHttp.Error, e));
        }
    }

    [Authorize]
    [HttpGet("whoami")]
    public IActionResult WhoAmI()
    {
        if (User.Identity == null || !User.Identity.IsAuthenticated)
        {
            return Unauthorized(new { message = "User is not authenticated" });
        }

        logger.LogInformation("User identity: {UserIdentity}", User.Identity.Name);
        return Ok(new { user = User.Identity?.Name });
    }

    
    [HttpPost]
    [Route(RouteWebVersion1.Secure.Device.SetBiometric)]
    public async Task<ApiResult> SetBiometric(BiometricRequest mobileBiometricRequest)
    {
        try
        {
            if (
                string.IsNullOrEmpty(mobileBiometricRequest.DeviceRequest.Identifier)
            ) return await Task.FromResult(ApiResponse.Error(ErrorHttp.BadRequest));

            if (
                mobileBiometricRequest.Biometric == null
            ) return await Task.FromResult(ApiResponse.Error(ErrorHttp.NotEncoded));
            var user = functions.GetUser(User.Identity?.Name);
            if (user == null) return await Task.FromResult(ApiResponse.Error(ErrorHttp.TokenExpired));

            var device = deviceRepository.FindByIdentifierAndUser(mobileBiometricRequest.DeviceRequest.Identifier, user);
            if (device == null) return await Task.FromResult(ApiResponse.Error(ErrorHttp.DeviceNotFound));

            device.Biometric = true;
            device.BiometricKey = mobileBiometricRequest.Biometric;
            deviceRepository.Update(device);
            return await Task.FromResult(ApiResponse.Success());
        }
        catch (Exception e)
        {
            logger.LogError("error");
            return await Task.FromResult(ApiResponse.Error(ErrorHttp.Error, e));
        }
    }
    

    [HttpPost]
    [Route(RouteWebVersion1.Secure.Device.Validate)]
    public async Task<ApiResult> Validate(OtpRequest mobileOtpRequest)
    {
        logger.LogInformation(User.Identity?.Name, "the token user");
        try
        {
            if (
                string.IsNullOrEmpty(mobileOtpRequest.DeviceRequest.Identifier)
            ) return await Task.FromResult(ApiResponse.Error(ErrorHttp.BadRequest));
            if (
                mobileOtpRequest.Otp == null
            ) return await Task.FromResult(ApiResponse.Error(ErrorHttp.NotEncoded));
            /*mobileOtpRequest.Otp = GtbCrypto.GtbDecrypt(mobileOtpRequest.Otp, true);*/

            var user = functions.GetUser(User.Identity?.Name) ?? userRepository
                .FindUncompletedRegistration(username: mobileOtpRequest.Username)?.LastOrDefault();
            logger.LogInformation(User.Identity?.Name, "the token user");
            if (user == null) return await Task.FromResult(ApiResponse.Error(ErrorHttp.UserNotFound));
            var device = deviceRepository.FindByIdentifierAndUser(mobileOtpRequest.DeviceRequest.Identifier, user);
            if (device == null) return await Task.FromResult(ApiResponse.Error(ErrorHttp.DeviceNotFound));
            var customer = user.Id;
            var otpResponse = oneTimePasswordRepository.OtpValidate(new BookazoneOtpValidate
                { Reference = device.OtpPhoneReference, Otp = mobileOtpRequest.Otp }).Result;
            if (otpResponse != true)
            {
                otpResponse = oneTimePasswordRepository.OtpValidate(new BookazoneOtpValidate
                    { Reference = device.OtpEmailReference, Otp = mobileOtpRequest.Otp }).Result;
                if (otpResponse != true) return await Task.FromResult(ApiResponse.Error(ErrorHttp.OtpInvalid));
                device.VerifyByEmail = true;
                user.EmailVerified = true;
                user.Active = true;
                user.Deleted = false;
            }
            else
            {
                device.VerifyByPhone = true;
                user.PhoneVerified = true;
                user.Active = true;
                user.Deleted = false;
            }

            if (customer != null)
            {
                if (user.EmailVerified != true && user.Email == user.Email) user.EmailVerified = user.EmailVerified;
                if (user.PhoneVerified != true && user.Phone == user.Phone) user.PhoneVerified = user.PhoneVerified;
                userRepository.Update(user);
            }
            else
                return await Task.FromResult(ApiResponse.Error(ErrorHttp.CustomerNotFound));

            user.Active = true;
            user.Deleted = false;
            userRepository.Update(user);
            /*userRepository.FindUncompletedRegistration(username: User.Identity?.Name)?.ForEach(a =>
            {
                userRepository.Delete(a.Id);
            });*/
            device.Verified = true;
            device.DateVerified = DateTime.UtcNow;
            deviceRepository.Update(device);

            /*await firebaseNotificationService.SendSecurityAlertAsync(user, deviceRepository.ListByUser(user)?.ToList()!,
                device);
            if (device.SecurityCountdown != null)
            {
                await firebaseNotificationService.PushToDevices(
                    deviceRepository.ListByUser(user),
                    "Device security alert 📲",
                    $" New connection detected to your account from a new device. No action is required if it is you, otherwise you can delete this device from GTWorld in the devices menu.");
            }*/

            return await Task.FromResult(ApiResponse.Success(new
            {
                DeviceVerified = true,
                user.EmailVerified,
                user.PhoneVerified,
                UserData = authenticateService.GetProfile(user)
            }));
        }
        catch (Exception e)
        {
            logger.LogError("error");
            return await Task.FromResult(ApiResponse.Error(ErrorHttp.Error, e));
        }
    }

    [HttpPost]
    [Route(RouteWebVersion1.Secure.Device.All)]
    public async Task<ApiResult> All()
    {
        try
        {
            var user = functions.GetUser(User.Identity?.Name);
            if (user == null) return await Task.FromResult(ApiResponse.Error(ErrorHttp.TokenExpired));

            var devices = deviceRepository.All(user);
            return await Task.FromResult(
                ApiResponse.Success((devices ?? new List<Device>()).Select(DeviceConverter.ToDto).ToList()));
        }
        catch (Exception e)
        {
            logger.LogError("error");
            return await Task.FromResult(ApiResponse.Error(ErrorHttp.Error, e));
        }
    }

    [HttpPost]
    [Route(RouteWebVersion1.Secure.Device.Find)]
    public async Task<ApiResult> Find(Guid id)
    {
        try
        {
            var user = functions.GetUser(User.Identity?.Name);
            if (user == null) return await Task.FromResult(ApiResponse.Error(ErrorHttp.TokenExpired));

            var device = deviceRepository.FindByIdAndUser(id, user);
            return await Task.FromResult(ApiResponse.Success(device != null ? DeviceConverter.ToDto(device) : null));
        }
        catch (Exception e)
        {
            logger.LogError("error");
            return await Task.FromResult(ApiResponse.Error(ErrorHttp.Error, e));
        }
    }

    [HttpPost]
    // [Authorize(Roles = Consts.Vars.Roles.UserManagement)]
    [Route(RouteWebVersion1.Secure.Device.Delete)]
    public async Task<ApiResult> Delete(Guid id)
    {
        try
        {
            var user = functions.GetUser(User.Identity?.Name);
            if (user == null) return await Task.FromResult(ApiResponse.Error(ErrorHttp.TokenExpired));

            var device = deviceRepository.FindByIdAndUser(id, user);
            if (device != null) deviceRepository.Delete(device.Id);
            return await Task.FromResult(ApiResponse.Success());
        }
        catch (Exception e)
        {
            logger.LogError("error");
            return await Task.FromResult(ApiResponse.Error(ErrorHttp.Error, e));
        }
    }
}