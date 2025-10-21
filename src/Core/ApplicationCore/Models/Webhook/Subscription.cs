using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Models.Webhook
{
    public class Subscription
    {
        public long Id { get; set; }
        public long VendorId { get; set; }
        public Vendor Vendor { get; set; } = default!;
        public string EventType { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
    }
}
