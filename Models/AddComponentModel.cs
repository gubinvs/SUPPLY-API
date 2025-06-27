namespace SUPPLY_API
{ 
      public record AddComponentModel(
        string VendorCodeComponent,
        string NameComponent,
        string guidIdManufacturer,
        string guidIdUnitMeasurement
    );
}