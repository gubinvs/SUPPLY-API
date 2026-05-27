using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SUPPLY_API.Models;

namespace SUPPLY_API
{

    /// <summary>
    /// Контроллер принимает список артикулов и возвращает список последних покупок (оприходований в 1С) по каждому артикулу
    /// 
    /// </summary>
    [Controller]
    [Route("api/[controller]")]
    public class ReturnListEntryPurchasePriceController : ControllerBase
    {
        private readonly ILogger<ReturnLastEntryPurchasePriceController> _logger;
        private readonly SupplyContext _db;

        public ReturnListEntryPurchasePriceController
        (
            ILogger<ReturnLastEntryPurchasePriceController> logger,
            SupplyContext db
        )
        {
            _logger = logger;
            _db = db;
        }

        /// Получить первую запись в отсортированном по убыванию списке на основании даты записи
        /// тем самым возвращаем последнюю (свежую) на основании даты запись
        [HttpPost]
        public async Task<IActionResult> ReturnDataListEntryPurchase(ListArticle vendorCode)
        {
          
            // Создаем список последних записей в 1С
            List<ReturnLastPrice> returnListPrice = new List<ReturnLastPrice>();

            // Получим все данные из базы
            var data = await _db.PurchasePrice.ToListAsync();

            // Оставим только те, которые соответствуют артикулу запроса
            foreach (string vendorCode.Articles in item)
            {
                
            }
            
            //.OrderByDescending(c => c.SaveDataPrice)


                // Список результата
    List<ReturnLastPrice> returnListPrice = new List<ReturnLastPrice>();

    // Получаем все записи
    var data = await _db.PurchasePrice.ToListAsync();

    // Фильтруем по артикулам
    var filteredData = data
        .Where(x => vendorCode.Articles.Contains(x.Article))
        .OrderByDescending(x => x.SaveDataPrice)
        .ToList();

    // Группируем по артикулу и берем последнюю цену
    returnListPrice = filteredData
        .GroupBy(x => x.Article)
        .Select(g => new ReturnLastPrice
        {
            Article = g.Key,
            Price = g.First().Price,
            SaveDataPrice = g.First().SaveDataPrice
        })
        .ToList();

    return Ok(returnListPrice);
                

            return Ok();
        }
    }
}