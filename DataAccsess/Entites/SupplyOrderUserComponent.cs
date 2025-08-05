
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    public class SupplyOrderUserComponentDb
    {
        public int Id { get; set; }

        public string? GuidIdSupplyOrder { get; set; }

        public string? GuidIdCollaborator { get; set; }

        public string? VendorCodeComponent { get; set; }

        public string? NameComponent { get; set; }

        public int? QuantityComponent { get; set; }

        public int? PriceComponent { get; set; }

        public DateTime DeliveryTimeComponent { get; set; }

        public SupplyOrderUserComponentDb() { }
    }
}