using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Inventory.Domain.ViewModels
{
    public class ProgramVM
    {
        public int ProgramId { get; set; }

        [Required(ErrorMessage = "Program No is required")]
        public string ProgramNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Party is required")]
        public int PartyId { get; set; }

        // For display purposes only, not required for input
        public string PartyName { get; set; }
        public string? Quality { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateOnly Date { get; set; }
        public int? MainCut { get; set; }
        public string? Fold { get; set; }
        public string? Finishing { get; set; }
        public int? Quantity { get; set; }
        public string? Remarks { get; set; }
        public int? Round { get; set; }
        public string? Rate { get; set; }

        [Required(ErrorMessage = "Design is required")]
        public int DesignId { get; set; }

        // For display purposes only, not required for input
        public string DesignNo { get; set; }

        [MinLength(1, ErrorMessage = "At least one Program Matching is required")]
        public List<ProgramMatchingVM> Matchings { get; set; } = new List<ProgramMatchingVM>();


        [Required(ErrorMessage = "Please select at least one matching.")]
        public List<int> SelectedMatchingIds { get; set; }
    }

    public class ProgramMatchingVM
    {
        public int ProgramMatchingId { get; set; }
        public int ProgramId { get; set; }
        public int DesignId { get; set; }
        public int DesignMatchingId { get; set; }
        public int PlateId { get; set; }
        public int MatchingNo { get; set; }
        public string Colour { get; set; } = string.Empty;
    }
}
