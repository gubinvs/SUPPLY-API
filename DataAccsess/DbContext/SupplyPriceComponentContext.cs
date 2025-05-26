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


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PriceDb>((pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("PriceComponent");
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