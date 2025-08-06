using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    public class SupplyOrderUserDb : DbContext
    {
        public int Id { get; set; }

        public string? GuidIdSupplyOrder { get; set; }

        public string? GuidIdPurchase { get; set; }

        public string? PurchaseId { get; set; }

        public string? PurchaseName { get; set; }

        public int? PurchasePrice { get; set; }

        public string? PurchaseCostomer { get; set; }

        public string? SupplyOrderUserStatus { get; set; }


        public SupplyOrderUserDb() { }
    }
}