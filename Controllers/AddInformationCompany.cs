using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    /// <summary>
    /// Контроллер для добавления или обновления информации о компании
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AddInformationCompanyController : ControllerBase
    {
        [HttpPost]
        public IActionResult AddInformationCompany([FromBody] InformationCompanyModel model)
        {
            string inn = model.InnCompany?.ToString() ?? "";

            if (!(inn.Length == 10 || inn.Length == 12) || !inn.All(char.IsDigit))
            {
                return BadRequest(new { message = "ИНН должен содержать 10 или 12 цифр." });
            }

            string guidIdCompany = Guid.NewGuid().ToString();

            using (var db = new SupplyCompanyContext())
            {
                var company = db.SupplyCompany.FirstOrDefault(p => p.InnCompany == model.InnCompany);

                if (company == null)
                {
                    db.Database.ExecuteSqlRaw(
                        "INSERT INTO SupplyCompany (GuidIdCompany, FullNameCompany, AbbreviatedNameCompany, InnCompany, AddressCompany) VALUES({0}, {1}, {2}, {3}, {4})",
                        guidIdCompany, model.FullNameCompany, model.AbbreviatedNameCompany, model.InnCompany, model.AddressCompany);

                    db.SaveChanges();

                    // Привязка к пользователю (кроме администратора)
                    if (model.GuidIdCollaborator != "12bc291f-fe13-41f6-ba03-c69c2f1d3a6e")
                    {
                        using (var companyCollabDb = new CompanyCollaboratorContext())
                        {
                            companyCollabDb.Database.ExecuteSqlRaw(
                                "INSERT INTO `CompanyCollaborator` (GuidIdCollaborator, GuidIdCompany) VALUES({0}, {1})",
                                model.GuidIdCollaborator, guidIdCompany);
                            companyCollabDb.SaveChanges();
                        }
                    }

                    // Проверка роли на "поставщик"
                    if (model.RoleCompany?.Trim() == "a5219e2b-12f3-490e-99f5-1be54c55cc6d")
                    {
                        using (var providerDb = new SupplyProviderContext())
                        {
                            var existingProvider = providerDb.SupplyProvider.FirstOrDefault(p => p.InnProvider == inn);
                            if (existingProvider == null)
                            {
                                var newProvider = new ProviderDb(guidIdCompany, model.AbbreviatedNameCompany, inn);
                                providerDb.SupplyProvider.Add(newProvider);
                                providerDb.SaveChanges();
                            }
                        }
                    }

                    return Ok(new { message = "Данные о новой компании внесены в базу." });
                }
                else
                {
                    // Обновление существующей компании
                    guidIdCompany = model.GuidIdCompany ?? company.GuidIdCompany;

                    db.Database.ExecuteSqlRaw(
                        "UPDATE SupplyCompany SET FullNameCompany = {0}, AbbreviatedNameCompany = {1}, InnCompany = {2}, AddressCompany = {3} WHERE GuidIdCompany = {4}",
                        model.FullNameCompany, model.AbbreviatedNameCompany, model.InnCompany, model.AddressCompany, guidIdCompany);
                    db.SaveChanges();
                }
            }

            // Обработка привязки пользователя к компании
            using (var collabDb = new CompanyCollaboratorContext())
            {
                var relationship = collabDb.CompanyCollaborator
                    .FirstOrDefault(p => p.GuidIdCollaborator == model.GuidIdCollaborator && p.GuidIdCompany == guidIdCompany);

                if (relationship == null)
                {
                    collabDb.Database.ExecuteSqlRaw(
                        "INSERT INTO `CompanyCollaborator` (GuidIdCollaborator, GuidIdCompany) VALUES({0}, {1})",
                        model.GuidIdCollaborator, guidIdCompany);
                    collabDb.SaveChanges();

                    return Ok(new { message = "Компания обновлена и связь установлена." });
                }

                return Ok(new { message = "Компания обновлена. Связь уже существует." });
            }
        }
    }
}
