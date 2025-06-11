namespace SUPPLY_API
{
    /// <summary>
    /// Модель передачи в контроллер новых или измененых данных о компании
    /// </summary>
    /// <param name="GuidIdCollaborator">Идентификатор пользователя</param>
    /// <param name="NameCollaborator">ФИО пользователя</param>
    /// <param name="PhoneCollaborator">Телефон пользователя</param>
    /// <param name="DeliveryAddress">Список адресов доставки</param>
    /// 

    public record InformationCollaboratorModel
    (
        string GuidIdCollaborator,
        string NameCollaborator,
        string PhoneCollaborator,
        List<string> DeliveryAddress
    );
};