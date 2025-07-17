namespace SUPPLY_API
{
    public class PurchaseComponent
    {
        public string? GuidIdPurchase { get; set; }
        public string? GuidIdComponent { get; set; }
        public string? VendorCodeComponent { get; set; }
        public string? NameComponent { get; set; }
        public int? RequiredQuantityItem { get; set; }
        public int? PurchaseItemPrice { get; set; }
        public string? BestComponentProvider { get; set; }
        public string? DeliveryTimeComponent { get; set; }
    }
}