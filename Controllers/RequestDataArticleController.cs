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
    public class RequestDataArticleController : ControllerBase
    {
        private readonly ILogger<AddComponentController> _logger;

        // База данных с информацией о комплектующих
        private readonly SupplyComponentContext _db;

        // База данных с ценами и сроками
        private readonly SupplyPriceComponentContext _dbPrice;

        // База данных с информацией о поставщиках
        private readonly SupplyProviderContext _dbProvider;

        public RequestDataArticleController(
            ILogger<AddComponentController> logger,
            SupplyComponentContext db,
            SupplyPriceComponentContext dbPrice,
            SupplyProviderContext dbProvider

            )
        {
            _logger = logger;
            _db = db;
            _dbPrice = dbPrice;
            _dbProvider = dbProvider;
        }

        [HttpGet("{article}")]
        public async Task<IActionResult> ReadComponent(string article)
        {
            try
            {
                var existing = await _db.SupplyComponent
                    .AnyAsync(c => c.VendorCodeComponent == article);

                if (!existing)
                {
                    return Conflict(new { message = $"Компонент с артикулом {article} отсутствует в базе данных" });
                }

                var guidIdComponent = await _db.SupplyComponent
                    .Where(c => c.VendorCodeComponent == article)
                    .Select(c => c.GuidIdComponent)
                    .FirstOrDefaultAsync();

                var nameComponent = await _db.SupplyComponent
                    .Where(c => c.VendorCodeComponent == article)
                    .Select(c => c.NameComponent)
                    .FirstOrDefaultAsync();

                var offers = await _dbPrice.PriceComponent
                    .Where(p => p.GuidIdComponent == guidIdComponent)
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


                return Ok(new
                {
                    Article = article,
                    NameComponent = nameComponent,
                    Offers = offersWithNames
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