using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ApplicationCore.Enumerators.Enum;

namespace ApplicationCore.Models.Webhook
{
    public class WebhookEvent
    {
        public long Id { get; set; }
        public string EventType { get; set; } = default!;
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
        public string Payload { get; set; } = default!; // stored as JSON string
        public WebhookEventStatus Status { get; set; } = WebhookEventStatus.Pending; // Pending, Processing, Completed, Failed
        public DateTime AvailableAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
