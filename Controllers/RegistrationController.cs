using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Text;

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
            using (CollaboratorSystemContext db = new CollaboratorSystemContext())
            {
                var user = db.CollaboratorSystem.FromSqlRaw("SELECT * FROM userSystems")
                                        .Where(p => p.EmailCollaborator == model.Email)
                                        .FirstOrDefault();

                if (user != null)
                {
                    return Ok(new { message = "Данный пользователь существует" });
                }

                var guidId = Guid.NewGuid();
                var token = _tokenService.GenerateToken(model.Email, "User");

                // Записываем пользователя в базу данных с полем IsConfirmed = false
                db.Database.ExecuteSqlRaw(
                    "INSERT INTO userSystems (Email, Password, GuidId, Token, DataReg, IsAdmin, IsConfirmed) VALUES({0}, {1}, {2}, {3}, {4}, {5}, {6})",
                    model.Email, model.Password, guidId, token, DateTime.Now, false, false
                );

                db.SaveChanges();

                // Отправка email с подтверждением
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

    public record RegistrationModel(string Email, string Password);
}
