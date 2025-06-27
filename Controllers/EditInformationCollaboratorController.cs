using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    [Controller]
    [Route("api/[controller]")]
    public class EditInformationCollaboratorController : ControllerBase
    {

        private readonly ILogger<EditInformationCollaboratorController> _logger;
        private readonly CollaboratorSystemContext _dbCollab;
        private readonly DeliveryAddressContext _dbDelivery;


        public EditInformationCollaboratorController
        (
            ILogger<EditInformationCollaboratorController> logger,
            CollaboratorSystemContext dbCollab,
            DeliveryAddressContext dbDelivery
        )
        {
            _logger = logger;
            _dbCollab = dbCollab;
            _dbDelivery = dbDelivery;
        }

        [HttpPost]
        public IActionResult EditInformationCollaborator([FromBody] InformationCollaboratorModel model)
        {
            // Обновление данных о пользователе
            var user = _dbCollab.CollaboratorSystem
                .FirstOrDefault(p => p.GuidIdCollaborator == model.GuidIdCollaborator);

            if (user == null)
            {
                return BadRequest("Пользователь не найден");
            }

            _dbCollab.Database.ExecuteSqlRaw(
                "UPDATE CollaboratorSystem SET NameCollaborator = {0}, PhoneCollaborator = {1} WHERE GuidIdCollaborator = {2}",
                model.NameCollaborator, model.PhoneCollaborator, model.GuidIdCollaborator
            );

            _dbCollab.SaveChanges();


            var existingAddresses = _dbDelivery.DeliveryAddress
                .Where(p => p.GuidIdCollaboratorSystem == model.GuidIdCollaborator)
                .ToList();

            // Удаляем адреса, которых больше нет в модели
            var addressesToRemove = existingAddresses
                .Where(ea => ea.DeliveryAddress != null && !model.DeliveryAddress.Contains(ea.DeliveryAddress))
                .ToList();

            _dbDelivery.DeliveryAddress.RemoveRange(addressesToRemove);

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
                    _dbDelivery.DeliveryAddress.Add(newAddress);
                }
            }

            _dbDelivery.SaveChanges();

            return Ok(new { message = "Информация о пользователе обновлена" });
        }
    }
}
