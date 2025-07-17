

namespace SUPPLY_API 
{
    /// <summary>
    /// Класс содержит поля данных о закупках
    /// </summary>
    public class SupplyPurchaseDb
    {
        public int Id { get; set; }
        public string? GuidIdPurchase { get; set; }
        // Идентивикатор, нумерация закупки, внутренне пользователя. По типу артикула
        public string? PurchaseId { get; set; }
        public string? PurchaseName { get; set; }
        // Общая стоимость закупки, высчитывается исходя из стоимости номенкладуры, которая входит в закупку
        public int PurchasePrice { get; set; }
        // Заказчик закупки, если токое необходимо. например для кого формируется закупка
        public string? PurchaseCostomer { get; set; }

        public SupplyPurchaseDb() { }

    }
}