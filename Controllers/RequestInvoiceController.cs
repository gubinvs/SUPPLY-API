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

        
            // Рентабельность по которой работает серви, также устанавливается и на frontende
            double profitability = 1.1;
            // Генерируем новый идентификатор заказа
            string newGuidIdSupplyOrder = Guid.NewGuid().ToString();

            try
            {
                // Название заказа на основании закупки (спецификации)
                var newPurchase = new SupplyOrderUserDb
                {
                    GuidIdSupplyOrder = newGuidIdSupplyOrder,
                    GuidIdPurchase = model.guidIdPurchase,
                    PurchaseId = model.purchaseId,
                    PurchaseName = model.purchaseName,
                    PurchasePrice = model.purchasePrice,
                    PurchaseCostomer = model.purchaseCostomer,
                    SupplyOrderUserStatus = "новый"
                };

                await _db.SupplyOrderUser.AddAsync(newPurchase);
                await _db.SaveChangesAsync();

                // Сколько прибавить дней доставки
                int dayDelivery = 0;
                foreach (var e in model.purchaseItem)
                {
                    if (e.deliveryTimeComponent == "В наличии") {dayDelivery = 7;}
                    else if (e.deliveryTimeComponent == "от 1 до 4 нед") {dayDelivery = 28;}
                    else if (e.deliveryTimeComponent == "от 4 до 8 нед") {dayDelivery = 56;}
                    else if (e.deliveryTimeComponent == "от 8 до 12 нед") {dayDelivery = 84;}
                    else if (e.deliveryTimeComponent == "от 12 до 16 нед") {dayDelivery = 112;}
                    else if (e.deliveryTimeComponent == "от 16 до 20 нед") {dayDelivery = 140;}
                    else if (e.deliveryTimeComponent == "от 20 до 24 нед") {dayDelivery = 168;}
                    
                    var newOrder = new SupplyOrderUserComponentDb
                    {
                        GuidIdSupplyOrderUser = newGuidIdSupplyOrder,
                        VendorCodeComponent = e.vendorCodeComponent,
                        NameComponent = e.nameComponent,
                        QuantityComponent = e.requiredQuantityItem,
                        PriceComponent = (int)Math.Round(e.purchaseItemPrice * profitability), // Рентабельность по которой работает сервис
                        DeliveryTimeComponent = DateTime.UtcNow.AddDays(dayDelivery)
                    };

                    await _db.SupplyOrderUserComponent.AddAsync(newOrder);
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

                await _db.OrderUserAuthorization.AddAsync(adminUserAccess);
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