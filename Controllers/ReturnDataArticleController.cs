using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API.Controllers
{
    /// <summary>
    /// Контроллер возвращает данные о компоненте согласно запросу на поиск по артикулу
    /// </summary>

    [ApiController]
    [Route("api/[controller]")]
    public class ReturnDataArticleController : ControllerBase
    {
        private readonly ILogger<AddComponentController> _logger;

        // База данных с информацией о комплектующих
        private readonly SupplyComponentContext _db;

        public ReturnDataArticleController(
            ILogger<AddComponentController> logger,
            SupplyComponentContext db
            )
        {
            _logger = logger;
            _db = db;
        }

        [HttpGet("{article}")]
        public async Task<IActionResult> ReturnDataArticle(string article)
        {
            try
            {
                // Проверка на наличие записи в базе данных
                var existing = await _db.SupplyComponent
                    .AnyAsync(c => c.VendorCodeComponent == article);

                if (!existing)
                {
                    return Conflict(new { message = $"Компонент с артикулом {article} отсутствует в базе данных" });
                }

                // На основании артикула достали GuidIdComponent
                var component = await _db.SupplyComponent
                    .Where(c => c.VendorCodeComponent == article)
                    .FirstOrDefaultAsync();


                return Ok(new
                {
                    component
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