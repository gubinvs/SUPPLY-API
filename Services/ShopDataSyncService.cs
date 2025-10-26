using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using System.Threading.Tasks.Dataflow;



/// Основные моменты:
/// Сервис запускается каждый день в 8:00.
/// Берёт данные из GoodsTableDb через ShopContext.
/// Если NameComponent или Manufacturer пустые — подставляет дефолтные значения.
/// UnitMeasurementComponent всегда "шт".
/// Поставщик фиксирован: "Компонент энергии".
/// Срок доставки:
/// Quantity > 0 → "в наличии"
/// Quantity == 0 → "от 1 до 4 нед"(случайное число недель)
/// Через HttpClient отправляет данные в AddComponentController и ChangePriceController.

namespace SUPPLY_API.Services
{
    public class ShopDataSyncService : IHostedService, IDisposable
    {
        private readonly ILogger<ShopDataSyncService> _logger;
        private readonly IServiceProvider _services;
        private Timer? _timer;
        private readonly HttpClient _httpClient;

        // Ограничение параллелизма
        private const int MaxDegreeOfParallelism = 10;

        public ShopDataSyncService(ILogger<ShopDataSyncService> logger, IServiceProvider services)
        {
            _logger = logger;
            _services = services;
            _httpClient = new HttpClient();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Сервис синхронизации данных запущен.");

            var now = DateTime.Now;
            var firstRun = new DateTime(now.Year, now.Month, now.Day, 8, 0, 0);
            if (now > firstRun) firstRun = firstRun.AddDays(1);

            var initialDelay = firstRun - now;

            _timer = new Timer(SyncShopData, null, initialDelay, TimeSpan.FromHours(24));

            return Task.CompletedTask;
        }

        private async void SyncShopData(object? state)
        {
            _logger.LogInformation("Начало синхронизации данных с магазином: {time}", DateTime.Now);

            try
            {
                using var scope = _services.CreateScope();
                var shopDb = scope.ServiceProvider.GetRequiredService<ShopContext>();

                var goods = await shopDb.GoodsTable.ToListAsync();

                var block = new ActionBlock<GoodsTableDb>(async item =>
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(item.VendorCode)) return;

                        var componentModel = new
                        {
                            VendorCodeComponent = item.VendorCode,
                            NameComponent = item.NameComponent ?? "Без названия",
                            guidIdManufacturer = item.Manufacturer ?? "",
                            guidIdUnitMeasurement = "шт"
                        };

                        var addComponentResponse = await _httpClient.PostAsJsonAsync(
                            "https://localhost:5001/api/AddComponent", componentModel);

                        if (!addComponentResponse.IsSuccessStatusCode)
                        {
                            _logger.LogWarning("Не удалось добавить/обновить компонент {vendor}. Статус: {status}",
                                item.VendorCode, addComponentResponse.StatusCode);
                            return;
                        }

                        string deliveryTime = item.Quantity > 0 ? "в наличии" : $"{new Random().Next(1, 5)} нед";

                        var priceModel = new
                        {
                            VendorCodeComponent = item.VendorCode,
                            GuidIdProvider = "Компонент энергии",
                            PriceComponent = item.Price ?? 0,
                            DeliveryTimeComponent = deliveryTime
                        };

                        var changePriceResponse = await _httpClient.PostAsJsonAsync(
                            "https://localhost:5001/api/ChangePrice", priceModel);

                        if (!changePriceResponse.IsSuccessStatusCode)
                        {
                            _logger.LogWarning("Не удалось обновить цену для {vendor}. Статус: {status}",
                                item.VendorCode, changePriceResponse.StatusCode);
                            return;
                        }

                        _logger.LogInformation("Компонент {vendor} успешно синхронизирован.", item.VendorCode);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ошибка при обработке артикула {vendor}", item.VendorCode);
                    }
                }, new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = MaxDegreeOfParallelism
                });

                foreach (var item in goods)
                    block.Post(item);

                block.Complete();
                await block.Completion;

                _logger.LogInformation("Синхронизация завершена.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при синхронизации данных с магазином.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Сервис синхронизации данных останавливается.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _httpClient?.Dispose();
        }
    }
}