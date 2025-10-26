using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks.Dataflow;
using System;
using System.Linq;
using System.Threading;
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
                var componentDb = scope.ServiceProvider.GetRequiredService<SupplyComponentContext>();
                var priceDb = scope.ServiceProvider.GetRequiredService<SupplyPriceComponentContext>();
                var manufactDb = scope.ServiceProvider.GetRequiredService<ManufacturerComponentContext>();
                var unitDb = scope.ServiceProvider.GetRequiredService<UnitMeasurementComponentContext>();
                var providerDb = scope.ServiceProvider.GetRequiredService<SupplyProviderContext>();

                var loggerAdd = scope.ServiceProvider.GetRequiredService<ILogger<AddComponentController>>();
                var loggerPrice = scope.ServiceProvider.GetRequiredService<ILogger<ChangePriceController>>();

                // Берём только первую запись для отладки, потом убрать Take(1)
                // var goods = await shopDb.GoodsTable.Take(1).ToListAsync();
                var goods = await shopDb.GoodsTable.ToListAsync();

                // Получаем скидку KEAZ из базы магазина
                var keazDiscount = shopDb.DiscountTable
                    .Where(d => d.Manufacturer == "KEAZ")
                    .Select(d => d.Discount ?? 1m)
                    .AsEnumerable()
                    .DefaultIfEmpty(1m)
                    .First();

                var block = new ActionBlock<GoodsTableDb>(async item =>
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(item.VendorCode)) return;

                        // Контроллер AddComponent
                        var addController = new AddComponentController(
                            loggerAdd, componentDb, priceDb, manufactDb, unitDb
                        );

                        var addModel = new AddComponentModel(
                            VendorCodeComponent: item.VendorCode,
                            NameComponent: item.NameComponent ?? "Без названия",
                            guidIdManufacturer: item.Manufacturer ?? "",
                            guidIdUnitMeasurement: "шт"
                        );

                        await addController.AddComponent(addModel);

                        // Рассчитываем срок поставки
                        string deliveryTime = item.Quantity > 0 ? "в наличии" : "от 1 до 4 нед";

                        // Применяем скидку KEAZ
                        int finalPrice = (int)Math.Round((item.Price ?? 0) * keazDiscount);

                        // Контроллер ChangePrice
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
        }
    }
}
