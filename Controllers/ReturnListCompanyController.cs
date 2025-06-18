using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace SUPPLY_API.Controllers
{

    /// <summary>
    /// Контроллер возвращает на запрос данные о всех компаниях (поставщиках) записанных в базе данных
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]

    public class ReturnListCompanyController : ControllerBase
    {
        private readonly ILogger<AddComponentController> _logger;

        // База данных с информацией о поставщиках
        private readonly SupplyCompanyContext _SupplyCompany;

        public ReturnListCompanyController(
                ILogger<AddComponentController> logger,
                SupplyCompanyContext SupplyCompany
            )
        {
            _logger = logger;
            _SupplyCompany = SupplyCompany;
        }

        [HttpGet]
        public async Task<IActionResult> ReturnListProvider()
        {
            // Достали данные о компаниях в базе
            var company = await _SupplyCompany.SupplyCompany
                .ToListAsync();

            return Ok(new
            { 
                company
            });
        }
        
    };

};