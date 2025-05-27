using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API.Controllers
{
    /// <summary>
    /// Контроллер возвращает данные о компоненте согласно запросу на поиск по артикулу
    /// </summary>

    [ApiController]
    [Route("api/[controller]")]
    public class ReturnListDataComponentController : ControllerBase
    {
        private readonly ILogger<AddComponentController> _logger;

        // База данных с информацией о комплектующих
        private readonly SupplyComponentContext _db;

        public ReturnListDataComponentController(
            ILogger<AddComponentController> logger,
            SupplyComponentContext db
            )
        {
            _logger = logger;
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> ReturnListDataComponent()
        {
            try
            {

                // Cобираем все данные из таблицы
                var component = await _db.SupplyComponent
                    .ToListAsync();


                return Ok(new
                {
                    component
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке запроса:");
                return StatusCode(500, new { message = "Произошла ошибка при обработке запроса." });
            }
        }
    }

}