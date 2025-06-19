using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;




// Контроллер принимает данный методом пост по модели и заполняет базу данных новой компанией поставщиком
namespace SUPPLY_API
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddCompanyProviderController : ControllerBase
    {
        private readonly ILogger<AddCompanyProviderController> _logger;
        private readonly SupplyProviderContext _db;

        public AddCompanyProviderController(ILogger<AddCompanyProviderController> logger, SupplyProviderContext db)
        {
            _logger = logger;
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> AddSupplyProvider([FromBody] SupplyProviderModel model)
        {
            try
            {
                // Проверка: существует ли компания поставщик с таким-же инн
                var existing = await _db.SupplyProvider
                    .AnyAsync(c => c.InnProvider == model.InnCompany.ToString());

                if (existing)
                {
                    return Conflict(new { message = "Компания с таким ИНН уже существует." });
                }

                // Создание нового поставщика
                var newProvider = new ProviderDb
                {
                    GuidIdProvider = Guid.NewGuid().ToString(),
                    NameProvider = model.AbbreviatedNameCompany,
                    InnProvider = model.InnCompany.ToString()
                };

                _db.Add(newProvider);
                await _db.SaveChangesAsync();

                return Ok(new { message = "Поставщик успешно добавлен", id = newProvider.GuidIdProvider });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении компонента: {InnProvider}", model.InnCompany.ToString());
                return StatusCode(500, new { message = "Произошла ошибка при обработке запроса." });
            }
        }
    }
}