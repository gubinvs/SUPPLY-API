using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API.Controllers
{
    /// <summary>
    /// Контроллер принимает GUID CollaboratorProvider идентификатор компании поставщика
    /// опрашивает базу данных CollaboratorProvider и возвращает список менеждеров компании поставщика
    /// </summary>
    /// 
    

    [ApiController]
    [Route("api/[controller]")]
    public class ReturnInfoCollaboratorProviderController : ControllerBase
    {
        private readonly ILogger<ReturnInfoCollaboratorProviderController> _logger;
        private readonly SupplyContext _db;

        public ReturnInfoCollaboratorProviderController (
            ILogger<ReturnInfoCollaboratorProviderController> logger,
            SupplyContext db
        )
        {
            _logger = logger;
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> ListInfoCollaboratorProvider(string guid)
        {

            var response = await _db.CollaboratorProvider
                    .Where(x => x.GuidIdCompanyProvider == guid)
                    .ToListAsync();
        
            
            return Ok(response);


        }
    }
}