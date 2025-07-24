namespace SUPPLY_API
{
    public record SaveNewDataPurchaseNameModel
    {
        public string? guidIdPurchase { get; set; }
        public string? purchaseId { get; set; } 
        public string? purchaseName { get; set; } 
        public int purchasePrice { get; set;}
        public string? purchaseCostomer { get; set; } 
    }
}