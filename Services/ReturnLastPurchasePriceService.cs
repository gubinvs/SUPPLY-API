


using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    /// Сервис аналогичен контроллеру ReturnLastEntryPurchasePriceController
    /// /// <summary>
    /// Принимает артикул и достает из базы данных запись соответствующую артикулу
    /// метод ReturnDataLastEntryPurchasePrice возвращает последнюю на основании даты записи, а метод
    /// ReturnAllPurchasePrice возвращает все записи соответствующие артикулу
    /// </summary>
    /// 
    /// 
    
    public class ReturnLastPurchasePriceService
    {
        private readonly SupplyContext _db;
        private readonly ReturnMaxPriceProviderService _returnMaxOffer;

        public ReturnLastPurchasePriceService
        (
            ReturnMaxPriceProviderService returnMaxOffer,
            SupplyContext db
        )
        {
            _returnMaxOffer = returnMaxOffer;
            _db = db;
        }

        /// Получить первую запись в отсортированном по убыванию списке на основании даты записи
        /// тем самым возвращаем последнюю (свежую) на основании даты запись
        public async Task<ReturnOffer?> ReturnLastPrice (string vendorCode)
        {
            var data = await _db.PurchasePrice
                .Where(c => c.Article == vendorCode)
                .OrderByDescending(c => c.SaveDataPrice)
                .FirstOrDefaultAsync();
            
            if (data == null)
            {
                return null;
            }

            var maxOffer = await _returnMaxOffer.GetMaxPriceProvider(vendorCode);

            if (maxOffer == null)
            {
                return null;
            }

            
            ReturnOffer newOffer = new ReturnOffer
            (
                vendorCode,
                data.NameComponent ?? "",
                data.PurchasePrice ?? 0,
                maxOffer.DeliveryTimeComponent ?? "Нет данных о сроках поставки",
                data.SaveDataPrice.ToString("dd.MM.yyyy"),
                data.Manufacturer ?? "",
                data.UnitMeasurement ?? "",
                "Последняя цена покупки"
            );

            return (newOffer);
        }
    }

}