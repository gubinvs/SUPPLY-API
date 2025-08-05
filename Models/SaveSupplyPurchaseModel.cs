namespace SUPPLY_API
{
    /// <summary>
    /// Модель передачи в контроллер новых данных о компании поставщике
    /// </summary>

    public record SaveSupplyPurchaseModel
    (
        string guidIdCollaborator,
        string guidIdPurchase,
        string purchaseId,
        string purchaseName,
        int purchasePrice,
        string purchaseCostomer,
        List<PurchaseItemModel> purchaseItem
    );

    public record PurchaseItemModel (
        string guidIdComponent,
        string vendorCodeComponent,
        string nameComponent,
        int requiredQuantityItem,
        int purchaseItemPrice,
        string bestComponentProvider,
        string deliveryTimeComponent
    );
};