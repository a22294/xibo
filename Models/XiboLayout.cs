using System.Text.Json.Serialization;

namespace Xibo_CMVV.Models
{
    public class XiboLayout
    {
        [JsonPropertyName("layoutId")]
        public int layoutId { get; set; }

        [JsonPropertyName("layout")]
        public string layout { get; set; }

        [JsonPropertyName("description")]
        public string description { get; set; }

        [JsonPropertyName("status")]
        public string status { get; set; }

        // Propriedade apenas para uso interno na interface (não vem da API)
        [JsonIgnore]
        public string assignedDisplayText { get; set; } = "Nenhum display atribuído";
    }
}