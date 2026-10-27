
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    
    /// Контроллер принимает артикул номенклатуры, делает запрос в базу данных PurchasePrice и выбирает последнюю запись, 
    /// анализируя дату записи
    /// и возвращает объект с найденной информацией. Если соответствующих записей в базе данных нет, то делается запрос в базу данных 
    /// PriceComponent,
    /// и аналогично контроллеру ReturnPriceProviderArticle возьмет данные по всем предложениям и выберет одно 
    /// с максимальной ценой предложения.
    /// 
    
    
    [Controller]
    [Route("api/[controller]")]
    public class ReturnPricePurchaseAndProviderArticleController : ControllerBase
    {
        private readonly ReturnMaxPriceProviderService _returnMaxOffer;
        private readonly SupplyPriceComponentContext _dbPrice;
        private readonly SupplyComponentContext _db;
        private readonly ReturnLastPurchasePriceService _ruturnLastPrice;
        private readonly SupplyManufacturerContext _dbSupplyManufact;
        private readonly ManufacturerComponentContext _dbManufact;
        private readonly UnitMeasurementComponentContext _dbUnit;
        private readonly SupplyUnitMeasurementContext _dbSupplyUnit;


        public ReturnPricePurchaseAndProviderArticleController
        (
            ReturnMaxPriceProviderService returnMaxOffer,
            ReturnLastPurchasePriceService ruturnLastPrice,
            SupplyPriceComponentContext dbPrice,
            SupplyComponentContext db,
            SupplyManufacturerContext dbSupplyManufact,
            ManufacturerComponentContext dbManufact,
            UnitMeasurementComponentContext dbUnit,
            SupplyUnitMeasurementContext dbSupplyUnit
        )
        {
            _db =  db;
            _dbPrice = dbPrice;
            _returnMaxOffer = returnMaxOffer;
            _ruturnLastPrice = ruturnLastPrice;
            _dbSupplyManufact = dbSupplyManufact;
            _dbManufact = dbManufact;
            _dbUnit = dbUnit;
            _dbSupplyUnit = dbSupplyUnit;

        }
        

        [HttpGet]
        public async Task<IActionResult> CreateReport(string article)
        {
            // Today берет только дату без времени (00:00:00), что идеально для сравнения дат
            DateTime currentDate = DateTime.Today;
            
            // Массив шаблонов для поддержки обоих форматов дат ("20.05.2026" и "2026-05-20")
            string[] dateFormats = { "dd.MM.yyyy", "yyyy-MM-dd" };
            var culture = System.Globalization.CultureInfo.InvariantCulture;

            // 1. Запрашиваем цену последней фактической покупки
            var lastPrice = await _ruturnLastPrice.ReturnLastPrice(article);

            if (lastPrice != null && !string.IsNullOrEmpty(lastPrice.SaveDataPrice))
            {
                if (DateTime.TryParseExact(lastPrice.SaveDataPrice, dateFormats, culture, System.Globalization.DateTimeStyles.None, out DateTime originalDateLastPrice))
                {
                    // ЗНАК ИЗМЕНЕН НА >= (Прошло НЕ БОЛЕЕ 6 месяцев)
                    if (originalDateLastPrice.AddMonths(6) >= currentDate)
                    {
                        Console.WriteLine($"=== Найдена свежая цена покупки: {originalDateLastPrice:dd.MM.yyyy} ===");
                        return Ok(lastPrice);
                    }
                }
            }

            // 2. Если цены покупки нет или она устарела (старше 6 месяцев), проверяем предложение поставщика
            // var maxOffer = await _returnMaxOffer.GetMaxPriceProvider(article);

            // if (maxOffer != null && !string.IsNullOrEmpty(maxOffer.SaveDataPrice))
            // {
            //     if (DateTime.TryParseExact(maxOffer.SaveDataPrice, dateFormats, culture, System.Globalization.DateTimeStyles.None, out DateTime originalDateMaxOffer))
            //     {
            //         // ЗНАК ИЗМЕНЕН НА >= (Прошло НЕ БОЛЕЕ 6 месяцев)
            //         if (originalDateMaxOffer.AddMonths(6) >= currentDate)
            //         {
            //             Console.WriteLine($"+++ Найдено свежее предложение поставщика: {originalDateMaxOffer:dd.MM.yyyy} +++");
            //             return Ok(maxOffer);
            //         }
            //     }
            // }

            // ?. Если данные не найдены ИЛИ все они старше 6 месяцев
            // Возвращаем 404, так как актуальных (свежих) цен в базе нет
            // Пестое сообщение если такого артикула вообще нет в базе данных
            ReturnOffer errorOffer = new ReturnOffer
            (
                1, // Если 0 то это означает ,что нет ошибки, если 1 то ошибка
                "",
                "",
                0,
                "",
                "",
                "",
                "",
                ""
            );


            // 1. Текущая дата и точка отсчета (минус 6 месяцев)
            var sixMonthsAgo = currentDate.AddMonths(-6);
        
            // 2. Получаем идентификатор запрашиваемого артикула
            var component = await _db.SupplyComponent
                .Where(x => x.VendorCodeComponent == article)
                .FirstOrDefaultAsync(); // Безопаснее использовать FirstOrDefault

            if (component == null)
            {
                return NotFound(errorOffer);
            }

            // 3. Запрашиваем из базы сразу ОТФИЛЬТРОВАННОЕ и МАКСИМАЛЬНОЕ предложение
            // Предполагаем, что в базе дата хранится в типе DateTime, а цена в decimal/double
            var maxOfferProvider = await _dbPrice.PriceComponent
                .Where(p => p.GuidIdComponent == component.GuidIdComponent 
                        && p.SaveDataPrice >= sixMonthsAgo) // Только не старше 6 месяцев
                .OrderByDescending(p => p.PriceComponent)   // Сортируем по убыванию цены
                .FirstOrDefaultAsync();                     // Берем самое первое (максимальное)
            

            if (maxOfferProvider == null) 
            {
                return NotFound(errorOffer);
            }

            var manufacturerComponent = await _dbManufact.ManufacturerComponent
                                .Where(c => c.GuidIdComponent == component.GuidIdComponent)
                                .Select(c => c.GuidIdManufacturer)
                                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(manufacturerComponent))
            {
                return NotFound(errorOffer);
            }

            var manufacturerName = await _dbSupplyManufact.SupplyManufacturer
                    .Where(sm => sm.GuidIdManufacturer == manufacturerComponent)
                    .Select(sm => sm.NameManufacturer)
                    .FirstOrDefaultAsync();
                            var unitComponent = await _dbUnit.UnitMeasurementComponent
                    .Where(c => c.GuidIdComponent == component.GuidIdComponent)
                    .Select(c => c.GuidIdUnitMeasurement)
                    .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(unitComponent))
             {
                return NotFound(errorOffer);
            }

            var unitName = await _dbSupplyUnit.SupplyUnitMeasurement
                .Where(su => su.GuidIdUnitMeasurement == unitComponent)
                .Select(su => su.NameUnitMeasurement)
                .FirstOrDefaultAsync();

                
            if (unitName == null)
   {
                return NotFound(errorOffer);
            }

            ReturnOffer newOffer = new ReturnOffer
                (
                    0,
                    article,
                    component.NameComponent ?? "",
                    maxOfferProvider.PriceComponent ?? 0,
                    maxOfferProvider.DeliveryTimeComponent ?? "",
                    maxOfferProvider.SaveDataPrice.ToString("dd.MM.yyyy"),
                    manufacturerName ?? "",
                    unitName ?? "",
                    "Максимальная цена предложений"
                    
                );


            return Ok(newOffer);          
        }

    }
}