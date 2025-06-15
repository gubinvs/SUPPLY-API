using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SUPPLY_API.Models;

namespace SUPPLY_API.Controllers
{
    /// <summary>
    /// Контроллер возвращает данные о компоненте согласно запросу на поиск по СПИСКУ артикулов
    /// </summary>

    [ApiController]
    [Route("api/[controller]")]
    public class ReturnDataListArticleController : ControllerBase
    {
        private readonly ILogger<AddComponentController> _logger;

        // База данных с информацией о комплектующих
        private readonly SupplyComponentContext _db;

        public ReturnDataListArticleController(
            ILogger<AddComponentController> logger,
            SupplyComponentContext db
            )
        {
            _logger = logger;
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> ReturnDataListArticle([FromBody] ListArticle model)
        {
            try
            {
                if (model?.Articles is not { Count: > 0 })
                {
                    return BadRequest(new { message = "Список артикулов пуст или не указан." });
                }

                // Отфильтровали null и пустые строки
                var validArticles = model.Articles
                    .Where(a => !string.IsNullOrWhiteSpace(a))
                    .ToList();

                foreach (var article in validArticles)
                {
                    _logger.LogInformation("Обрабатываем артикул: {article}", article);
                }

                // Получаем компоненты с проверкой на null
                var components = await _db.SupplyComponent
                    .Where(c => c.VendorCodeComponent != null && validArticles.Contains(c.VendorCodeComponent))
                    .ToListAsync();

                var foundArticles = components.Select(c => c.VendorCodeComponent!).ToHashSet();
                var notFoundArticles = validArticles.Where(a => !foundArticles.Contains(a)).ToList();

                return Ok(new
                {
                    found = components,
                    notFound = notFoundArticles
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