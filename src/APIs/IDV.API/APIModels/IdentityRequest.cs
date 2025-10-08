using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDV.API.APIModels
{
    public class IdentityRequest
    {
        public required long IdentityNumber { get; set; }
        public bool isRealTimeHomeAffairsVerification { get; set; } = false;
        public int lastRequestValidityPeriod { get; set; } = 0;
    }
}
