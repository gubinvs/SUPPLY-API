using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    /// <summary>
    /// Контроллер для добавления в базу данных или изменения информации о новой компании
    /// </summary>

    [Controller]
    [Route("api/[controller]")]
    public class AddInformationCompanyController : ControllerBase
    {
        [HttpPost]
        public IActionResult AddInformationCompany([FromBody] InformationCompanyModel model)
        {
            // Сгенерировали новый идентификатор компании
            string GuidIdCompany = Guid.NewGuid().ToString();

            // Подключаемся к базе данных
            using (var _db = new SupplyCompanyContext())
            {
                // Проверяем информацию в базе данных о наличии записи с таким ИНН (вместо GuidIdCompany)
                var company = _db.SupplyCompany
                        .Where(p => p.InnCompany == model.InnCompany)
                        .FirstOrDefault();

                // Если компании нет в базе данных, то записываем данные
                if (company == null)
                {
                    // Сформировали запрос в базу данных о данных компании
                    _db.Database.ExecuteSqlRaw(
                        "INSERT INTO SupplyCompany (GuidIdCompany, FullNameCompany, AbbreviatedNameCompany, InnCompany, AddressCompany) VALUES({0}, {1}, {2}, {3}, {4})",
                        GuidIdCompany, model.FullNameCompany, model.AbbreviatedNameCompany, model.InnCompany, model.AddressCompany
                    );

                    // Записали в базу данных
                    _db.SaveChanges();

                    // Теперь нужно идентифицировать новую компанию с пользователем системы
                    // Для этого берём данные о новом (GuidIdCompany) и (GuidIdCollaborator) — то что пришло к нам из формы запроса.
                    // Пользователь, который создавал эту компанию

                    // Подключаемся к новой базе данных
                    using (var _dbCompanyCollaborator = new CompanyCollaboratorContext())
                    {
                        // Формируем запрос в базу данных
                        _dbCompanyCollaborator.Database.ExecuteSqlRaw(
                            "INSERT INTO `CompanyCollaborator` (GuidIdCollaborator, GuidIdCompany) VALUES({0}, {1})",
                            model.GuidIdCollaborator, GuidIdCompany
                        );

                        // Записали в базу данных
                        _dbCompanyCollaborator.SaveChanges();
                    }

                    // Возвращаем ответ
                    return Ok(new { message = "Данные о новой компании внесены в базу." });

                }




                // Теперь если компания уже есть в базе



                // Возвращаем ответ
                return Ok(new { message = "Данные о компании изменены в базу." });
            }
        }
    }
}
