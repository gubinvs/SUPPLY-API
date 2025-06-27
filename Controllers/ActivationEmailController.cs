using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace SUPPLY_API
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActivationEmailController : ControllerBase
    {

        private readonly ILogger<ActivationEmailController> _logger;
        private readonly CollaboratorSystemContext _db;
        private readonly EmailSender _emailSender;
        private readonly CurrentServer _serverAddresses;


        public ActivationEmailController(
            ILogger<ActivationEmailController> logger,
            CollaboratorSystemContext db,
            EmailSender emailSender,
            IOptions<CurrentServer> serverAddresses)
            
        {
            _logger = logger;
            _db = db;
            _emailSender = emailSender;
            _serverAddresses = serverAddresses.Value;
        }


        [HttpPost]
        public IActionResult ResendActivationEmail([FromBody] ActivationEmailModel model)
        {
            
            
                var user = _db.CollaboratorSystem.FirstOrDefault(p => p.EmailCollaborator == model.Email);

                if (user == null)
                {
                    return Ok(new { message = "Данный пользователь не зарегистрирован." });
                }

                if (user.ActivationEmailCollaborator)
                {
                    return Ok(new { message = "Email уже подтвержден." });
                }

                // Используем уже существующий GUID или создаём новый
                if (string.IsNullOrWhiteSpace(user.GuidIdCollaborator))
                {
                    user.GuidIdCollaborator = Guid.NewGuid().ToString();
                    _db.SaveChanges();
                }

                if (string.IsNullOrWhiteSpace(user.EmailCollaborator))
{
                    return BadRequest(new { message = "Email пользователя отсутствует." });
                }

                // Отправка email с подтверждением
                SendConfirmationEmail(user.EmailCollaborator, user.GuidIdCollaborator);

                return Ok(new { message = "Проверьте ваш email для подтверждения регистрации." });
            }
        

        [HttpGet("confirm/{guid}")]
        public IActionResult ConfirmEmail(string guid)
        {
    
            
                var user = _db.CollaboratorSystem.FirstOrDefault(p => p.GuidIdCollaborator == guid);

                if (user == null)
                {
                    return BadRequest(new { message = "Пользователь не найден." });
                }

                user.ActivationEmailCollaborator = true;
                user.GuidIdCollaborator = null; // обнуляем GUID после подтверждения
                _db.SaveChanges();

                // return Ok(new { message = "Email подтверждён успешно!" });
                // Перенаправляем на страницу успеха
                return Redirect($"{_serverAddresses.ServerAddressFrontend}");
            
        }

        private void SendConfirmationEmail(string email, string guid)
        {
            var confirmationLink = $"{_serverAddresses.ServerAddressApi}/api/activationemail/confirm/{guid}";
            var subject = "Подтверждение регистрации";
            var body = $"Пожалуйста, подтвердите вашу регистрацию, перейдя по ссылке:\n\n{confirmationLink}";

            _emailSender.SendEmail(email, subject, body);

        }
    }

    public record ActivationEmailModel(string Email);
}
