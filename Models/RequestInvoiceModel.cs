namespace SUPPLY_API
{
    /// <summary>
    /// Модель передачи в контроллер данных о закупке для выставления счетов
    /// </summary>

   
    public record RequestInvoiceModel
    (
        string guidIdCollaborator,
        string vendorCodeComponent,
        string nameComponent,
        int quantityComponent,
        int priceComponent,
        string deliveryTimeComponent
    );
};