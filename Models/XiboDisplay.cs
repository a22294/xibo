using System.Text.Json.Serialization;

namespace Xibo_CMVV.Models
{
    public class XiboDisplay
    {
        [JsonPropertyName("displayId")]
        public int displayId { get; set; }

        [JsonPropertyName("display")]
        public string display { get; set; }

        [JsonPropertyName("description")]
        public string description { get; set; }

        [JsonPropertyName("isActive")]
        public bool isActive { get; set; }

        [JsonPropertyName("loggedIn")]
        public bool loggedIn { get; set; }
    }
}
