using System;
using System.Collections.Generic;

namespace RenPyTRLauncher.Models
{
    public class MembershipTier
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = "💎";
        public decimal Price { get; set; }
        public string PriceLabel { get; set; } = string.Empty;
        public List<string> Features { get; set; } = new();
        public string PurchaseUrl { get; set; } = string.Empty;
        public string AccentColor { get; set; } = "#9B59FF";
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
