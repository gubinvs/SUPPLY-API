using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    /// <summary>
    /// Подключение к таблице "SupplyComponent" базы данных "gubinv_supply"
    /// </summary>
    public class SupplyCompanyContext : DbContext
    {
        /// <summary>
        /// Таблица с данными о пользователях системы
        /// </summary>
        public DbSet<SupplyCompanyDb> SupplyCompany { get; set; } = null!;

        public SupplyCompanyContext(DbContextOptions<SupplyCompanyContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SupplyCompanyDb>((pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("SupplyCompany");
            }));
        }

        internal object Find(string email)
        {
            throw new NotImplementedException();
        }
    }
}
