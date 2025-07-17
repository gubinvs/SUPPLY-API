

namespace SUPPLY_API 
{
    /// <summary>
    /// Класс содержит данные о взамосвязи пользователя системы и закупки, по сути разрешение на просморт данной закупки
    /// </summary>
    public class PurchaseAuthorizationDb
    {
        public int Id { get; set; }

        public string? GuidIdCollaborator { get; set; }

        public string? GuidIdPurchase { get; set; }

        public PurchaseAuthorizationDb() { }
    }
}