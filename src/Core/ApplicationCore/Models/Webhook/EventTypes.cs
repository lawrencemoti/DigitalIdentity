using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Models.Webhook
{
    public class EventTypes
    {
        public const string IDV = "IDV"; // Identity Verification
        public const string PDD = "PDD"; // Party Due Diligence
        public const string AHV = "AHV"; // Bank Account Holder Verification


        public static readonly string[] All = new[] { IDV, PDD, AHV };
        public static bool IsValid(string s) => All.Contains(s, StringComparer.OrdinalIgnoreCase);
    }
}
