using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    /// <summary>
    /// Подключение к таблице "SupplyComponent" базы данных "gubinv_supply"
    /// </summary>
    public class CompanyCollaboratorContext : DbContext
    {
        /// <summary>
        /// Таблица с данными о пользователях системы
        /// </summary>
        public DbSet<CompanyCollaboratorDb> CompanyCollaborator { get; set; } = null!;

        public CompanyCollaboratorContext(DbContextOptions<CompanyCollaboratorContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CompanyCollaboratorDb>((pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("CompanyCollaborator");
            }));
        }

        internal object Find(string email)
        {
            throw new NotImplementedException();
        }
    }
}
