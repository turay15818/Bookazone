using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;

namespace Bookazone.Domain.Entities.Others;

public class AppConfig : UserBaseEntity
{
    [MaxLength(50)] public string? Slug { get; set; }
    [MaxLength(500)] public string? Value { get; set; }
    [MaxLength(3000)] public string? Message { get; set; }
}