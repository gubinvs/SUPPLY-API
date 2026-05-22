using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SUPPLY_API.Services;

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

            // Сохранение полученного файла на сервер получение пути к нему
            SavingFileFolder newFile = new SavingFileFolder();
            string filePath = newFile.ReturnNameFile(formFile);

            // Получение данных из файла в виде строки json
            ParserExcelFile parser = new ParserExcelFile();
            string json = parser.ParserPurchasePrice(filePath);

            // Подчищаем за собой, удаляем отработанный файл
            newFile.DeletingFile(filePath);

            return Ok(new { message = "Данные о новых закупках внесены в базу данных" });
        }
    };
};