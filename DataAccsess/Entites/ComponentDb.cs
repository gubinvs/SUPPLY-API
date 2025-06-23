// Модель данных комплектующих

namespace SUPPLY_API
{

    public class ComponentDb
    {
        public int Id { get; set; }

        public string? GuidIdComponent { get; set; }

        public string? VendorCodeComponent { get; set; }

        public string? NameComponent { get; set; }

        // Производитель
        public string? NameManufacturer { get; set; }

        // Единица измерения
        public string? UnitMeasurement { get; set; }


        // Пустой конструктор для EF
        public ComponentDb() { }

        public ComponentDb
                                (string guid,
                                    string vendorCode,
                                    string nameComponent,
                                    string nameManufacturer
                                )
        {
            GuidIdComponent = guid;
            VendorCodeComponent = vendorCode;
            NameComponent = nameComponent;
            NameManufacturer = nameManufacturer;

        }
        
    };


};