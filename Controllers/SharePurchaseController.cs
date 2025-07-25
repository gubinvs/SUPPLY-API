
#nullable enable

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    /// <summary>
    /// Прописываем зависимость в тублицу PurchaseAuthorization, где сопоставляем GuidIdCollaborator и GuidIdPurchase
    /// контроллер принимает модель, где GuidIdPurchase - это идентификатор закупки
    /// и логин пользователя, которому предоставляется доступ. (email)
    /// Контроллер проверяет в таблице CollaboratorSystem наличие такого логина в поле EmailCollaborator, если находит
    /// достает GuidIdCollaborator и записывает в таблицу PurchaseAuthorization зависимость GuidIdCollaborator и GuidIdPurchase
    /// </summary>
    /// <summary>
    /// Контроллер для предоставления доступа к закупке пользователю по email
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SharePurchaseController : ControllerBase
    {
        private readonly ILogger<SharePurchaseController> _logger;
        private readonly SupplyContext _db;

        public SharePurchaseController(ILogger<SharePurchaseController> logger, SupplyContext db)
        {
            _logger = logger;
            _db = db;
        }


        [HttpPost]
        public async Task<IActionResult> SharePurchase([FromBody] SharePurchaseModel model)
        {
            if (string.IsNullOrWhiteSpace(model.GuidIdPurchase) || string.IsNullOrWhiteSpace(model.EmailCollaborator))
            {
                return BadRequest(new { message = "GuidIdPurchase и EmailCollaborator обязательны." });
            }

            try
            {
                // Поиск сотрудника по email
                var collaborator = await _db.CollaboratorSystem
                    .FirstOrDefaultAsync(c => c.EmailCollaborator.ToLower() == model.EmailCollaborator.ToLower());

                if (collaborator == null)
                {
                    return NotFound(new { message = "Пользователь с таким email не найден." });
                }

                // Проверка существующей привязки
                var existingAuth = await _db.PurchaseAuthorization
                    .FirstOrDefaultAsync(auth =>
                        auth.GuidIdPurchase == model.GuidIdPurchase &&
                        auth.GuidIdCollaborator == collaborator.GuidIdCollaborator);

                if (existingAuth != null)
                {
                    return Conflict(new { message = "Пользователь уже имеет доступ к данной закупке." });
                }

                // Создание новой авторизации
                var newAuthorization = new PurchaseAuthorizationDb
                {
                    GuidIdPurchase = model.GuidIdPurchase,
                    GuidIdCollaborator = collaborator.GuidIdCollaborator
                };

                _db.PurchaseAuthorization.Add(newAuthorization);
                await _db.SaveChangesAsync();

                return Ok(new { message = "Доступ к закупке предоставлен." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при предоставлении доступа к закупке");
                return StatusCode(500, new { message = "Произошла ошибка при предоставлении доступа." });
            }
        }
    }
}
