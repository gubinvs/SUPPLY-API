using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using SUPPLY_API.Models; // Подключи свой namespace для моделей
using Microsoft.EntityFrameworkCore.Storage;

namespace SUPPLY_API
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestInvoiceController : ControllerBase
    {
        private readonly ILogger<RequestInvoiceController> _logger;
        private readonly SupplyContext _db;
        private readonly EmailSender _emailSender;

        private const double DefaultProfitability = 1.1;
        private const int DefaultDeliveryDays = 5;
        private const string AdminCollaboratorGuid = "b3c406b3-bbca-414a-959f-ee774655718a";
        private const string AdminEmail = "gubinvs@gmail.com";

        public RequestInvoiceController(
            ILogger<RequestInvoiceController> logger,
            SupplyContext db,
            EmailSender emailSender)
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

            if (model.purchaseItem == null || !model.purchaseItem.Any())
                return BadRequest(new { message = "Не указаны элементы закупки" });

            var user = await _db.CollaboratorSystem
                .FirstOrDefaultAsync(u => u.GuidIdCollaborator == model.guidIdCollaborator);

            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });

            string newGuidIdSupplyOrder = Guid.NewGuid().ToString();

            using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // Создаем запись заказа
                var newPurchase = new SupplyOrderUserDb
                {
                    GuidIdSupplyOrder = newGuidIdSupplyOrder,
                    GuidIdPurchase = model.guidIdPurchase,
                    PurchaseId = model.purchaseId,
                    PurchaseName = model.purchaseName,
                    PurchasePrice = model.purchasePrice,
                    PurchaseCostomer = model.purchaseCostomer,
                };

                await _db.SupplyOrderUser.AddAsync(newPurchase);
                await _db.SaveChangesAsync();

                foreach (var e in model.purchaseItem)
                {
                    var newOrder = new SupplyOrderUserComponentDb
                    {
                        GuidIdSupplyOrder = newGuidIdSupplyOrder,
                        VendorCodeComponent = e.VendorCodeComponent,
                        NameComponent = e.NameComponent,
                        QuantityComponent = e.RequiredQuantityItem,
                        PriceComponent = Convert.ToInt32(e.PurchaseItemPrice * DefaultProfitability),
                        DeliveryTimeComponent = DateTime.UtcNow.AddDays(DefaultDeliveryDays)
                    };

                    await _db.SupplyOrderUserComponent.AddAsync(newOrder);
                }

                await _db.SaveChangesAsync();

                // Доступ заказчику
                var userAccess = new OrderUserAuthorizationDb
                {
                    GuidIdSupplyOrderUser = newGuidIdSupplyOrder,
                    GuidIdCollaborator = model.guidIdCollaborator
                };
                await _db.OrderUserAuthorization.AddAsync(userAccess);

                // Доступ администратору
                var adminUserAccess = new OrderUserAuthorizationDb
                {
                    GuidIdSupplyOrderUser = newGuidIdSupplyOrder,
                    GuidIdCollaborator = AdminCollaboratorGuid
                };
                await _db.OrderUserAuthorization.AddAsync(adminUserAccess);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                // Отправка письма
                // var emailBody = $"Создан заказ: {newGuidIdSupplyOrder}\n" +
                //                 $"Заказчик: {""} ({FullName})\n" +
                //                 $"Спецификация: {model.purchaseName} на сумму {model.purchasePrice}₽";
                // await _emailSender.SendEmail(AdminEmail, "Запрос счета", emailBody);

                // _logger.LogInformation("Создан заказ {OrderId} пользователем {UserId}", newGuidIdSupplyOrder, user.GuidIdCollaborator);

                return Ok(new
                {
                    message = "Заказ успешно создан",
                    orderId = newGuidIdSupplyOrder
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка при создании заказа");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }
    }
}
