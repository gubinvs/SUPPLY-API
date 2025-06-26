// Класс с данными о текущем адресе сервера

namespace SUPPLY_API
{
    public static class CurrentServer
    {

        //public static string ServerAddress = "http://localhost:8080"; // Локальный сервер API

        // Сервер на котором крутиться frontend
        //public static string ServerAddressFrontend = "http://localhost:3000";  // Локальный сервер для тренировок


        // Адрес на котором лежит этот API сервер
        public static string ServerAddress = "http://31.129.97.48:1030"; // Боевой сервер API

        // Сервер на котором крутиться frontend
        public static string ServerAddressFrontend = "http://31.129.97.48:1040";  // Боевой сервер frond
    }
}