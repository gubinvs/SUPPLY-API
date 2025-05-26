namespace SUPPLY_API
{
    public class PriceDb
    {
        public int Id { get; set; }

        public string? GuidIdComponent { get; set; }

        public string? GuidIdProvider { get; set; }

        public string? PriceComponent { get; set; }

        public DateTime SaveDataPrice { get; set; }

        // Пустой конструктор для EF Core
        public PriceDb() { }


        public PriceDb
                        (
                            string guidComponent,
                            string guidProvider,
                            string price,
                            DateTime dateTime
                        )
        {
            GuidIdComponent = guidComponent;
            GuidIdProvider = guidProvider;
            PriceComponent = price;
            SaveDataPrice = dateTime;
        }
    }
}