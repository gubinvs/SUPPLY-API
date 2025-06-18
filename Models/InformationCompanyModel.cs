
namespace SUPPLY_API
{
    /// <summary>
    /// Модель передачи в контроллер новых или измененых данных о компании
    /// </summary>
    /// <param name="GuidIdCompany">Идентификатор компании</param>
    /// <param name="FullNameCompany">Полное наименование компании</param>
    /// <param name="AbbreviatedNameCompany">Сокращенное наименование компании</param>
    /// <param name="InnCompany">ИНН компании</param>
    /// <param name="roleCompany">Роль компании в системе</param>
    /// <param name="AddressCompany">Юридический адрес компании</param>
    public record InformationCompanyModel
    (
        string GuidIdCompany,
        string FullNameCompany,
        string AbbreviatedNameCompany,
        long InnCompany,
        string AddressCompany,
        string RoleCompany,
        string GuidIdCollaborator
    );
};