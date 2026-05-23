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
        private readonly ManufacturerComponentContext _dbManufacturer;
        private readonly UnitMeasurementComponentContext _dbUnit;
        private readonly SupplyUnitMeasurementContext _dbUnitName;
        private readonly SupplyManufacturerContext _dbSupplyManufacturer;

        public AddRecordingPurchasePriceController (
            ILogger<AddRecordingPurchasePriceController> logger,
            SupplyContext db,
            ManufacturerComponentContext dbManufacturer,
            UnitMeasurementComponentContext dbUnit,
            SupplyUnitMeasurementContext dbUnitName,
            SupplyManufacturerContext dbSupplyManufacturer

        )
        {
            _logger = logger;
            _db = db;
            _dbManufacturer = dbManufacturer;
            _dbUnit = dbUnit;
            _dbUnitName = dbUnitName;
            _dbSupplyManufacturer = dbSupplyManufacturer;
        }


        [HttpPost]
        public async Task<IActionResult> AddRecordingPurchasePrice (IFormFile formFile)
        {

            // Сохранение полученного файла на сервер и получение пути к нему
            SavingFileFolder newFile = new SavingFileFolder();
            string filePath = await newFile.ReturnNameFile(formFile);

            // Получение данных из файла в виде заполненного класса
            ParserExcelFile parser = new ParserExcelFile();
            List<ParserPurchasePrice> listPurchasePrice = await parser.ParserFilePurchasePrice(filePath);

            // Подчищаем за собой, удаляем отработанный файл
            newFile.DeletingFile(filePath);

            // Записываем в базу данных информацию из полученного заполненного класса
            if (listPurchasePrice != null)
            {
                // Заполняем класс для базы данных
                foreach (var item  in listPurchasePrice)
                {
                    // Запрос в базу данных, для сопоставления номенклатуры по артикулу и определения значения поля GuidIdComponent
                    var component = await _db.SupplyComponent.FirstOrDefaultAsync(c => c.VendorCodeComponent == item.VendorCode);

                    // Проверка на наличии номенклатуры в базе данных
                    if (component != null)
                    {
                        // Достали GuidIdComponent
                        string? guidIdComponent = component.GuidIdComponent;

                        // Ищем GuidIdProvider идентификатор поставщика есть в базе или нет
                        var provider = await _db.SupplyProvider.FirstOrDefaultAsync(c => c.InnProvider == item.InnPurchase);

                        // Проверка на наличие такого поставщика
                        if (provider != null)
                        {
                            // Достали нужный нам GuidIdProvider и наименование поставщика в базе
                            string? guidIdProvider = provider.GuidIdProvider;
                            string? nameProvider = provider.NameProvider;

                            // Достаем из базы данных данные о производителе номенклатуры по GuidIdComponent
                            var manufacturer = await _dbManufacturer.ManufacturerComponent.FirstOrDefaultAsync(c => c.GuidIdComponent == component.GuidIdComponent);

                            if (manufacturer != null)
                            {
                            
                                // Создаем переменную GuidIdManufacturer
                                string? guidIdManufacturer = manufacturer.GuidIdManufacturer;

                                // Достаем наименование производителя
                                var supplyManufacturer = await _dbSupplyManufacturer.SupplyManufacturer.FirstOrDefaultAsync(c => c.GuidIdManufacturer == guidIdManufacturer);
                                
                                if (supplyManufacturer != null)
                                {
                                    // Достаем название производителя
                                    string? manufacturerName = supplyManufacturer.NameManufacturer;

                                    // Аналогично достаем параметр единицы измерения номенклатуры UnitMeasurement
                                    var unitMeasurement = await _dbUnit.UnitMeasurementComponent.FirstOrDefaultAsync(c => c.GuidIdComponent == component.GuidIdComponent);

                                    if (unitMeasurement != null)
                                    {
                                        // Получаем идентификатор единицы измерения GuidIdUnitMeasurement
                                        string? guidIdUnitMeasurement = unitMeasurement.GuidIdUnitMeasurement;

                                        // Получаем саму единицу измерения
                                        var unit = await _dbUnitName.SupplyUnitMeasurement.FirstOrDefaultAsync(c => c.GuidIdUnitMeasurement == guidIdUnitMeasurement);

                                        if (unit != null)
                                        {
                                            // Записываем ее в переменную
                                            string? unitName = unit.NameUnitMeasurement;

                                            // Все необходимые данные для заполнения базы данных собраны, создаем объект для записи
                                            var recording = new PurchasePriceDb
                                            {
                                                GuidIdComponent = guidIdComponent,
                                                Article = component.VendorCodeComponent,
                                                NameComponent = component.NameComponent,
                                                GuidIdProvider = guidIdProvider,
                                                NameProvider = nameProvider,
                                                PurchasePrice = item.PurchasePrice,
                                                SaveDataPrice = item.SaveDataPrice,
                                                Manufacturer = manufacturerName,
                                                UnitMeasurement = unitName
                                            };

                                            _db.Add(recording);                
                                        }
                                    }
                                    
                                }
                            }
                        }
                    }
                }
            }

            await _db.SaveChangesAsync();
            
            return Ok(new { message = "Данные о новых закупках внесены в базу данных" });
        }
    };
};