namespace Bookazone.Application.DTOs.Converter.Inventory.UnitConversion;

using System;
using System.ComponentModel.DataAnnotations;

public class ProductUnitConversionCreateRequest
{
    public Guid ProductId { get; set; }
    public Guid FromUnitId { get; set; }
    public Guid ToUnitId { get; set; }
    [Range(0.000001, double.MaxValue)] public decimal Factor { get; set; } 
}


public class ProductUnitConversionUpdateRequest
{
    public Guid? Id { get; set; }
    public Guid FromUnitId { get; set; }
    public Guid ToUnitId { get; set; }
    [Range(0.000001, double.MaxValue)] public decimal Factor { get; set; } = 1;
    public bool? Active { get; set; }
}

public class ProductUnitConversionVm
{
    public Guid Id { get; set; }
    public Guid FkProductId { get; set; }
public string? ProductName { get; set; }
    public Guid FkTenantId { get; set; }
    public Guid FkFromUnitId { get; set; }
    public Guid FkToUnitId { get; set; }
    public string FromUnitName { get; set; } = string.Empty;
    public string ToUnitName { get; set; } = string.Empty;
    public decimal Factor { get; set; }
    public bool Active { get; set; }
    public DateTime? DateCreated { get; set; }
}
