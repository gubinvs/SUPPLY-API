using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace SUPPLY_API
{
    /// <summary>
    /// Задача контроллера собрать данные о производителях записанных в базе данных и вернуть массив данных
    /// </summary>
    /// 

    [ApiController]
    [Route("api/[controller]")]
    public class ReturnListManufacturerController : ControllerBase
    {
        
        private readonly ILogger<ReturnListManufacturerController> _logger;

        // База данных с информацией о поставщиках
        private readonly SupplyManufacturerContext _db;

        public ReturnListManufacturerController(

                ILogger<ReturnListManufacturerController> logger,
                SupplyManufacturerContext db
            )
        {
            _logger = logger;
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> ReturnListManufacturer()
        {
            // Достали данные о компаниях в базе
            var manufacturer = await _db.SupplyManufacturer
                .ToListAsync();

            return Ok(new {manufacturer});
        }

    }
}