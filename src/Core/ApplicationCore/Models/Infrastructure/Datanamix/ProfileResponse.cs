using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ApplicationCore.Models.Infrastructure.Datanamix
{
    public class ProfileResponse
    {
        // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
        public class ConsumerDetail
        {
            [JsonPropertyName("IDNumber")]
            public string IDNumber { get; set; }

            [JsonPropertyName("FirstName")]
            public string FirstName { get; set; }

            [JsonPropertyName("SecondName")]
            public string SecondName { get; set; }

            [JsonPropertyName("Surname")]
            public string Surname { get; set; }

            [JsonPropertyName("DeceasedStatus")]
            public string DeceasedStatus { get; set; }

            [JsonPropertyName("IDIssuedDate")]
            public string IDIssuedDate { get; set; }

            [JsonPropertyName("EnquiryID")]
            public int EnquiryID { get; set; }

            [JsonPropertyName("EnquiryResultID")]
            public int EnquiryResultID { get; set; }
        }

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

        public class ProfileIdSearchResult
        {
            [JsonPropertyName("ConsumerDetails")]
            public List<ConsumerDetail> ConsumerDetails { get; set; }
        }

        public class Root
        {
            [JsonPropertyName("Header")]
            public Header Header { get; set; }

            [JsonPropertyName("ProfileIdSearchResult")]
            public ProfileIdSearchResult ProfileIdSearchResult { get; set; }

            [JsonPropertyName("Success")]
            public bool Success { get; set; }

            [JsonPropertyName("Messages")]
            public List<string> Messages { get; set; }

            [JsonPropertyName("ResponseCode")]
            public int ResponseCode { get; set; }
        }
    }
}
