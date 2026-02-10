using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Bookazone.Api.Controllers.V1.Config.Request;

public class EmailPhoneRequest : BaseRequest
{
    [NotNull]
    [MaxLength(300)]
    [MinLength(6)]
    public string? Phone { get; set; }

    [NotNull]
    [EmailAddress]
    [MaxLength(300)]
    [MinLength(4)]
    public string? Email { get; set; }
}