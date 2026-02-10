using System.Net;
using System.Net.Mail;
using System.Linq;
using Microsoft.Extensions.Options;
using Bookazone.Application.DTOs;

namespace Bookazone.Infrastructure.Services;

public interface IEmailService
{
    public string GenerateDeviceChallengeEmailHtml(string userName, string numericOtp);
    Task<bool> SendOtpEmailAsync(string email, string otp, string purpose, string reference);
    Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task<bool> SendSecurityAlertEmailAsync(string email, string userName, string deviceName, string deviceInfo,
        DateTime? loginTime);
    public string GenerateVerificationEmailHtml(string userName, string verificationLink);
   public string  GenerateDeviceVerificationEmailHtml(string name, string link);
   public string GeneratePasswordResetEmailHtml(string userName, string resetLink);
   string GenerateUserInviteEmailHtml(string inviteeName, string inviterName, string tenantName, string inviteLink);



   public string GenerateSubscriptionEmailHtml(string tenantName, string planName, string description, string price,
       string period);

   public string GenerateSubscriptionExpiryReminderHtml(string tenantName, string planName, string expiryDate);
   public string GenerateSubscriptionExpiredEmailHtml(string tenantName, string planName);
   public string GenerateEventAnnouncementEmailHtml(
       string userName,
       string eventTitle,
       string categoryName,
       DateTime startUtc,
       string? venueName,
       string? city,
       decimal? priceFrom,
       string? currency,
       string? coverUrl,
       string eventUrl);
}

public class EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    : IEmailService
{
    private readonly EmailSettings _emailSettings = emailSettings.Value;

    
    public string GenerateSubscriptionEmailHtml(string tenantName, string planName, string description, string price, string period)
{
    return $@"
        <div style='font-family:Arial,sans-serif;color:#333;max-width:600px;margin:auto;'>
            <h2>🎉 Welcome to Bookazone, {tenantName}!</h2>
            <p>Your <strong>{planName}</strong> plan has been successfully activated.</p>
            <p>{description}</p>
            <p><strong>Price:</strong> {price} per {period}</p>
            <p>You can now access premium features immediately.</p>
            <br/>
            <a href='https://app.Bookazone.com/dashboard' 
               style='background:#4F46E5;color:white;padding:10px 20px;text-decoration:none;border-radius:5px;'>Go to Dashboard</a>
        </div>";
}

public string GenerateSubscriptionExpiryReminderHtml(string tenantName, string planName, string expiryDate)
{
    return $@"
        <div style='font-family:Arial,sans-serif;color:#333;max-width:600px;margin:auto;'>
            <h2>⏰ Subscription Expiring Soon</h2>
            <p>Hello {tenantName},</p>
            <p>Your <strong>{planName}</strong> subscription will expire on <strong>{expiryDate}</strong>.</p>
            <p>To avoid service interruption, please renew your plan before it expires.</p>
            <br/>
            <a href='https://app.Bookazone.com/subscription' 
               style='background:#4F46E5;color:white;padding:10px 20px;text-decoration:none;border-radius:5px;'>Renew Subscription</a>
        </div>";
}

public string GenerateSubscriptionExpiredEmailHtml(string tenantName, string planName)
{
    return $@"
        <div style='font-family:Arial,sans-serif;color:#333;max-width:600px;margin:auto;'>
            <h2>💔 Subscription Expired</h2>
            <p>Hello {tenantName},</p>
            <p>Your <strong>{planName}</strong> subscription has expired. You no longer have access to premium features.</p>
            <p>Renew now to reactivate your account and continue using Bookazone.</p>
            <br/>
            <a href='https://app.Bookazone.com/subscription' 
               style='background:#4F46E5;color:white;padding:10px 20px;text-decoration:none;border-radius:5px;'>Renew Now</a>
        </div>";
}

public string GenerateEventAnnouncementEmailHtml(
    string userName,
    string eventTitle,
    string categoryName,
    DateTime startUtc,
    string? venueName,
    string? city,
    decimal? priceFrom,
    string? currency,
    string? coverUrl,
    string eventUrl)
{
    var dateLabel = startUtc.ToString("dddd, MMMM dd, yyyy 'at' HH:mm 'UTC'");
    var location = string.Join(", ", new[] { venueName, city }.Where(v => !string.IsNullOrWhiteSpace(v)));
    var priceLabel = priceFrom.HasValue
        ? $"{(currency ?? "USD")} {priceFrom.Value:N0}"
        : "Free";
    var safeCover = string.IsNullOrWhiteSpace(coverUrl)
        ? "https://imgcdn.dev/i/Ys7lIM"
        : coverUrl;

    return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>New Event on Bookazone</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f6f8;
            margin: 0;
            padding: 0;
            color: #333;
        }}
        .container {{
            max-width: 600px;
            margin: 24px auto;
            background: #ffffff;
            border-radius: 10px;
            overflow: hidden;
            box-shadow: 0 4px 10px rgba(0,0,0,0.08);
        }}
        .header {{
            background-color: #0f172a;
            color: #ffffff;
            padding: 18px 24px;
            text-align: center;
        }}
        .header h1 {{
            margin: 0;
            font-size: 22px;
        }}
        .cover img {{
            width: 100%;
            height: auto;
            display: block;
        }}
        .content {{
            padding: 24px;
        }}
        .content h2 {{
            margin-top: 0;
            font-size: 20px;
        }}
        .meta {{
            margin: 16px 0;
            padding: 12px 16px;
            background-color: #f8fafc;
            border-radius: 8px;
            font-size: 14px;
        }}
        .meta p {{
            margin: 6px 0;
        }}
        .cta {{
            text-align: center;
            margin: 24px 0 8px;
        }}
        .cta a {{
            background-color: #2563eb;
            color: #ffffff !important;
            text-decoration: none;
            padding: 12px 20px;
            border-radius: 6px;
            font-weight: bold;
            display: inline-block;
        }}
        .footer {{
            text-align: center;
            padding: 16px;
            font-size: 12px;
            color: #94a3b8;
            background-color: #f8fafc;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Bookazone Event Alert</h1>
        </div>
        <div class='cover'>
            <img src='{safeCover}' alt='Event cover'>
        </div>
        <div class='content'>
            <p>Hello {userName},</p>
            <h2>{eventTitle}</h2>
            <p>A new <strong>{categoryName}</strong> event has just been published.</p>
            <div class='meta'>
                <p><strong>Date:</strong> {dateLabel}</p>
                <p><strong>Location:</strong> {location}</p>
                <p><strong>Price:</strong> {priceLabel}</p>
            </div>
            <div class='cta'>
                <a href='{eventUrl}'>View Event</a>
            </div>
        </div>
        <div class='footer'>
            © {DateTime.UtcNow.Year} Bookazone. All rights reserved.
        </div>
    </div>
</body>
</html>";
}

    
    public string GeneratePasswordResetEmailHtml(string userName, string resetLink)
{
    return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Password Reset</title>
    <style>
        body {{
            font-family: 'Segoe UI', Arial, sans-serif;
            background-color: #f4f6f8;
            margin: 0;
            padding: 0;
            color: #333;
        }}
        .container {{
            max-width: 600px;
            margin: 40px auto;
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 4px 10px rgba(0,0,0,0.05);
            overflow: hidden;
        }}
        .header {{
            background-color: #0066cc;
            color: white;
            text-align: center;
            padding: 20px 10px;
        }}
        .header h1 {{
            margin: 0;
            font-size: 22px;
        }}
        .content {{
            padding: 30px 40px;
        }}
        .content h2 {{
            color: #333;
            font-size: 20px;
            margin-bottom: 15px;
        }}
        .content p {{
            line-height: 1.6;
            font-size: 16px;
            margin-bottom: 25px;
        }}
        .button {{
            display: inline-block;
            background-color: #0066cc;
            color: white !important;
            text-decoration: none;
            padding: 12px 20px;
            border-radius: 5px;
            font-weight: 600;
        }}
        .footer {{
            text-align: center;
            padding: 20px;
            font-size: 13px;
            color: #888;
            background-color: #f4f6f8;
        }}
        .footer a {{
            color: #0066cc;
            text-decoration: none;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Bookazone Password Reset</h1>
        </div>
        <div class=""content"">
            <h2>Hello {userName},</h2>
            <p>We received a request to reset your password for your Bookazone account. You can reset your password by clicking the button below:</p>
            
            <p style=""text-align:center;"">
                <a href=""{resetLink}"" class=""button"">Reset Password</a>
            </p>
            
            <p>If you didn’t request a password reset, you can safely ignore this email. This link will expire in 15 minutes for your security.</p>
            <p>Thank you,<br>The Bookazone Team</p>
        </div>
        <div class=""footer"">
            &copy; {DateTime.UtcNow.Year} Bookazone. All rights reserved.<br>
            <a href=""https://www.Bookazone.com"">www.Bookazone.com</a>
        </div>
    </div>
</body>
</html>";
}

    
    public string GenerateDeviceChallengeEmailHtml(string userName, string numericOtp)
    {
        return $@"
    <html><body>
    <div style='font-family:Segoe UI, sans-serif;'>
      <img src='https://imgcdn.dev/i/Ys7lIM' width='120' alt='Bookazone'/>
      <h2>Device verification required</h2>
      <p>Hello {userName},</p>
      <p>We noticed you're attempting to verify from a different device. Please provide the code below to confirm this action:</p>
      <h1 style='letter-spacing:6px'>{numericOtp}</h1>
      <p>This code expires in 10 minutes. If you didn't request this, contact support immediately.</p>
      <p>— Bookazone</p>
    </div>
    </body></html>";
    }

    
    public string GenerateDeviceVerificationEmailHtml(string name, string link)
    {
        return $@"
        <html>
            <body style='font-family:Arial;'>
                <h3>Hi {name},</h3>
                <p>We detected a login attempt from a new device. 
                Please verify this device to continue.</p>
                <a href='{link}' style='background:#007bff;color:#fff;
                padding:10px 15px;text-decoration:none;border-radius:5px;'>Verify Device</a>
                <p>This link will expire in 10 minutes.</p>
                <p>— The Bookazone Security Team</p>
            </body>
        </html>";
    }

    
   public string GenerateVerificationEmailHtml(string userName, string verificationLink)
    {
        return $@"
        <!DOCTYPE html>
        <html lang='en'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>Verify Your Account</title>
            <style>
                body {{
                    background-color: #f4f7f9;
                    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                    color: #333;
                    margin: 0;
                    padding: 0;
                }}
                .container {{
                    max-width: 600px;
                    margin: 40px auto;
                    background: #ffffff;
                    border-radius: 12px;
                    overflow: hidden;
                    box-shadow: 0 5px 15px rgba(0,0,0,0.08);
                }}
                .header {{
                    background-color: #004aad;
                    padding: 25px 0;
                    text-align: center;
                }}
                .header img {{
                    width: 140px;
                    height: auto;
                }}
                .content {{
                    padding: 35px;
                    text-align: center;
                }}
                .content h1 {{
                    color: #222;
                    font-size: 24px;
                    margin-bottom: 10px;
                }}
                .content p {{
                    color: #555;
                    font-size: 16px;
                    line-height: 1.6;
                    margin-bottom: 25px;
                }}
                .btn {{
                    background-color: #004aad;
                    color: #fff !important;
                    text-decoration: none;
                    padding: 14px 28px;
                    border-radius: 8px;
                    font-weight: bold;
                    display: inline-block;
                }}
                .footer {{
                    background-color: #f0f3f6;
                    padding: 20px;
                    text-align: center;
                    font-size: 13px;
                    color: #777;
                }}
                .footer a {{
                    color: #004aad;
                    text-decoration: none;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <img src='https://raw.githubusercontent.com/turay15818/APIBookazone/refs/heads/feature/mt/develop/v1.0/wwwroot/AppFiles/logo.jpg?token=GHSAT0AAAAAADMKFDPYJQHRFGSNMBPWBVVE2HO7X5A' alt='Bookazone Logo'>
                </div>
                <div class='content'>
                    <h1>Welcome to Bookazone, {userName}!</h1>
                    <p>We’re excited to have you on board. To get started, please verify your account by clicking the button below:</p>
                    <p><a href='{verificationLink}' class='btn'>Verify My Account</a></p>
                    <p>If you didn’t create an account, you can safely ignore this email.</p>
                </div>
                <div class='footer'>
                    <p>© {DateTime.UtcNow.Year} Bookazone SL. All rights reserved.</p>
                    <p><a href='https://sierraBookazone-li.com'>Visit our website</a></p>
                </div>
            </div>
        </body>
        </html>";
    }
    
    public string GenerateUserInviteEmailHtml(string inviteeName, string inviterName, string tenantName, string inviteLink)
{
    return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
    <title>You're Invited to Join {tenantName}</title>
    <style>
        body {{
            font-family: 'Segoe UI', Roboto, Arial, sans-serif;
            background-color: #f9fafb;
            margin: 0;
            padding: 0;
        }}
        .container {{
            max-width: 600px;
            margin: 40px auto;
            background: #ffffff;
            border-radius: 12px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
            padding: 32px;
        }}
        h2 {{
            color: #111827;
            text-align: center;
        }}
        p {{
            color: #4b5563;
            line-height: 1.6;
        }}
        .cta-button {{
            display: inline-block;
            background-color: #4F46E5;
            color: #ffffff !important;
            text-decoration: none;
            padding: 12px 24px;
            border-radius: 6px;
            font-weight: 600;
            margin: 24px 0;
        }}
        .footer {{
            text-align: center;
            color: #9ca3af;
            font-size: 12px;
            margin-top: 24px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h2>You're Invited to Join {tenantName}!</h2>
        <p>Hi {(string.IsNullOrEmpty(inviteeName) ? "there" : inviteeName)},</p>
        <p><strong>{inviterName}</strong> has invited you to join <strong>{tenantName}</strong> on Bookazone.</p>
        <p>Click the button below to accept your invitation and complete your account setup:</p>
        <div style='text-align:center;'>
            <a href='{inviteLink}' class='cta-button'>Accept Invitation</a>
        </div>
        <p>If you did not expect this invitation, you can safely ignore this email.</p>
        <div class='footer'>
            &copy; {DateTime.UtcNow.Year} Bookazone. All rights reserved.
        </div>
    </div>
</body>
</html>
";
}
    
    public async Task<bool> SendOtpEmailAsync(string email, string otp, string purpose, string reference)
    {
        try
        {
            var subject = "SkillConnect SL - Your OTP Code";
            var body = GenerateOtpEmailTemplate(otp, purpose, reference);
            
            return await SendEmailAsync(email, subject, body, true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending OTP email to {Email}", email);
            return false;
        }
    }
    public async Task<bool> SendSecurityAlertEmailAsync(string email, string userName, string deviceName,
        string deviceInfo, DateTime? loginTime)
    {
        try
        {
            var subject = "SkillConnect SL - Security Alert: New Device Login";
            var body = GenerateSecurityAlertTemplate(userName, deviceName, deviceInfo, loginTime);
            
            return await SendEmailAsync(email, subject, body, true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending security alert email to {Email}", email);
            return false;
        }
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        try
        {
            using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
            {
                EnableSsl = _emailSettings.EnableSsl,
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            message.To.Add(to);

            await client.SendMailAsync(message);
            logger.LogInformation("Email sent successfully to {Email}", to);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending email to {Email}", to);
            return false;
        }
    }

     private string GenerateSecurityAlertTemplate(string userName, string deviceName, string deviceInfo, DateTime? loginTime)
    {
        var timeZone = "UTC"; 
        var formattedTime = loginTime?.ToString("dddd, MMMM dd, yyyy 'at' hh:mm tt") + $" ({timeZone})";
        
        return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>SkillConnect SL - Security Alert</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        body {{
            font-family: 'Arial', sans-serif;
            line-height: 1.6;
            color: #333;
            background-color: #f4f4f4;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            box-shadow: 0 0 20px rgba(0,0,0,0.1);
        }}
        .header {{
            background: linear-gradient(135deg, #dc3545 0%, #c82333 100%);
            color: white;
            padding: 30px 20px;
            text-align: center;
        }}
        .header h1 {{
            font-size: 28px;
            margin-bottom: 10px;
        }}
        .header p {{
            font-size: 16px;
            opacity: 0.9;
        }}
        .content {{
            padding: 40px 30px;
        }}
        .alert-icon {{
            text-align: center;
            font-size: 60px;
            color: #dc3545;
            margin-bottom: 20px;
        }}
        .alert-title {{
            text-align: center;
            color: #dc3545;
            font-size: 24px;
            margin-bottom: 20px;
            font-weight: bold;
        }}
        .greeting {{
            font-size: 18px;
            margin-bottom: 20px;
        }}
        .alert-message {{
            background-color: #f8d7da;
            color: #721c24;
            padding: 20px;
            border-radius: 10px;
            border-left: 4px solid #dc3545;
            margin: 20px 0;
        }}
        .device-info {{
            background-color: #f8f9fa;
            padding: 20px;
            border-radius: 10px;
            margin: 20px 0;
            border: 1px solid #dee2e6;
        }}
        .device-info h3 {{
            color: #495057;
            margin-bottom: 15px;
            font-size: 18px;
        }}
        .info-row {{
            display: flex;
            justify-content: space-between;
            margin-bottom: 10px;
            padding: 8px 0;
            border-bottom: 1px solid #e9ecef;
        }}
        .info-row:last-child {{
            border-bottom: none;
        }}
        .info-label {{
            font-weight: bold;
            color: #6c757d;
        }}
        .info-value {{
            color: #495057;
        }}
        .action-section {{
            background-color: #fff3cd;
            color: #856404;
            padding: 20px;
            border-radius: 10px;
            margin: 20px 0;
            border-left: 4px solid #ffc107;
        }}
        .action-section h3 {{
            margin-bottom: 15px;
        }}
        .action-list {{
            margin: 10px 0;
            padding-left: 20px;
        }}
        .action-list li {{
            margin: 8px 0;
        }}
        .security-tips {{
            background-color: #d1ecf1;
            color: #0c5460;
            padding: 20px;
            border-radius: 10px;
            margin: 20px 0;
            border-left: 4px solid #17a2b8;
        }}
        .security-tips h3 {{
            margin-bottom: 15px;
        }}
        .tips-list {{
            margin: 10px 0;
            padding-left: 20px;
        }}
        .tips-list li {{
            margin: 8px 0;
        }}
        .footer {{
            background-color: #343a40;
            color: white;
            padding: 30px 20px;
            text-align: center;
        }}
        .footer p {{
            margin: 10px 0;
            font-size: 14px;
        }}
        .social-links {{
            margin: 20px 0;
        }}
        .social-links a {{
            color: #17a2b8;
            text-decoration: none;
            margin: 0 10px;
            font-size: 16px;
        }}
        .contact-support {{
            background-color: #17a2b8;
            color: white;
            padding: 15px;
            border-radius: 10px;
            text-align: center;
            margin: 20px 0;
        }}
        .contact-support a {{
            color: white;
            text-decoration: none;
            font-weight: bold;
        }}
        @media only screen and (max-width: 600px) {{
            .container {{
                margin: 0;
                box-shadow: none;
            }}
            .content {{
                padding: 20px 15px;
            }}
            .info-row {{
                flex-direction: column;
            }}
            .info-label {{
                margin-bottom: 5px;
            }}
        }}
    </style>
</head>
<body>
    <div class='container'>
        <!-- Header -->
        <div class='header'>
            <h1>🔐 SkillConnect SL</h1>
            <p>Security Alert Notification</p>
        </div>

        <!-- Main Content -->
        <div class='content'>
            <div class='alert-icon'>🚨</div>
            <div class='alert-title'>Security Alert: New Device Login</div>
            
            <div class='greeting'>
                Hello {userName},
            </div>

            <div class='alert-message'>
                <strong>⚠️ We detected a new login to your SkillConnect SL account from a device we don't recognize.</strong>
                <br><br>
                If this was you, you can safely ignore this email. If this wasn't you, please secure your account immediately.
            </div>

            <div class='device-info'>
                <h3>📱 Device Information</h3>
                <div class='info-row'>
                    <span class='info-label'>Device Name:</span>
                    <span class='info-value'>{deviceName}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Device Info:</span>
                    <span class='info-value'>{deviceInfo}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Login Time:</span>
                    <span class='info-value'>{formattedTime}</span>
                </div>
            </div>

            <div class='action-section'>
                <h3>🛡️ What should you do?</h3>
                <p><strong>If this was you:</strong></p>
                <ul class='action-list'>
                    <li>No action required - you can continue using your account normally</li>
                    <li>Consider this a routine security notification</li>
                </ul>
                
                <p><strong>If this wasn't you:</strong></p>
                <ul class='action-list'>
                    <li>Change your password immediately</li>
                    <li>Check your account for any unauthorized activity</li>
                    <li>Remove unknown devices from your account</li>
                    <li>Contact our support team if you need assistance</li>
                </ul>
            </div>

            <div class='security-tips'>
                <h3>🔒 Security Tips</h3>
                <ul class='tips-list'>
                    <li>Use a strong, unique password for your SkillConnect SL account</li>
                    <li>Enable biometric authentication when available</li>
                    <li>Regularly review your connected devices</li>
                    <li>Never share your login credentials with others</li>
                    <li>Log out from public or shared devices</li>
                </ul>
            </div>

            <div class='contact-support'>
                <p>Need help securing your account?</p>
                <a href='mailto:support@skillconnectsl.com'>📧 Contact Support</a>
            </div>
        </div>

        <!-- Footer -->
        <div class='footer'>
            <p><strong>SkillConnect SL</strong></p>
            <p>Connecting Skills, Creating Opportunities</p>
            <div class='social-links'>
                <a href='mailto:support@skillconnectsl.com'>📧 support@skillconnectsl.com</a>
                <a href='#'>🌐 www.skillconnectsl.com</a>
            </div>
            <p>© 2024 SkillConnect SL. All rights reserved.</p>
            <p style='font-size: 12px; opacity: 0.7;'>
                This is an automated security alert. Please do not reply directly to this message.
            </p>
        </div>
    </div>
</body>
</html>";
    }

    
    private string GenerateOtpEmailTemplate(string otp, string purpose, string reference)
    {
        return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>SkillConnect SL - OTP Verification</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        body {{
            font-family: 'Arial', sans-serif;
            line-height: 1.6;
            color: #333;
            background-color: #f4f4f4;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            box-shadow: 0 0 20px rgba(0,0,0,0.1);
        }}
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 30px 20px;
            text-align: center;
        }}
        .header h1 {{
            font-size: 28px;
            margin-bottom: 10px;
        }}
        .header p {{
            font-size: 16px;
            opacity: 0.9;
        }}
        .content {{
            padding: 40px 30px;
            text-align: center;
        }}
        .otp-section {{
            background-color: #f8f9fa;
            border-radius: 15px;
            padding: 30px;
            margin: 30px 0;
            border: 2px dashed #667eea;
        }}
        .otp-code {{
            font-size: 48px;
            font-weight: bold;
            color: #667eea;
            letter-spacing: 10px;
            margin: 20px 0;
            text-shadow: 2px 2px 4px rgba(0,0,0,0.1);
        }}
        .purpose {{
            background-color: #e3f2fd;
            color: #1976d2;
            padding: 15px;
            border-radius: 10px;
            margin: 20px 0;
            border-left: 4px solid #1976d2;
        }}
        .warning {{
            background-color: #fff3cd;
            color: #856404;
            padding: 15px;
            border-radius: 10px;
            margin: 20px 0;
            border-left: 4px solid #ffc107;
        }}
        .reference {{
            background-color: #f8f9fa;
            color: #6c757d;
            padding: 10px;
            border-radius: 5px;
            font-size: 14px;
            margin: 20px 0;
        }}
        .footer {{
            background-color: #343a40;
            color: white;
            padding: 30px 20px;
            text-align: center;
        }}
        .footer p {{
            margin: 10px 0;
            font-size: 14px;
        }}
        .social-links {{
            margin: 20px 0;
        }}
        .social-links a {{
            color: #667eea;
            text-decoration: none;
            margin: 0 10px;
            font-size: 16px;
        }}
        .timer {{
            color: #dc3545;
            font-weight: bold;
            font-size: 16px;
        }}
        @media only screen and (max-width: 600px) {{
            .container {{
                margin: 0;
                box-shadow: none;
            }}
            .content {{
                padding: 20px 15px;
            }}
            .otp-code {{
                font-size: 36px;
                letter-spacing: 5px;
            }}
        }}
    </style>
</head>
<body>
    <div class='container'>
        <!-- Header -->
        <div class='header'>
            <h1>🔐 SkillConnect SL</h1>
            <p>Your trusted platform for connecting skills and opportunities</p>
        </div>

        <!-- Main Content -->
        <div class='content'>
            <h2>OTP Verification Required</h2>
            <p>Hello! We received a request for <strong>{purpose}</strong> verification.</p>
            
            <div class='purpose'>
                <strong>Purpose:</strong> {purpose}
            </div>

            <div class='otp-section'>
                <h3>Your OTP Code</h3>
                <div class='otp-code'>{otp}</div>
                <p>Enter this code to complete your verification</p>
            </div>

            <div class='warning'>
                <strong>⚠️ Important:</strong><br>
                • This OTP is valid for <span class='timer'>15 minutes</span> only<br>
                • Do not share this code with anyone<br>
                • If you didn't request this, please ignore this email
            </div>

            <div class='reference'>
                <strong>Reference ID:</strong> {reference}
            </div>

            <p>If you're having trouble, please contact our support team with the reference ID above.</p>
        </div>

        <!-- Footer -->
        <div class='footer'>
            <p><strong>SkillConnect SL</strong></p>
            <p>Connecting Skills, Creating Opportunities</p>
            <div class='social-links'>
                <a href='#'>📧 support@skillconnectsl.com</a>
                <a href='#'>🌐 www.skillconnectsl.com</a>
            </div>
            <p>© 2024 SkillConnect SL. All rights reserved.</p>
            <p style='font-size: 12px; opacity: 0.7;'>
                This is an automated email. Please do not reply directly to this message.
            </p>
        </div>
    </div>
</body>
</html>";
    }
}
