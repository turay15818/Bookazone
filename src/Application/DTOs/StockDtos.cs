using System;

namespace Bookazone.Application.DTOs;

public class StockItemVm
{
    public Guid Id { get; set; }
    public Guid FkProductId { get; set; }
    public Guid FkLocationId { get; set; }
    public decimal Quantity { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime? DateCreated { get; set; }
}

public sealed class StockOverviewVm
{
    public Guid? ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? Sku { get; set; }
    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; } = null!;
    public string LocationName { get; set; } = null!;

    public Guid LocationId { get; set; }
    public string LocationCode { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public decimal Quantity { get; set; }
    public decimal OnHand { get; set; }
    public decimal Reserved { get; set; }   // for now = 0
    public decimal Available { get; set; }  // for now = OnHand
}

public class StockItemUpdateRequest
{
    public decimal Quantity { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class InventoryTransactionCreateRequest
{
    public string TransactionType { get; set; } = null!;
    public string? Reference { get; set; }
    public Guid FkProductId { get; set; }
    public Guid? FromLocationId { get; set; }
    public Guid? ToLocationId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public Guid? RelatedDocumentId { get; set; }
    public string? RelatedDocumentType { get; set; }
}

public class InventoryTransactionVm
{
    public Guid Id { get; set; }
    public string TransactionType { get; set; } = null!;
    public string? Reference { get; set; }
    public Guid FkProductId { get; set; }
    public Guid? FromLocationId { get; set; }
    public Guid? ToLocationId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public DateTime? DateCreated { get; set; }
}

public class StockAdjustmentCreateRequest
{
    public Guid FkWarehouseId { get; set; }
    public string? Reason { get; set; }
    public InventoryTransactionCreateRequest[] Transactions { get; set; } = Array.Empty<InventoryTransactionCreateRequest>();
}

public class StockTransferCreateRequest
{
    public string TransferNumber { get; set; } = null!;
    public Guid FromWarehouseId { get; set; }
    public Guid ToWarehouseId { get; set; }
    public string? Notes { get; set; }
    public InventoryTransactionCreateRequest[] Transactions { get; set; } = Array.Empty<InventoryTransactionCreateRequest>();
}


public class StockAvailabilityVm
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? Sku { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal OnHand { get; set; }
    public decimal Reserved { get; set; }
    public decimal Available { get; set; }
    public bool Sellable => Available > 0;
}

public class LocationStockAvailabilityVm
{
    public Guid LocationId { get; set; }
    public string LocationCode { get; set; } = null!;
    public Guid WarehouseId { get; set; }

    public decimal OnHand { get; set; }
    public decimal Reserved { get; set; }
    public decimal Available { get; set; }
}