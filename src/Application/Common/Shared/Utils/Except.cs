namespace Bookazone.Application.Common.Shared.Utils;

public class Except(ApiResultError errorHttp, string? exception = null) : Exception
{
    public int? Status { get; } = errorHttp.Status;
    public override string Message { get; } = (errorHttp.Message ?? ErrorHttp.Error.Message) ?? string.Empty;
    public int? StatusCode { get; } = errorHttp.HttpCode;
    public string? ErrorCode { get; } = (errorHttp.ErrorCode ?? ErrorHttp.Error.ErrorCode) ?? string.Empty;
    private ApiResultError ApiResultError { get; } = errorHttp;
    public string? Exception { get; } = exception;

    public ApiResultError GetError()
    {
        return ApiResultError;
    }
}