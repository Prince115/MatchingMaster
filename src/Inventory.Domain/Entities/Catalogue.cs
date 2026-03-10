namespace Inventory.Domain.Entities;

public class Catalogue
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Packing { get; set; }
    public string? Remark { get; set; }
    public string? PhotoFileName { get; set; }
    public string? PdfFileName { get; set; }
    public bool IsDeleted { get; set; }

    public ICollection<InventoryTransaction> Transactions { get; set; }
        = new List<InventoryTransaction>();
}