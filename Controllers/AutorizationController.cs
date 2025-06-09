using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API.Controllers
{

    /// <summary>
    /// Контроллер принимает методом POST данные о логине и пароле пользователя,
    /// - проверяет по базе данных наличие данного пользователя и сопоставляет пароль
    /// - если такой пользователь существует и пароль верный, отправляет токен
    /// - предварительно записав сгенерированный токен в базу данных
    /// В процессе работы контроллер проверяет по базе данных наличие пользователя (email - логин) и пароль,
    /// если эти данные верны, то выполняет основные задачи описанные выше.
    /// - если пользователь в базе данных есть, а пароль не верен, то отпраляет ответ о неверном пароле (react приложение предложить ввести пароль повторно в одно поле для пароля)
    /// - если пользователя в базе данных нет, то отправляет ответ о необходимости зарегистрироваться. (добавить коды, чтобы на их основании react приложение перенаправляло на страницу регистрации)
    /// </summary>
    /// 

    [ApiController]
    [Route("api/[controller]")]
    public class AutorizationController : ControllerBase
    {
        private readonly TokenService _tokenService;

        public AutorizationController()
        {
            _tokenService = new TokenService();
        }

        [HttpPost]
        public IActionResult Login([FromBody] LoginModel model)
        {
            // Подключаемся к базе данных
            using (CollaboratorSystemContext db = new CollaboratorSystemContext())
            {

                // Загрузил из базы данных информацию о пользователе (по email), который пришел в запросе
                var user = db.CollaboratorSystem.FromSqlRaw("SELECT * FROM CollaboratorSystem")
                                        .Where(p => p.EmailCollaborator == model.Email)
                                        .FirstOrDefault(); // Используем FirstOrDefault, чтобы сразу получить одного пользователя

                // Если пользователь не найден
                if (user == null)
                {
                    return Ok(new { message = "Пользователь не найден!" });
                }

                // Проверка подтверждения почты
                if (!user.ActivationEmailCollaborator)
                {
                    return Ok(new { message = "Пожалуйста, подтвердите свой адрес электронной почты. В противном случае, ваш аккаунт может быть удалён в ближайшее время." });
                }

                // Проверка пароля сохраненного в базе данных и который пришел в запросе
                if (model.Password == user.PasswordCollaborator)
                {
                    var token = _tokenService.GenerateToken(model.Email, "User");

                    if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(user.EmailCollaborator))
                    {
                        db.Database.ExecuteSqlRaw(
                            "UPDATE CollaboratorSystem SET TokenSystem={0} WHERE EmailCollaborator={1}",
                            token, user.EmailCollaborator);
                    }

                    db.SaveChanges();

                    // Отправляем токен и GuidIdRoleSystem
                    return Ok(new
                    {
                        Token = token,
                        RoleId = user.GuidIdRoleSystem,  // ← возвращаем нужное поле
                        guidIdCollaborator = user.GuidIdCollaborator
                    });
                }

                // Если проверка не прошла, отправляем ответ
                return Ok(new { message = "Неверный логин или пароль" });
            }
        }
    }

    public record LoginModel(string Email, string Password);
}
