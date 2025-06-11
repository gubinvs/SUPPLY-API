using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    /// <summary>
    /// Контроллер отвечает за изменение данных полязователя
    /// </summary>
    /// 
    
    [Controller]
    [Route("api/[controller]")]
    public class EditInformationCollaboratorController : ControllerBase
    {
        // Принимаем методом POST данные согласно модели
        [HttpPost]
        public IActionResult EditInformationCollaborator([FromBody] InformationCollaboratorModel model)
        {
            // Подключемся к базе данных с пользователями и проверяем наличие такого пользователя
            using (var _dbCollaboratorSystem = new CollaboratorSystemContext())
            {
                var user = _dbCollaboratorSystem.CollaboratorSystem
                    .FirstOrDefault(p => p.GuidIdCollaborator == model.GuidIdCollaborator);

                if (user == null)
                {
                    return BadRequest("Пользователь не найден");
                }

                // Если пользователь найден обновляем данные о пользователе
                _dbCollaboratorSystem.Database.ExecuteSqlRaw(
                    "UPDATE CollaboratorSystem SET NameCollaborator = {0}, PhoneCollaborator = {1} WHERE GuidIdCollaborator = {2}",
                        model.NameCollaborator, model.PhoneCollaborator, model.GuidIdCollaborator
                );

                // И записываем в базу данных
                _dbCollaboratorSystem.SaveChanges();
            }

            using (var _dbDeliveryAddress = new DeliveryAddressContext())
            {
                // Получаем текущие адреса пользователя из БД
                var existingAddresses = _dbDeliveryAddress.DeliveryAddress
                    .Where(p => p.GuidIdCollaboratorSystem == model.GuidIdCollaborator)
                    .ToList();

                // Обновляем адреса по индексу
                for (int i = 0; i < model.DeliveryAddress.Count && i < existingAddresses.Count; i++)
                {
                    existingAddresses[i].DeliveryAddress = model.DeliveryAddress[i].Trim();
                }

                // Сохраняем изменения в базе
                _dbDeliveryAddress.SaveChanges();

                return Ok(new { message = "Адреса доставки обновлены" });
            }

        }
    }
}