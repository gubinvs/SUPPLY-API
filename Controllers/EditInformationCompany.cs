using Microsoft.AspNetCore.Mvc;

namespace SUPPLY_API
{
    /// <summary>
    /// Контроллер для изменения информации о компании
    /// </summary>
    /// 

    [Controller]
    [Route("api/[controller]")]
    public class EditInformationCompanyController : ControllerBase
    {
        public IActionResult EditInformationCompany([FromBody] EditInformationCompanyModel model)
        {
            return Ok();
        }

    }
    
    public record EditInformationCompanyModel(string Email, string Password, string GuidIdRoleSystem);
}