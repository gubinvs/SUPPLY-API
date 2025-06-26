using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SUPPLY_API
{
    public class DuplicateCleanupService : BackgroundService
    {
        /// <summary>
        /// 📌 Что делает этот код:
        /// Выбирает все группы с одинаковым VendorCodeComponent, где есть дубли;
        /// Сохраняет одну основную запись;
        /// Переносит все связанные данные с дубликатов на основную запись, если таких данных ещё нет;
        /// Удаляет дубликаты и их связи, если они не нужны;
        /// Запускается автоматически раз в сутки.
        /// </summary>
        /// 
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DuplicateCleanupService> _logger;

        public DuplicateCleanupService(IServiceProvider serviceProvider, ILogger<DuplicateCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<SupplyComponentContext>();
                var dbPrice = scope.ServiceProvider.GetRequiredService<SupplyPriceComponentContext>();
                var dbManufact = scope.ServiceProvider.GetRequiredService<ManufacturerComponentContext>();
                var dbUnit = scope.ServiceProvider.GetRequiredService<UnitMeasurementComponentContext>();

                try
                {
                    var grouped = await db.SupplyComponent
                        .GroupBy(c => c.VendorCodeComponent)
                        .Where(g => g.Count() > 1)
                        .ToListAsync(stoppingToken);

                    foreach (var group in grouped)
                    {
                        var toKeep = group.First(); // основная запись
                        var toRemove = group.Skip(1).ToList(); // дубликаты

                        foreach (var duplicate in toRemove)
                        {
                            // Переносим цены
                            var prices = await dbPrice.PriceComponent
                                .Where(p => p.GuidIdComponent == duplicate.GuidIdComponent)
                                .ToListAsync(stoppingToken);

                            foreach (var price in prices)
                            {
                                price.GuidIdComponent = toKeep.GuidIdComponent;
                            }

                            // Переносим производителя
                            var manufact = await dbManufact.ManufacturerComponent
                                .FirstOrDefaultAsync(m => m.GuidIdComponent == duplicate.GuidIdComponent, stoppingToken);
                            if (manufact != null)
                            {
                                var existing = await dbManufact.ManufacturerComponent
                                    .AnyAsync(m => m.GuidIdComponent == toKeep.GuidIdComponent, stoppingToken);

                                if (!existing)
                                {
                                    manufact.GuidIdComponent = toKeep.GuidIdComponent;
                                }
                                else
                                {
                                    dbManufact.ManufacturerComponent.Remove(manufact);
                                }
                            }

                            // Переносим единицу измерения
                            var unit = await dbUnit.UnitMeasurementComponent
                                .FirstOrDefaultAsync(u => u.GuidIdComponent == duplicate.GuidIdComponent, stoppingToken);
                            if (unit != null)
                            {
                                var existing = await dbUnit.UnitMeasurementComponent
                                    .AnyAsync(u => u.GuidIdComponent == toKeep.GuidIdComponent, stoppingToken);

                                if (!existing)
                                {
                                    unit.GuidIdComponent = toKeep.GuidIdComponent;
                                }
                                else
                                {
                                    dbUnit.UnitMeasurementComponent.Remove(unit);
                                }
                            }

                            db.SupplyComponent.Remove(duplicate);
                        }

                        _logger.LogInformation("Объединены и очищены дубли для: {VendorCode}", group.Key);
                    }

                    await dbPrice.SaveChangesAsync(stoppingToken);
                    await dbManufact.SaveChangesAsync(stoppingToken);
                    await dbUnit.SaveChangesAsync(stoppingToken);
                    await db.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при очистке дублей компонентов");
                }
            }
        }
    }
}