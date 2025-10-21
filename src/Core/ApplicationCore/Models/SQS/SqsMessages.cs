using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Models.SQS
{
    public class SqsMessages
    {
        public record IdentityMessage
        {
           public Guid IdentityId { get; set; }

            public int ValidityPeriod { get; set; }

            public bool IsVerificationWithDHA { get; set; }
        }
    }
}
