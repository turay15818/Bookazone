using System.Linq;
using System.Text.RegularExpressions;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Bookazone.Application.Interfaces.Repository.Profile;
using Bookazone.Domain.Entities.Profile;

namespace Bookazone.Infrastructure.Services;

public class FirebaseNotificationService
{
    private readonly ILogger<FirebaseNotificationService> _logger;
    private readonly FirebaseMessaging _firebaseMessaging;
    private readonly IEmailService _emailService;
    private readonly IDeviceRepository _deviceRepository;
    
    // FCM token validation regex pattern
    private static readonly Regex FcmTokenPattern = new(@"^[a-zA-Z0-9_:-]{140,200}$");

    public FirebaseNotificationService(
        IConfiguration configuration,
        IDeviceRepository deviceRepository,
        ILogger<FirebaseNotificationService> logger,
        IEmailService emailService)
    {
        this._deviceRepository = deviceRepository;
        _logger = logger;
        _emailService = emailService;

        if (FirebaseApp.DefaultInstance == null)
        {
            try
            {
                var serviceAccountPath = configuration["Firebase:CredentialPath"];
                if (!string.IsNullOrEmpty(serviceAccountPath))
                {
                    var fullPath = Path.IsPathRooted(serviceAccountPath)
                        ? serviceAccountPath
                        : Path.Combine(Directory.GetCurrentDirectory(), serviceAccountPath);
                    
                    if (File.Exists(fullPath))
                    {
                        FirebaseApp.Create(new AppOptions
                        {
                            Credential = GoogleCredential.FromFile(fullPath),
                            ProjectId = configuration["Firebase:ProjectId"]
                        });
                        _logger.LogInformation("Firebase Admin SDK initialized successfully for Bookazone");
                    }
                    else
                    {
                        _logger.LogError("Firebase service account file not found at: {Path}", fullPath);
                    }
                }
                else
                {
                    _logger.LogWarning("Firebase service account path not configured. Push notifications disabled.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Firebase Admin SDK");
            }
        }

        _firebaseMessaging = FirebaseMessaging.DefaultInstance;
    }

    private bool IsValidFcmToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;
            
        return FcmTokenPattern.IsMatch(token);
    }

    private async Task<bool> HandleInvalidToken(string invalidToken, Guid? userId = null)
    {
        _logger.LogWarning("Invalid FCM token detected for user {UserId}. Token: {Token}", 
            userId, invalidToken?.Substring(0, Math.Min(10, invalidToken?.Length ?? 0)) + "...");
        
        // Here you could add logic to remove the invalid token from your database
        // Example: await _userService.ClearFcmTokenAsync(userId);
        
        return false;
    }

    public async Task<string?> SendNotificationToUserAsync(Users user, string title, string body, Dictionary<string, string>? data = null)
    {
        try
        {

            if (user?.FcmToken == null)
            {
                _logger.LogWarning("Cannot send notification: FCM token missing for user {UserId}", user?.Id);
                return null;
            }

            // Validate FCM token format
            if (!IsValidFcmToken(user.FcmToken))
            {
                await HandleInvalidToken(user.FcmToken, user.Id);
                return null;
            }

            var message = new Message
            {
                Token = user.FcmToken,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data ?? new Dictionary<string, string>()
            };

            var response = await _firebaseMessaging.SendAsync(message);
            _logger.LogInformation("Notification sent to user {UserId}: {MessageId}", user.Id, response);
            return response;
        }
        catch (FirebaseMessagingException fex) when (fex.MessagingErrorCode == MessagingErrorCode.InvalidArgument ||
                                                     fex.MessagingErrorCode == MessagingErrorCode.Unregistered)
        {
            _logger.LogWarning("Invalid or unregistered FCM token for user {UserId}: {Error}", user?.Id, fex.Message);
            await HandleInvalidToken(user?.FcmToken ?? "", user?.Id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to user {UserId}", user?.Id);
            return null;
        }
    }
    
    public async Task<int> PushToDevices(List<Device> devices, string title, string body, Dictionary<string, string>? data = null)
    {
        try
        {
            var validTokens = devices
                .Where(d => !string.IsNullOrEmpty(d.NotificationToken) && IsValidFcmToken(d.NotificationToken))
                .Select(d => d.NotificationToken!)
                .ToList();

            if (validTokens.Count == 0)
            {
                _logger.LogWarning("No valid notification tokens found in devices");
                return 0;
            }

            _logger.LogInformation("FCM multicast tokens ({Count}): {Tokens}", validTokens.Count, string.Join(",", validTokens));

            var message = new MulticastMessage
            {
                Tokens = validTokens,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data ?? new Dictionary<string, string>()
            };

            var response = await _firebaseMessaging.SendMulticastAsync(message);
            
            // Handle failed tokens
            if (response.FailureCount > 0)
            {
                for (int i = 0; i < response.Responses.Count; i++)
                {
                    var sendResponse = response.Responses[i];
                    if (!sendResponse.IsSuccess)
                    {
                        var failedToken = validTokens[i];
                        _logger.LogWarning("Failed to send to token {Token}: {Error}", 
                            failedToken.Substring(0, Math.Min(10, failedToken.Length)) + "...", 
                            sendResponse.Exception?.Message);
                        
                        // Handle specific error cases
                        if (sendResponse.Exception is FirebaseMessagingException fex &&
                            (fex.MessagingErrorCode == MessagingErrorCode.Unregistered ||
                             fex.MessagingErrorCode == MessagingErrorCode.InvalidArgument))
                        {
                            await HandleInvalidToken(failedToken);
                        }
                    }
                }
            }
            
            _logger.LogInformation("Sent {SuccessCount}/{TotalCount} device notifications", 
                response.SuccessCount, validTokens.Count);
            
            return response.SuccessCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notifications to devices");
            return 0;
        }
    }

    public async Task<int> PushToTokensAsync(
        IEnumerable<string> tokens,
        string title,
        string body,
        Dictionary<string, string>? data = null)
    {
        try
        {
            var validTokens = tokens
                .Where(t => !string.IsNullOrWhiteSpace(t) && IsValidFcmToken(t))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (validTokens.Count == 0)
            {
                _logger.LogWarning("No valid notification tokens provided");
                return 0;
            }

            var totalSuccess = 0;
            foreach (var batch in validTokens.Chunk(500))
            {
                var message = new MulticastMessage
                {
                    Tokens = batch.ToList(),
                    Notification = new Notification
                    {
                        Title = title,
                        Body = body
                    },
                    Data = data ?? new Dictionary<string, string>()
                };

                var response = await _firebaseMessaging.SendMulticastAsync(message);
                totalSuccess += response.SuccessCount;

                if (response.FailureCount > 0)
                {
                    for (int i = 0; i < response.Responses.Count; i++)
                    {
                        var sendResponse = response.Responses[i];
                        if (!sendResponse.IsSuccess)
                        {
                            var failedToken = batch[i];
                            _logger.LogWarning("Failed to send to token {Token}: {Error}",
                                failedToken.Substring(0, Math.Min(10, failedToken.Length)) + "...",
                                sendResponse.Exception?.Message);

                            if (sendResponse.Exception is FirebaseMessagingException fex &&
                                (fex.MessagingErrorCode == MessagingErrorCode.Unregistered ||
                                 fex.MessagingErrorCode == MessagingErrorCode.InvalidArgument))
                            {
                                await HandleInvalidToken(failedToken);
                            }
                        }
                    }
                }
            }

            _logger.LogInformation("Sent {SuccessCount}/{TotalCount} notifications", totalSuccess, validTokens.Count);
            return totalSuccess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notifications to tokens");
            return 0;
        }
    }
    
    public async Task<int> SendNotificationToUsersAsync(List<Users> users, string title, string body, Dictionary<string, string>? data = null)
    {
        try
        {
            var validTokens = users
                .Where(u => !string.IsNullOrEmpty(u.FcmToken) && IsValidFcmToken(u.FcmToken))
                .Select(u => u.FcmToken!)
                .ToList();

            if (validTokens.Count == 0)
            {
                _logger.LogWarning("No valid FCM tokens found");
                return 0;
            }

            var message = new MulticastMessage
            {
                Tokens = validTokens,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data ?? new Dictionary<string, string>()
            };

            var response = await _firebaseMessaging.SendMulticastAsync(message);
            
            // Handle failed tokens
            if (response.FailureCount > 0)
            {
                var usersWithValidTokens = users
                    .Where(u => !string.IsNullOrEmpty(u.FcmToken) && IsValidFcmToken(u.FcmToken))
                    .ToList();
                    
                for (int i = 0; i < response.Responses.Count && i < usersWithValidTokens.Count; i++)
                {
                    var sendResponse = response.Responses[i];
                    if (!sendResponse.IsSuccess)
                    {
                        var user = usersWithValidTokens[i];
                        _logger.LogWarning("Failed to send to user {UserId}: {Error}", 
                            user.Id, sendResponse.Exception?.Message);
                        
                        if (sendResponse.Exception is FirebaseMessagingException fex &&
                            (fex.MessagingErrorCode == MessagingErrorCode.Unregistered ||
                             fex.MessagingErrorCode == MessagingErrorCode.InvalidArgument))
                        {
                            await HandleInvalidToken(user.FcmToken!, user.Id);
                        }
                    }
                }
            }
            
            _logger.LogInformation("Sent {SuccessCount}/{TotalCount} notifications", 
                response.SuccessCount, validTokens.Count);
            
            return response.SuccessCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk notifications");
            return 0;
        }
    }
    
    public async Task<string?> SendSecurityAlertAsync(Users user, List<Device> devices, Device newDevice)
    {
        var tasks = new List<Task>();
        
        // Send push notifications to devices
        var data = new Dictionary<string, string>
        {
            { "type", "security_alert" },
            { "alert_type", "new_device_login" },
            { "device_name", newDevice.Name ?? "Unknown Device" }
        };

        var pushTask = PushToDevices(
            devices,
            "Device security alert 📲",
            "New connection detected to your account from a new device. No action is required if it is you, otherwise you can delete this device from SkillConnect in the devices menu.",
            data
        );
        tasks.Add(pushTask);

        // Send email notification
        if (!string.IsNullOrEmpty(user.Email))
        {
            var deviceInfo = $"{newDevice.System} - {newDevice.Os} ({newDevice.AppVersion})";
            var emailTask = _emailService.SendSecurityAlertEmailAsync(
                user.Email,
                $"{user.Firstname} {user.Lastname}".Trim(),
                newDevice.Name ?? "Unknown Device",
                deviceInfo,
                newDevice.DateCreated
            );
            tasks.Add(emailTask);
        }

        await Task.WhenAll(tasks);
        
        var pushResult = await pushTask;
        _logger.LogInformation("Security alert sent - Push: {PushCount} notifications", pushResult);
        
        return pushResult > 0 ? "Security alert sent successfully" : null;
    }

    public async Task<string?> SendChatMessageNotificationAsync(Users recipient, string senderName, string messageContent, int chatRoomId, int messageId)
    {
        var data = new Dictionary<string, string>
        {
            { "type", "chat_message" },
            { "chatRoomId", chatRoomId.ToString() },
            { "messageId", messageId.ToString() }
        };

        return await SendNotificationToUserAsync(
            recipient,
            $"New message from {senderName}",
            messageContent,
            data
        );
    }

    public async Task<string?> SendChatRoomInvitationAsync(Users recipient, string inviterName, string chatRoomName, Guid chatRoomId)
    {
        var data = new Dictionary<string, string>
        {
            { "type", "chat_invitation" },
            { "chatRoomId", chatRoomId.ToString() }
        };

        return await SendNotificationToUserAsync(
            recipient,
            "Chat Invitation",
            $"{inviterName} invited you to join {chatRoomName}",
            data
        );
    }
    
    
   
    public async Task<ReviewNotificationResult> SendReviewNotificationAsync(
        Users reviewee, 
        Users reviewer, 
        Guid reviewId, 
        int rating, 
        string? comment, 
        string targetType, 
        Guid targetId)
    {
        try
        {
            if (reviewee == null)
            {
                _logger.LogWarning("Cannot send review notification: reviewee is null");
                return new ReviewNotificationResult { Success = false, Message = "Reviewee not found" };
            }

            // Craft professional notification messages
            var title = GetReviewNotificationTitle(rating);
            var body = GetReviewNotificationBody(reviewer, rating, comment);
            
            var data = new Dictionary<string, string>
            {
                { "type", "review_received" },
                { "reviewId", reviewId.ToString() },
                { "rating", rating.ToString() },
                { "reviewerId", reviewer.Id.ToString() },
                { "reviewerName", $"{reviewer.Firstname} {reviewer.Lastname}".Trim() },
                { "targetType", targetType.ToLower() },
                { "targetId", targetId.ToString() },
                { "timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() }
            };

            // Try device-specific tokens first (preferred method)
            var devices = _deviceRepository?.GetUserDevicesWithTokens(reviewee);
            int devicesPushed = 0;
            
            if (devices != null && devices.Any())
            {
                devicesPushed = await PushToDevices(devices, title, body, data);
                _logger.LogInformation("Review notification sent to {DeviceCount} devices for user {UserId}", 
                    devicesPushed, reviewee.Id);
            }

            // Fallback to user FCM token if no devices were notified
            string? directMessageId = null;
            if (devicesPushed == 0)
            {
                directMessageId = await SendNotificationToUserAsync(reviewee, title, body, data);
                _logger.LogInformation("Review notification sent directly to user {UserId}: {MessageId}", 
                    reviewee.Id, directMessageId ?? "failed");
            }

            var success = devicesPushed > 0 || !string.IsNullOrEmpty(directMessageId);
            
            return new ReviewNotificationResult
            {
                Success = success,
                DevicesNotified = devicesPushed,
                MessageId = directMessageId,
                Message = success ? "Review notification sent successfully" : "Failed to send review notification"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending review notification to user {RevieweeId} from reviewer {ReviewerId}", 
                reviewee?.Id, reviewer?.Id);
            
            return new ReviewNotificationResult 
            { 
                Success = false, 
                Message = $"Failed to send notification: {ex.Message}" 
            };
        }
    }

    private static string GetReviewNotificationTitle(int rating)
    {
        return rating switch
        {
            5 => "⭐ Excellent Review Received!",
            4 => "⭐ Great Review Received!",
            3 => "⭐ Good Review Received",
            2 => "⭐ Review Received",
            1 => "⭐ Review Received",
            _ => "⭐ New Review Received"
        };
    }

    private static string GetReviewNotificationBody(Users reviewer, int rating, string? comment)
    {
        var reviewerName = $"{reviewer.Firstname} {reviewer.Lastname}".Trim();
        if (string.IsNullOrEmpty(reviewerName))
            reviewerName = reviewer.Username ?? "A user";

        var stars = new string('⭐', rating);
        var baseMessage = $"{reviewerName} rated you {rating}/5 {stars}";

        if (!string.IsNullOrWhiteSpace(comment) && comment.Length > 0)
        {
            var shortComment = comment.Length > 50 ? comment[..47] + "..." : comment;
            return $"{baseMessage}\n\"{shortComment}\"";
        }

        return baseMessage;
    }

    // Result class for better return type handling
    public class ReviewNotificationResult
    {
        public bool Success { get; set; }
        public int DevicesNotified { get; set; }
        public string? MessageId { get; set; }
        public string Message { get; set; } = string.Empty;
    } 
    
    
    
    
    
    
    
    
    
    

    public async Task<string?> SendTypingNotificationAsync(Users recipient, string typerName, Guid chatRoomId, bool isTyping)
    {
        if (string.IsNullOrEmpty(recipient.FcmToken) || !IsValidFcmToken(recipient.FcmToken))
        {
            if (!string.IsNullOrEmpty(recipient.FcmToken))
                await HandleInvalidToken(recipient.FcmToken, recipient.Id);
            return null;
        }

        var data = new Dictionary<string, string>
        {
            { "type", "typing_status" },
            { "chatRoomId", chatRoomId.ToString() },
            { "isTyping", isTyping.ToString().ToLower() }
        };

        var message = new Message
        {
            Token = recipient.FcmToken,
            Data = data
        };

        try
        {
            var response = await _firebaseMessaging.SendAsync(message);
            _logger.LogInformation("Typing notification sent to user {UserId}", recipient.Id);
            return response;
        }
        catch (FirebaseMessagingException fex) when (fex.MessagingErrorCode == MessagingErrorCode.InvalidArgument ||
                                                     fex.MessagingErrorCode == MessagingErrorCode.Unregistered)
        {
            _logger.LogWarning("Invalid or unregistered FCM token for user {UserId}: {Error}", recipient.Id, fex.Message);
            await HandleInvalidToken(recipient.FcmToken, recipient.Id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending typing notification to user {UserId}", recipient.Id);
            return null;
        }
    }
}
