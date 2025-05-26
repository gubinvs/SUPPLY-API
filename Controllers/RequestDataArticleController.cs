using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API.Controllers
{
    /// <summary>
    /// Контроллер принимает артикул компонента,
    /// - проверяет наличие в базе данных соответствующей записи, если ее нет отправляет ответ: такого компонента нет,
    /// если запись существует:
    /// берет из базы данных "guidIdComponent" соответствующих данному артикулу и собирает всю информацию по этому компоненту.
    /// - В таблицах о стоимости и сроках поставках данного "guidIdComponent", будет несколько записей с аналогичными "guidIdComponent",
    /// так как несколько поставщиков занесли данные о своей цене и сроке поставки.
    /// Данный контроллер берет всю информацию о всех записях и возврящает в формате json
    /// </summary>

    [ApiController]
    [Route("api/[controller]/[article]")]
    public class RequestDataArticleController : ControllerBase
    {
                private readonly ILogger<AddComponentController> _logger;
        private readonly SupplyComponentContext _db;

        public RequestDataArticleController (ILogger<AddComponentController> logger, SupplyComponentContext db)
        {
            _logger = logger;
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> ReadComponent(string article)
        {
            try
            {
                // Проверка: существует ли компонент с таким же VendorCode
                var existing = await _db.SupplyComponent
                    .AnyAsync(c => c.VendorCodeComponent == article);

                if (existing)
                {
                    return Conflict(new { message = "Компонент с артикулом {article} отсутствует в базе данных" });
                }

                // Если существует собираем данные из разных таблиц












                
               

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке запроса: {article}", article);
                return StatusCode(500, new { message = "Произошла ошибка при обработке запроса." });
            }
        }
    }

}