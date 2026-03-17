namespace Inventory.Domain.Entities
{
    public class DesignMatching
    {
        public int DesignMatchingId { get; set; }
        public int DesignPlateId { get; set; }
        public int MatchingNo { get; set; }
        public string? Colour { get; set; }

        public DesignPlate DesignPlate { get; set; } = null!;
    }
}
