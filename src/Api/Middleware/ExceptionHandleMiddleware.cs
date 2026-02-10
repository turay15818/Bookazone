using System.Text;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.Interfaces.Repository.Others;
using Bookazone.Domain.Entities.Others;
using Newtonsoft.Json;

namespace Bookazone.Api.Middleware;

public class ExceptionHandleMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext httpContext, IAuditLogRepository auditLogRepository)
    {
        var auditLogSave = false;
        var auditLog = new AuditLog();
        try
        {
            httpContext.Request.EnableBuffering();
            var requestBody = await ReadRequestBody(httpContext.Request);
            var bodyStream = httpContext.Response.Body;
            using var responseBodyStream = new MemoryStream();
            httpContext.Response.Body = responseBodyStream;
            auditLog.Url = httpContext.Request.Path;
            auditLog.RequestHeaders = JsonConvert.SerializeObject(httpContext.Request.Headers);
            auditLog.IpAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            auditLog.Payload = auditLog.Url.Contains("auth") ? "" : requestBody;
            auditLog.RequestContentType = httpContext.Request.ContentType;
            auditLog.RequestMethod = httpContext.Request.Method;
            auditLog.DateCreated = DateTime.UtcNow;
            auditLog.QueryString = httpContext.Request.QueryString.ToString();
            
            await next(httpContext);
            
            auditLog.User = httpContext.User.Identity?.Name;
            var responseBody = await ReadResponseBody(httpContext.Response);
            await responseBodyStream.CopyToAsync(bodyStream);
            auditLog.ResponseStatusCode = httpContext.Response.StatusCode.ToString();
            auditLog.ResponseHeaders = JsonConvert.SerializeObject(httpContext.Response.Headers);
            auditLog.ResponseContentType = httpContext.Response.ContentType;
            auditLog.Response = auditLog.Url.Contains("auth") ? "" : responseBody;
            auditLog.ResponseTimestamp = DateTime.UtcNow;
            auditLog.DateUpdated = DateTime.UtcNow;
            auditLogRepository.Create(auditLog);
            auditLogSave = true;
        }
        catch (Exception e)
        {
            await HandleException(e, httpContext);
            auditLog.Response = $"{e.Message} - {e.Source} - {e.StackTrace}";
            if (auditLogSave)
            {
                auditLogRepository.Update(auditLog);
            }
            else
            {
                auditLogRepository.Create(auditLog);
            }
            // throw;
        }
    }

    private static async Task HandleException(Exception e, HttpContext httpContext)
    {
        Console.WriteLine("============> message: " + e.Message);
        Console.WriteLine("============> source: " + e.Source);
        Console.WriteLine("============> stacktrace: " + e.StackTrace);
        Console.WriteLine("============> inner: " + e.InnerException);
        Console.WriteLine("============> targetsite: " + e.TargetSite);
        
        var error = e is not Except exception? 
            new
            {
                ErrorHttp.Error.Status,
                ErrorHttp.Error.Message,
                ErrorHttp.Error.ErrorCode,
                ErrorHttp.Error.Exception
            } : new
            {
                exception.GetError().Status,
                exception.GetError().Message,
                exception.GetError().ErrorCode,
                exception.GetError().Exception
            };
        httpContext.Response.StatusCode = error.Status;
        
        await httpContext.Response.WriteAsJsonAsync(error);
    }
    
    public async Task<string> ReadRequestBody(HttpRequest request)
    {
        using var reader = new StreamReader(
            request.Body,
            encoding: Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 30,
            leaveOpen: true);

        var requestBody = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        return requestBody;
    }

    public async Task<string?> ReadResponseBody(HttpResponse? response)
    {
        if (response == null) return null;
        response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(response.Body).ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);

        return responseBody;
    }
}

public static class ExceptionHandleMiddlewareExtensions
{
    public static void UseExceptionHandleMiddleware(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<ExceptionHandleMiddleware>();
    }
}