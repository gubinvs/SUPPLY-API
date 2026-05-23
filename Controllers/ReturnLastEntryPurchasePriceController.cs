using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    [Controller]
    [Route("api/[controller]")]
    public class ReturnLastEntryPurchasePriceController : ControllerBase
    {
        private readonly ILogger<ReturnLastEntryPurchasePriceController> _logger;
        private readonly SupplyContext _db;

        public ReturnLastEntryPurchasePriceController
        (
            ILogger<ReturnLastEntryPurchasePriceController> logger,
            SupplyContext db
        )
        {
            _logger = logger;
            _db = db;
        }

        /// Получить последнюю запись по артикулу
        [HttpGet("last")]
        public async Task<IActionResult> ReturnDataLastEntryPurchasePrice(string vendorCode)
        {
            var data = await _db.PurchasePrice
                .Where(c => c.Article == vendorCode)
                .OrderByDescending(c => c.SaveDataPrice)
                .FirstOrDefaultAsync();

            return Ok(data);
        }

        /// Получить весь список записей по артикулу
        [HttpGet("list")]
        public async Task<IActionResult> ReturnAllPurchasePrice(string vendorCode)
        {
            var listData = await _db.PurchasePrice
                .Where(c => c.Article == vendorCode)
                .OrderByDescending(c => c.SaveDataPrice)
                .ToListAsync();

            return Ok(listData);
        }
    }
}