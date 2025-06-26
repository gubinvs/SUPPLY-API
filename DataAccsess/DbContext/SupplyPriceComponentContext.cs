using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    /// <summary>
    /// Подключение к таблице "SupplyComponent" базы данных "gubinv_supply"
    /// </summary>
    public class SupplyPriceComponentContext : DbContext
    {
        /// <summary>
        /// Таблица с данными о ценах поставщиков и сроках поставки
        /// </summary>
        public DbSet<PriceDb> PriceComponent { get; set; } = null!;

        public SupplyPriceComponentContext(DbContextOptions<SupplyPriceComponentContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PriceDb>((pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("PriceComponent");
            }));
        }

        internal object Find(string email)
        {
            throw new NotImplementedException();
        }
    }
}
