namespace SUPPLY_API
{
    /// <summary>
    /// Модель передачи в контроллер новых данных о компании поставщике
    /// </summary>

    /// <param name="AbbreviatedNameCompany">Сокращенное наименование компании</param>
    /// <param name="InnCompany">ИНН компании</param>
    /// 
    public record SupplyProviderModel
    (
        string AbbreviatedNameCompany,
        long InnCompany
    );
};