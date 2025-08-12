using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{

    // Контроллер принимает данные о статусе счета и меняет статус в базе данных

    [ApiController]
    [Route("api/[controller]")]
    public class EditInvoiceController : ControllerBase
    {
        private readonly ILogger<EditInvoiceController> _logger;
        private readonly SupplyContext _db;
        private readonly EmailSender _emailSender;


        public EditInvoiceController(
                ILogger<EditInvoiceController> logger,
                SupplyContext db,
                EmailSender emailSender
            )
        {
            _logger = logger;
            _db = db;
            _emailSender = emailSender;
        }


       [HttpPost]
        public async Task<IActionResult> EditInvoice([FromBody] EditInvoiceModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var order = await _db.SupplyOrderUser
                .FirstOrDefaultAsync(u => u.GuidIdSupplyOrder == model.guidIdSupplyOrder);

            if (order == null)
                return NotFound(new { message = "Нет такого заказа б базе данных" });

            try
            {
                // Меняем статус
                order.SupplyOrderUserStatus = model.supplyOrderUserStatus;



                // Обновляем запись
                _db.SupplyOrderUser.Update(order);

                await _db.SaveChangesAsync();


                return Ok(new
                {
                    message = "Статус заказа успешно изменен"
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