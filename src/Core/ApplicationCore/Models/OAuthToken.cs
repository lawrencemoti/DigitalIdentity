using System;
using System.Text.Json.Serialization;

namespace ApplicationCore.Models
{

    public class OAuthToken
    {
        public OAuthToken()
        {
            Issued = DateTime.Now;
        }

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonPropertyName("as:client_id")]
        public string ClientId { get; set; }

        [JsonPropertyName("userName")]
        public string UserName { get; set; }

        [JsonPropertyName("as:region")]
        public string Region { get; set; }

        [JsonPropertyName(".issued")]
        public DateTime Issued { get; set; }

        [JsonPropertyName(".expires")]
        public DateTime Expires
        {
            get { return Issued.AddSeconds(ExpiresIn); }
        }

        [JsonPropertyName("bearer")]
        public string Bearer { get; set; }
    }
}
