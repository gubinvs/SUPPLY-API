namespace SUPPLY_API
{
    public class OrderUserAuthorizationDb
    {
        public int Id { get; set; }

        public string? GuidIdSupplyOrderUser { get; set; }

        public string? GuidIdCollaborator { get; set; }

        public OrderUserAuthorizationDb() { }
    }
}
