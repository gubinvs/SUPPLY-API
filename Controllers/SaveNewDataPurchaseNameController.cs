

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    /// <summary>
    /// Контроллер принимает данные о закупке наименовании и идентификаторе закупке и ее заказчике 
    /// и если она есть в базе данных изменяет данные, если нет создает новую.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SaveNewDataPurchaseNameController : ControllerBase
    {
        private readonly ILogger<SaveNewDataPurchaseNameController> _logger;
        private readonly SupplyContext _db;

        public SaveNewDataPurchaseNameController(
            ILogger<SaveNewDataPurchaseNameController> logger,
            SupplyContext db)
        {
            _logger = logger;
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> SaveSupplyPurchase([FromBody] SaveNewDataPurchaseNameModel model)
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
                    
                }
                
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                
                return Ok(new { message = "Новые данные о закупке успешно сохранены." });
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