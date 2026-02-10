using System.ComponentModel;

namespace Bookazone.Application.Common.Shared.Utils;

public static class ApiResponse
{
    public static ApiResult SuccessPaged<T>(
        IEnumerable<T> items,
        int totalPages,
        int pageIndex,
        string? message = null
    )
    {
        return new ApiResultSuccess
        {
            Status = 1,
            Message = message ?? "success",
            Data = items,
            Pages = totalPages,
            PageIndex = pageIndex
        };
    }


    public static ApiResult Success(
        object? data = null,
        string? message = null,
        int? pages = null,
        int? pageIndex = null
    )
    {
        return new ApiResultSuccess
        {
            Status = 1,
            Message = message ?? "success",
            Data = data,
            Pages = pages,
            PageIndex = pageIndex
        };
    }
    public static ApiResult OtpRequired(dynamic? data = null, string? message = null)
    {
        return new ApiResultSuccess
        {
            Data = data,
            Status = -1,
            Message =
                message
                ?? "OTP required, we have sent you an OTP via email/SMS to confirm your identity.",
        };
    }

    public static ApiResult Error(ApiResultError? errorHttp, Exception? exception = null)
    {
        var error = errorHttp ?? ErrorHttp.Error;
        if (exception is Except except)
        {
            error.ErrorCode = except.ErrorCode;
        }
        error.Message = exception == null ? error.Message : exception.Message;
        error.Exception = exception == null ? "" : exception.Message;
        return error;
    }

    public static ApiResult ErrorWithMessage(ApiResultError? errorHttp, string? message = null)
    {
        var error = errorHttp ?? ErrorHttp.Error;
        error.Message = (message ?? "");
        return error;
    }
}

public abstract class ApiResult
{
    [DefaultValue(0)]
    public int Status { get; set; } = 0;

    public string Message { get; set; } = "success";

    public int? Pages { get; set; }

    public int? PageIndex { get; set; }
}

public class ApiResultSuccess : ApiResult
{
    public object? Data { get; set; }
}

public class ApiResultError : ApiResult
{
    public string? ErrorCode { get; set; } = "ERROR";
    public int HttpCode { get; set; } = 500;
    public string? Exception { get; set; }
}

public class ApiResultErrorWithData : ApiResult
{
    public dynamic? Data { get; set; } = null;
    public string? ErrorCode { get; set; } = "ERROR";
    public int HttpCode { get; set; } = 500;
    public string? Exception { get; set; }
}
