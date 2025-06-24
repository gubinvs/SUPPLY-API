using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    /// <summary>
    /// Задача контроллера собрать данны о единицах измерения и передать их массивом
    /// </summary>
    /// 
    
    [ApiController]
    [Route("api/[controller]")]
    public class ReturnListUnitMeasurementController : ControllerBase
    {
        private readonly ILogger<ReturnListUnitMeasurementController> _logger;

        // База данных с информацией о поставщиках
        private readonly SupplyUnitMeasurementContext _db;

        public ReturnListUnitMeasurementController(

                ILogger<ReturnListUnitMeasurementController> logger,
                SupplyUnitMeasurementContext db
            )
        {
            _logger = logger;
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> ReturnListManufacturer()
        {
            // Достали данные о компаниях в базе
            var unitMeasurement = await _db.SupplyUnitMeasurement
                .ToListAsync();

            return Ok(new { unitMeasurement });
        }

    }
}