using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Models
{
    public class WebhookSubscription
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required] public string VendorName { get; set; } = default!;
        [Required] public string CallbackUrl { get; set; } = default!;
        public List<string> EventTypes { get; set; } // e.g., ["IDV", "PDD"]
        public DateTime CreatedAt { get; set; }
    }
}
