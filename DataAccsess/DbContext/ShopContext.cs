using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{

    public class ShopContext : DbContext
    {
        public ShopContext(DbContextOptions<HandyDbContext> options) : base(options) { }

        public DbSet<ProviderDb> SupplyProvider { get; set; } = null!;
  

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GoodsTableDb>((pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("goods_table");
            }));
        }


        internal object Find(string email)
        {
            throw new NotImplementedException();
        }

    }
}
