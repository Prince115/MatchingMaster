using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Inventory.Domain.ViewModels
{
    public class ProgramVM
    {
        public int ProgramId { get; set; }

        [Required(ErrorMessage = "Program No is required")]
        public string ProgramNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select party.")]
        public int PartyId { get; set; }

        [Required(ErrorMessage = "Please select design.")]
        public int DesignId { get; set; }

        public string? Quality { get; set; }
        public string? Fold { get; set; }
        public string? Finishing { get; set; }
        public string? Remarks { get; set; }
        public string? Rate { get; set; }
        public DateOnly Date { get; set; }
        public decimal? MainCut { get; set; }
        public int? Quantity { get; set; }
        public int? Round { get; set; }

        [ValidateNever]
        public List<int> SelectedMatchingNo { get; set; } = new();

        [ValidateNever]
        public List<ProgramMatchingVM> Matchings { get; set; } = new();

        [ValidateNever]
        public string? PartyName { get; set; }  // ✅ nullable — display only

        [ValidateNever]
        public string? DesignNo { get; set; }

        [ValidateNever]
        public int TotalMatchings { get; set; }
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
