using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace SUPPLY_API.Controllers
{

    /// <summary>
    /// Контроллер возвращает на запрос данные о всех компаниях (поставщиках) записанных в базе данных
    /// </summary>
    [ApiController]
    [Route("api")]

    public class ReturnListProviderController : ControllerBase
    {
        private readonly ILogger<AddComponentController> _logger;

        // База данных с информацией о поставщиках
        private readonly SupplyProviderContext _dbProvider;

        public ReturnListProviderController(
            ILogger<AddComponentController> logger,
            SupplyProviderContext dbProvider

            )
        {
            _logger = logger;
            _dbProvider = dbProvider;
        }

        [HttpGet]
        public async Task<IActionResult> ReturnListProvider()
        {
            // Достали данные о компаниях в базе
            var providers = await _dbProvider.SupplyProvider
                .ToListAsync();

            return Ok(new
            { 
                providers
            });
        }
        
    };

};