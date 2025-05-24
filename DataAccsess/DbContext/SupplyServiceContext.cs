using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{

    /// <summary>
    /// Подключение к таблице "userSystems" базы данных "gubinv_component"
    /// </summary>
    public class CompanyContext : DbContext
    {
        /// <summary>
        /// Таблица с описанием и стоимостью НКУ
        /// </summary>
        public DbSet<CompanyDb> companySystems { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CompanyDb>((pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("companySystems");
            }));
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql("server=gubinv.beget.tech;user=gubinv_component;password=2MC&bZgO;database=gubinv_component;",
                new MySqlServerVersion(new Version(8, 0, 25)), options => options.EnableRetryOnFailure());

        }

        internal object Find(string email)
        {
            throw new NotImplementedException();
        }
    }
}