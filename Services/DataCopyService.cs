using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;  // Добавлено для IServiceScopeFactory
using System.Net.Http;
using System.Net.Http.Json;
using MySqlConnector;

namespace SUPPLY_API
{
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
    public class DataCopyService : IHostedService, IDisposable
    {
        private readonly ILogger<DataCopyService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory; // для создания scope
        private System.Timers.Timer _timer = new System.Timers.Timer();

        private string _connectionHandy;
        private string _connectionEncomponent;
        private string _currenServerApi;

        public DataCopyService(
            ILogger<DataCopyService> logger,
            IConfiguration configuration,
            IServiceScopeFactory scopeFactory  // Инжектируем scope factory
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
            // Планируем запуск каждый день в 01:00
            var now = DateTime.Now;
            var nextRun = now.Date.AddDays(now.Hour >= 1 ? 1 : 0).AddHours(1);
            var delay = nextRun - now;

            _logger.LogInformation($"DataCopyService будет запущен через {delay}");

            _timer = new System.Timers.Timer(delay.TotalMilliseconds);
            _timer.Elapsed += async (sender, args) =>
            {
                _timer.Interval = TimeSpan.FromDays(1).TotalMilliseconds; // далее раз в сутки
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

                // Создаём scope для получения scoped сервисов DbContext
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

                    // Копированее номенклатуры
                    await CopySupplyComponentAsync(dbHandyDbContext, dbSupplyComponent);
                    // Копирование данных поставщиков
                    await CopySupplyProviderAsync(dbHandyDbContext, dbSupplyProvider);
                    // Копирование данных о производителях
                    await CopySupplyManufacturerAsync(dbHandyDbContext, dbSupplyManufacturer);
                    // Процесс копирования данных о единицах измерения
                    await CopySupplyUnitMeasurementAsync(dbHandyDbContext, dbSupplyUnitMeasurement);
                    // Процесс копирования зависимостей номенклатуры и производителя
                    await CopyManufacturerComponentAsync(dbHandyDbContext, dbManufacturerComponent);
                    // Процесс копирования зависимостей о номенклатуре и ее единице измерения
                    await CopyUnitMeasurementComponentAsync(dbHandyDbContext, dbUnitMeasurementComponent);
                    // Процесс переноса данных о ценах поставщиков в целевую базу данных
                    await CopyPriceComponentAsync(dbHandyDbContext, dbSupplyPriceComponent, dbSupplyProvider);

                }

                _logger.LogInformation("Перенос данных завершён успешно.");
                // TODO: отправить email/telegram
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка переноса данных: {ex.Message}");
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

        // Метод копирования данных поставщиков (провайдеров)
        private async Task CopySupplyProviderAsync(HandyDbContext dbHandyDbContext, SupplyProviderContext dbSupplyProvider)
        {
            var providers = await dbHandyDbContext.SupplyProvider.ToListAsync();

            foreach (var provider in providers)
            {
                string? guidIdProvider = provider.GuidIdProvider;
                string? nameProvider = provider.NameProvider;
                string? innProvider = provider.InnProvider;

                // Проверка на наличие в базе, в которую копируются данные, совпадений по ИНН
                bool exists = await dbSupplyProvider.SupplyProvider.AnyAsync(c => c.InnProvider == innProvider);

                // Если такой записи нет, то отправляем запрос на текущий API сервер с целью записи данных
                if (!exists)
                {
                    await AddProviderViaApiAsync(guidIdProvider, nameProvider, Convert.ToInt64(innProvider));
                }
            }
        }

        // Метод копирования номенклатуры
        private async Task CopySupplyComponentAsync(HandyDbContext dbHandyDbContext, SupplyComponentContext dbSupplyComponent)
        {
            // existingGuids — предварительно получаем список идентификаторов, чтобы не делать по одному запросу на каждый компонент.
            // AddRangeAsync — добавляем сразу все новые записи, а не по одной.
            //SaveChangesAsync() вызывается один раз — это эффективно.

            var components = await dbHandyDbContext.SupplyComponent.ToListAsync();

            var existingGuids = await dbSupplyComponent.SupplyComponent
                .Select(c => c.GuidIdComponent)
                .ToListAsync();

            var newComponents = components
                .Where(c => !existingGuids.Contains(c.GuidIdComponent))
                .ToList();

            if (newComponents.Any())
            {
                await dbSupplyComponent.SupplyComponent.AddRangeAsync(newComponents);
                await dbSupplyComponent.SaveChangesAsync();
            }
        }

        // Копирование наименование компаний производителей
        private async Task CopySupplyManufacturerAsync(HandyDbContext dbHandyDbContext, SupplyManufacturerContext dbSupplyManufacturer)
        {
            var manufacturer = await dbHandyDbContext.SupplyManufacturer.ToListAsync();
            var existingGuids = await dbSupplyManufacturer.SupplyManufacturer
                    .Select(c => c.GuidIdManufacturer)
                    .ToListAsync();

            var newManufacturer = manufacturer
                .Where(c => !existingGuids.Contains(c.GuidIdManufacturer))
                .ToList();

            if (newManufacturer.Any())
            {
                await dbSupplyManufacturer.SupplyManufacturer.AddRangeAsync(newManufacturer);
                await dbSupplyManufacturer.SaveChangesAsync();
            }
        }

        // Копирование единиц измерения
        private async Task CopySupplyUnitMeasurementAsync(HandyDbContext dbHandyDbContext, SupplyUnitMeasurementContext dbSupplyUnitMeasurement)
        {
            var unit = await dbHandyDbContext.SupplyUnitMeasurement.ToListAsync();
            var existingGuids = await dbSupplyUnitMeasurement.SupplyUnitMeasurement
                    .Select(c => c.GuidIdUnitMeasurement)
                    .ToListAsync();

            var newUnit = unit
                .Where(c => !existingGuids.Contains(c.GuidIdUnitMeasurement))
                .ToList();

            if (newUnit.Any())
            {
                await dbSupplyUnitMeasurement.SupplyUnitMeasurement.AddRangeAsync(newUnit);
                await dbSupplyUnitMeasurement.SaveChangesAsync();
            }
        }

        // Метод копирование данных о зависимостях номенклатуры и единицы измерения
        private async Task CopyManufacturerComponentAsync(HandyDbContext dbHandyDbContext, ManufacturerComponentContext dbManufacturerComponent)
        {
            var addiction = await dbHandyDbContext.ManufacturerComponent.ToListAsync();
            var existingGuids = await dbManufacturerComponent.ManufacturerComponent
                    .Select(c => c.GuidIdComponent)
                    .ToListAsync();

            var newAddiction = addiction
                .Where(c => !existingGuids.Contains(c.GuidIdComponent))
                .ToList();

            if (newAddiction.Any())
            {
                await dbManufacturerComponent.ManufacturerComponent.AddRangeAsync(newAddiction);
                await dbManufacturerComponent.SaveChangesAsync();
            }
        }

        // Метод переноса данных о принадлежности номенклатуры и единицы измерения
        private async Task CopyUnitMeasurementComponentAsync(HandyDbContext dbHandyDbContext, UnitMeasurementComponentContext dbUnitMeasurementComponent)
        {
            var addiction = await dbHandyDbContext.UnitMeasurementComponent.ToListAsync();
            var existingGuids = await dbUnitMeasurementComponent.UnitMeasurementComponent
                    .Select(c => c.GuidIdComponent)
                    .ToListAsync();

            var newAddiction = addiction
                .Where(c => !existingGuids.Contains(c.GuidIdComponent))
                .ToList();

            if (newAddiction.Any())
            {
                await dbUnitMeasurementComponent.UnitMeasurementComponent.AddRangeAsync(newAddiction);
                await dbUnitMeasurementComponent.SaveChangesAsync();
            }
        }

        // Метод копирования базы данных о ценах поставщиков на номенклатуру
        private async Task CopyPriceComponentAsync(
            HandyDbContext dbHandyDbContext,
            SupplyPriceComponentContext dbSupplyPriceComponent,
            SupplyProviderContext dbSupplyProvider)
        {
            // Загрузили все предложения из исходной базы
            var offers = await dbHandyDbContext.PriceComponent.ToListAsync();

            // Загрузили Guid поставщиков, которые уже есть в целевой базе
            var existingProviderGuids = await dbSupplyProvider.SupplyProvider
                .Select(p => p.GuidIdProvider)
                .ToListAsync();

            // Загрузили Guid компонентов, которые уже есть в целевой базе
            var existingGuids = await dbSupplyPriceComponent.PriceComponent
                .Select(c => c.GuidIdComponent)
                .ToListAsync();

            // Оставляем только те предложения, которых ещё нет в целевой базе и у которых поставщик уже есть
            var newOffers = offers
                .Where(c => !existingGuids.Contains(c.GuidIdComponent) &&
                            existingProviderGuids.Contains(c.GuidIdProvider))
                .ToList();

            if (newOffers.Any())
            {
                await dbSupplyPriceComponent.PriceComponent.AddRangeAsync(newOffers);
                await dbSupplyPriceComponent.SaveChangesAsync();
            }
        }

        // Метод отправки запроса на добавление новой компании провайдера
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

