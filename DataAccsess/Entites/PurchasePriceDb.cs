// Модель данный о пользователях системы


namespace SUPPLY_API
{
    public class PurchasePriceDb
    {
        public int Id { get; set; }

        // Идентификатор номенклатуры
        public string? GuidIdComponent { get; set; }

        // Артикул номенклатуры
        public string? Article { get; set; }

        // Наименование номенклатуры
        public string? NameComponent { get; set; }

        // Идентификатор поставщика
        public string? GuidIdProvider { get; set; }

        // Наименование поставщика
        public string? NameProvider { get; set; }

        // Цена покупки
        public int? PurchasePrice { get; set; }

        // Дата покупки
        public DateTime SaveDataPrice { get; set; }

        // Производитель
        public string? Manufacturer { get; set; }

        // Единица измерения
        public string? UnitMeasurement { get; set; }


        public PurchasePriceDb() { }
        public PurchasePriceDb(
                    string guidIdComponent,
                    string article,
                    string nameComponent,
                    string guidIdProvider,
                    string nameProvider,
                    int purchasePrice,
                    DateTime saveDataPrice,
                    string manufacturer,
                    string unitMeasurement 
                )
        {
            GuidIdComponent = guidIdComponent;
            Article = article;
            NameComponent = nameComponent;
            GuidIdProvider = guidIdProvider;
            NameProvider = nameProvider;
            PurchasePrice = purchasePrice;
            SaveDataPrice = saveDataPrice;
            Manufacturer = manufacturer;
            UnitMeasurement = unitMeasurement;
        }
    }
}
