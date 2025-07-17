

namespace SUPPLY_API
{
    /// <summary>
    ///  Класс содержит данные о взаимосвязи закупки и номенклатуры, тоесть какая номенклатура состоит в данной закупке
    /// </summary>
    public class PurchaseComponentDb
    {
        public int Id { get; set; }
        public string? GuidIdPurchase { get; set; }

        public string? GuidIdComponent { get; set; }

        // Количество номенклатуры в закупке
        public int? RequiredQuantityItem { get; set; }

        // Стоимость номенклатуры
        public int? PurchaseItemPrice { get; set; }

        // Выбранный лучший поставщик для этой номенклатуры
        public string? BestComponentProvider { get; set; }

        // Предложенный срок поставки номенклатуры
        public string? DeliveryTimeComponent { get; set; }




        public PurchaseComponentDb() { }
    }
}