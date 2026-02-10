using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Bookazone.Api.Controllers.V1.Config.Request;

public class UserRequest : BaseRequest
{
    [NotNull] [MaxLength(300)] public string? Firstname { get; set; }
    [NotNull] [MaxLength(300)] [MinLength(1)] public string? Lastname { get; set; }
    
    [NotNull] [MaxLength(300)] [MinLength(6)] public string? Phone { get; set; }
    [NotNull] [EmailAddress] [MaxLength(300)] [MinLength(4)] public string? Email { get; set; }
    [NotNull] [MaxLength(300)] [MinLength(4)] public string? Address { get; set; }
    [NotNull] [MaxLength(15)] [MinLength(4)] [DefaultValue("Male")] public string? Gender { get; set; }

    [PasswordPropertyText] [MaxLength(300)] [MinLength(6)] public string? Password { get; set; }
    [NotNull] public DateTime? Birthdate { get; set; }
    

}
