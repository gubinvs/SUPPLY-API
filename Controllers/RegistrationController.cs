using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationController : ControllerBase
    {
        private readonly TokenService _tokenService;

        public RegistrationController()
        {
            _tokenService = new TokenService();
        }

        [HttpPost]
        public IActionResult Register([FromBody] RegistrationModel model)
        {
         // Простая проверка на недопустимые значения
            if (model.GuidIdRoleSystem == "string" || model.GuidIdRoleSystem == "Выбор роли")
            {
                return BadRequest(new { message = "Пожалуйста, выберите корректную роль." });
            }

            using (CollaboratorSystemContext db = new CollaboratorSystemContext())
            {
                var user = db.CollaboratorSystem.FromSqlRaw("SELECT * FROM CollaboratorSystem")
                                        .Where(p => p.EmailCollaborator == model.Email)
                                        .FirstOrDefault();

                if (user != null)
                {
                    return Ok(new { message = "Данный пользователь существует" });
                }

                var guidId = Guid.NewGuid();
                var token = _tokenService.GenerateToken(model.Email, "User");

                db.Database.ExecuteSqlRaw(
                    "INSERT INTO CollaboratorSystem (GuidIdCollaborator, GuidIdRoleSystem, EmailCollaborator, PasswordCollaborator, DataRegistrationCollaborator, ActivationEmailCollaborator) VALUES({0}, {1}, {2}, {3}, {4}, {5})",
                    guidId, model.GuidIdRoleSystem, model.Email, model.Password, DateTime.Now, false
                );

                db.SaveChanges();

                SendConfirmationEmail(model.Email, guidId);

                return Ok(new { message = "Регистрация прошла успешно! Проверьте ваш email для подтверждения регистрации." });
            }
        }


        // Метод подтверждения по ссылке
        [HttpGet("confirm/{guid}")]
        public IActionResult ConfirmEmail(Guid guid)
        {
            using (CollaboratorSystemContext db = new CollaboratorSystemContext())
            {
                var user = db.CollaboratorSystem.FirstOrDefault(p => p.GuidIdCollaborator == guid.ToString());

                if (user == null)
                {
                    return BadRequest(new { message = "Пользователь не найден" });
                }

                // Подтверждаем почту
                user.ActivationEmailCollaborator = true;

                db.SaveChanges();

                return Ok(new { message = "Email подтвержден успешно!" });
            }
        }

        private void SendConfirmationEmail(string email, Guid guid)
        {
            var confirmationLink = $"{CurrentServer.ServerAddress}/api/registration/confirm/{guid}";

            var subject = "Подтверждение регистрации";
            var body = $"Пожалуйста, подтвердите вашу регистрацию, перейдя по ссылке: {confirmationLink}";

            // Используем EmailSender для отправки письма
            var emailSender = new EmailSender();
            emailSender.SendEmail(email, subject, body);
        }
    }

    public record RegistrationModel(string Email, string Password, string GuidIdRoleSystem);
}
