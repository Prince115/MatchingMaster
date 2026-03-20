namespace Inventory.Domain.Entities
{
    public class Design
    {
        public int DesignId { get; set; }
        public int? DesignNo { get; set; }
        public int? PartyId { get; set; }
        public DateOnly Date { get; set; }

        public Party Party { get; set; } = null!;
        public ICollection<DesignPlate> DesignPlates { get; set; } = new List<DesignPlate>();
    }
}
