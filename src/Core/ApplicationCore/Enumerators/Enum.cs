using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Enumerators
{
    public class Enum
    {
        public enum IdentityStatus{Pending,Verified,Failed }

        public enum IdentityVerificationStatus {Pending,Success,Error }

        public enum WebhookEventStatus
        {
            Pending,
            Processing,
            Completed,
            Failed
        }
    }
}
