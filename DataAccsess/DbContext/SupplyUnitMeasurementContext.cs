using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    /// <summary>
    /// Подключение к таблице "UnitMeasurementComponent"
    /// </summary>
    public class SupplyUnitMeasurementContext : DbContext
    {
        /// <summary>
        /// Таблица с данными о пользователях системы
        /// </summary>
        public DbSet<SupplyUnitMeasurementDb> SupplyUnitMeasurement { get; set; } = null!;

        public SupplyUnitMeasurementContext(DbContextOptions<SupplyUnitMeasurementContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SupplyUnitMeasurementDb>(pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("SupplyUnitMeasurement");
            });
        }

        internal object Find(string email)
        {
            throw new NotImplementedException();
        }
    }
}

