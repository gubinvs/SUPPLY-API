using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    /// <summary>
    /// Контроллер принимает данные о закупке (спецификации) по модели и записывает в базу данных
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SaveSupplyPurchaseController : ControllerBase
    {
        private readonly ILogger<SaveSupplyPurchaseController> _logger;
        private readonly SupplyContext _db;

        public SaveSupplyPurchaseController(
            ILogger<SaveSupplyPurchaseController> logger,
            SupplyContext db)
        {
            _logger = logger;
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> SaveSupplyPurchase([FromBody] SaveSupplyPurchaseModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // Поиск существующей закупки
                var existingPurchase = await _db.SupplyPurchase
                    .FirstOrDefaultAsync(p => p.GuidIdPurchase == model.guidIdPurchase);

                if (existingPurchase == null)
                {
                    // Создание новой закупки
                    existingPurchase = new SupplyPurchaseDb
                    {
                        GuidIdPurchase = model.guidIdPurchase,
                        PurchaseId = model.purchaseId,
                        PurchaseName = model.purchaseName,
                        PurchasePrice = model.purchasePrice,
                        PurchaseCostomer = model.purchaseCostomer
                    };

                    _db.SupplyPurchase.Add(existingPurchase);
                }
                else
                {
                    // Обновление существующей закупки
                    existingPurchase.PurchaseId = model.purchaseId;
                    existingPurchase.PurchaseName = model.purchaseName;
                    existingPurchase.PurchasePrice = model.purchasePrice;
                    existingPurchase.PurchaseCostomer = model.purchaseCostomer;

                    _db.SupplyPurchase.Update(existingPurchase);

                    // Удаление старых компонентов закупки
                    var oldItems = await _db.PurchaseComponent
                        .Where(pc => pc.GuidIdPurchase == model.guidIdPurchase)
                        .ToListAsync();

                    _db.PurchaseComponent.RemoveRange(oldItems);
                }

                // Добавление новых компонентов закупки
                var newItems = model.purchaseItem.Select(item => new PurchaseComponentDb
                {
                    GuidIdPurchase = model.guidIdPurchase,
                    GuidIdComponent = item.guidIdComponent,
                    VendorCodeComponent = item.vendorCodeComponent,
                    NameComponent = item.nameComponent,
                    RequiredQuantityItem = item.requiredQuantityItem,
                    PurchaseItemPrice = item.purchaseItemPrice,
                    BestComponentProvider = item.bestComponentProvider,
                    DeliveryTimeComponent = item.deliveryTimeComponent
                });

                await _db.PurchaseComponent.AddRangeAsync(newItems);

                await _db.SaveChangesAsync();

                // Проверка привязки в PurchaseAuthorization
                var existingAuthorization = await _db.PurchaseAuthorization
                    .FirstOrDefaultAsync(auth =>
                        auth.GuidIdCollaborator == model.guidIdCollaborator &&
                        auth.GuidIdPurchase == model.guidIdPurchase);

                if (existingAuthorization == null)
                {
                    var newAuthorization = new PurchaseAuthorizationDb
                    {
                        GuidIdCollaborator = model.guidIdCollaborator,
                        GuidIdPurchase = model.guidIdPurchase,
                    };

                    _db.PurchaseAuthorization.Add(newAuthorization);
                    await _db.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return Ok(new { message = "Закупка и её компоненты успешно сохранены." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении закупки");
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Произошла ошибка при сохранении." });
            }
        }
    }
}
