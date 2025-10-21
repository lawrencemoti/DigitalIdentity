using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Models.API.Dtos
{
    public class IdentityDto
    {
        public required long IdentityNumber { get; set; }
        public string? FirstName { get; set; }
        public string? Surname { get; set; }
        public bool isVerificationWithDHA { get; set; } = false;
        public int lastRequestValidityPeriod { get; set; } = 0;
        public string? CallbackUrl { get; set; }
    }
}
