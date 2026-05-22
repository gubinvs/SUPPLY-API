

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
        string? VendorCode {get; set;}

        // Наименование номенклатуры
        string? NameComponent {get; set;}

        // Дата оприходования номенклатуры
        DateTime SaveDataPrice {get; set;}

        // ИНН Поставщика
        string? InnPurchase {get; set;}

        // Цена номенклатуры
        int? PurchasePrice {get; set;}


        ParserPurchasePrice (
            string vendorCode,
            string  nameComponent,
            string saveDataPrice,
            string innPurchase,
            string purchasePrice
        )
        {
            VendorCode = vendorCode;
            NameComponent = nameComponent;
            DateTime.TryParse(saveDataPrice, out DateTime date);
            SaveDataPrice = date;
            InnPurchase = innPurchase;
            if (int.TryParse(purchasePrice, out int price))
            {
                PurchasePrice = price;
            }
        }
    }
}