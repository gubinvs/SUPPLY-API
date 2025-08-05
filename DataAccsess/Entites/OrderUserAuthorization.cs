using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    public class OrderUserAuthorizationDb : DbContext
    {
        public int Id { get; set; }

        public string? GuidIdSupplyOrderUser { get; set; }

        public string? GuidIdCollaborator { get; set; }

        public OrderUserAuthorizationDb() { }
    }
}