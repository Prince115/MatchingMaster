using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory.Domain.Entities
{
    [Table("Program")]
    public class ProgramEntry
    {
        [Key]
        public int ProgramId { get; set; }

        public string ProgramNo { get; set; } = string.Empty;

        public int PartyId { get; set; }
        public string? Quality { get; set; }

        public DateOnly Date { get; set; }
        public decimal? MainCut { get; set; }
        public string? Fold { get; set; }
        public string? Finishing { get; set; }
        public int? Quantity { get; set; }
        public string? Remarks { get; set; }
        public int? Round { get; set; }
        public string? Rate { get; set; }
        public string? PhotoFileName { get; set; }


        [MinLength(1, ErrorMessage = "At least one Program Matching is required")]
        public ICollection<ProgramMatching> ProgramMatchings { get; set; } = new List<ProgramMatching>();
        public Party Party { get; set; } = null!;   // FK → Party
    }
}
