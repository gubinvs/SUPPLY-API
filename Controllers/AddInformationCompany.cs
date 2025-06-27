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

        private readonly ILogger<AddInformationCompanyController> _logger;

        private readonly SupplyCompanyContext _dbCompany;

        private readonly CompanyCollaboratorContext _dbCollaborator;

        private readonly SupplyProviderContext _dbProvider;

        public AddInformationCompanyController(
            ILogger<AddInformationCompanyController> logger,
            SupplyCompanyContext dbCompany,
            CompanyCollaboratorContext dbCollaborator,
            SupplyProviderContext dbProvider

        )
        {
            _logger = logger;
            _dbCompany = dbCompany;
            _dbCollaborator = dbCollaborator;
            _dbProvider = dbProvider;
        }


        [HttpPost]
        public IActionResult AddInformationCompany([FromBody] InformationCompanyModel model)
        {
            string inn = model.InnCompany.ToString() ?? "";

            if (!(inn.Length == 10 || inn.Length == 12) || !inn.All(char.IsDigit))
            {
                return BadRequest(new { message = "ИНН должен содержать 10 или 12 цифр." });
            }

            string guidIdCompany = Guid.NewGuid().ToString();

            
            var company = _dbCompany.SupplyCompany.FirstOrDefault(p => p.InnCompany == model.InnCompany);

            if (company == null)
            {
                _dbCompany.Database.ExecuteSqlRaw(
                    "INSERT INTO SupplyCompany (GuidIdCompany, FullNameCompany, AbbreviatedNameCompany, InnCompany, AddressCompany) VALUES({0}, {1}, {2}, {3}, {4})",
                    guidIdCompany, model.FullNameCompany, model.AbbreviatedNameCompany, model.InnCompany, model.AddressCompany);

                _dbCompany.SaveChanges();

                // Привязка к пользователю (кроме администратора)
                if (model.GuidIdCollaborator != "12bc291f-fe13-41f6-ba03-c69c2f1d3a6e")
                {
                    
                    _dbCollaborator.Database.ExecuteSqlRaw(
                        "INSERT INTO `CompanyCollaborator` (GuidIdCollaborator, GuidIdCompany) VALUES({0}, {1})",
                        model.GuidIdCollaborator, guidIdCompany);
                    _dbCollaborator.SaveChanges();
                    
                }

                // Проверка роли на "поставщик"
                if (model.RoleCompany?.Trim() == "a5219e2b-12f3-490e-99f5-1be54c55cc6d")
                {
                   
                    var existingProvider = _dbProvider.SupplyProvider.FirstOrDefault(p => p.InnProvider == inn);
                    if (existingProvider == null)
                    {
                        var newProvider = new ProviderDb(guidIdCompany, model.AbbreviatedNameCompany, inn);
                        _dbProvider.SupplyProvider.Add(newProvider);
                        _dbProvider.SaveChanges();
                    }
                   
                }

                return Ok(new { message = "Данные о новой компании внесены в базу." });
            }
            else
            {
                // Обновление существующей компании
                if (model.GuidIdCompany != null && company.GuidIdCompany != null)
                {
                    guidIdCompany = model.GuidIdCompany ?? company.GuidIdCompany;
                }
                else
                { 
                    guidIdCompany = Guid.NewGuid().ToString();
                }
                

                _dbCompany.Database.ExecuteSqlRaw(
                    "UPDATE SupplyCompany SET FullNameCompany = {0}, AbbreviatedNameCompany = {1}, InnCompany = {2}, AddressCompany = {3} WHERE GuidIdCompany = {4}",
                    model.FullNameCompany, model.AbbreviatedNameCompany, model.InnCompany, model.AddressCompany, guidIdCompany);
                _dbCompany.SaveChanges();
            }
            

            // Обработка привязки пользователя к компании
           
                var relationship = _dbCollaborator.CompanyCollaborator
                    .FirstOrDefault(p => p.GuidIdCollaborator == model.GuidIdCollaborator && p.GuidIdCompany == guidIdCompany);

                if (relationship == null)
                {
                    _dbCollaborator.Database.ExecuteSqlRaw(
                        "INSERT INTO `CompanyCollaborator` (GuidIdCollaborator, GuidIdCompany) VALUES({0}, {1})",
                        model.GuidIdCollaborator, guidIdCompany);
                    _dbCollaborator.SaveChanges();

                    return Ok(new { message = "Компания обновлена и связь установлена." });
                }

                return Ok(new { message = "Компания обновлена. Связь уже существует." });
            
        }
    }
}
