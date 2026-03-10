using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Domain.Entities
{
    public class Party
    {
        public int PartyId { get; set; }
        public string PartyName { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Remarks{ get; set; }

        public ICollection<Design> Design { get; set; } = new List<Design>();
    }
}
