using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks.Dataflow;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SUPPLY_API.Controllers;

namespace SUPPLY_API.Services
{
    public class ShopDataSyncService : IHostedService, IDisposable
    {
        private readonly ILogger<ShopDataSyncService> _logger;
        private readonly IServiceProvider _services;
        private Timer? _timer;
        private const int MaxDegreeOfParallelism = 10;

        public ShopDataSyncService(ILogger<ShopDataSyncService> logger, IServiceProvider services)
        {
            _logger = logger;
            _services = services;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Сервис синхронизации данных запущен.");

            var now = DateTime.Now;
            var firstRun = new DateTime(now.Year, now.Month, now.Day, 19, 26, 0);
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

                // Берём товары для синхронизации
                var goods = await shopDb.GoodsTable.Take(3).ToListAsync(); // Только три строчки

                var block = new ActionBlock<GoodsTableDb>(async item =>
                {
                    if (string.IsNullOrWhiteSpace(item.VendorCode))
                        return;

                    // 🟢 создаём новый scope для каждой операции
                    using var innerScope = _services.CreateScope();

                    var componentDb = innerScope.ServiceProvider.GetRequiredService<SupplyComponentContext>();
                    var priceDb = innerScope.ServiceProvider.GetRequiredService<SupplyPriceComponentContext>();
                    var manufactDb = innerScope.ServiceProvider.GetRequiredService<ManufacturerComponentContext>();
                    var unitDb = innerScope.ServiceProvider.GetRequiredService<UnitMeasurementComponentContext>();
                    var providerDb = innerScope.ServiceProvider.GetRequiredService<SupplyProviderContext>();
                    var loggerAdd = innerScope.ServiceProvider.GetRequiredService<ILogger<AddComponentController>>();
                    var loggerPrice = innerScope.ServiceProvider.GetRequiredService<ILogger<ChangePriceController>>();

                    try
                    {
                        // Добавляем или обновляем компонент
                        var addController = new AddComponentController(
                            loggerAdd, componentDb, priceDb, manufactDb, unitDb
                        );

                        var addModel = new AddComponentModel(
                            VendorCodeComponent: item.VendorCode,
                            NameComponent: item.NameComponent ?? "Без названия",
                            guidIdManufacturer: item.Manufacturer ?? "",
                            guidIdUnitMeasurement: "f1f3d9f5-1085-40fe-82ec-c693ac60664b"
                        );

                        await addController.AddComponent(addModel);

                        // Срок поставки
                        string deliveryTime = item.Quantity > 0 ? "в наличии" : "от 1 до 4 нед";

                        // Цена
                        int finalPrice = (int)(item.Price ?? 0);

                        // Обновляем цену
                        var changeController = new ChangePriceController(
                            loggerPrice, componentDb, priceDb, providerDb
                        );

                        var priceModel = new ChangePriceModel(
                            VendorCodeComponent: item.VendorCode,
                            GuidIdProvider: "48fcbc8d-3b82-42b9-8dfe-41f1c03c44ec",
                            PriceComponent: finalPrice,
                            DeliveryTimeComponent: deliveryTime
                        );

                        await changeController.ReadComponent(priceModel);

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

                _logger.LogInformation("Синхронизация завершена ({count} товаров).", goods.Count);
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
        }
    }
}
