using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SUPPLY_API
{
    public class RemoveDuplicatesManufacturer : BackgroundService
    {
        /// <summary>
        /// 📌 Что делает этот код:
        /// Выбирает всех поставщиков с одинаковым наменованием NameManufacturer, где есть дубли;
        /// Сохраняет одну основную запись;
        /// Переносит все связанные данные с дубликатов на основную запись, если таких данных ещё нет;
        /// Удаляет дубликаты и их связи, если они не нужны;
        /// Запускается автоматически раз в сутки.
        /// </summary>
        /// 
        
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RemoveDuplicatesManufacturer> _logger;

        public RemoveDuplicatesManufacturer(IServiceProvider serviceProvider, ILogger<RemoveDuplicatesManufacturer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 🔹 Немедленный запуск (для отладки)
            await DoWorkAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                // ⏱ Пауза на 1 день
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);

                // 🔁 Повтор выполнения
                await DoWorkAsync(stoppingToken);
            }
        }

        private async Task DoWorkAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SupplyManufacturerContext>();
            var dbManufact = scope.ServiceProvider.GetRequiredService<ManufacturerComponentContext>();

            try
            {
                var grouped = await db.SupplyManufacturer
                    .GroupBy(c => c.NameManufacturer)
                    .Where(g => g.Count() > 1)
                    .ToListAsync(stoppingToken);

                foreach (var group in grouped)
                {
                    var toKeep = group.First(); // основная запись
                    var toRemove = group.Skip(1).ToList(); // дубликаты

                    foreach (var duplicate in toRemove)
                    {
                        var manufact = await dbManufact.ManufacturerComponent
                            .FirstOrDefaultAsync(m => m.GuidIdComponent == duplicate.GuidIdManufacturer, stoppingToken);
                        if (manufact != null)
                        {
                            var existing = await dbManufact.ManufacturerComponent
                                .AnyAsync(m => m.GuidIdComponent == toKeep.GuidIdManufacturer, stoppingToken);

                            if (!existing)
                            {
                                manufact.GuidIdComponent = toKeep.GuidIdManufacturer;
                            }
                            else
                            {
                                dbManufact.ManufacturerComponent.Remove(manufact);
                            }
                        }

                        db.SupplyManufacturer.Remove(duplicate);
                    }

                    _logger.LogInformation("Объединены и очищены дубли для: {VendorCode}", group.Key);
                }

                await dbManufact.SaveChangesAsync(stoppingToken);
                await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при очистке дублей компонентов");
            }
        }
    }
}