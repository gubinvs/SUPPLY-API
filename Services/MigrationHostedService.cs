using Microsoft.EntityFrameworkCore;
using SUPPLY_API;

public class MigrationHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MigrationHostedService> _logger;

    public MigrationHostedService(IServiceProvider serviceProvider, ILogger<MigrationHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("--> Проверка и применение миграций к MySQL...");

        using (var scope = _serviceProvider.CreateScope())
        {
            try
            {
                var сollaboratorSystem = scope.ServiceProvider.GetRequiredService<CollaboratorSystemContext>();
                await сollaboratorSystem.Database.MigrateAsync(cancellationToken);

                var сompanyCollaborator = scope.ServiceProvider.GetRequiredService<CompanyCollaboratorContext>();
                await сompanyCollaborator.Database.MigrateAsync(cancellationToken);

                var deliveryAddress = scope.ServiceProvider.GetRequiredService<DeliveryAddressContext>();
                await deliveryAddress.Database.MigrateAsync(cancellationToken);

                var handyDb = scope.ServiceProvider.GetRequiredService<HandyDbContext>();
                await handyDb.Database.MigrateAsync(cancellationToken);

                var manufacturerComponent = scope.ServiceProvider.GetRequiredService<ManufacturerComponentContext>();
                await manufacturerComponent.Database.MigrateAsync(cancellationToken);

                var supplyCompany = scope.ServiceProvider.GetRequiredService<SupplyCompanyContext>();
                await supplyCompany.Database.MigrateAsync(cancellationToken);

                var supplyComponent = scope.ServiceProvider.GetRequiredService<SupplyComponentContext>();
                await supplyComponent.Database.MigrateAsync(cancellationToken);

                var dbContext = scope.ServiceProvider.GetRequiredService<SupplyContext>();
                await dbContext.Database.MigrateAsync(cancellationToken);

                var supplyManufacturer = scope.ServiceProvider.GetRequiredService<SupplyManufacturerContext>();
                await supplyManufacturer.Database.MigrateAsync(cancellationToken);

                var supplyPriceComponent = scope.ServiceProvider.GetRequiredService<SupplyPriceComponentContext>();
                await supplyPriceComponent.Database.MigrateAsync(cancellationToken);

                var supplyProvider = scope.ServiceProvider.GetRequiredService<SupplyProviderContext>();
                await supplyProvider.Database.MigrateAsync(cancellationToken);

                var supplyUnitMeasurement = scope.ServiceProvider.GetRequiredService<SupplyUnitMeasurementContext>();
                await supplyUnitMeasurement.Database.MigrateAsync(cancellationToken);

                var unitMeasurementComponent = scope.ServiceProvider.GetRequiredService<UnitMeasurementComponentContext>();
                await unitMeasurementComponent.Database.MigrateAsync(cancellationToken);
                
                _logger.LogInformation("--> Миграции успешно применились, таблицы созданы!");
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"--> КРИТИЧЕСКАЯ ОШИБКА МИГРАЦИИ: {ex.Message}");
                
                // Если миграции не накатились, обычно имеет смысл остановить приложение,
                // закомментируйте 'throw', если хотите, чтобы приложение всё равно запустилось:
                throw; 
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}