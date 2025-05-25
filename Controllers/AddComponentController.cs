using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using SUPPLY_API.Models;

namespace SUPPLY_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddComponentController : ControllerBase
    {
        private readonly ILogger<AddComponentController> _logger;
        private readonly SupplyComponentContext _db;

        public AddComponentController(ILogger<AddComponentController> logger, SupplyComponentContext db)
        {
            _logger = logger;
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> AddComponent([FromBody] AddComponentModel model)
        {
            try
            {
                // Проверка: существует ли компонент с таким же VendorCode
                var existing = await _db.SupplyComponent
                    .AnyAsync(c => c.VendorCodeComponent == model.VendorCodeComponent);

                if (existing)
                {
                    return Conflict(new { message = "Компонент с таким VendorCode уже существует." });
                }

                // Создание нового компонента
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
                _logger.LogError(ex, "Ошибка при добавлении компонента: {VendorCode}", model.VendorCodeComponent);
                return StatusCode(500, new { message = "Произошла ошибка при обработке запроса." });
            }
        }
    }

    public record AddComponentModel(
        string VendorCodeComponent,
        string NameComponent
    );
}
