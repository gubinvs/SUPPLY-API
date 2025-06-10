using System.Data.Common;
using Microsoft.AspNetCore.Mvc;

namespace SUPPLY_API
{
    /// <summary>
    /// Контроллер для добавления в базу данных или изменения информации о новой компании
    /// </summary>
    /// 

    [Controller]
    [Route("api/[controller]")]
    public class AddInformationCompanyController : ControllerBase
    {
        public IActionResult AddInformationCompany([FromBody] InformationCompanyModel model)
        {
            //Подключаемся к базе данных
            using (var _db = new SupplyCompanyContext())
            {
                // Проверяем информацию в базе данных о наличии записи с GuidIdCompany
                var company = _db.SupplyCompany
                        .Where(p => p.GuidIdCompany == model.GuidIdCompany);

                return Ok(company);
            }

            
            
        }

    }
}