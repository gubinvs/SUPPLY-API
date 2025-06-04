using Microsoft.AspNetCore.Mvc;

namespace SUPPLY_API
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActivationEmailController : ControllerBase
    {
        [HttpPost]
        public IActionResult ResendActivationEmail([FromBody] ActivationEmailModel model)
        {
            using (var db = new CollaboratorSystemContext())
            {
                var user = db.CollaboratorSystem.FirstOrDefault(p => p.EmailCollaborator == model.Email);

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
                    db.SaveChanges();
                }

                if (string.IsNullOrWhiteSpace(user.EmailCollaborator))
{
                    return BadRequest(new { message = "Email пользователя отсутствует." });
                }

                // Отправка email с подтверждением
                SendConfirmationEmail(user.EmailCollaborator, user.GuidIdCollaborator);

                return Ok(new { message = "Проверьте ваш email для подтверждения регистрации." });
            }
        }

        [HttpGet("confirm/{guid}")]
        public IActionResult ConfirmEmail(string guid)
        {
            using (var db = new CollaboratorSystemContext())
            {
                var user = db.CollaboratorSystem.FirstOrDefault(p => p.GuidIdCollaborator == guid);

                if (user == null)
                {
                    return BadRequest(new { message = "Пользователь не найден." });
                }

                user.ActivationEmailCollaborator = true;
                user.GuidIdCollaborator = null; // обнуляем GUID после подтверждения
                db.SaveChanges();

                return Ok(new { message = "Email подтверждён успешно!" });
            }
        }

        private void SendConfirmationEmail(string email, string guid)
        {
            var confirmationLink = $"{CurrentServer.ServerAddress}/api/activationemail/confirm/{guid}";
            var subject = "Подтверждение регистрации";
            var body = $"Пожалуйста, подтвердите вашу регистрацию, перейдя по ссылке:\n\n{confirmationLink}";

            var emailSender = new EmailSender();
            emailSender.SendEmail(email, subject, body);
        }
    }

    public record ActivationEmailModel(string Email);
}
