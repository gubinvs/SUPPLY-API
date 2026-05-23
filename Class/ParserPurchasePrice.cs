
using System.Globalization;
using IdentityModel.Client;

namespace SUPPLY_API {
    /// <summary>
    /// Класс содержит поля для создания списка при парсинге данных из Exel файла Ювыгрузки данных из 1С о последних покупках
    /// называется: выгрузка цен закупки 
    /// покупках номенклатуры
    /// </summary>
    public class ParserPurchasePrice
    {
        // Артикул номенклатуры
        public string? VendorCode {get; set;}

        // Дата оприходования номенклатуры
        public DateTime SaveDataPrice {get; set;}

        // ИНН Поставщика
        public string? InnPurchase {get; set;}

        // Цена номенклатуры
        public int? PurchasePrice {get; set;}


        public ParserPurchasePrice (
            string vendorCode,
            DateTime saveDataPrice,
            string innPurchase,
            string purchasePrice
        )
        {
            VendorCode = vendorCode;
            SaveDataPrice = saveDataPrice;
            InnPurchase = innPurchase;
            purchasePrice = purchasePrice
                .Replace(" ", "")
                .Replace(",", ".");

            if (decimal.TryParse(
                purchasePrice,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out decimal price))
            {
                PurchasePrice = (int)Math.Round(price);
            }
            else
            {
                PurchasePrice = 0;
            }
        }
    }
}