

using Microsoft.AspNetCore.Mvc;

namespace SUPPLY_API
{
    
    /// Контроллер принимает артикул номенклатуры, делает запрос в базу данных PurchasePrice и выбирает последнюю запись, анализируя дату записи
    /// и возвращает объект с найденной информацией. Если соответствующих записей в базе данных нет, то делается запрос в базу данных PriceComponent,
    /// и аналогично контроллеру ReturnPriceProviderArticle возьмет данные по всем предложениям и выберет одно с максимальной ценой предложения.
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
            // Сначала запрашиваем цену последей фактической покупки, а если ее нет то запрашиваем максимальное предложение поставщиков
            var lastPrice = await _ruturnLastPrice.ReturnLastPrice(article);

            if (lastPrice == null )
            {
                // Если записей о последней цене покупки нет, то отправляем данные о последней покупке,
                // можно еще настроить, что дата цены покупки старше 3-х месяцев, предлагаем предложение
                var maxOffer = await _returnMaxOffer.GetMaxPriceProvider(article);

                return Ok(maxOffer);
            } 
            else
            {
                return Ok(lastPrice);
            }            
        }
    }
}