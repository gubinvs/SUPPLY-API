using Microsoft.AspNetCore.Mvc;

namespace SUPPLY_API
{
    /// <summary>
    /// Контроллер для добавления информации о новой компании в базу данных
    /// </summary>
    /// 

    [Controller]
    [Route("api/[controller]")]
    public class AddInformationCompanyController : ControllerBase
    {
        public IActionResult AddInformationCompany([FromBody] AddInformationCompanyModel model)
        {
            return Ok();

        }

    }
    
    public record AddInformationCompanyModel(string Email, string Password, string GuidIdRoleSystem);
}