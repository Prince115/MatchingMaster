using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Inventory.Domain.Entities;

public enum TransactionType
{
    Stock = 1,
    Order = 2
}

public class InventoryTransaction
{
    public int Id { get; set; }
    public int CatalogueId { get; set; }
    public TransactionType TransactionType { get; set; }
    public int Quantity { get; set; }
    public DateTime TransactionDate { get; set; }

    [ValidateNever]
    public Catalogue Catalogue { get; set; } = null!;
}