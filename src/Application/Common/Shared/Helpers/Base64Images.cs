namespace Bookazone.Application.Common.Shared.Helpers;

public class Base64Images
{
    private readonly IConfiguration _configuration;

    public Base64Images(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public string NotFoundImage => _configuration["Base64Images:NotFoundImage"] 
                                   ?? throw new InvalidOperationException("NotFoundImage configuration is missing");
}