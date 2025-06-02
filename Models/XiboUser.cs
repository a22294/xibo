using System.Text.Json.Serialization;

namespace Xibo_CMVV.Models
{
    public class XiboUser
    {
        [JsonPropertyName("userId")]
        public int userId { get; set; }

        [JsonPropertyName("userName")]
        public string userName { get; set; }

        [JsonPropertyName("email")]
        public string email { get; set; }

        [JsonPropertyName("password")]
        public string password { get; set; }

        [JsonPropertyName("userType")]
        public string userType { get; set; } = "User";
    }
}
