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
        private readonly ILogger<AutorizationController> _logger;
        private readonly CollaboratorSystemContext _db;

        public AutorizationController(
            ILogger<AutorizationController> logger,
            CollaboratorSystemContext db,
            TokenService tokenService
        )
        {
            _tokenService = tokenService;
            _logger = logger;
            _db = db;
        }

        [HttpPost]
        public IActionResult Login([FromBody] LoginModel model)
        {
            var user = _db.CollaboratorSystem
                .Where(p => p.EmailCollaborator == model.Email)
                .FirstOrDefault();

            if (user == null)
                return Ok(new { message = "Пользователь не найден!" });

            if (!user.ActivationEmailCollaborator)
                return Ok(new { message = "Пожалуйста, подтвердите свой адрес электронной почты." });

            if (model.Password == user.PasswordCollaborator)
            {
                var token = _tokenService.GenerateToken(model.Email, "User");

                if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(user.EmailCollaborator))
                {
                    _db.Database.ExecuteSqlRaw(
                        "UPDATE CollaboratorSystem SET TokenSystem={0} WHERE EmailCollaborator={1}",
                        token, user.EmailCollaborator);
                }

                _db.SaveChanges();

                return Ok(new
                {
                    Token = token,
                    RoleId = user.GuidIdRoleSystem,
                    guidIdCollaborator = user.GuidIdCollaborator
                });
            }

            return Ok(new { message = "Неверный логин или пароль" });
        }
    }

    public record LoginModel(string Email, string Password);
}
