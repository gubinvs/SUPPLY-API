// Модель данных комплектующих

namespace SUPPLY_API
{

    public class ComponentDb
    {
        public int Id { get; set; }

        public string? GuidIdComponent { get; set; }

        public string? VendorCodeComponent { get; set; }

        public string? NameComponent { get; set; }


        // Пустой конструктор для EF
        public ComponentDb() { }

        public ComponentDb (
                            string guid,
                            string vendorCode,
                            string nameComponent
                        )
        {
            GuidIdComponent = guid;
            VendorCodeComponent = vendorCode;
            NameComponent = nameComponent;
        }
        
    };


};