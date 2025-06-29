using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{

    public class HandyDbContext : DbContext
    {
        public HandyDbContext(DbContextOptions<HandyDbContext> options) : base(options) { }

        public DbSet<ProviderDb> SupplyProvider { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProviderDb>((pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("SupplyProvider");
            }));
        }




        // другие таблицы
    }
}
