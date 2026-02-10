namespace Bookazone.Application.Common.Shared.Utils;

public static class ErrorHttp
{
    public static readonly ApiResultError TooManyRequests = new() { Status = 0, ErrorCode = "ERROR", Message = "Too many request sent, please try again after 30 minutes.", HttpCode = 500 };
    public static readonly ApiResultError WeakPassword = new() { Status = 0, ErrorCode = "ERROR", Message = "Password must be at least 8 characters long, contain uppercase, lowercase, number, and special character.", HttpCode = 500 };
    public static readonly ApiResultError PasswordReused = new() { Status = 0, ErrorCode = "ERROR", Message = "You cannot reuse a previous password", HttpCode = 500 };
    public static readonly ApiResultError SuspiciousActivity = new() { Status = 0, ErrorCode = "ERROR", Message = "Suspicious activity on your account, the location is unusual", HttpCode = 500 };
    public static readonly ApiResultError Conflict = new() { Status = 0, ErrorCode = "ERROR", Message = "Order is being edited by another user.", HttpCode = 500 };
    
    
    public static readonly ApiResultError AccountTypeNotExist = new() { Status = 0, ErrorCode = "ERROR", Message = "Account Type does not exist.", HttpCode = 500 };
    public static readonly ApiResultError SpecializationAlreadyExists = new() { Status = 0, ErrorCode = "ERROR", Message = "Specialization already exist.", HttpCode = 500 };
    public static readonly ApiResultError FileUploadFailed = new() { Status = 0, ErrorCode = "ERROR", Message = "File upload fail.", HttpCode = 500 };
    public static readonly ApiResultError OtpExpired = new() { Status = 0, ErrorCode = "ERROR", Message = "OTP has expired", HttpCode = 401 };
    public static readonly ApiResultError UserPhoneAlreadyExists = new() { Status = 0, ErrorCode = "ERROR", Message = "Phone Number already taken, please try with a different Phone Number.", HttpCode = 500 };
    public static readonly ApiResultError UserEmailAlreadyExists = new() { Status = 0, ErrorCode = "ERROR", Message = "Email already taken, please try with a different Email address.", HttpCode = 500 };
    public static readonly ApiResultError UserUsernameAlreadyExists = new() { Status = 0, ErrorCode = "ERROR", Message = "Username already taken, please try with a different Username.", HttpCode = 500 };
    public static readonly ApiResultError Error = new() { Status = 0, ErrorCode = "ERROR", Message = "Sorry, it seems that something went wrong, please try again now or later.", HttpCode = 500 };
    public static readonly ApiResultError AlreadyExists = new() { Status = 0, ErrorCode = "ERROR", Message = "Profile already exit.", HttpCode = 500 };
    
    public static readonly ApiResultError AppInactive = new() { Status = 0, ErrorCode = "ERROR", Message = "Oops! your version is outdated, a new release of the same application is available on the Store, please upgrade your app to enjoy better features.", HttpCode = 500 };
    public static readonly ApiResultError BadRequest = new() { Status = 0, ErrorCode = "ERROR", Message = "Bad Request executed", HttpCode = 500 };
    public static readonly ApiResultError SelfActionNotAllow = new() { Status = 0, ErrorCode = "ERROR", Message = "This self action is not allowed", HttpCode = 500 };
    public static readonly ApiResultError FormInvalid = new() { Status = 0, ErrorCode = "ERROR", Message = "Some fields are empty, fill the form before send request.", HttpCode = 500 };
    public static readonly ApiResultError NotFound = new() { Status = 0, ErrorCode = "ERROR", Message = "Data not found", HttpCode = 404 };
    public static readonly ApiResultError DbOpenConnectionFailed = new() { Status = 0, ErrorCode = "ERROR", Message = "Database can't open", HttpCode = 500 };
    public static readonly ApiResultError DbQueryRunFailed = new() { Status = 0, ErrorCode = "ERROR", Message = "Database query failed", HttpCode = 500 };
    public static readonly ApiResultError DbCreateError = new() { Status = 0, ErrorCode = "ERROR", Message = "Failed to create", HttpCode = 500 };
    public static readonly ApiResultError DbErrorObjectEmpty = new() { Status = 0, ErrorCode = "ERROR", Message = "Object empty, they is no field", HttpCode = 500 };
    public static readonly ApiResultError DbUpdateError = new() { Status = 0, ErrorCode = "ERROR", Message = "Object not found", HttpCode = 500 };
    public static readonly ApiResultError DbFindError = new() { Status = 0, ErrorCode = "ERROR", Message = "Object not found", HttpCode = 500 };
    public static readonly ApiResultError DbStatusError = new() { Status = 0, ErrorCode = "ERROR", Message = "Status failed", HttpCode = 500 };
    public static readonly ApiResultError DbDeleteError = new() { Status = 0, ErrorCode = "ERROR", Message = "Delete failed", HttpCode = 500 };
    public static readonly ApiResultError NotEncoded = new() { Status = 0, ErrorCode = "ERROR", Message = "Data not encoded", HttpCode = 500 };
    public static readonly ApiResultError CustomerNotFound = new() { Status = 0, ErrorCode = "ERROR", Message = "Your customer profile is not found on the system.", HttpCode = 500 };
    public static readonly ApiResultError UserNotFound = new() { Status = 0, ErrorCode = "USER_NOT_FOUND", Message = "Your user/signatory is not existing or it has been deleted.", HttpCode = 500 };
    public static readonly ApiResultError UserLocked = new() { Status = 0, ErrorCode = "ERROR", Message = "Your user/signatory account is locked. Kindly try back few minutes.", HttpCode = 500 };
    public static readonly ApiResultError UserFrozen = new() { Status = 0, ErrorCode = "ERROR", Message = "Your user/signatory account is frozen. Kindly contact your account officer or your user/signatory management.", HttpCode = 500 };
    public static readonly ApiResultError UserAccountNotFound = new() { Status = 0, ErrorCode = "USER_CREDENTIAL_NOT_FOUND", Message = "We are unable to retrieve your bank account. Please check your details or contact your account officer.", HttpCode = 500 };
    public static readonly ApiResultError TokenExpired = new() { Status = 0, ErrorCode = "ERROR", Message = "token expired", HttpCode = 500 };
    public static readonly ApiResultError UsernameExisting = new() { Status = 0, ErrorCode = "ERROR", Message = "The username already used by another user.", HttpCode = 500 };
    public static readonly ApiResultError UserWithoutEmail = new() { Status = 0, ErrorCode = "ERROR", Message = "Your profile doesn't have an email address.", HttpCode = 500 };
    public static readonly ApiResultError UserWithoutPhone = new() { Status = 0, ErrorCode = "ERROR", Message = "Your profile doesn't have a phone number.", HttpCode = 500 };
    public static readonly ApiResultError UserWithoutEmailPhone = new() { Status = 0, ErrorCode = "ERROR", Message = "Your profile doesn't have an email address and phone number.", HttpCode = 500 };
    public static readonly ApiResultError EmailExisting = new() { Status = 0, ErrorCode = "ERROR", Message = "The email address already used by another user.", HttpCode = 500 };
    public static readonly ApiResultError PermissionExisting = new() { Status = 0, ErrorCode = "ERROR", Message = "The Permissions is already existing", HttpCode = 500 };

    public static readonly ApiResultError PhoneExisting = new() { Status = 0, ErrorCode = "ERROR", Message = "The phone number already used by another user", HttpCode = 500 };
    public static readonly ApiResultError PhoneEmailExistingForDiffCustomer = new() { Status = 0, ErrorCode = "ERROR", Message = "Different email and phone numbers belong to two different customers", HttpCode = 500 };
    public static readonly ApiResultError PasswordExpired = new() { Status = 0, ErrorCode = "PASSWORD_EXPIRED", Message = "You are invited to change your password in order to perform any other action.", HttpCode = 500 };
    public static readonly ApiResultError PasswordNotMatch = new() { Status = 0, ErrorCode = "ERROR", Message = "The passwords are not the same", HttpCode = 500 };
    public static readonly ApiResultError OldPasswordNotMatching = new() { Status = 0, ErrorCode = "ERROR", Message = "The password provided does not match the existing password", HttpCode = 500 };
    public static readonly ApiResultError DeviceNotFound = new() { Status = 0, ErrorCode = "ERROR", Message = "Devices not found", HttpCode = 500 };
    public static readonly ApiResultError BiometricUnset = new() { Status = 0, ErrorCode = "ERROR", Message = "Biometric authentication is not configured between this phone and the server or has been reset, please use your password to authenticate.", HttpCode = 500 };
    public static readonly ApiResultError OtpInvalid = new() { Status = 0, ErrorCode = "ERROR", Message = "The one time pasword (OTP) provided is invalid.", HttpCode = 500 };
    public static readonly ApiResultError OtpNotSent = new() { Status = 0, ErrorCode = "ERROR", Message = "Two-factor verification is currently unavailable, please try again later if this error persists.", HttpCode = 500 };
    public static readonly ApiResultError DeviceNotVerified = new() { Status = 0, ErrorCode = "ERROR", Message = "Your device is not yet verified.", HttpCode = 500 };
    public static readonly ApiResultError DeviceVerificationRequired = new() { Status = 0, ErrorCode = "ERROR", Message = "For security reason please verify this device, before you can login", HttpCode = 500 };

    public static readonly ApiResultError DeviceCountdown = new() { Status = 0, ErrorCode = "ERROR", Message = "For security reasons, this device can only be used for transactions after _DATE_.", HttpCode = 500 };
    public static readonly ApiResultError DeviceVerifyNotAvailable = new() { Status = 0, ErrorCode = "ERROR", Message = "Devices verification not available.", HttpCode = 500 };
    public static readonly ApiResultError BadCredentials = new() { Status = 0, ErrorCode = "BAD_CREDENTIALS", Message = "Invalid username or password. Your account will be blocked after 3 attempts.", HttpCode = 500 };
    public static readonly ApiResultError FormNotFound = new() { Status = 0, ErrorCode = "ERROR", Message = "Customer's form collection not found", HttpCode = 500 };
    public static readonly ApiResultError Unauthorized = new() { Status = 0, ErrorCode = "ERROR", Message = "You are Unauthorized to perform this action", HttpCode = 401 };
    public static readonly ApiResultError Forbidden = new() { Status = 0, ErrorCode = "ERROR", Message = "You are Forbidden to perform this action", HttpCode = 403 };

    // Job related errors
    public static readonly ApiResultError CannotUpdateApplication = new() { Status = 0, ErrorCode = "ERROR", Message = "Cannot update application as it is no longer in pending status.", HttpCode = 400 };
    public static readonly ApiResultError CannotDeleteApplication = new() { Status = 0, ErrorCode = "ERROR", Message = "Cannot delete application as it is no longer in pending status.", HttpCode = 400 };
    public static readonly ApiResultError InvalidStatus = new() { Status = 0, ErrorCode = "ERROR", Message = "Invalid status provided.", HttpCode = 400 };
    public static readonly ApiResultError InvalidDateRange = new() { Status = 0, ErrorCode = "ERROR", Message = "Invalid date range. Start date must be before end date.", HttpCode = 400 };
    public static readonly ApiResultError CategoryDoesNotExist = new() { Status = 0, ErrorCode = "ERROR", Message = "Category does not exist.", HttpCode = 404 };
    public static readonly ApiResultError CompanyDoesNotExist = new() { Status = 0, ErrorCode = "ERROR", Message = "Company does not exist.", HttpCode = 404 };
    public static readonly ApiResultError UploadFailed = new() { Status = 0, ErrorCode = "ERROR", Message = "Failed to upload file.", HttpCode = 500 };
}
