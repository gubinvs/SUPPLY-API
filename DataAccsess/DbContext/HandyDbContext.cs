using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{

    public class HandyDbContext : DbContext
    {
        public HandyDbContext(DbContextOptions<HandyDbContext> options) : base(options) { }

        public DbSet<ProviderDb> SupplyProvider { get; set; } = null!;
        public DbSet<ComponentDb> SupplyComponent { get; set; } = null!;
        public DbSet<SupplyManufacturerDb> SupplyManufacturer { get; set; } = null!;
        public DbSet<SupplyUnitMeasurementDb> SupplyUnitMeasurement { get; set; } = null!;
        public DbSet<ManufacturerComponentDb> ManufacturerComponent { get; set; } = null!;
        public DbSet<UnitMeasurementComponentDb> UnitMeasurementComponent { get; set; } = null!;
        public DbSet<PriceDb> PriceComponent { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
        }


        internal object Find(string email)
        {
            throw new NotImplementedException();
        }

    }
}
