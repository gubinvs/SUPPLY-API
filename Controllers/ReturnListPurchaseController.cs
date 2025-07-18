


using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    /// <summary>
    /// Контроллер возвращает на запрос данные о всех закупках доступных данному пользователю
    /// </summary>
    /// 
    [ApiController]
    [Route("api/[controller]/{guidIdCollaborator}")]
    public class ReturnListPurchaseController : ControllerBase
    {

        private readonly ILogger<ReturnListPurchaseController> _logger;

        // База данных с информацией о поставщиках
        private readonly SupplyContext _db;

        public ReturnListPurchaseController (
                ILogger<ReturnListPurchaseController> logger,
                SupplyContext db
            )
        {
            _logger = logger;
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> ReturnListPurchase(string guidIdCollaborator)
        {
            // Достали данные о доступных закупках данному польз
            var myPurchases = await _db.PurchaseAuthorization
                .Where(c => c.GuidIdCollaborator == guidIdCollaborator)
                .ToListAsync();

            if (myPurchases != null)
            { 
                
            }

            return Ok(new
            {
                myPurchases
            });
        }
    }
}