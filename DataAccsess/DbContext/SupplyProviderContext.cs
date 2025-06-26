using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    /// <summary>
    /// Подключение к таблице "SupplyComponent" базы данных "gubinv_supply"
    /// </summary>
    public class SupplyProviderContext : DbContext
    {
        /// <summary>
        /// Таблица с данными о ценах поставщиков и сроках поставки
        /// </summary>
        public DbSet<ProviderDb> SupplyProvider { get; set; } = null!;

        public SupplyProviderContext(DbContextOptions<SupplyProviderContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProviderDb>((pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("SupplyProvider");
            }));
        }

        internal object Find(string email)
        {
            throw new NotImplementedException();
        }
    }
}
