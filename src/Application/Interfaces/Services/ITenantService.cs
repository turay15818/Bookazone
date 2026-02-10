using Bookazone.Api.Controllers.V1.Config.Request;
using Bookazone.Application.Common.Shared.Utils;

namespace Bookazone.Application.Interfaces.Services;

public interface ITenantService
{
    Task<ApiResult> GetAllAsync(string? q, int pageSize, int pageIndex);
    Task<ApiResult> GetByIdAsync(Guid id);
    Task<ApiResult> CreateAsync(TenantRequest request);
    Task<ApiResult> VerifyAccountAsync(VerifyAccountRequest request);
    Task<ApiResult> UpdateAsync(TenantUpdateRequest request);
    Task<ApiResult> DeleteAsync(Guid id);
    Task<ApiResult> GetProfileAsync();
}