/// <summary>
/// Копированиеданных о компания поставщиках из базы данных HANDY AUTOMATION в базу данных ENCOMPONENT
/// каждый день в 01:00 скачивает таблицу SupplyProvider из базы данных HANDY и начинает сопоставлять с данными аналогичной таблицы
/// базы данных ENCOMPONENT, если такой компании в базе данных нет, то по ее ИНН запрашивает полные данные о компании на сервере DaData
/// и если их получет записывает эти данные в новую базу в таблицы SupplyProvider и SupplyCompany.
///  - Далее подключается к таблице SupplyComponent и аналогично копирует данные
///  - Далее подключется к таблице PriceComponent и переносит данные, предварительно проверив наличие компании GuidIdProvider в базе,
///     в которую переносятся данные, если есть то переносим.
///  - Далее подключемся к таблице SupplyManufacturer и переносим данные
///  - Далее проверяем перенос данных из таблицы ManufacturerComponent
///  - аналогично из таблиц SupplyUnitMeasurement и UnitMeasurementComponent
///  Все работа по переносу данных закончена, можно отправить сообщение на адрес администратора
/// </summary>
/// 


using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using System.Net.Http;
    using System.Net.Http.Json;
    using MySqlConnector;

    namespace SUPPLY_API
    {
        public class DataCopyService : IHostedService, IDisposable
        {
            private readonly ILogger<DataCopyService> _logger;
            private readonly IConfiguration _configuration;
            private readonly IServiceScopeFactory _scopeFactory;
            private System.Timers.Timer _timer = new System.Timers.Timer();

            private string _connectionHandy;
            private string _connectionEncomponent;
            private string _currenServerApi;

            public DataCopyService(
                ILogger<DataCopyService> logger,
                IConfiguration configuration,
                IServiceScopeFactory scopeFactory
            )
            {
                _logger = logger;
                _configuration = configuration;
                _scopeFactory = scopeFactory;

                _connectionEncomponent = _configuration["ConnectionStrings:AppDatabase"]
                     ?? throw new InvalidOperationException("ConnectionStrings:AppDatabase is not configured.");
                _connectionHandy = _configuration["ConnectionStrings:HandyDatabase"]
                    ?? throw new InvalidOperationException("ConnectionStrings:HandyDatabase is not configured.");
                _currenServerApi = _configuration["ServerAddresses:ServerAddressApi"]
                    ?? throw new InvalidOperationException("ServerAddressApi is not configured.");
            }

            public Task StartAsync(CancellationToken cancellationToken)
            {
                var now = DateTime.Now;
<<<<<<< HEAD
                var nextRun = new DateTime(now.Year, now.Month, now.Day, 17, 51, 0); // каждый день в 7:00
=======
                var nextRun = new DateTime(now.Year, now.Month, now.Day, 10, 03, 0); // каждый день в 7:00
>>>>>>> d482ff20bbb067d8e2f35279fe6e438fcb7a0b04
                var delay = nextRun - now;
                if (delay.TotalMilliseconds < 0)
                    delay = delay.Add(TimeSpan.FromDays(1));

                _logger.LogInformation($"DataCopyService будет запущен через {delay}");

                _timer = new System.Timers.Timer(delay.TotalMilliseconds);
                _timer.Elapsed += async (sender, args) =>
                {
                    _timer.Interval = TimeSpan.FromDays(1).TotalMilliseconds;
                    await RunDataCopyAsync();
                };
                _timer.AutoReset = false;
                _timer.Start();

                return Task.CompletedTask;
            }

            private async Task RunDataCopyAsync()
            {
                try
                {
                    _logger.LogInformation("Начало переноса данных...");

                    using var handyConn = new MySqlConnection(_connectionHandy);
                    using var encomponentConn = new MySqlConnection(_connectionEncomponent);

                    await handyConn.OpenAsync();
                    await encomponentConn.OpenAsync();

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var dbHandyDbContext = scope.ServiceProvider.GetRequiredService<HandyDbContext>();
                        var dbSupplyProvider = scope.ServiceProvider.GetRequiredService<SupplyProviderContext>();
                        var dbSupplyComponent = scope.ServiceProvider.GetRequiredService<SupplyComponentContext>();
                        var dbSupplyManufacturer = scope.ServiceProvider.GetRequiredService<SupplyManufacturerContext>();
                        var dbSupplyUnitMeasurement = scope.ServiceProvider.GetRequiredService<SupplyUnitMeasurementContext>();
                        var dbManufacturerComponent = scope.ServiceProvider.GetRequiredService<ManufacturerComponentContext>();
                        var dbUnitMeasurementComponent = scope.ServiceProvider.GetRequiredService<UnitMeasurementComponentContext>();
                        var dbSupplyPriceComponent = scope.ServiceProvider.GetRequiredService<SupplyPriceComponentContext>();

                        await CopySupplyComponentAsync(dbHandyDbContext, dbSupplyComponent);
                        await CopySupplyProviderAsync(dbHandyDbContext, dbSupplyProvider);
                        await CopySupplyManufacturerAsync(dbHandyDbContext, dbSupplyManufacturer);
                        await CopySupplyUnitMeasurementAsync(dbHandyDbContext, dbSupplyUnitMeasurement);
                        await CopyManufacturerComponentAsync(dbHandyDbContext, dbManufacturerComponent);
                        await CopyUnitMeasurementComponentAsync(dbHandyDbContext, dbUnitMeasurementComponent);
                        await CopyPriceComponentAsync(dbHandyDbContext, dbSupplyPriceComponent, dbSupplyProvider);
                    }

                    _logger.LogInformation("Перенос данных завершён успешно.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Ошибка переноса данных: {ex}");
                }
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                _logger.LogInformation("Остановка сервиса переноса данных");
                _timer?.Stop();
                return Task.CompletedTask;
            }

            public void Dispose()
            {
                _timer?.Dispose();
            }

            // ----- Методы копирования данных -----

            private async Task CopySupplyProviderAsync(HandyDbContext dbHandy, SupplyProviderContext dbTarget)
            {
                var providers = await dbHandy.SupplyProvider.ToListAsync();
                foreach (var provider in providers)
                {
                    bool exists = await dbTarget.SupplyProvider.AnyAsync(c => c.InnProvider == provider.InnProvider);
                    if (!exists)
                    {
                        await AddProviderViaApiAsync(provider.GuidIdProvider, provider.NameProvider, Convert.ToInt64(provider.InnProvider));
                    }
                }
            }

            private async Task CopySupplyComponentAsync(HandyDbContext dbHandy, SupplyComponentContext dbTarget)
            {
                var components = await dbHandy.SupplyComponent.ToListAsync();
                var existingGuids = await dbTarget.SupplyComponent.Select(c => c.GuidIdComponent).ToListAsync();
                var newComponents = components.Where(c => !existingGuids.Contains(c.GuidIdComponent)).ToList();

                if (newComponents.Any())
                {
                    // Обнуляем Id для автоинкремента
                    newComponents.ForEach(c => c.Id = 0);
                    await dbTarget.SupplyComponent.AddRangeAsync(newComponents);
                    await dbTarget.SaveChangesAsync();
                }
            }

            private async Task CopySupplyManufacturerAsync(HandyDbContext dbHandy, SupplyManufacturerContext dbTarget)
            {
                var manufacturers = await dbHandy.SupplyManufacturer.ToListAsync();
                var existingGuids = await dbTarget.SupplyManufacturer.Select(c => c.GuidIdManufacturer).ToListAsync();
                var newManufacturers = manufacturers.Where(c => !existingGuids.Contains(c.GuidIdManufacturer)).ToList();

                if (newManufacturers.Any())
                {
                    newManufacturers.ForEach(c => c.Id = 0);
                    await dbTarget.SupplyManufacturer.AddRangeAsync(newManufacturers);
                    await dbTarget.SaveChangesAsync();
                }
            }

            private async Task CopySupplyUnitMeasurementAsync(HandyDbContext dbHandy, SupplyUnitMeasurementContext dbTarget)
            {
                var units = await dbHandy.SupplyUnitMeasurement.ToListAsync();
                var existingGuids = await dbTarget.SupplyUnitMeasurement.Select(c => c.GuidIdUnitMeasurement).ToListAsync();
                var newUnits = units.Where(c => !existingGuids.Contains(c.GuidIdUnitMeasurement)).ToList();

                if (newUnits.Any())
                {
                    newUnits.ForEach(c => c.Id = 0);
                    await dbTarget.SupplyUnitMeasurement.AddRangeAsync(newUnits);
                    await dbTarget.SaveChangesAsync();
                }
            }

            private async Task CopyManufacturerComponentAsync(HandyDbContext dbHandy, ManufacturerComponentContext dbTarget)
            {
                var items = await dbHandy.ManufacturerComponent.ToListAsync();
                var existingGuids = await dbTarget.ManufacturerComponent.Select(c => c.GuidIdComponent).ToListAsync();
                var newItems = items.Where(c => !existingGuids.Contains(c.GuidIdComponent)).ToList();

                if (newItems.Any())
                {
                    newItems.ForEach(c => c.Id = 0);
                    await dbTarget.ManufacturerComponent.AddRangeAsync(newItems);
                    await dbTarget.SaveChangesAsync();
                }
            }

            private async Task CopyUnitMeasurementComponentAsync(HandyDbContext dbHandy, UnitMeasurementComponentContext dbTarget)
            {
                var items = await dbHandy.UnitMeasurementComponent.ToListAsync();
                var existingGuids = await dbTarget.UnitMeasurementComponent.Select(c => c.GuidIdComponent).ToListAsync();
                var newItems = items.Where(c => !existingGuids.Contains(c.GuidIdComponent)).ToList();

                if (newItems.Any())
                {
                    newItems.ForEach(c => c.Id = 0);
                    await dbTarget.UnitMeasurementComponent.AddRangeAsync(newItems);
                    await dbTarget.SaveChangesAsync();
                }
            }

            private async Task CopyPriceComponentAsync(HandyDbContext dbHandy, SupplyPriceComponentContext dbTarget, SupplyProviderContext dbProvider)
            {
                var offers = await dbHandy.PriceComponent.ToListAsync();
                var existingProviderGuids = await dbProvider.SupplyProvider.Select(p => p.GuidIdProvider).ToListAsync();
                var existingGuids = await dbTarget.PriceComponent.Select(c => c.GuidIdComponent).ToListAsync();

                var newOffers = offers
                    .Where(c => !existingGuids.Contains(c.GuidIdComponent) &&
                                existingProviderGuids.Contains(c.GuidIdProvider))
                    .ToList();

                if (newOffers.Any())
                {
                    newOffers.ForEach(c => c.Id = 0);
                    await dbTarget.PriceComponent.AddRangeAsync(newOffers);
                    await dbTarget.SaveChangesAsync();
                }
            }

            private async Task AddProviderViaApiAsync(string? guidIdProvider, string? nameProvider, long? innProvider)
            {
                var client = new HttpClient();
                var provider = new
                {
                    GuidIdProvider = guidIdProvider,
                    NameProvider = nameProvider,
                    InnProvider = innProvider,
                };

                var response = await client.PostAsJsonAsync(_currenServerApi + "/api/CopyCompanyProvider", provider);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Ошибка при добавлении поставщика через API: {response.StatusCode}");
                }
            }
        }
    }
