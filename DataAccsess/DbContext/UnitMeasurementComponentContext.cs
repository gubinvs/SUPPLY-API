using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    /// <summary>
    /// Подключение к таблице "UnitMeasurementComponent"
    /// </summary>
    public class UnitMeasurementComponentContext : DbContext
    {
        /// <summary>
        /// Таблица с данными о пользователях системы
        /// </summary>
        public DbSet<UnitMeasurementComponentDb> UnitMeasurementComponent { get; set; } = null!;

        public UnitMeasurementComponentContext(DbContextOptions<UnitMeasurementComponentContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UnitMeasurementComponentDb>(pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("UnitMeasurementComponent");
            });
        }

        internal object Find(string email)
        {
            throw new NotImplementedException();
        }
    }
}
