using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API.Controllers
{
    /// <summary>
    /// Контроллер для получения списка поставщиков, связанных с конкретным производителем.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ReturnListProviderManufacturerController : ControllerBase
    {
        private readonly ILogger<ReturnListProviderManufacturerController> _logger;
        private readonly SupplyContext _db;

        public ReturnListProviderManufacturerController(
            ILogger<ReturnListProviderManufacturerController> logger,
            SupplyContext db)
        {
            _logger = logger;
            _db = db;
        }

        /// <summary>
        /// Возвращает список компаний-поставщиков для номенклатуры указанного производителя.
        /// </summary>
        /// <param name="manufacturerId">GUID идентификатор производителя</param>
        /// 
        
       [HttpPost]
        public async Task<IActionResult> ListProviderManufacturer([FromBody] string manufacturerId) // <-- Изменено на [FromBody]
        {
            try
            {
                // Запрос объединяет таблицу связей и таблицу поставщиков, выбирая только данные поставщиков
                var providers = await _db.ProviderManufacturer
                    .Where(pm => pm.GuidIdManufacturer == manufacturerId)
                    .Join(
                        _db.SupplyProvider,
                        pm => pm.GuidIdProvider,    // Ключ из таблицы связей
                        sp => sp.GuidIdProvider,    // Ключ из таблицы поставщиков
                        (pm, sp) => sp              // Выбираем объект поставщика
                    )
                    .AsNoTracking()                 // Отключаем трекинг для ускорения Read-Only запроса
                    .ToListAsync();

                if (providers == null || !providers.Any())
                {
                    return NotFound($"Поставщики для производителя с ID {manufacturerId} не найдены.");
                }

                return Ok(providers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка поставщиков для производителя {ManufacturerId}", manufacturerId);
                return StatusCode(500, "Внутренняя ошибка сервера при обработке запроса.");
            }
        }
    }
}