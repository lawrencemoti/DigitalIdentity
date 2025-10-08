using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ApplicationCore.Models.Infrastructure.Datanamix
{
    public class HomeAffairsIDVCheck
    {
        // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
        public class Header
        {
            [JsonPropertyName("SearchDate")]
            public DateTime SearchDate { get; set; }

            [JsonPropertyName("CreatedUserId")]
            public int CreatedUserId { get; set; }

            [JsonPropertyName("ReportName")]
            public string ReportName { get; set; }

            [JsonPropertyName("ReportReference")]
            public string ReportReference { get; set; }

            [JsonPropertyName("ClientReference")]
            public string ClientReference { get; set; }

            [JsonPropertyName("ReportType")]
            public string ReportType { get; set; }
        }

        public class RealTimeIDVerification
        {
            [JsonPropertyName("InputIDNO")]
            public string InputIDNO { get; set; }

            [JsonPropertyName("IDNO")]
            public string IDNO { get; set; }

            [JsonPropertyName("IDNOMatchStatus")]
            public string IDNOMatchStatus { get; set; }

            [JsonPropertyName("Names")]
            public string Names { get; set; }

            [JsonPropertyName("FirstName")]
            public string FirstName { get; set; }

            [JsonPropertyName("LastName")]
            public string LastName { get; set; }

            [JsonPropertyName("Gender")]
            public string Gender { get; set; }

            [JsonPropertyName("DateOfBirth")]
            public string DateOfBirth { get; set; }

            [JsonPropertyName("Age")]
            public string Age { get; set; }

            [JsonPropertyName("DeceasedStatus")]
            public string DeceasedStatus { get; set; }

            [JsonPropertyName("MaritalStatus")]
            public string MaritalStatus { get; set; }

            [JsonPropertyName("IDCardIndicator")]
            public string IDCardIndicator { get; set; }

            [JsonPropertyName("IDCardDate")]
            public string IDCardDate { get; set; }

            [JsonPropertyName("CountryOfBirth")]
            public string CountryOfBirth { get; set; }

            [JsonPropertyName("IDBlocked")]
            public string IDBlocked { get; set; }

            [JsonPropertyName("Citizenship")]
            public string Citizenship { get; set; }
        }

        public class Results
        {
            [JsonPropertyName("RealTimeIDVerification")]
            public RealTimeIDVerification RealTimeIDVerification { get; set; }
        }

        public class Root
        {
            [JsonPropertyName("Header")]
            public Header Header { get; set; }

            [JsonPropertyName("Results")]
            public Results Results { get; set; }

            [JsonPropertyName("Success")]
            public bool Success { get; set; }

            [JsonPropertyName("Messages")]
            public List<string> Messages { get; set; }

            [JsonPropertyName("ResponseCode")]
            public int ResponseCode { get; set; }
        }
    }
}
