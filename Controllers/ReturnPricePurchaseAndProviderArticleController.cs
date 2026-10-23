
using System;
using Microsoft.AspNetCore.Mvc;

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

        private readonly ReturnLastPurchasePriceService _ruturnLastPrice;

        public ReturnPricePurchaseAndProviderArticleController
        (
            ReturnMaxPriceProviderService returnMaxOffer,
            ReturnLastPurchasePriceService ruturnLastPrice
        )
        {
            _returnMaxOffer = returnMaxOffer;
            _ruturnLastPrice = ruturnLastPrice;
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
            var maxOffer = await _returnMaxOffer.GetMaxPriceProvider(article);

            if (maxOffer != null && !string.IsNullOrEmpty(maxOffer.SaveDataPrice))
            {
                if (DateTime.TryParseExact(maxOffer.SaveDataPrice, dateFormats, culture, System.Globalization.DateTimeStyles.None, out DateTime originalDateMaxOffer))
                {
                    // ЗНАК ИЗМЕНЕН НА >= (Прошло НЕ БОЛЕЕ 6 месяцев)
                    if (originalDateMaxOffer.AddMonths(6) >= currentDate)
                    {
                        Console.WriteLine($"+++ Найдено свежее предложение поставщика: {originalDateMaxOffer:dd.MM.yyyy} +++");
                        return Ok(maxOffer);
                    }
                }
            }

            // 3. Если данные не найдены ИЛИ все они старше 6 месяцев
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

            return Ok(errorOffer);          
        }

    }
}