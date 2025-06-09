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
                // Поиск пользователя
                var user = db.CollaboratorSystem.FirstOrDefault(p => p.GuidIdCollaborator == guidId);

                if (user == null)
                {
                    return Ok(new { message = "Пользователь не найден." });
                }

                // Вынесем переменную наружу
                List<string?> companyGuids;

                using (var dbCompanyCollaborator = new CompanyCollaboratorContext())
                {
                    companyGuids = dbCompanyCollaborator.CompanyCollaborator
                        .FromSqlRaw("SELECT * FROM CompanyCollaborator WHERE GuidIdCollaborator = {0}", user.GuidIdCollaborator)
                        .Select(p => p.GuidIdCompany)
                        .ToList();
                }

                return Ok(new
                {
                    message = "Данные о пользователе и компании",
                    user,
                    companyGuids
                });
            }
        }
    }
}
