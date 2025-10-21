using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ApplicationCore.Enumerators.Enum;

namespace ApplicationCore.Models
{
    public class IdentityVerification
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid IdentityId { get; set; }
        public Identity Identity { get; set; } = default!;
        public string Provider { get; set; } = default!;
        public IdentityVerificationStatus Status { get; set; } = IdentityVerificationStatus.Pending;
        public int HttpStatus { get; set; }
        public string? Payload { get; set; }
        public DateTime AttemptedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
