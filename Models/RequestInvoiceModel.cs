namespace SUPPLY_API
{
    /// <summary>
    /// Модель передачи в контроллер данных о закупке для выставления счетов
    /// </summary>


    public record RequestInvoiceModel
    (
        string guidIdCollaborator,
        string guidIdPurchase,
        string purchaseId,
        string purchaseName,
        int purchasePrice,
        string purchaseCostomer,
        List<RequestInvoicePurchaseItemModel> purchaseItem
    );


    public record RequestInvoicePurchaseItemModel
    (
        string guidIdPurchase,
        string guidIdComponent,
        string vendorCodeComponent,
        string nameComponent,
        int requiredQuantityItem,
        int purchaseItemPrice,
        string bestComponentProvider,
        string deliveryTimeComponent,
        List<OtherOffers> otherOffers
    );
    
    
    public record OtherOffers(
        string guidIdComponent,
        int purchaseItemPrice,
        string bestComponentProvider,
        string deliveryTimeComponent
    );
};