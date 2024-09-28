using System.Text.Json.Serialization;

namespace Portal.Fornecedor.Api.Models.User.Response
{
    public class CaptchaResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        [JsonPropertyName("challenge_ts")]
        public string Challenge_ts { get; set; }

        [JsonPropertyName("hostname")]
        public string Hostname { get; set; }
        
    }
}
