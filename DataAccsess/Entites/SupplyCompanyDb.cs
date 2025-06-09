namespace SUPPLY_API
{
    public class SupplyCompanyDb
    {
        public int Id { get; set; }

        public string? GuidIdCompany { get; set; }

        public string? FullNameCompany { get; set; }

        public string? AbbreviatedNameCompany { get; set; }

        public long? InnCompany { get; set; }

        public string? AddressCompany { get; set; }

        public SupplyCompanyDb() { }
    }
}