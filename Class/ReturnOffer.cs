

namespace SUPPLY_API
{
    public class ReturnOffer
    {
        public int Id { get; set; }

        public int Error {get; set;}

        // Артикул номенклатуры
        public string? Article { get; set; }

        // Наименование номенклатуры
        public string? NameComponent { get; set; }

        // Цена покупки
        public int? Price { get; set; }

        // Срок поставки
        public string? DeliveryTimeComponent {get; set;}


        // Дата покупки
        public string SaveDataPrice { get; set; }

        // Производитель
        public string? Manufacturer { get; set; }

        // Единица измерения
        public string? UnitMeasurement { get; set; }

        // Наименование предложения (например: цена покупки или цена предложения)
        public string? NamePrice {get; set;}


        public  ReturnOffer 
        (
            int error,
            string article,
            string nameComponent,
            int price,
            string deliveryTimeComponent,
            string saveDataPrice,
            string manufacturer,
            string unitMeasurement,
            string namePrice

        )
        {
            Error = error;
            Article = article;
            NameComponent = nameComponent;
            Price = price;
            DeliveryTimeComponent = deliveryTimeComponent;
            SaveDataPrice = saveDataPrice;
            Manufacturer = manufacturer;
            UnitMeasurement = unitMeasurement;
            NamePrice = namePrice;
        }
    }
}