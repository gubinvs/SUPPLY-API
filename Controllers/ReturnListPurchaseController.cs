using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    /// <summary>
    /// Контроллер возвращает на запрос данные о всех закупках, доступных данному пользователю
    /// </summary>
    [ApiController]
    [Route("api/[controller]/{guidIdCollaborator}")]
    public class ReturnListPurchaseController : ControllerBase
    {
        private readonly ILogger<ReturnListPurchaseController> _logger;

        // База данных с информацией о поставщиках
        private readonly SupplyContext _db;

        public ReturnListPurchaseController(
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

            // Запоминаем GuidIdPurchase - идентификаторы закупок
            var purchaseGuids = myPurchases.Select(p => p.GuidIdPurchase).ToList();

            // 2. Достаём закупки на основе GuidIdPurchase
            var purchases = await _db.SupplyPurchase
                .Where(p => purchaseGuids.Contains(p.GuidIdPurchase))
                .ToListAsync();

            // 3. Компоненты в этих закупках (номенклатура, входящая в состав закупки)
            var components = await _db.PurchaseComponent
                .Where(c => purchaseGuids.Contains(c.GuidIdPurchase))
                .ToListAsync();

            // 4. Все предложения по номенклатуре на основе GuidIdComponent
            var componentGuids = components.Select(c => c.GuidIdComponent).Distinct().ToList();
            var offers = await _db.PriceComponent
                .Where(o => componentGuids.Contains(o.GuidIdComponent))
                .ToListAsync();

            // 5. Поставщики по предложениям
            var providerGuids = offers
                .Where(o => !string.IsNullOrEmpty(o.GuidIdProvider))
                .Select(o => o.GuidIdProvider!)
                .Distinct()
                .ToList();

            var providers = await _db.SupplyProvider
                .Where(p => p.GuidIdProvider != null && providerGuids.Contains(p.GuidIdProvider))
                .ToDictionaryAsync(p => p.GuidIdProvider!, p => p.NameProvider);

            // 6. Сборка результата
            var result = purchases.Select(p => new
            {
                guidIdPurchase = p.GuidIdPurchase,
                purchaseId = p.PurchaseId,
                purchaseName = p.PurchaseName,
                purchasePrice = p.PurchasePrice,
                purchaseCostomer = p.PurchaseCostomer,
                purchaseItem = components
                    .Where(c => c.GuidIdPurchase == p.GuidIdPurchase)
                    .Select(c =>
                    {
                        // Предложения по данной номенклатуре
                        var offersForComponent = offers
                            .Where(o => o.GuidIdComponent == c.GuidIdComponent)
                            .ToList();

                        return new
                        {
                            guidIdPurchase = c.GuidIdPurchase,
                            guidIdComponent = c.GuidIdComponent,
                            vendorCodeComponent = c.VendorCodeComponent,
                            nameComponent = c.NameComponent,
                            requiredQuantityItem = c.RequiredQuantityItem,

                            // Используем сохранённую в компоненте информацию, а не выбираем "лучшее"
                            purchaseItemPrice = c.PurchaseItemPrice ?? 0,
                            bestComponentProvider = c.BestComponentProvider ?? string.Empty,
                            deliveryTimeComponent = c.DeliveryTimeComponent ?? string.Empty,

                            // Все остальные предложения
                            otherOffers = offersForComponent.Select(o => new
                            {
                                guidIdComponent = o.GuidIdComponent,
                                purchaseItemPrice = o.PriceComponent,
                                bestComponentProvider = (o.GuidIdProvider != null && providers.ContainsKey(o.GuidIdProvider))
                                    ? providers[o.GuidIdProvider]
                                    : string.Empty,
                                deliveryTimeComponent = o.DeliveryTimeComponent
                            }).ToList()
                        };
                    })
                    .ToList()
            }).ToList();

            return Ok(result);
        }
    }
}
