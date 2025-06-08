
using Microsoft.AspNetCore.Mvc;

namespace SUPPLY_API
{
    /// <summary>
    /// Контролер принимает (GuidIdCollaborator) - идентификатор пользователя в ситеме
    /// - проверяет принадлежность к компании
    /// - собирает всю информацию о пользователе и о компнаии
    /// и возвращает данные о компании пользователя и данные самого пользователя
    /// </summary>
    /// 
    /// 

    [ApiController]
    [Route("api/[controller]/{guidId}")]
    public class DataInfoPanelController : Controller
    {

        [HttpGet]
        public IActionResult DataInfoPanel(string guidId)
        {

            return Ok(new { massage = guidId });
        }

    }
}