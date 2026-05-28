using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SUPPLY_API.Models;

namespace SUPPLY_API
{

    /// <summary>
    /// Контроллер принимает список артикулов и возвращает список последних покупок (оприходований в 1С) 
    /// по каждому принятому в контроллер артикулу
    /// 
    /// </summary>
    [Controller]
    [Route("api/[controller]")]
    public class ReturnListEntryPurchasePriceController : ControllerBase
    {
        private readonly ILogger<ReturnListEntryPurchasePriceController> _logger;
        private readonly SupplyContext _db;

        public ReturnListEntryPurchasePriceController
        (
            ILogger<ReturnListEntryPurchasePriceController> logger,
            SupplyContext db
        )
        {
            _logger = logger;
            _db = db;
        }

        /// Получить первую запись в отсортированном по убыванию списке на основании даты записи
        /// тем самым возвращаем последнюю (свежую) на основании даты запись
        [HttpPost]
        public async Task<IActionResult> ReturnDataListEntryPurchase([FromBody] ListArticle vendorCode)
        {
          
            if (vendorCode.Articles == null) {
                return BadRequest(new { message = "Список артикулов пуст или не указан." });
            }; 

            // Выборка из массива только тех артикулов, которые пришли в запросе
            var result = (await _db.PurchasePrice
                .Where(x => x.Article != null &&
                        vendorCode.Articles.Contains(x.Article))
                    .OrderByDescending(x => x.SaveDataPrice)
                    .ToListAsync())
                .GroupBy(x => x.Article)
                .Select(g => g.First())
                .ToList();

            return Ok(result);
        }
    }
}