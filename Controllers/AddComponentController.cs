using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddComponentController : ControllerBase
    {
        private readonly ILogger<AddComponentController> _logger;
        private readonly SupplyComponentContext _db;
        private readonly SupplyPriceComponentContext _dbPrice;

        public AddComponentController(
            ILogger<AddComponentController> logger,
            SupplyComponentContext db,
            SupplyPriceComponentContext dbPrice)
        {
            _logger = logger;
            _db = db;
            _dbPrice = dbPrice;
        }

        [HttpPost]
        public async Task<IActionResult> AddComponent([FromBody] AddComponentModel model)
        {
            try
            {
                var existing = await _db.SupplyComponent
                    .FirstOrDefaultAsync(c => c.VendorCodeComponent == model.VendorCodeComponent);

                if (existing != null)
                {
                    existing.NameComponent = model.NameComponent;

                    _db.SupplyComponent.Update(existing);
                    await _db.SaveChangesAsync();

                    return Ok(new { message = "Компонент обновлён", id = existing.GuidIdComponent });
                }

                var newComponent = new ComponentDb
                {
                    GuidIdComponent = Guid.NewGuid().ToString(),
                    VendorCodeComponent = model.VendorCodeComponent,
                    NameComponent = model.NameComponent
                };

                _db.SupplyComponent.Add(newComponent);
                await _db.SaveChangesAsync();

                return Ok(new { message = "Компонент успешно добавлен", id = newComponent.GuidIdComponent });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении/обновлении компонента: {VendorCode}", model.VendorCodeComponent);
                return StatusCode(500, new { message = "Произошла ошибка при обработке запроса." });
            }
        }

        [HttpDelete("{vendorCode}")]
        public async Task<IActionResult> DeleteComponent(string vendorCode)
        {
            try
            {
                // Найти компонент по артикулу
                var component = await _db.SupplyComponent
                    .FirstOrDefaultAsync(c => c.VendorCodeComponent == vendorCode);

                if (component == null)
                {
                    return NotFound(new { message = "Компонент не найден." });
                }

                // Удалить связанные записи в таблице PriceComponent
                var relatedPrices = await _dbPrice.PriceComponent
                    .Where(p => p.GuidIdComponent == component.GuidIdComponent)
                    .ToListAsync();

                if (relatedPrices.Any())
                {
                    _dbPrice.PriceComponent.RemoveRange(relatedPrices);
                    await _dbPrice.SaveChangesAsync();
                }

                // Удалить сам компонент
                _db.SupplyComponent.Remove(component);
                await _db.SaveChangesAsync();

                return Ok(new { message = "Компонент и все связанные предложения успешно удалены." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении компонента: {VendorCode}", vendorCode);
                return StatusCode(500, new { message = "Произошла ошибка при удалении компонента." });
            }
        }
    }

    public record AddComponentModel(
        string VendorCodeComponent,
        string NameComponent
    );
}

