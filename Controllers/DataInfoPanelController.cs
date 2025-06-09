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
