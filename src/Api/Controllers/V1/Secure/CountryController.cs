using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.Provider.Api;

namespace Bookazone.Api.Controllers.V1.Secure;

[ApiController]
[Route(RouteWebVersion1.Secure.Country.Base)]
public class CountryController(
    ICountryService countryService,
    ILogger<CountryController> logger
) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [Route(RouteWebVersion1.Secure.Country.All)]
    public async Task<ApiResult> GetAll()
    {
        try
        {
            var plans = await countryService.GetCountriesAsync();
            return ApiResponse.Success(plans);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching countries");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}
