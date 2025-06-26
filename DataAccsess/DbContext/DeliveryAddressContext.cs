using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    /// <summary>
    /// Подключение к таблице "SupplyComponent" базы данных "gubinv_supply"
    /// </summary>
    public class DeliveryAddressContext : DbContext
    {
        /// <summary>
        /// Таблица с данными о пользователях системы
        /// </summary>
        public DbSet<DeliveryAddressDb> DeliveryAddress { get; set; } = null!;

        public DeliveryAddressContext(DbContextOptions<DeliveryAddressContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DeliveryAddressDb>((pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("DeliveryAddress");
            }));
        }

        internal object Find(string email)
        {
            throw new NotImplementedException();
        }
    }
}
