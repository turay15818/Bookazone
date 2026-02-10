using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Infrastructure.Services;
using Bookazone.Infrastructure.Services.Files;

namespace Bookazone.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController(FirebaseStorageService firebaseStorageService, ILogger<TestController> logger, IEmailService emailService)
    : ControllerBase
{
    [HttpPost("firebase")]
    [Consumes("multipart/form-data")] 
    public async Task<IActionResult> TestFirebase([FromForm] TestRequest request)
    {
        logger.LogInformation("Initiating Firebase connection test...");
        
            var logoUrl = await firebaseStorageService.UploadFileAsync(request.image, "test-Logo");
      logger.LogInformation("Logo URL: {LogoUrl}", logoUrl);
        
        var result = await firebaseStorageService.TestConnectionAsync();
        if (result)
        {
            logger.LogInformation("Firebase connection is successful.");
            return Ok(new { success = true, message = "Firebase connected successfully ✅" });
        }

        return BadRequest(new { success = false, message = "Firebase connection failed ❌" });
    }
    
    [HttpPost("/send-temp-email")]
    public async Task<ApiResult> SendTempEmail()
    {
        try
        {
            var emailBody = emailService.GenerateVerificationEmailHtml(
                "Moussa Toure",
                "Temp@1234"
            );
            await emailService.SendEmailAsync(
                "turaymusaa@gmail.com",
                "Verify Your Bookazone Account",
                emailBody
            );
            return await Task.FromResult(ApiResponse.Success(true));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating tenant");
            return await Task.FromResult(ApiResponse.Error(ErrorHttp.Error, e));
        }
    }

    [Authorize(Policy = "VerifiedDeviceOnly")]
    [HttpGet("dashboard")]
    public async Task<ApiResult> GetDashboard()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var tenantId = User.FindFirstValue("tenant_id");
        var deviceId = User.FindFirstValue("device_id");

        return ApiResponse.Success(new
        {
            Message = $"Access granted for user {userId} from device {deviceId} in tenant {tenantId}"
        });
    }

    
}


public class TestRequest
{
    public IFormFile? image { get; set; }
}