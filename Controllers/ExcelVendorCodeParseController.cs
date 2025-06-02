// using Microsoft.AspNetCore.Mvc;
// using EnComponentStore.Application;

// namespace EnComponentStore.Api.Controllers 
// {
//     /// <summary>
//     /// Контроллер обрабатывает файл excel, считывая из него построчно данные о артикулах товаров,
//     /// формирует массив этих данных, преобразует в json формат и отправляет обратно в виде json строки.
//     /// </summary>
//     /// 
    
//     [ApiController]
//     [Route("api/[controller]")]
//     public class ExcelVendorCodeParseController : ControllerBase 
//     {

//         [HttpPost]
//         public async Task<string> ParseFile(IFormFile formFile)
//         {
//             if (formFile != null)
//             {
//                 // Сохранение полученного файла на сервер получение пути к нему
//                 SavingFileFolder newFile = new SavingFileFolder();
//                 string filePath = await newFile.ReturnNameFile(formFile);

//                 // Получение данных из файла в виде строки json
//                 ParserExcelFile parser = new ParserExcelFile();
//                 string json = await parser.ParserVendorCode(filePath);

//                 // Подчищаем за собой, удаляем отработанный файл
//                 newFile.DeletingFile(filePath);
                
//                 return json;
//             }

//            return "Файл не обнаружен!";
//         }
//     };
// };
