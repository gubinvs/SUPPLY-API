// Класс с данными о текущем адресе сервера

namespace SUPPLY_API
{
    public static class CurrentServer
    {

        public static string ServerAddress = "http://localhost:8080"; // Локальный сервер для тренировок

        // Сервер на котором крутиться frontend
        public static string ServerAddressFrontend = "http://localhost:3000";  // Локальный сервер для тренировок


        // Адрес на котором ледит этот апи сервер
        //public static string ServerAddress = "http://31.129.97.48:1040"; // Боевой сервер

        // Сервер на котором крутиться frontend
        // public static string ServerAddressFrontend = "http://localhost:3000";  // Боевой сервер
    }
}