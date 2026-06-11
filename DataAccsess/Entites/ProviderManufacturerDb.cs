using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    
    public class ProviderManufacturerDb
    {
        public int Id {get; set;}

        public string GuidIdManufacturer {get; set;} = String.Empty;

        public string GuidIdProvider {get; set;} = String.Empty;

        public ProviderManufacturerDb () {}
    }
}