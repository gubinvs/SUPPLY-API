// using Microsoft.AspNetCore.Mvc;

// namespace SUPPLY_API
// {
//     // Контроллер принимает данные о составе закупки и рассылает запросы на выставление счетов, 
//     // компании котораю указана как поставщик. Если запрос приходит от FREE ПОЛЬЗОВАТЕЛЯ,
//     // запрос отправляется на один адрес и сохраняется в базе данных и больше не меняется
//     // в ответ отправляется сообщение о удачном или нет результате
//     [ApiController]
//     [Route("api/[controller]")]
//     public class RequestInvoiceController : ControllerBase
//     {
//         private readonly ILogger<RequestInvoiceController> _logger;
//         private readonly SupplyContext _db;

//         public RequestInvoiceController (
//                 ILogger<RequestInvoiceController> logger,
//                 SupplyContext db
//             )
//         {
//             _logger = logger;
//             _db = db;
//         }


//         [HttpPost]
//         public async Task<IActionResult> RequestInvoice([FromBody] RequestInvoiceModel model)
//         {
            
//         }

//     }
// }