namespace Inventory.Domain.Entities
{
    public class ProgramMatching
    {
        public int ProgramMatchingId { get; set; }
        public int ProgramId { get; set; }
        public int DesignId { get; set; }
        public int DesignMatchingId { get; set; }
        public int PlateId { get; set; }
        public int MatchingNo { get; set; }
        public string Colour { get; set; } = string.Empty;

        public ProgramEntry Program { get; set; } = null!;
    }
}
