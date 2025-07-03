using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SUPPLY_API
{
    public class RemoveDuplicatesManufacturer : BackgroundService
    {
        /// <summary>
        /// 📌 Что делает этот код:
        /// Ищет поставщиков с одинаковым именем (NameManufacturer);
        /// Сохраняет одну основную запись;
        /// Переносит связанные данные (ManufacturerComponent) с дубликатов на основную запись, если их нет;
        /// Удаляет дубликаты и лишние связи;
        /// Запускается автоматически раз в сутки.
        /// </summary>

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

                // Шаг 1: найти имена производителей с дубликатами
                var duplicateNames = await db.SupplyManufacturer
                    .FromSqlRaw(@"
                        SELECT NameManufacturer
                        FROM SupplyManufacturer
                        GROUP BY NameManufacturer
                        HAVING COUNT(*) > 1
                    ")
                    .Select(m => m.NameManufacturer)
                    .ToListAsync(stoppingToken);

                // Шаг 2: загрузить дублирующиеся записи и сгруппировать по имени
                var grouped = await db.SupplyManufacturer
                    .Where(m => duplicateNames.Contains(m.NameManufacturer))
                    .ToListAsync(stoppingToken);

                var groupedByName = grouped
                    .GroupBy(c => c.NameManufacturer)
                    .ToList();

                // Шаг 3: обработка каждой группы дубликатов
                foreach (var group in groupedByName)
                {
                    var toKeep = group.First(); // основная запись
                    var toRemove = group.Skip(1).ToList(); // дубликаты

                    foreach (var duplicate in toRemove)
                    {
                        // Найти связанные компоненты
                        var manufact = await dbManufact.ManufacturerComponent
                            .FirstOrDefaultAsync(m => m.GuidIdComponent == duplicate.GuidIdManufacturer, stoppingToken);

                        if (manufact != null)
                        {
                            // Проверить, есть ли уже связь с основной записью
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

                        // Удалить дублирующего производителя
                        db.SupplyManufacturer.Remove(duplicate);
                    }

                    _logger.LogInformation("Объединены и очищены дубли для: {VendorName}, удалено: {Count}", group.Key, toRemove.Count);
                }

                // Сохраняем изменения
                await dbManufact.SaveChangesAsync(stoppingToken);
                await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при очистке дублей производителей");
            }
        }
    }
}
