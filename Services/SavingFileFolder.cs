// using Microsoft.AspNetCore.Http;

// namespace EnComponentStore.Application {
//     public class SavingFileFolder {
//         /// <summary>
//         /// Путь к папке для временного хранения файла
//         /// </summary>
//         string UploadPath;

//         /// <summary>
//         /// Конструктор по умолчанию
//         /// </summary>
//         public SavingFileFolder()
//         {
//             // Путь к папке для хранения временных файлов
//             this.UploadPath = $"{Directory.GetCurrentDirectory()}/Files";
//             // создаем папку для хранения временный файлов
//             //Directory.CreateDirectory(this.UploadPath);
//         }

//         /// <summary>
//         /// Метод принимает файл, сохраняет его в папку для временного хранения 
//         /// и возвращает строку содержащую информацию о пути к загруженному файлу
//         /// </summary>
//         /// <param name="file">Получаемый методом post файл</param>
//         /// <returns name="fullPath">Путь к загруженному файлу на сервере</returns>
//         public async Task<string> ReturnNameFile (IFormFile file) {

//             Guid guid= Guid.NewGuid();

//             // формирование пути к загруженному файлу
//             string fullPath = $"{this.UploadPath}/{guid}{file.FileName}";

//             await Task.Run(() => {
//                 using (var fileStream = new FileStream(fullPath, FileMode.Create))
//                 {
//                     // сохраняем файл в папку uploads
//                     file.CopyTo(fileStream);
//                 }
//             });
            
//             return fullPath;
//         }

//         /// <summary>
//         /// Удаление файла
//         /// </summary>
//         /// <param name="pathFile">Полный путь к файлу для удаления</param>
//         public void DeletingFile(string pathFile)
//         {
//             // удаляем файл по указанному пути
//             FileInfo fileInfo = new FileInfo(pathFile);
//             fileInfo.Delete();

//         }
//     }
// }