using System.ComponentModel.DataAnnotations;

namespace Bookazone.Api.Controllers.V1.Config.Request;

public class VehicleMediaUploadRequest
{
    [Required]
    public IFormFile File { get; set; } = null!;
    public bool IsCover { get; set; }
    public int SortOrder { get; set; }
}
