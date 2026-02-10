namespace Bookazone.Application.DTOs.Response.Others;

public sealed class SalesFinancialSnapshot
{
    public decimal OriginalSaleTotal { get; init; }
    public decimal ReturnedTotal { get; init; }
    public decimal ExchangeTotal { get; init; }
    public decimal PaidByCustomer { get; init; }
    public decimal RefundedToCustomer { get; init; }
    
    public decimal TotalTaxAmount { get; init; }
    public decimal TotalDiscountAmount { get; init; }
    public decimal TotalPromotionAmount { get; init; }
    
    
    
    // 🔹 ORIGINAL composition (issued sale only)
    public decimal OriginalTaxAmount { get; init; }
    public decimal OriginalDiscountAmount { get; init; }
    public decimal OriginalPromotionAmount { get; init; }

    // 🔹 RETURN composition
    public decimal ReturnedTaxAmount { get; init; }
    public decimal ReturnedDiscountAmount { get; init; }
    public decimal ReturnedPromotionAmount { get; init; }

    // 🔹 EXCHANGE composition
    public decimal ExchangeTaxAmount { get; init; }
    public decimal ExchangeDiscountAmount { get; init; }
    public decimal ExchangePromotionAmount { get; init; }
}

