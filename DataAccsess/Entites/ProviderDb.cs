namespace SUPPLY_API
{
    public class ProviderDb
    {
        public int Id { get; set; }

        public string? GuidIdProvider { get; set; }

        public string? NameProvider { get; set; }

        public int? InnProvider { get; set; }

        public ProviderDb() { }

        public ProviderDb(string guid, string name, int inn)
        {
            GuidIdProvider = guid;
            NameProvider = name;
            InnProvider = inn;
            
        }
        
    }
}