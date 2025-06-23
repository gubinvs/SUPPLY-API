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


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SupplyManufacturerDb>((pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("SupplyManufacturer");
            }));
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseMySql("server=gubinv.beget.tech;user=gubinv_component;password=2MC&bZgO;database=gubinv_component;",
            optionsBuilder.UseMySql($"server={SqlConnect.ServerName};user={SqlConnect.User};password={SqlConnect.Password};database={SqlConnect.Database};",
                new MySqlServerVersion(new Version(8, 0, 25)), options => options.EnableRetryOnFailure());

        }

        internal object Find(string email)
        {
            throw new NotImplementedException();
        }
    }
}