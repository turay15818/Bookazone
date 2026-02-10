using Microsoft.EntityFrameworkCore.Metadata;

namespace Bookazone.Application.Provider.Api;

public interface ICountryService
{
    Task<IReadOnlyList<CountryDto>> GetCountriesAsync();
}

public class CountryService(HttpClient httpClient, IConfiguration configuration) : ICountryService
{
    private readonly HttpClient _httpClient = httpClient;

    private readonly string _url =
        configuration["Country:CountryApiUrl"] ?? "hhttps://restcountries.com/v3.1/all?fields=name,cca2,flags";
    public async Task<IReadOnlyList<CountryDto>> GetCountriesAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<RestCountry>>(_url) ?? new List<RestCountry>();
        return response
            .Where(c => !string.IsNullOrWhiteSpace(c.Cca2))
            .OrderBy(c => c.Name.Common)
            .Select(c => new CountryDto
            {
                Code = c.Cca2,
                Name = c.Name.Common,
                FlagSvg = c.Flags.Svg,
                FlagPng = c.Flags.Png,
            })
            .ToList();
    }

    private sealed class RestCountry
    {
        public NameInfo Name { get; set; } = null!;
        public string Cca2 { get; set; } = null!;
        public FlagInfo Flags { get; set; } = null!;
    }

    private sealed class NameInfo
    {
        public string Common { get; set; } = null!;
    }

    private sealed class FlagInfo
    {
        public string Svg { get; set; } = null!;
        public string Png { get; set; } = null!;
    }
}

public sealed class CountryDto
{
    public string Code { get; set; } = null!; // SL
    public string Name { get; set; } = null!; // Sierra Leone
    public string FlagSvg { get; set; } = null!;
    public string FlagPng { get; set; } = null!;
}
