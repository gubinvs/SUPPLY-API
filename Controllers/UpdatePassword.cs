using DocumentFormat.OpenXml.Office.CustomUI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
//using System.Web.Mvc;

namespace SUPPLY_API
{

    /// <summary>
    /// Контроллер принимает методом POST данные о email пользователе,
    /// - проверяет по базе данных наличие данного пользователя и если такой пользователь существует, то
    /// - отправляет на почту новый пароль
    /// - и выводит сообщение о том, что нужно проверить пароль на почте
    /// если в базе данных такого email нет
    /// - отправляет соответствующее сообщение
    // </summary>
    /// 

    [ApiController]
    [Route("api/[controller]")]
    public class UpdatePasswordController : ControllerBase
    {
        [HttpPost]
        public IActionResult Login([FromBody] EmailModel model)
        {
            using (CollaboratorSystemContext db = new CollaboratorSystemContext())
            {
                // Загрузить пользователя из базы данных по email
                var user = db.CollaboratorSystem.FromSqlRaw("SELECT * FROM CollaboratorSystem")
                                        .Where(p => p.EmailCollaborator == model.Email)
                                        .FirstOrDefault();

                if (user != null)
                {
                    // Проверяем, подтвержден ли email
                    if (!user.ActivationEmailCollaborator)
                    {
                        // Если email не подтвержден, отправляем соответствующее сообщение
                        return BadRequest(new { message = "Email не подтвержден. Пожалуйста, подтвердите ваш email перед сбросом пароля." });
                    }

                    // Генерация нового пароля
                    int passwordLength = 12; // Указываем длину пароля
                    string password = PasswordGenerator.GeneratePassword(passwordLength);

                    // Обновляем пароль в базе данных
                    db.Database.ExecuteSqlRaw("UPDATE CollaboratorSystem SET PasswordCollaborator={0} WHERE EmailCollaborator={1}", password, model.Email);

                    // Сохраняем изменения в базе данных
                    db.SaveChanges();

                    // Формируем сообщение с новым паролем
                    string body = "Ваш новый пароль: " + password;

                    // Отправляем новый пароль на почту
                    var mail = new EmailSender();
                    mail.SendEmail(model.Email, "Ваш новый пароль", body);

                    // Отправляем ответ с сообщением о сбросе пароля
                    return Ok(new { message = "Новый пароль отправлен на ваш email." });
                }

                // Если email не найден в базе данных
                return BadRequest(new { message = "Email не найден в базе." });
            }
        }
    }

    public record EmailModel(string Email);

}