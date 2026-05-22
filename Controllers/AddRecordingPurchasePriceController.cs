using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    /// <summary>
    /// Контроллер принимает файл в формате exel с данными о купленной номенклатуре ее стоимости и информации о поставщике.
    /// Эти данные формируются из 1с отчеты выгрузка покупок
    /// </summary>
    /// 
    [ApiController]
    [Route("api/[controller]")]
    public class AddRecordingPurchasePriceController : ControllerBase
    {

        private readonly ILogger<AddRecordingPurchasePriceController> _logger;

        private readonly SupplyContext _db;

        public AddRecordingPurchasePriceController (
            ILogger<AddRecordingPurchasePriceController> logger,
            SupplyContext db

        )
        {
            _logger = logger;
            _db = db;

        }


        [HttpPost]
        public IActionResult AddRecordingPurchasePrice (IFormFile formFile)
        {


            return Ok(new { message = "Данные о новых закупках внесены в базу данных" });
        }
    };
};