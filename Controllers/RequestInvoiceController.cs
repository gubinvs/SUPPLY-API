using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    // Контроллер принимает данные о составе закупки и рассылает запросы на выставление счетов, 
    // компании котораю указана как поставщик. Если запрос приходит от FREE ПОЛЬЗОВАТЕЛЯ,
    // запрос отправляется на один адрес и сохраняется в базе данных и больше не меняется
    // в ответ отправляется сообщение о удачном или нет результате

    [ApiController]
    [Route("api/[controller]")]
    public class RequestInvoiceController : ControllerBase
    {
        private readonly ILogger<RequestInvoiceController> _logger;
        private readonly SupplyContext _db;

        public RequestInvoiceController (
                ILogger<RequestInvoiceController> logger,
                SupplyContext db
            )
        {
            _logger = logger;
            _db = db;
        }


        [HttpPost]
        public async Task<IActionResult> RequestInvoice([FromBody] RequestInvoiceModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _db.CollaboratorSystem
                .FirstOrDefaultAsync(u => u. == model.guidIdCollaborator);

            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });

            var supplyOrder = new SupplyOrderDb
            {
                GuidIdSupplyOrder = Guid.NewGuid().ToString(),
                GuidIdCollaborator = model.guidIdCollaborator,
                VendorCodeComponent = model.vendorCodeComponent,
                NameComponent = model.nameComponent,
                QuantityComponent = model.quantityComponent,
                PriceComponent = model.priceComponent,
                DeliveryTimeComponent = DateTime.UtcNow.AddDays(5)
            };

            try
            {
                await _db.SupplyOrderUser.AddAsync(supplyOrder);
                await _db.SaveChangesAsync();

               
                // Здесь можно добавить логику отправки email/уведомления
                // await _notificationService.SendEmail(...);
                
                return Ok(new { message = "Заказ успешно записан в базу данных", id = supplyOrder.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании заказа");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }



    }
}