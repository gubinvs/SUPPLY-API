using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    /// <summary>
    /// Контроллер для добавления данных о менеджере компании поставщика
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PushNewDataCollaboratorProviderController : ControllerBase
    {
        private readonly ILogger<PushNewDataCollaboratorProviderController> _logger;
        private readonly SupplyContext _dbCollaboratorProvider;

        public PushNewDataCollaboratorProviderController(
            ILogger<PushNewDataCollaboratorProviderController> logger,
            SupplyContext dbCollaboratorProvider)
        {
            _logger = logger;
            _dbCollaboratorProvider = dbCollaboratorProvider;
        }

        [HttpPost]
        public async Task<IActionResult> PushNewDataCollaboratorProvider([FromBody] NewDataCollaboratorModel model)
        {
            if (model == null)
            {
                return BadRequest("Данные не переданы.");
            }

            try
            {
                string newCollaboratorProviderId = Guid.NewGuid().ToString();

                // Используем ваш реальный класс CollaboratorProviderDb
                var collaboratorProvider = new CollaboratorProviderDb
                {
                    GuidIdCollaboratorProvider = newCollaboratorProviderId,
                    GuidIdCompanyProvider = model.GuidIdCompanyProvider,
                    NameCollaboratorProvider = model.NameCollaboratorProvider, 
                    PhoneCollaboratorProvider = model.PhoneCollaboratorProvider,
                    EmailCollaboratorProvider = model.EmailCollaboratorProvider
                };

                // Добавление в БД. Замените CollaboratorProviders на имя вашего DbSet в SupplyContext
                await _dbCollaboratorProvider.CollaboratorProvider.AddAsync(collaboratorProvider);
                await _dbCollaboratorProvider.SaveChangesAsync();

                return Ok(new 
                { 
                    message = "Данные успешно добавлены", 
                    guidIdCollaboratorProvider = newCollaboratorProviderId 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении данных менеджера поставщика");
                return StatusCode(500, "Внутренняя ошибка сервера при записи в БД");
            }
        }
    }
}