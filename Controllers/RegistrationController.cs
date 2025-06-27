using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SUPPLY_API.Models;

namespace SUPPLY_API
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationController : ControllerBase
    {
        private readonly TokenService _tokenService;
        private readonly CollaboratorSystemContext _dbContext;
        private readonly EmailSender _emailSender;

        public RegistrationController(
            TokenService tokenService,
            CollaboratorSystemContext dbContext,
            EmailSender emailSender)
        {
            _tokenService = tokenService;
            _dbContext = dbContext;
            _emailSender = emailSender;
        }

        [HttpPost]
        public IActionResult Register([FromBody] RegistrationModel model)
        {
            if (model.GuidIdRoleSystem == "string" || model.GuidIdRoleSystem == "Выбор роли")
            {
                return BadRequest(new { message = "Пожалуйста, выберите корректную роль." });
            }

            var user = _dbContext.CollaboratorSystem
                .FromSqlRaw("SELECT * FROM CollaboratorSystem")
                .FirstOrDefault(p => p.EmailCollaborator == model.Email);

            if (user != null)
            {
                return Ok(new { message = "Данный пользователь существует" });
            }

            var guidId = Guid.NewGuid();
            var token = _tokenService.GenerateToken(model.Email, "User");

            _dbContext.Database.ExecuteSqlRaw(
                "INSERT INTO CollaboratorSystem (GuidIdCollaborator, GuidIdRoleSystem, EmailCollaborator, PasswordCollaborator, DataRegistrationCollaborator, ActivationEmailCollaborator) VALUES({0}, {1}, {2}, {3}, {4}, {5})",
                guidId, model.GuidIdRoleSystem, model.Email, model.Password, DateTime.Now, false
            );

            _dbContext.SaveChanges();

            SendConfirmationEmail(model.Email, guidId);

            return Ok(new { message = "Регистрация прошла успешно! Проверьте ваш email для подтверждения регистрации." });
        }

        [HttpGet("confirm/{guid}")]
        public IActionResult ConfirmEmail(Guid guid)
        {
            var user = _dbContext.CollaboratorSystem
                .FirstOrDefault(p => p.GuidIdCollaborator == guid.ToString());

            if (user == null)
            {
                return BadRequest(new { message = "Пользователь не найден" });
            }

            user.ActivationEmailCollaborator = true;

            _dbContext.SaveChanges();

            return Ok(new { message = "Email подтвержден успешно!" });
        }

        private void SendConfirmationEmail(string email, Guid guid)
        {
            var confirmationLink = $"{Request.Scheme}://{Request.Host}/api/registration/confirm/{guid}";


            var subject = "Подтверждение регистрации";
            var body = $"Пожалуйста, подтвердите вашу регистрацию, перейдя по ссылке: {confirmationLink}";

            _emailSender.SendEmail(email, subject, body);
        }
    }

    public record RegistrationModel(string Email, string Password, string GuidIdRoleSystem);
}
