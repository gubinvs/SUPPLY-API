


using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    /// <summary>
    /// Контроллер возвращает на запрос данные о всех закупках доступных данному пользователю
    /// </summary>
    /// 
    [ApiController]
    [Route("api/[controller]/{guidIdCollaborator}")]
    public class ReturnListPurchaseController : ControllerBase
    {

        private readonly ILogger<ReturnListPurchaseController> _logger;

        // База данных с информацией о поставщиках
        private readonly SupplyContext _db;

        public ReturnListPurchaseController (
                ILogger<ReturnListPurchaseController> logger,
                SupplyContext db
            )
        {
            _logger = logger;
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> ReturnListPurchase(string guidIdCollaborator)
        {
            // 1. Получаем доступные закупки для пользователя
            var myPurchases = await _db.PurchaseAuthorization
                .Where(c => c.GuidIdCollaborator == guidIdCollaborator)
                .ToListAsync();

            if (myPurchases == null || !myPurchases.Any())
                return NotFound("Нет доступных закупок для данного пользователя.");

            var purchaseGuids = myPurchases.Select(p => p.GuidIdPurchase).ToList();

            // 2. Достаём закупки
            var purchases = await _db.PurchaseComponent
                .Where(p => purchaseGuids.Contains(p.GuidIdPurchase))
                .ToListAsync();

            // 3. Компоненты в этих закупках
            var components = await _db.PurchaseComponent
                .Where(c => purchaseGuids.Contains(c.GuidIdPurchase))
                .ToListAsync();

            // 4. Все предложения по компонентам
            var componentGuids = components.Select(c => c.GuidIdComponent).Distinct().ToList();

            var offers = await _db.PriceComponent
                .Where(o => componentGuids.Contains(o.GuidIdComponent))
                .ToListAsync();

            // 5. Сборка результата
            var result = purchases.Select(p => new
            {
                guidIdPurchase = p.GuidIdPurchase,
                purchaseId = p.PurchaseId,
                purchaseName = p.PurchaseName,
                purchasePrice = p.PurchasePrice,
                purchaseCostomer = p.PurchaseCustomer,
                purchaseItem = components
                    .Where(c => c.GuidIdPurchase == p.GuidIdPurchase)
                    .Select(c =>
                    {
                        // Все предложения по компоненту
                        var offersForComponent = offers
                            .Where(o => o.GuidIdComponent == c.GuidIdComponent)
                            .OrderBy(o => o.PriceComponent) // ЗАМЕНИ если поле называется иначе
                            .ToList();

                        var bestOffer = offersForComponent.FirstOrDefault(); // самое дешевое предложение

                        return new
                        {
                            guidIdPurchase = c.GuidIdPurchase,
                            guidIdComponent = c.GuidIdComponent,
                            vendorCodeComponent = c.VendorCodeComponent,
                            nameComponent = c.NameComponent,
                            requiredQuantityItem = c.RequiredQuantityItem,
                            purchaseItemPrice = bestOffer?.PriceComponent ?? 0, // минимальная цена
                            bestComponentProvider = bestOffer?.ProviderName ?? string.Empty, // поставщик из минимального предложения
                            deliveryTimeComponent = bestOffer?.DeliveryTime ?? string.Empty, // срок поставки лучшего предложения
                            otherOffers = offersForComponent
                                .Skip(1) // исключаем лучшее
                                .Select(o => new
                                {
                                    guidIdComponent = o.GuidIdComponent,
                                    purchaseItemPrice = o.Price,
                                    bestComponentProvider = o.ProviderName,
                                    deliveryTimeComponent = o.DeliveryTime
                                }).ToList()
                        };
                    })
                    .ToList()
            }).ToList();

            return Ok(result);
        }


    }
}