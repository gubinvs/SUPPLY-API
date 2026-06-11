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
        public DbSet<PriceDb> PriceComponent { get; set; } = null!;
        public DbSet<CollaboratorSystemDb> CollaboratorSystem { get; set; } = null!;
        public DbSet<SupplyOrderUserDb> SupplyOrderUser { get; set; } = null!;
        public DbSet<SupplyOrderUserComponentDb> SupplyOrderUserComponent { get; set; } = null!;
        public DbSet<OrderUserAuthorizationDb> OrderUserAuthorization { get; set; } = null!;
        public DbSet<PurchasePriceDb> PurchasePrice { get; set; } = null!;
        public DbSet<CollaboratorProviderDb> CollaboratorProvider { get; set; } = null!;
        public DbSet<ProviderManufacturerDb> ProviderManufacturer { get; set; } = null!;

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

            modelBuilder.Entity<PriceDb>((pc =>
           {
               pc.HasKey(u => u.Id);
               pc.ToTable("PriceComponent");
           }));

            modelBuilder.Entity<CollaboratorSystemDb>(pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("CollaboratorSystem");
            });

            modelBuilder.Entity<SupplyOrderUserDb>(pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("SupplyOrderUser");
            });

            modelBuilder.Entity<SupplyOrderUserComponentDb>(pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("SupplyOrderUserComponent");
            });

            modelBuilder.Entity<OrderUserAuthorizationDb>(pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("OrderUserAuthorization");
            });

            modelBuilder.Entity<PurchasePriceDb>(pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("PurchasePrice");
            });

            modelBuilder.Entity<CollaboratorProviderDb>(pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("CollaboratorProvider");
            });

            modelBuilder.Entity<ProviderManufacturerDb>(pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("ProviderManufacturer");
            });


        }
        internal object Find(string email)
        {
            throw new NotImplementedException();
        }
    }
}