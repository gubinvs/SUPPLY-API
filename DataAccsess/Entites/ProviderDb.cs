namespace SUPPLY_API
{
    public class Provider
    {
        public int Id { get; set; }

        public string? GuidIdProvider { get; set; }

        public string? NameProvider { get; set; }

        public int? InnProvider { get; set; }

        public Provider() { }

        public Provider(string guid, string name, int inn)
        {
            GuidIdProvider = guid;
            NameProvider = name;
            InnProvider = inn;
            
        }
        
    }
}