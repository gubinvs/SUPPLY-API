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
        int    purchasePrice,
        string purchaseCostomer,
        List<PurchaseComponentDb> purchaseItem
    );
};