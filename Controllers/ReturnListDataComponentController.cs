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

        // База данных с производителями
        private readonly ManufacturerComponentContext _dbManufact;

        // База данных с единицами измерения
        private readonly UnitMeasurementComponentContext _dbUm;

        public ReturnListDataComponentController(
            ILogger<AddComponentController> logger,
            SupplyComponentContext db,
            UnitMeasurementComponentContext dbUm,
            ManufacturerComponentContext dbManufact
        )
        {
            _logger = logger;
            _db = db;
            _dbUm = dbUm;
            _dbManufact = dbManufact;
        }

        [HttpGet]
        public async Task<IActionResult> ReturnListDataComponent()
        {
            try
            {
                // Cобираем все данные из таблицы
                var component = await _db.SupplyComponent
                    .ToListAsync();

                // Собираем данные о привязках guidId производителя
                var manufacturer = await _dbManufact.ManufacturerComponent
                    .ToListAsync();

                // Собираем данные о привязках guidId единиц измерений
                var measurement = await _dbUm.UnitMeasurementComponent
                    .ToListAsync();

                // Подключаемся к базе данных и сопоставляем guidId и наименование производителя


                // Подключаемся к базе данных и сопоставляем guidId единицы измерения и наименования ед. измерения




                // Теперь перебираем данные и при совпадении GuidIdComponent дописываем данные в массив про производителя
                // Ну и про единицу измерения






                return Ok(new { component });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке запроса:");
                return StatusCode(500, new { message = "Произошла ошибка при обработке запроса." });
            }
        }
    }

}