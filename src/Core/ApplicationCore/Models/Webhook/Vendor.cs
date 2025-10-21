using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Models.Webhook
{
    public class Vendor
    {
        public long Id { get; set; }
        [Required][MaxLength(200)] public string Name { get; set; } = default!;
        [Required][MaxLength(2048)] public string CallbackUrl { get; set; } = default!;
        public byte[] SecretHash { get; set; } = default!; // Store password-hash of vendor secret
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
