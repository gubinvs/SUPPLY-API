

namespace SUPPLY_API
{
    public class ReturnOffer
    {
        public int Id { get; set; }

        // Артикул номенклатуры
        public string? Article { get; set; }

        // Наименование номенклатуры
        public string? NameComponent { get; set; }

        // Цена покупки
        public int? Price { get; set; }

        // Дата покупки
        public DateTime SaveDataPrice { get; set; }

        // Производитель
        public string? Manufacturer { get; set; }

        // Единица измерения
        public string? UnitMeasurement { get; set; }

        // Наименование предложения (например: цена покупки или цена предложения)
        public string? NamePrice {get; set;}


        public  ReturnOffer 
        (
            string article,
            string nameComponent,
            int price,
            DateTime saveDataPrice,
            string manufacturer,
            string unitMeasurement,
            string namePrice

        )
        {
            Article = article;
            NameComponent = nameComponent;
            Price = price;
            SaveDataPrice = saveDataPrice;
            Manufacturer = manufacturer;
            UnitMeasurement = unitMeasurement;
            NamePrice = namePrice;
        }
    }
}