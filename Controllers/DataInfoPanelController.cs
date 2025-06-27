using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace SUPPLY_API
{
    [ApiController]
    [Route("api/[controller]/{guidId}")]
    public class DataInfoPanelController : Controller
    {

        private readonly ILogger<DataInfoPanelController> _logger;
        private readonly CollaboratorSystemContext _dbCollab;
        private readonly DeliveryAddressContext _dbDelivery;
        private readonly CompanyCollaboratorContext _dbCompany;
        private readonly SupplyCompanyContext _dbSupply;

        public DataInfoPanelController(
            ILogger<DataInfoPanelController> logger,
            CollaboratorSystemContext dbCollab,
            DeliveryAddressContext dbDelivery,
            CompanyCollaboratorContext dbCompany,
            SupplyCompanyContext dbSupply

            )
        {
            _logger = logger;
            _dbCollab = dbCollab;
            _dbDelivery = dbDelivery;
            _dbCompany = dbCompany;
            _dbSupply = dbSupply;
        }

        [HttpGet]
        public IActionResult DataInfoPanel(string guidId)
        {
            
            // Поиск пользователя и сбор данных о нем
            var user = _dbCollab.CollaboratorSystem
                .Where(p => p.GuidIdCollaborator == guidId)
                .Select(p => new
                {
                    p.GuidIdCollaborator,
                    p.GuidIdRoleSystem,
                    p.NameCollaborator,
                    p.EmailCollaborator,
                    p.PhoneCollaborator,
                    p.DataRegistrationCollaborator

                })
                .FirstOrDefault();

            if (user == null)
            {
                return Ok(new { message = "Пользователь не найден." });
            }


            // Соберем адреса доставки пользователя
            List<string?> deliveryAddress;
            deliveryAddress = _dbDelivery.DeliveryAddress
                .Where(p => p.GuidIdCollaboratorSystem == user.GuidIdCollaborator)
                .Select(p => p.DeliveryAddress)
                .ToList();                 


            // Соберем список GuidIdCompany, к которым относится данный пользователь
            List<string?> companyGuids;       
            if (string.IsNullOrEmpty(user.GuidIdCollaborator))
            {
                return BadRequest(new { message = "Некорректный GuidIdCollaborator у пользователя." });
            }

            companyGuids = _dbCompany.CompanyCollaborator
                .Where(p => p.GuidIdCollaborator == user.GuidIdCollaborator)
                .Select(p => p.GuidIdCompany)
                .Where(g => g != null)
                .ToList();
                

            // Теперь соберем данные о компаниях которые представляет пользователь
            List<SupplyCompanyDb> companyInfo; 
            companyInfo = _dbSupply.SupplyCompany
                .Where(p => companyGuids.Contains(p.GuidIdCompany))
                .ToList();
                
            return Ok(new
            {
                message = "Данные о пользователе и компании",
                user,
                deliveryAddress,
                companyGuids,
                companyInfo
            });
            
        }
    }
}
