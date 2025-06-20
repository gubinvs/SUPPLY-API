using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SUPPLY_API.Models;

namespace SUPPLY_API.Controllers
{
    /// <summary>
    /// Контроллер принимает СПИСОК артикулов,
    /// проверяет наличие каждого артикула в базе,
    /// для найденных получает guidIdComponent и возвращает всю информацию о предложениях поставщиков.
    /// Если артикула нет в базе — возвращает информацию о ненайденных артикулах.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ReturnPriceProviderListArticleController : ControllerBase
    {
        private readonly ILogger<ReturnPriceProviderListArticleController> _logger;
        private readonly SupplyComponentContext _db;
        private readonly SupplyPriceComponentContext _dbPrice;
        private readonly SupplyProviderContext _dbProvider;

        public ReturnPriceProviderListArticleController(
            ILogger<ReturnPriceProviderListArticleController> logger,
            SupplyComponentContext db,
            SupplyPriceComponentContext dbPrice,
            SupplyProviderContext dbProvider)
        {
            _logger = logger;
            _db = db;
            _dbPrice = dbPrice;
            _dbProvider = dbProvider;
        }

        [HttpPost]
        public async Task<IActionResult> ReadComponents([FromBody] ListArticle model)
        {
            if (model?.Articles == null || model.Articles.Count == 0)
                return BadRequest(new { message = "Список артикулов пуст или не указан." });

            try
            {
                // Фильтруем корректные артикулы
                var validArticles = model.Articles
                    .Where(a => !string.IsNullOrWhiteSpace(a))
                    .Distinct()
                    .ToList();

                // Получаем компоненты, которые есть в базе
                var components = await _db.SupplyComponent
                    .Where(c => c.VendorCodeComponent != null && validArticles.Contains(c.VendorCodeComponent))
                    .ToListAsync();

                // Артикулы, которых нет в базе
                var foundArticles = components.Select(c => c.VendorCodeComponent!).ToHashSet();
                var notFoundArticles = validArticles.Where(a => !foundArticles.Contains(a)).ToList();

                // Для каждого найденного компонента получаем предложения
                var result = new List<object>();

                foreach (var component in components)
                {
                    var offers = await _dbPrice.PriceComponent
                        .Where(p => p.GuidIdComponent == component.GuidIdComponent)
                        .ToListAsync();

                    var providerIds = offers.Select(o => o.GuidIdProvider).Distinct().ToList();

                    var providers = await _dbProvider.SupplyProvider
                        .Where(pr => providerIds.Contains(pr.GuidIdProvider))
                        .ToListAsync();

                    var offersWithProviders = offers.Select(offer =>
                    {
                        var provider = providers.FirstOrDefault(p => p.GuidIdProvider == offer.GuidIdProvider);
                        return new
                        {
                            NameProvider = provider?.NameProvider ?? "Неизвестный поставщик",
                            innProvider = provider?.InnProvider ?? "",
                            offer.PriceComponent,
                            offer.DeliveryTimeComponent,
                            offer.SaveDataPrice
                        };
                    }).ToList();

                    result.Add(new
                    {
                        Article = component.VendorCodeComponent,
                        NameComponent = component.NameComponent,
                        Offers = offersWithProviders
                    });
                }

                return Ok(new
                {
                    Found = result,
                    NotFound = notFoundArticles
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке списка артикулов");
                return StatusCode(500, new { message = "Произошла ошибка при обработке запроса." });
            }
        }
    }
}
