using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Inventory.Domain.ViewModels
{
    public class MatchingVM
    {
        public int DesignId { get; set; }
        public string? DesignNo { get; set; }
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);
        public int? PartyId { get; set; }
        public int Plates { get; set; }
        public int Matching { get; set; }

        [ValidateNever]
        public int PlateCount { get; set; }

        [ValidateNever]
        public int MatchingCount { get; set; }

        [ValidateNever]   
        public string PartyName { get; set; }

        [ValidateNever]
        public List<PlateRow> Rows { get; set; } = new();
    }

    public class PlateRow
    {
        [ValidateNever]
        public int DesignPlateId { get; set; } 
        public string? PlateName { get; set; }

        [ValidateNever]
        public string? PlateNo { get; set; }

        [ValidateNever]
        public List<MatchingCell> Matchings { get; set; } = new();
    }

    public class MatchingCell
    {
        [ValidateNever]
        public int DesignMatchingId { get; set; }
        
        [ValidateNever]
        public int MatchingNo { get; set; }
        public string? Colour { get; set; }
    }
}
