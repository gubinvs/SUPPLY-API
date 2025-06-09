using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace SUPPLY_API
{
    [ApiController]
    [Route("api/[controller]/{guidId}")]
    public class DataInfoPanelController : Controller
    {
        [HttpGet]
        public IActionResult DataInfoPanel(string guidId)
        {
            using (var db = new CollaboratorSystemContext())
            {
                // Поиск пользователя и сбор данных о нем
                var user = db.CollaboratorSystem
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

                using (var _dbDeliveryAddress = new DeliveryAddressContext())
                { 
                    deliveryAddress = _dbDeliveryAddress.DeliveryAddress
                        .Where(p => p.GuidIdCollaboratorSystem == user.GuidIdCollaborator)
                        .Select(p => p.DeliveryAddress)
                        .ToList();                 

                }


                // Соберем список GuidIdCompany, к которым относится данный пользователь
                List<string?> companyGuids;
                
                using (var dbCompanyCollaborator = new CompanyCollaboratorContext())
                {
                    if (string.IsNullOrEmpty(user.GuidIdCollaborator))
                    {
                        return BadRequest(new { message = "Некорректный GuidIdCollaborator у пользователя." });
                    }

                    companyGuids = dbCompanyCollaborator.CompanyCollaborator
                        .Where(p => p.GuidIdCollaborator == user.GuidIdCollaborator)
                        .Select(p => p.GuidIdCompany)
                        .Where(g => g != null)
                        .ToList();
                }

                // Теперь соберем данные о компаниях которые представляет пользователь
                List<SupplyCompanyDb> companyInfo;

                using (var dbCompany = new SupplyCompanyContext())
                {
                    companyInfo = dbCompany.SupplyCompany
                        .Where(p => companyGuids.Contains(p.GuidIdCompany))
                        .ToList();
                }



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
}
