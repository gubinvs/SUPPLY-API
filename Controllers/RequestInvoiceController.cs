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
        private readonly EmailSender _emailSender;


        public RequestInvoiceController(
                ILogger<RequestInvoiceController> logger,
                SupplyContext db,
                EmailSender emailSender
            )
        {
            _logger = logger;
            _db = db;
            _emailSender = emailSender;
        }


       [HttpPost]
        public async Task<IActionResult> RequestInvoice([FromBody] RequestInvoiceModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _db.CollaboratorSystem
                .FirstOrDefaultAsync(u => u.GuidIdCollaborator == model.guidIdCollaborator);

            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });

            // Проверка на совпадение размеров списков
            int itemCount = model.vendorCodeComponent.Count;
            if (model.nameComponent.Count != itemCount || 
                model.quantityComponent.Count != itemCount || 
                model.priceComponent.Count != itemCount)
            {
                return BadRequest(new { message = "Длины списков не совпадают" });
            }

            var createdOrders = new SupplyOrderDb();
            string newGuidIdSupplyOrder = Guid.NewGuid().ToString();

            try
            {
                for (int i = 0; i < itemCount; i++)
                {
                    var newOrder = new SupplyOrderDb
                    {
                        GuidIdSupplyOrder = newGuidIdSupplyOrder,
                        VendorCodeComponent = model.vendorCodeComponent[i],
                        NameComponent = model.nameComponent[i],
                        QuantityComponent = model.quantityComponent[i],
                        PriceComponent = model.priceComponent[i],
                        DeliveryTimeComponent = DateTime.UtcNow.AddDays(5) // временно статично
                    };

                    createdOrders.Add(newOrder);
                    await _db.SupplyOrderUser.AddAsync(newOrder);
                    await _db.SaveChangesAsync();
                }

                // Пропишем зависимости для доступа к заказам для пользователя заказчика
                var userAccess = new OrderUserAuthorizationDb
                {
                    GuidIdSupplyOrderUser = newGuidIdSupplyOrder,
                    GuidIdCollaborator = model.guidIdCollaborator
                };

                await _db.OrderUserAuthorization.AddAsync(userAccess);
                await _db.SaveChangesAsync();

                // И для администратора
                var adminUserAccess = new OrderUserAuthorizationDb
                {
                    GuidIdSupplyOrderUser = newGuidIdSupplyOrder,
                    GuidIdCollaborator = "b3c406b3-bbca-414a-959f-ee774655718a"
                };

                await _db.OrderUserAuthorization.AddAsync(userAccess);
                await _db.SaveChangesAsync();


                // Пример: отправка письма админу
                string emailAdmin = "gubinvs@gmail.com";
                string body = $"Создан заказ: {newGuidIdSupplyOrder}";
                _emailSender.SendEmail(emailAdmin, "Запрос счета", body);

                return Ok(new
                {
                    message = "Заказ успешно создан"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании заказа");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }
    }
}