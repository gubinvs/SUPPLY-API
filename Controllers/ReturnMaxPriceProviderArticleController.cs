

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    /// <summary>
    /// Контроллер делает все тоже самое, что и ReturnPriceProviderArticleController, 
    /// но дополнительно выбирает максимальное предложение по цене и возвращает массив с одной максимальной ценой поставщика
    /// </summary>
    /// 
    /// 
    
    [ApiController]
    [Route("api/[controller]")]
    public class ReturnMaxPriceProviderArticleController : ControllerBase
    {
        private readonly ILogger<ReturnMaxPriceProviderArticleController> _logger;
        private readonly SupplyComponentContext _db;
        private readonly SupplyPriceComponentContext _dbPrice;
        private readonly SupplyProviderContext _dbProvider;
        private readonly ManufacturerComponentContext _dbManufact;
        private readonly UnitMeasurementComponentContext _dbUnit;
        private readonly SupplyManufacturerContext _dbSupplyManufact;
        private readonly SupplyUnitMeasurementContext _dbSupplyUnit;

        public ReturnMaxPriceProviderArticleController(
                ILogger<ReturnMaxPriceProviderArticleController> logger,
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
                // Загрузили данные о номенклатуре согласно запрашиваемому артикулу
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

                // Загрузили все предложения имеющиеся по данной номенклатуре
                var offers = await _dbPrice.PriceComponent
                    .Where(p => p.GuidIdComponent == component.GuidIdComponent)
                    .ToListAsync();

                var providerIds = offers.Select(o => o.GuidIdProvider).Distinct().ToList();

                var providers = await _dbProvider.SupplyProvider
                    .Where(pr => providerIds.Contains(pr.GuidIdProvider))
                    .ToListAsync();

                // Отфильтровали по максимальной цене
                int? maxPrice = offers.Max(o => o.PriceComponent);

                var offerWithNames = offers
                    .Where(o => o.PriceComponent == maxPrice)
                    .Select(offer =>
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


                // Загрузили данные о производителе
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
                    Offers = offerWithNames,
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