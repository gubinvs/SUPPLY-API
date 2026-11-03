using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{

    /// <summary>
    /// Все данные по таблицам в одном месте для развертывания базы данных
    /// </summary>
    public class MigrationDbContext : DbContext
    {
        public DbSet<CollaboratorSystemDb> CollaboratorSystem { get; set; } = null!;
        public DbSet<CompanyCollaboratorDb> CompanyCollaborator { get; set; } = null!;
        public DbSet<DeliveryAddressDb> DeliveryAddress { get; set; } = null!;
        public DbSet<ProviderDb> SupplyProvider { get; set; } = null!;
        public DbSet<ComponentDb> SupplyComponent { get; set; } = null!;
        public DbSet<SupplyManufacturerDb> SupplyManufacturer { get; set; } = null!;
        public DbSet<SupplyUnitMeasurementDb> SupplyUnitMeasurement { get; set; } = null!;
        public DbSet<ManufacturerComponentDb> ManufacturerComponent { get; set; } = null!;
        public DbSet<UnitMeasurementComponentDb> UnitMeasurementComponent { get; set; } = null!;
        public DbSet<PriceDb> PriceComponent { get; set; } = null!;
        public DbSet<GoodsTableDb> GoodsTable { get; set; }
        public DbSet<DiscountTableDb> DiscountTable { get; set; }
        public DbSet<SupplyCompanyDb> SupplyCompany { get; set; } = null!;
        public DbSet<SupplyPurchaseDb> SupplyPurchase { get; set; } = null!;
        public DbSet<PurchaseComponentDb> PurchaseComponent { get; set; } = null!;
        public DbSet<PurchaseAuthorizationDb> PurchaseAuthorization { get; set; } = null!;
        public DbSet<SupplyOrderUserDb> SupplyOrderUser { get; set; } = null!;
        public DbSet<SupplyOrderUserComponentDb> SupplyOrderUserComponent { get; set; } = null!;
        public DbSet<OrderUserAuthorizationDb> OrderUserAuthorization { get; set; } = null!;
        public DbSet<CollaboratorProviderDb> CollaboratorProvider { get; set; } = null!;
        public DbSet<ProviderManufacturerDb> ProviderManufacturer { get; set; } = null!;







        public MigrationDbContext (DbContextOptions<MigrationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CollaboratorSystemDb>(pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("CollaboratorSystem");
            });

            modelBuilder.Entity<CompanyCollaboratorDb>((pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("CompanyCollaborator");
            }));
            
            modelBuilder.Entity<DeliveryAddressDb>((pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("DeliveryAddress");
            }));

            modelBuilder.Entity<ProviderDb>((pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("SupplyProvider");
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

            modelBuilder.Entity<SupplyUnitMeasurementDb>(pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("SupplyUnitMeasurement");
            });

            modelBuilder.Entity<ManufacturerComponentDb>((pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("ManufacturerComponent");
            }));

            modelBuilder.Entity<UnitMeasurementComponentDb>(pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("UnitMeasurementComponent");
            });
            
            modelBuilder.Entity<PriceDb>((pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("PriceComponent");
            }));

            modelBuilder.Entity<GoodsTableDb>((pc =>
                {
                    pc.HasKey(u => u.Id);
                    pc.ToTable("goods_table");
                }));

            modelBuilder.Entity<DiscountTableDb>((pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("discountTable");
            }));

            modelBuilder.Entity<SupplyCompanyDb>((pc =>
            {
                pc.HasKey(u => u.Id);
                pc.ToTable("SupplyCompany");
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