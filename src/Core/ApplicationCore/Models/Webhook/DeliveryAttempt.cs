using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Models.Webhook
{
    public class DeliveryAttempt
    {
        public long Id { get; set; }
        public long WebhookEventId { get; set; }
        public WebhookEvent WebhookEvent { get; set; } = default!;
        public long VendorId { get; set; }
        public Vendor Vendor { get; set; } = default!;
        public int AttemptNumber { get; set; }
        public int? StatusCode { get; set; }
        public bool Success { get; set; }
        public string? Error { get; set; }
        public int? DurationMs { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
