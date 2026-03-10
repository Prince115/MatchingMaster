using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Domain.Entities
{
    public class DesignPlate
    {
        public int DesignPlateId { get; set; }
        public int DesignId { get; set; }
        public string? PlateName { get; set; }
        public string? PlateNo { get; set; }

        public Design Design { get; set; } = null!;
        public ICollection<DesignMatching> DesignMatchings { get; set; } = new List<DesignMatching>();
    }
}
