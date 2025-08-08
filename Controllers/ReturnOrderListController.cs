using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    /// <summary>
    /// Контроллер принимает идентификатор пользователя, проверяет взаимосвязи в таблице "OrderUserAuthorization"
    /// берет список идентификаторов доступных для данного пользователя и формирует массив данных о заказах из таблицы "SupplyOrderUser", 
    /// а список комплектующих из таблицы "SupplyOrderUserComponent" формирует массив данных и возвращает его.
    /// если GuidIdCollaborator == "b5aff5b0-c3ac-4f1e-9467-fe13a14f6de3", тоесть администратор, то возвращает весь объем данных
    /// 
    /// </summary>
    /// 
    [ApiController]
    [Route("api/[controller]/{guidId}")]
    public class ReturnOrderListController : ControllerBase
    {
        private readonly SupplyContext _context;
        private readonly string AdminGuid = "b5aff5b0-c3ac-4f1e-9467-fe13a14f6de3";

        public ReturnOrderListController(SupplyContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders(string guidId)
        {
            List<SupplyOrderUserDb> orders;

            if (guidId == AdminGuid)
            {
                orders = await _context.SupplyOrderUser.ToListAsync();
            }
            else
            {
                var authorizedOrderIds = await _context.OrderUserAuthorization
                    .Where(a => a.GuidIdCollaborator == guidId)
                    .Select(a => a.GuidIdSupplyOrderUser)
                    .ToListAsync();

                orders = await _context.SupplyOrderUser
                    .Where(o => authorizedOrderIds.Contains(o.GuidIdSupplyOrder))
                    .ToListAsync();
            }

            var orderIds = orders.Select(o => o.GuidIdSupplyOrder).ToList();

            var components = await _context.SupplyOrderUserComponent
                .Where(c => orderIds.Contains(c.GuidIdSupplyOrderUser))
                .ToListAsync();

            var result = orders.Select(order => new OrderDto
            {
                GuidIdSupplyOrder = order.GuidIdSupplyOrder,
                GuidIdPurchase = order.GuidIdPurchase,
                PurchaseId = order.PurchaseId ?? string.Empty,
                PurchaseName = order.PurchaseName ?? string.Empty,
                PurchasePrice = order.PurchasePrice ?? 0,
                PurchaseCustomer = order.PurchaseCostomer ?? string.Empty,
                SupplyOrderUserStatus = order.SupplyOrderUserStatus ?? string.Empty,
                OrderComponent = components
                    .Where(c => c.GuidIdSupplyOrderUser == order.GuidIdSupplyOrder)
                    .Select(c => new OrderComponentDto
                    {
                        VendorCodeComponent = c.VendorCodeComponent ?? string.Empty,
                        NameComponent = c.NameComponent ?? string.Empty,
                        QuantityComponent = c.QuantityComponent ?? 0,
                        PriceComponent = c.PriceComponent ?? 0,
                        DeliveryTimeComponent = c.DeliveryTimeComponent == default
                                ? string.Empty
                                : c.DeliveryTimeComponent.ToString("dd-MM-yyyy")
                    })
                    .ToList()
            }).ToList();

            return Ok(result);
        }
    }

}