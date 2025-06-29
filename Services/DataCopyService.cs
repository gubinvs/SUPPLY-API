using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;





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
        private System.Timers.Timer _timer = new System.Timers.Timer();

        private string _connectionHandy;
        private string _connectionEncomponent;
        private string _currenServerApi;
        private readonly HandyDbContext _handyDbContext;
        private readonly SupplyProviderContext _dbSupplyProvider;

        public DataCopyService(
            ILogger<DataCopyService> logger,
            IConfiguration configuration,
            HandyDbContext handyDbContext,
            SupplyProviderContext dbSupplyProvider

            )
        {
            _logger = logger;
            _configuration = configuration;
            _dbSupplyProvider = dbSupplyProvider;
            _connectionHandy = _configuration["ConnectionStringsHandy:DefaultConnection"]
                ?? throw new InvalidOperationException("ConnectionStringsHandy:DefaultConnection is not configured.");

            _connectionEncomponent = _configuration["ConnectionStrings:DefaultConnection"]
                ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

            _currenServerApi = _currenServerApi = _configuration["ServerAddresses:ServerAddressApi"]
                  ?? throw new InvalidOperationException("ServerAddressApi is not configured.");
            _handyDbContext = handyDbContext;
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

                await CopySupplyProviderAsync();
                // Далее аналогично

                // await CopySupplyComponentAsync(handyConn, encomponentConn);
                // await CopyPriceComponentAsync(handyConn, encomponentConn);
                // await CopySupplyManufacturerAsync(handyConn, encomponentConn);
                // await CopyManufacturerComponentAsync(handyConn, encomponentConn);
                // await CopySupplyUnitMeasurementAsync(handyConn, encomponentConn);
                // await CopyUnitMeasurementComponentAsync(handyConn, encomponentConn);

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

        // Пример копирования провайдеров
        private async Task CopySupplyProviderAsync()
        {
            var providers = await _handyDbContext.SupplyProvider.ToListAsync();

            foreach (var provider in providers)
            {
                string? guidIdProvider = provider.GuidIdProvider;
                string? nameProvider = provider.NameProvider;
                string? innProvider =  provider.InnProvider;
                
                

                using var targetConn = new MySqlConnection(_connectionEncomponent);
                await targetConn.OpenAsync();

                // Проверка на наличие в базе, в которую компируются данные, совпадений по ИНН
                var checkCmd = _dbSupplyProvider.SupplyProvider
                    .AnyAsync(c => c.InnProvider == innProvider);
                // Если такой записи нет, то отправляем запрос на текущий API сервер с целью записи данных
                if (checkCmd == null)
                {
                    await AddProviderViaApiAsync(guidIdProvider, nameProvider, Convert.ToInt64(innProvider));
                }
            }
        }


        // TODO: реализовать остальные методы аналогично
        // private async Task CopySupplyComponentAsync(MySqlConnection source, MySqlConnection target) { /* ... */ }
        // private async Task CopyPriceComponentAsync(MySqlConnection source, MySqlConnection target) { /* ... */ }
        // private async Task CopySupplyManufacturerAsync(MySqlConnection source, MySqlConnection target) { /* ... */ }
        // private async Task CopyManufacturerComponentAsync(MySqlConnection source, MySqlConnection target) { /* ... */ }
        // private async Task CopySupplyUnitMeasurementAsync(MySqlConnection source, MySqlConnection target) { /* ... */ }
        // private async Task CopyUnitMeasurementComponentAsync(MySqlConnection source, MySqlConnection target) { /* ... */ }


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
