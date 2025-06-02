// using ClosedXML.Excel;
// using System.Text.Json;


// namespace SUPPLY_API {
//     /// <summary>
//     /// Чтение данных из Excel файлов
//     /// </summary>
//     public class ParserExcelFile 
//     {

//         /// <summary>
//         /// Конструктор по умолчанию
//         /// </summary>
//         public ParserExcelFile() {}

//         /// <summary>
//         /// Метод принимает путь к excel файлу на сервере, парсит первый столбец со второй строки,
//         /// возвращает сформированное в json строку содержимое файла
//         /// </summary>
//         /// <param name="filePath">Путь к файлу для парсинга</param>
//         /// <returns name="json">Строку в формате json с садержимым файла</returns>
//         public async Task<string> ParserVendorCode (string filePath) {       
//             // Открываем книгу Excel
//             var workbook = new XLWorkbook(filePath);

//             // Выбрали Лист_1 книги Excel 
//             var worksheet = workbook.Worksheet(1);

//             // Получаем количество заполненных строк
//             var count = worksheet.RangeUsed().RowCount();

//             // Получаем диапазон строк от второй до последней заполненной
//             // var rows = worksheet.RangeUsed().RowsUsed(); // Получаем все заполненные строки в файле
//             // var row2 = worksheet.Row(2); // Получаем указанную строку
//             var range = worksheet.Rows(2, count);

//             // Инициализируем массив с количеством заполненных строк
//             string[] vendorCode = new string[count];

//             // Заполняем массив данными
//             int i = 0;
//             await Task.Run(() => {
//                 foreach (var row in @range)
//                     {
//                         vendorCode[i] = row.Cell(1).Value.ToString(); // Преобразовали в строку
//                         i++;
//                     };
//             });
 
//             // Сериализуем в json формат
//             string json = JsonSerializer.Serialize(vendorCode);
            
//             return json;
//         }

//         /// <summary>
//         /// Парсинг цен из файла загрузки с сайта IEK GROOP https://www.iek.ru/products/price/
//         /// </summary>
//         /// <param name="filePath"></param>
//         /// <returns></returns>
//         public async Task<List<ParserPriceIekExcel>> ParserPriceIekExcel (string filePath)
//         {
//             // Открываем книгу Excel
//             var workbook = new XLWorkbook(filePath);

//             // Выбрали Лист_1 книги Excel 
//             var worksheet = workbook.Worksheet(1);

//             // Получаем количество заполненных строк
//             var count = worksheet.RangeUsed().RowCount();

//             // Получаем диапазон строк от второй до последней заполненной
//             // var rows = worksheet.RangeUsed().RowsUsed(); // Получаем все заполненные строки в файле
//             // var row = worksheet.Row(13); // Получаем указанную строку
//             // Получаем начиная с указанной строки до указанной или count - все строки var count = worksheet.RangeUsed().RowCount();
//             var range = worksheet.Rows(13, count);

//             List<ParserPriceIekExcel> priceList = new List<ParserPriceIekExcel>();

//             foreach (var row in @range)
//             {
//                 if (row != null) {
//                     /// row.Cell(1) - значение в столбце № 11
//                     await Task.Run(() => priceList.Add(new ParserPriceIekExcel(row.Cell(1).Value.ToString(), row.Cell(11).Value.ToString())));
//                 }
//             };

//             // Сериализуем в json формат
//             // string json = JsonSerializer.Serialize(priceList);

//             return priceList;
//         } 
//     };
// };