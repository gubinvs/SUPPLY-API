namespace SUPPLY_API
{
    public class ReturnLastPrice
    {
        public int Id { get; set; }


        // Артикул номенклатуры
        public string? Article { get; set; }

        // Наименование номенклатуры
        public string? NameComponent { get; set; }

        // Цена покупки
        public int? Price { get; set; }


        // Дата покупки
        public string SaveDataPrice { get; set; }

        // Производитель
        public string? Manufacturer { get; set; }

        // Единица измерения
        public string? UnitMeasurement { get; set; }



        public  ReturnLastPrice
        (
           
            string article,
            string nameComponent,
            int price,
            string saveDataPrice,
            string manufacturer,
            string unitMeasurement

        )
        {
    
            Article = article;
            NameComponent = nameComponent;
            Price = price;
            SaveDataPrice = saveDataPrice;
            Manufacturer = manufacturer;
            UnitMeasurement = unitMeasurement;
        
        }
    }
}