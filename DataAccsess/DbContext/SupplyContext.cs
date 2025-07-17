using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    /// <summary>
    /// Подключение к таблице "SupplyComponent" базы данных "gubinv_supply"
    /// </summary>
    public class SupplyContext : DbContext
    {
        /// <summary>
        /// Таблица с данными о пользователях системы
        /// </summary>
        
        public SupplyContext(DbContextOptions<SupplyContext> options): base(options){}
        public DbSet<SupplyCompanyDb> SupplyCompany { get; set; } = null!;
        public DbSet<ComponentDb> SupplyComponent { get; set; } = null!;
        public DbSet<SupplyManufacturerDb> SupplyManufacturer { get; set; } = null!;
        public DbSet<ProviderDb> SupplyProvider { get; set; } = null!;
        public DbSet<SupplyPurchaseDb> SupplyPurchase { get; set; } = null!;
        public DbSet<PurchaseComponentDb> PurchaseComponent { get; set; } = null!;
        
        public DbSet<PurchaseAuthorizationDb> PurchaseAuthorization { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SupplyCompanyDb>((pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("SupplyCompany");
            }));

            modelBuilder.Entity<ComponentDb>((pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("SupplyComponent");
            }));

            modelBuilder.Entity<SupplyManufacturerDb>((pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("SupplyManufacturer");
            }));

            modelBuilder.Entity<ProviderDb>((pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("SupplyProvider");
            }));

            modelBuilder.Entity<SupplyPurchaseDb>((pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("SupplyPurchase");
            }));

            modelBuilder.Entity<PurchaseComponentDb>((pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("PurchaseComponent");
            }));

            modelBuilder.Entity<PurchaseAuthorizationDb>((pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("PurchaseAuthorization");
            }));


        }
        internal object Find(string email)
        {
            throw new NotImplementedException();
        }
    }
}