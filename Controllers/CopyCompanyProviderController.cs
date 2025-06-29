using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SUPPLY_API;

namespace SUPPLY_API
{
    [ApiController]
    [Route("api/[controller]")]
    public class CopyCompanyProviderController : ControllerBase
    {
        private readonly ILogger<AddCompanyProviderController> _logger;
        private readonly SupplyProviderContext _dbProvider;
        private readonly SupplyCompanyContext _dbCompany;
        private readonly string _token;
        private readonly DaDataService _daDataService;


        public CopyCompanyProviderController(
            ILogger<AddCompanyProviderController> logger,
            SupplyProviderContext dbProvider,
            SupplyCompanyContext dbCompany,
            IOptions<RuTokenSettings> tokenOptions,
            DaDataService daDataService)
        {
            _logger = logger;
            _dbProvider = dbProvider ?? throw new ArgumentNullException(nameof(dbProvider));
            _dbCompany = dbCompany ?? throw new ArgumentNullException(nameof(dbCompany));
            _token = tokenOptions.Value.Token;
            _daDataService = daDataService;
        }

        [HttpPost]
        public async Task<IActionResult> CopySupplyProvider([FromBody] CopyProviderModel model)
        {
            if (model == null || model.innProvider <= 0)
            {
                return BadRequest(new { message = "Некорректные данные: ИНН обязателен и должен быть положительным числом." });
            }

            try
            {
                string inn = model.innProvider.ToString(); // используем строку для запроса в DaData

                var existing = await _dbProvider.SupplyProvider
                    .AnyAsync(c => c.InnProvider == inn);

                if (existing)
                {
                    return Conflict(new { message = "Компания с таким ИНН уже существует." });
                }

                var newProvider = new ProviderDb
                {
                    GuidIdProvider = model.guidIdProvider,
                    NameProvider = model.nameProvider,
                    InnProvider = inn
                };

                // Получение данных из DaData
                var party = await _daDataService.FindPartyAsync(inn);

                // Если данные удалось получить, то
                if (party != null)
                {
                    // Создаем экземпляр класса и записываем компанию в базу данных
                    var newCompany = new SupplyCompanyDb
                    {
                        GuidIdCompany = newProvider.GuidIdProvider,
                        FullNameCompany = party?.name?.full_with_opf ?? "_",
                        AbbreviatedNameCompany = party?.name?.short_with_opf ?? "_",
                        InnCompany = Convert.ToInt64(newProvider.InnProvider), // конвертируем в long
                        AddressCompany = party?.address?.value ?? "_"
                    };

                    _dbCompany.Add(newCompany);
                    await _dbCompany.SaveChangesAsync();

                    // итолько после этого записываем провайдера (поставщика)
                    _dbProvider.Add(newProvider);
                    await _dbProvider.SaveChangesAsync();
                }
                else
                {
                    _logger.LogWarning("DaData не вернула данных для ИНН {Inn}", inn);
                }

                return Ok(new { message = "Поставщик успешно добавлен", id = newProvider.GuidIdProvider });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении поставщика с ИНН {InnProvider}", model.innProvider);
                return StatusCode(500, new { message = "Произошла ошибка при обработке запроса." });
            }
        }
    }

    public record CopyProviderModel (
            string guidIdProvider,
            string nameProvider,
            long innProvider
        );
}