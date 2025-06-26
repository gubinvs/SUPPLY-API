using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    /// <summary>
    /// Подключение к таблице "UnitMeasurementComponent"
    /// </summary>
    public class SupplyManufacturerContext : DbContext
    {
        /// <summary>
        /// Таблица с данными о пользователях системы
        /// </summary>
        public DbSet<SupplyManufacturerDb> SupplyManufacturer { get; set; } = null!;

        public SupplyManufacturerContext(DbContextOptions<SupplyManufacturerContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SupplyManufacturerDb>((pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("SupplyManufacturer");
            }));
        }

        internal object Find(string email)
        {
            throw new NotImplementedException();
        }
    }
}
