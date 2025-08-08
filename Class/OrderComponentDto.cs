namespace SUPPLY_API 
{
    public class OrderComponentDto
    {
        public string VendorCodeComponent { get; set; } = null!;
        public string NameComponent { get; set; } = null!;
        public int QuantityComponent { get; set; }
        public int PriceComponent { get; set; }
        public string DeliveryTimeComponent { get; set; } = null!;
    }
}