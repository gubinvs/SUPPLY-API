using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    /// <summary>
    /// Подключение к таблице "ManufacturerComponent"
    /// </summary>
    public class ManufacturerComponentContext : DbContext
    {
        /// <summary>
        /// Таблица с данными о пользователях системы
        /// </summary>
        public DbSet<ManufacturerComponentDb> ManufacturerComponent { get; set; } = null!;

        public ManufacturerComponentContext(DbContextOptions<ManufacturerComponentContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ManufacturerComponentDb>((pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("ManufacturerComponent");
            }));
        }

        internal object Find(string email)
        {
            throw new NotImplementedException();
        }
    }
}
