using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    [Controller]
    [Route("api/[controller]")]
    public class EditInformationCollaboratorController : ControllerBase
    {
        [HttpPost]
        public IActionResult EditInformationCollaborator([FromBody] InformationCollaboratorModel model)
        {
            // Обновление данных о пользователе
            using (var _dbCollaboratorSystem = new CollaboratorSystemContext())
            {
                var user = _dbCollaboratorSystem.CollaboratorSystem
                    .FirstOrDefault(p => p.GuidIdCollaborator == model.GuidIdCollaborator);

                if (user == null)
                {
                    return BadRequest("Пользователь не найден");
                }

                _dbCollaboratorSystem.Database.ExecuteSqlRaw(
                    "UPDATE CollaboratorSystem SET NameCollaborator = {0}, PhoneCollaborator = {1} WHERE GuidIdCollaborator = {2}",
                    model.NameCollaborator, model.PhoneCollaborator, model.GuidIdCollaborator
                );

                _dbCollaboratorSystem.SaveChanges();
            }

            using (var _dbDeliveryAddress = new DeliveryAddressContext())
            {
                var existingAddresses = _dbDeliveryAddress.DeliveryAddress
                    .Where(p => p.GuidIdCollaboratorSystem == model.GuidIdCollaborator)
                    .ToList();

                // Удаляем адреса, которых больше нет в модели
                var addressesToRemove = existingAddresses
                    .Where(ea => ea.DeliveryAddress != null && !model.DeliveryAddress.Contains(ea.DeliveryAddress))
                    .ToList();

                _dbDeliveryAddress.DeliveryAddress.RemoveRange(addressesToRemove);

                // Обновляем существующие адреса (по порядку)
                for (int i = 0; i < existingAddresses.Count && i < model.DeliveryAddress.Count; i++)
                {
                    existingAddresses[i].DeliveryAddress = model.DeliveryAddress[i].Trim();
                }

                // Добавляем новые адреса
                if (model.DeliveryAddress.Count > existingAddresses.Count)
                {
                    for (int i = existingAddresses.Count; i < model.DeliveryAddress.Count; i++)
                    {
                        var newAddress = new DeliveryAddressDb
                        {
                            GuidIdCollaboratorSystem = model.GuidIdCollaborator,
                            DeliveryAddress = model.DeliveryAddress[i].Trim()
                        };
                        _dbDeliveryAddress.DeliveryAddress.Add(newAddress);
                    }
                }

                _dbDeliveryAddress.SaveChanges();
            }

            return Ok(new { message = "Информация о пользователе обновлена" });
        }
    }
}
