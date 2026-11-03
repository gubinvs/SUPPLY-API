using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
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
                var dbContext = scope.ServiceProvider.GetRequiredService<SupplyContext>();
                
                // Используем асинхронную версию для предотвращения блокировки потока
                await dbContext.Database.MigrateAsync(cancellationToken);
                
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