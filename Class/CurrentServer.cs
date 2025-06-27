// Класс с данными о текущем адресе сервера

namespace SUPPLY_API
{
    // public static class CurrentServer
    // {
    //     // Адрес сервера API
    //     //public static string ServerAddressApi = "http://31.129.97.48:1030"; 

    //     // Перенаправление на страницу сайта
    //     // public static string ServerAddressFrontend = "http://31.129.97.48:1040";

    //     // Адрес сервера API
    //     //public static string ServerAddressApi = "https://supplyapi.encomponent.ru"; 

    //     // Перенаправление на страницу сайта
    //     // public static string ServerAddressFrontend = "https://supply.encomponent.ru";

    //     // Адрес сервера API
    //     public static string ServerAddressApi = "http://localhost:8080"; 

    //     // Перенаправление на страницу сайта
    //     public static string ServerAddressFrontend = "http://http://localhost:3001";

    // }

      public class CurrentServer
    {
        public required string ServerAddressApi { get; set; }
        public required string ServerAddressFrontend { get; set; }
    }
}