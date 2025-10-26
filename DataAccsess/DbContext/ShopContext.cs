using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{

    public class ShopContext : DbContext
    {
        public ShopContext(DbContextOptions<ShopContext> options) : base(options) {}

        public DbSet<GoodsTableDb> GoodsTable { get; set; }
        public DbSet<DiscountTableDb> DiscountTable { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Здесь добавляем DbSet для таблицы товаров

            modelBuilder.Entity<GoodsTableDb>((pc =>
                {
                    pc.HasKey(u => u.Id);
                    pc.ToTable("goods_table");
                }));

                modelBuilder.Entity<DiscountTableDb>((pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("discountTable");
            }));
        }


        internal object Find(string email)
        {
            throw new NotImplementedException();
        }

    }
}
