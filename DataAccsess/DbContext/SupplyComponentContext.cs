using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    /// <summary>
    /// Подключение к таблице "SupplyComponent" базы данных "gubinv_supply"
    /// </summary>
    public class SupplyComponentContext : DbContext
    {
        /// <summary>
        /// Таблица с данными о пользователях системы
        /// </summary>
        public DbSet<ComponentDb> SupplyComponent { get; set; } = null!;

        public SupplyComponentContext(DbContextOptions<SupplyComponentContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ComponentDb>((pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("SupplyComponent");
            }));
        }

        internal object Find(string email)
        {
            throw new NotImplementedException();
        }
    }
}
