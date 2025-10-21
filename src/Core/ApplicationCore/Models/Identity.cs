using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ApplicationCore.Enumerators.Enum;

namespace ApplicationCore.Models
{
    public class Identity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required] public long IdentityNumber { get; set; } = default!;

        public string FirstName { get; set; } = default!;

        public string? SecondName { get; set; }

        public string Surname { get; set; } = default!;

        public IdentityStatus Status { get; set; } = IdentityStatus.Pending; // PENDING, VALIDATED, FAILED

        public string? DeceasedStatus { get; set; }

        public string? MaritalStatus { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public DateTime? IDIssuedDate { get; set; }

        public string? HomeAffairsIDBlocked { get; set; }

        public DateTime LastRequestDate { get; set; } = DateTime.Now;

        public string? VendorEnquiryID { get; set; }

        public string? VendorEnquiryResultID { get; set; }
        
        public string? IdentityNumberMatchStatus { get; set; }

        public string? Gender { get; set; }

        public string CountryOfBirth { get; set; }

        public string Citizenship { get; set; }

        public string? CallbackUrl { get; set; }

        public ICollection<IdentityVerification> Verifications { get; set; } = new List<IdentityVerification>();

        public Identity()
        {

        }
    }
}
