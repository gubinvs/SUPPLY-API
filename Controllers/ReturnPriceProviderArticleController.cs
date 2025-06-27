using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API.Controllers
{
    /// <summary>
    /// Контроллер принимает артикул компонента,
    /// - проверяет наличие в базе данных соответствующей записи, если ее нет отправляет ответ: такого компонента нет,
    /// если запись существует:
    /// берет из базы данных "guidIdComponent" соответствующих данному артикулу и собирает всю информацию по этому компоненту.
    /// - В таблицах о стоимости и сроках поставках данного "guidIdComponent", будет несколько записей с аналогичными "guidIdComponent",
    /// так как несколько поставщиков занесли данные о своей цене и сроке поставки.
    /// Данный контроллер берет всю информацию о всех записях и возврящает в формате json
    /// </summary>

    [ApiController]
    [Route("api/[controller]")]
    public class ReturnPriceProviderArticleController : ControllerBase
    {
        private readonly ILogger<AddComponentController> _logger;
        private readonly SupplyComponentContext _db;
        private readonly SupplyPriceComponentContext _dbPrice;
        private readonly SupplyProviderContext _dbProvider;
        private readonly ManufacturerComponentContext _dbManufact;
        private readonly UnitMeasurementComponentContext _dbUnit;
        private readonly SupplyManufacturerContext _dbSupplyManufact;
        private readonly SupplyUnitMeasurementContext _dbSupplyUnit;

        public ReturnPriceProviderArticleController(
                ILogger<AddComponentController> logger,
                SupplyComponentContext db,
                SupplyPriceComponentContext dbPrice,
                SupplyProviderContext dbProvider,
                ManufacturerComponentContext dbManufact,
                SupplyManufacturerContext dbSupplyManufact,
                UnitMeasurementComponentContext dbUnit,
                SupplyUnitMeasurementContext dbSupplyUnit
            )
        {
            _logger = logger;
            _db = db;
            _dbPrice = dbPrice;
            _dbProvider = dbProvider;
            _dbManufact = dbManufact;
            _dbSupplyManufact = dbSupplyManufact;
            _dbUnit = dbUnit;
            _dbSupplyUnit = dbSupplyUnit;
        }


        [HttpGet("{article}")]
        public async Task<IActionResult> ReadComponent(string article)
        {
            try
            {
                var component = await _db.SupplyComponent
                    .Where(c => c.VendorCodeComponent == article)
                    .Select(c => new 
                    {
                        c.GuidIdComponent,
                        c.NameComponent
                    })
                    .FirstOrDefaultAsync();

                if (component == null)
                {
                    return NotFound(new { message = $"Компонент с артикулом {article} отсутствует в базе данных" });
                }

                var offers = await _dbPrice.PriceComponent
                    .Where(p => p.GuidIdComponent == component.GuidIdComponent)
                    .ToListAsync();

                var providerIds = offers.Select(o => o.GuidIdProvider).Distinct().ToList();

                var providers = await _dbProvider.SupplyProvider
                    .Where(pr => providerIds.Contains(pr.GuidIdProvider))
                    .ToListAsync();

                var offersWithNames = offers.Select(offer =>
                {
                    var provider = providers.FirstOrDefault(p => p.GuidIdProvider == offer.GuidIdProvider);
                    return new
                    {
                        NameProvider = provider?.NameProvider ?? "Неизвестный поставщик",
                        offer.PriceComponent,
                        offer.DeliveryTimeComponent,
                        offer.SaveDataPrice
                    };
                }).ToList();

                var manufacturerComponent = await _dbManufact.ManufacturerComponent
                    .Where(c => c.GuidIdComponent == component.GuidIdComponent)
                    .Select(c => c.GuidIdManufacturer)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(manufacturerComponent))
                    return NotFound("Производитель не найден.");

                var manufacturerName = await _dbSupplyManufact.SupplyManufacturer
                    .Where(sm => sm.GuidIdManufacturer == manufacturerComponent)
                    .Select(sm => sm.NameManufacturer)
                    .FirstOrDefaultAsync();

                var unitComponent = await _dbUnit.UnitMeasurementComponent
                    .Where(c => c.GuidIdComponent == component.GuidIdComponent)
                    .Select(c => c.GuidIdUnitMeasurement)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(unitComponent))
                    return NotFound("Единица измерения не найдена.");

                var unitName = await _dbSupplyUnit.SupplyUnitMeasurement
                    .Where(su => su.GuidIdUnitMeasurement == unitComponent)
                    .Select(su => su.NameUnitMeasurement)
                    .FirstOrDefaultAsync();

                return Ok(new
                {
                    Article = article,
                    NameComponent = component.NameComponent,
                    Offers = offersWithNames,
                    Manufacturer = manufacturerName,
                    Unit = unitName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке запроса: {article}", article);
                return StatusCode(500, new { message = "Произошла ошибка при обработке запроса." });
            }
        }

    }

}