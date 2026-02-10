using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Api.Controllers.V1.Config.Request;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.Interfaces.Repository.Profile;
using Bookazone.Infrastructure.Services;

namespace Bookazone.Api.Controllers.V1.Secure.Notification;

[Authorize]
[ApiController]
[ApiExplorerSettings(GroupName = SwaggerConfig.SwaggerDocName.WebVersion1.Slug)]
[Route(RouteWebVersion1.Secure.Notification.Base)]
public class NotificationController(
    ILogger<NotificationController> logger,
    IUserRepository userRepository,
    IDeviceRepository deviceRepository,
    FirebaseNotificationService firebaseNotificationService,
    IEmailService emailService)
    : ControllerBase
{
    [HttpPost]
    [Route(RouteWebVersion1.Secure.Notification.TestNotification)]
    public async Task<ApiResult> TestNotification([FromBody] NotificationTestRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Email is required.");

            var users = userRepository.FindByEmailOrPhoneOrUsername(request.Email);
            var user = users?.FirstOrDefault(u =>
                !string.IsNullOrWhiteSpace(u.Email) &&
                string.Equals(u.Email, request.Email, StringComparison.OrdinalIgnoreCase))
                ?? users?.FirstOrDefault();

            if (user == null)
                return ApiResponse.ErrorWithMessage(ErrorHttp.NotFound, "User not found.");

            var title = string.IsNullOrWhiteSpace(request.Title)
                ? "Bookazone test notification"
                : request.Title;
            var body = string.IsNullOrWhiteSpace(request.Body)
                ? "This is a test notification from Bookazone."
                : request.Body;

            var data = new Dictionary<string, string>
            {
                { "type", "test_notification" },
                { "requestedAtUtc", DateTime.UtcNow.ToString("O") }
            };

            var devices = deviceRepository.GetUserDevicesWithTokens(user);
            var pushCount = devices.Count > 0
                ? await firebaseNotificationService.PushToDevices(devices, title, body, data)
                : 0;

            var emailSent = false;
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                var emailBody = $@"
<div style='font-family:Arial,sans-serif;color:#111;'>
  <h2>Bookazone test notification</h2>
  <p><strong>Title:</strong> {System.Net.WebUtility.HtmlEncode(title)}</p>
  <p><strong>Message:</strong> {System.Net.WebUtility.HtmlEncode(body)}</p>
  <p style='color:#6b7280;'>Sent at {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC</p>
</div>";
                emailSent = await emailService.SendEmailAsync(
                    user.Email,
                    "Bookazone test notification",
                    emailBody,
                    true);
            }

            return ApiResponse.Success(new
            {
                user.Id,
                user.Email,
                PushCount = pushCount,
                DirectMessageId = (string?)null,
                EmailSent = emailSent
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending test notification to {Email}", request.Email);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}
