using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Models
{
    public class Identity
    {
        public long IdentityNumber { get; set; } = default!;

        public string FirstName { get; set; } = default!;

        public string? SecondName { get; set; }

        public string Surname { get; set; } = default!;

        public string Status { get; set; } = "PENDING"; // PENDING, VALIDATED, FAILED

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

        public Identity()
        {

        }
    }
}
