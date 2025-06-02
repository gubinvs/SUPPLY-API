using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace encomponent.api.Controllers;

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
public class AuthController : ControllerBase
{
    private readonly TokenService _tokenService;

    public AuthController()
    {
        _tokenService = new TokenService();
    }

    [HttpPost]
    public IActionResult Login([FromBody] LoginModel model)
    {
        // Подключаемся к базе данных
        using (UserSystemContext db = new UserSystemContext())
        {

            // Загрузил из базы данных информацию о пользователе (по email), который пришел в запросе
            var user = db.UserSystem.FromSqlRaw("SELECT * FROM userSystems")
                                    .Where(p => p.Email == model.Email)
                                    .FirstOrDefault(); // Используем FirstOrDefault, чтобы сразу получить одного пользователя

            // Если пользователь не найден
            if (user == null)
            {
                return Ok(new { message = "Пользователь не найден. Пожалуйста, зарегистрируйтесь." });
            }

            // Проверка подтверждения почты
            if (!user.IsConfirmed)
            {
                return Ok(new { message = "Пожалуйста, подтвердите свой адрес электронной почты. В противном случае, ваш аккаунт может быть удалён в ближайшее время." });
            }

            // Проверка пароля сохраненного в базе данных и который пришел в запросе
            if (model.Password == user.Password)
            {
                // Если проверка прошла успешно - создаем токен
                var token = _tokenService.GenerateToken(model.Email, "User");

                // Формируем запрос к базе данных для изменения в ней токена
                db.Database.ExecuteSqlRaw("UPDATE userSystems SET Token={0} WHERE Email={1}", token, user.Email);

                // Записываем в базу данных новый токен
                db.SaveChanges();

                // Отправляем токен в ответ на запрос
                return Ok(new { Token = token });
            }

            // Если проверка не прошла, отправляем ответ
            return Ok(new { message = "Неверный логин или пароль" });
        }
    }
}

public record LoginModel(string Email, string Password);
