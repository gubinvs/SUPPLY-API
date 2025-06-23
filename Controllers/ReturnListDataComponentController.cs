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

        // База данных с зависимостями по производителям
        private readonly ManufacturerComponentContext _dbManufact;

        // База данных с зависимостями по единицам измерения
        private readonly UnitMeasurementComponentContext _dbUm;

        // База данных с наименованиями производителей
        private readonly SupplyManufacturerContext _dbManufactName;

        // База данных с наименованиями единиц измерения
        private readonly SupplyUnitMeasurementContext _dbUmName;

        public ReturnListDataComponentController(
            ILogger<AddComponentController> logger,
            SupplyComponentContext db,
            UnitMeasurementComponentContext dbUm,
            ManufacturerComponentContext dbManufact,
            SupplyUnitMeasurementContext dbUmName,
            SupplyManufacturerContext dbManufactName

        )
        {
            _logger = logger;
            _db = db;
            _dbUm = dbUm;
            _dbManufact = dbManufact;
            _dbUmName = dbUmName;
            _dbManufactName = dbManufactName;

        }

        [HttpGet]
        public async Task<IActionResult> ReturnListDataComponent()
        {
            try
            {
                var components = await _db.SupplyComponent.ToListAsync();
                var manufacturerLinks = await _dbManufact.ManufacturerComponent.ToListAsync();
                var unitMeasurementLinks = await _dbUm.UnitMeasurementComponent.ToListAsync();
                var manufacturerNames = await _dbManufactName.SupplyManufacturer.ToListAsync();
                var unitMeasurementNames = await _dbUmName.SupplyUnitMeasurement.ToListAsync();

                var enrichedComponents = components.Select(c =>
                {
                    // Найдём привязку производителя по компоненту
                    var manufactLink = manufacturerLinks.FirstOrDefault(m => m.GuidIdComponent == c.GuidIdComponent);
                    var manufactName = manufactLink != null
                        ? manufacturerNames.FirstOrDefault(mn => mn.GuidIdManufacturer == manufactLink.GuidIdManufacturer)?.NameManufacturer
                        : null;

                    // Найдём привязку единицы измерения по компоненту
                    var umLink = unitMeasurementLinks.FirstOrDefault(u => u.GuidIdComponent == c.GuidIdComponent);
                    var umName = umLink != null
                        ? unitMeasurementNames.FirstOrDefault(un => un.GuidIdUnitMeasurement == umLink.GuidIdUnitMeasurement)?.NameUnitMeasurement
                        : null;

                    return new
                    {
                        c.Id,
                        c.GuidIdComponent,
                        c.VendorCodeComponent,
                        c.NameComponent,
                        ManufacturerName = manufactName,
                        UnitMeasurementName = umName
                    };
                }).ToList();

                return Ok(enrichedComponents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке запроса:");
                return StatusCode(500, new { message = "Произошла ошибка при обработке запроса." });
            }
        }
    }

}