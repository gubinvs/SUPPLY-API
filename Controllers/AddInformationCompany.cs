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
            // Проверка: ИНН должен быть из 10 или 12 цифр
            string inn = model.InnCompany.ToString();
            if (!(inn.Length == 10 || inn.Length == 12) || !inn.All(char.IsDigit))
            {
                return BadRequest(new { message = "ИНН должен содержать 10 или 12 цифр." });
            }

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
                    // Подключаемся к другой базе данных, там где хранятся привязки пользователя к компании

                    // Если администратор привязку делать не нужно
                    if (model.GuidIdCollaborator != "12bc291f-fe13-41f6-ba03-c69c2f1d3a6e")
                    {
                        using (var _dbCompanyCollaborator = new CompanyCollaboratorContext())
                        {
                            // Формируем запрос в базу данных для записи данных
                            _dbCompanyCollaborator.Database.ExecuteSqlRaw(
                                "INSERT INTO `CompanyCollaborator` (GuidIdCollaborator, GuidIdCompany) VALUES({0}, {1})",
                                model.GuidIdCollaborator, GuidIdCompany
                            );

                            // Записали в базу данных
                            _dbCompanyCollaborator.SaveChanges();
                        }
                    }

                    // Теперь проверим данная компания является поставщиком или нет, 
                    // если да то нужно добавить данние еще и в таблицу SupplyProvider
                    if (model.RoleCompany?.Trim() == "a5219e2b-12f3-490e-99f5-1be54c55cc6d")
                    {
                        using (var _SupplyProvider = new SupplyProviderContext())
                        {
                            // Проверка по ИНН
                            var existing = _SupplyProvider.SupplyProvider
                                .FirstOrDefault(p => p.InnProvider == Convert.ToString(model.InnCompany));

                            if (existing == null)
                            {
                                var newProvider = new ProviderDb(
                                    GuidIdCompany,
                                    model.AbbreviatedNameCompany,
                                    Convert.ToString(model.InnCompany) // уже строка, как нужно
                                );

                                _SupplyProvider.SupplyProvider.Add(newProvider);
                                _SupplyProvider.SaveChanges();
                            }
                            else
                            {
                                // Лог или предупреждение — поставщик с таким ИНН уже есть
                                Console.WriteLine($"Компания с ИНН {model.InnCompany} уже зарегистрирована как поставщик.");
                            }
                        }
                    }


                    // Возвращаем ответ
                    return Ok(new { message = "Данные о новой компании внесены в базу." });

                }

                // Теперь если компания уже есть в базе
                // Заменяем данные новыми
                
                _db.Database.ExecuteSqlRaw(
                        "UPDATE SupplyCompany SET FullNameCompany = {0}, AbbreviatedNameCompany = {1}, InnCompany = {2}, AddressCompany = {3} WHERE GuidIdCompany = {4}",
                        model.FullNameCompany, model.AbbreviatedNameCompany, model.InnCompany, model.AddressCompany, model.GuidIdCompany
                    );

                // Сохраняем изменения (необязательно после ExecuteSqlRaw, но пусть будет для единообразия)
                _db.SaveChanges();

                // Снова подключаемся к базе данных, в которой хранятся привязки пользователя к компании
                using (var _dbCompanyCollaborator = new CompanyCollaboratorContext())
                {

                    // И проверяем существование данной зависимости между пользователем и компанией
                    var relationship = _dbCompanyCollaborator.CompanyCollaborator
                        .Where(p => p.GuidIdCollaborator == model.GuidIdCollaborator && p.GuidIdCompany == model.GuidIdCompany)
                        .FirstOrDefault();

                    //если такой зависимости нет, создаем ее
                    if (relationship == null)
                    {
                        // Формируем запрос в базу данных для записи данных
                        _dbCompanyCollaborator.Database.ExecuteSqlRaw(
                            "INSERT INTO `CompanyCollaborator` (GuidIdCollaborator, GuidIdCompany) VALUES({0}, {1})",
                            model.GuidIdCollaborator, GuidIdCompany
                        );

                        // Записали в базу данных
                        _dbCompanyCollaborator.SaveChanges();
 
                        // Возвращаем ответ
                        return Ok(new { message = "Зависимость пользователя и компании установлена." });

                    }
                    
                    // Возвращаем ответ
                    return Ok(new { message = "Зависимость пользователя и компании существует." });    
                }
            }
        }
    }
}