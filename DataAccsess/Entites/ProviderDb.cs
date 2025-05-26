namespace SUPPLY_API
{
    public class ProviderDb
    {
        public int Id { get; set; }

        public string? GuidIdProvider { get; set; }

        public string? NameProvider { get; set; }

        public string? InnProvider { get; set; }

        public ProviderDb() { }

        public ProviderDb(string guid, string name, string inn)
        {
            GuidIdProvider = guid;
            NameProvider = name;
            InnProvider = inn;
            
        }
        
    }
}