namespace SUPPLY_API
{
    public class OrderDto
{
    public string? GuidIdSupplyOrder { get; set; }
    public string? GuidIdPurchase { get; set; }
    public string PurchaseId { get; set; } = null!;
    public string PurchaseName { get; set; } = null!;
    public int PurchasePrice { get; set; }
    public string PurchaseCustomer { get; set; } = null!;
    public string SupplyOrderUserStatus { get; set; } = null!;
    public List<OrderComponentDto> OrderComponent { get; set; } = new();
}
}