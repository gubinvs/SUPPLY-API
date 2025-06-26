
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    /// <summary>
    /// Добавление или редактирование наименования производителя
    /// </summary>
    /// 

    [Controller]
    [Route("api/[controller]/{name}")]
    public class AddCompanyManufacturerController : ControllerBase
    {
        private readonly ILogger<AddCompanyManufacturerController> _logger;
        private readonly SupplyManufacturerContext _dbManuf;


        public AddCompanyManufacturerController(
                ILogger<AddCompanyManufacturerController> logger,
                SupplyManufacturerContext dbManuf)
        {
            _logger = logger;
            _dbManuf = dbManuf;
        }

        [HttpPost]
        public async Task<IActionResult> AddCompanyManufacturer(string name)
        {
            if (name == null)
            {
                return BadRequest(new { message = "Некорректные данные: Наименование производителя не указано." });
            }
            
            try
            {
                var existing = await _dbManuf.SupplyManufacturer
                    .AnyAsync(c => c.NameManufacturer == name);

                if (existing)
                {
                    return Conflict(new { message = "Компания с таким наименование уже присутствует в базе." });
                }

                var newManufacturer = new SupplyManufacturerDb
                {
                    GuidIdManufacturer = Guid.NewGuid().ToString(),
                    NameManufacturer = name
                };

                

                _dbManuf.Add(newManufacturer);
                await _dbManuf.SaveChangesAsync();

                return Ok(new { message = "Производитель успешно добавлен в базу."});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении производителя {name}", name);
                return StatusCode(500, new { message = "Произошла ошибка при обработке запроса." });
            }

        }

    }
}