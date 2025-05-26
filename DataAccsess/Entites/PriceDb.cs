namespace SUPPLY_API
{
    public class PriceDb
    {
        public int Id { get; set; }

        public string? GuidIdComponent { get; set; }

        public string? GuidIdProvider { get; set; }

        public int? PriceComponent { get; set; }

        public string? DeliveryTimeComponent { get; set; }

        public DateTime SaveDataPrice { get; set; }

        // Пустой конструктор для EF Core
        public PriceDb() { }


        public PriceDb
                        (
                            string guidComponent,
                            string guidProvider,
                            int price,
                            string delivery,
                            DateTime dateTime
                        )
        {
            GuidIdComponent = guidComponent;
            GuidIdProvider = guidProvider;
            PriceComponent = price;
            DeliveryTimeComponent = delivery;
            SaveDataPrice = dateTime;
        }
    }
}