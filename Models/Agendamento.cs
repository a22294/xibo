using System.Text.Json.Serialization;

namespace Xibo_CMVV.Models
{
    public class Agendamento
{
    [JsonPropertyName("eventId")]
    public int EventId { get; set; }

    [JsonPropertyName("eventTypeId")]
    public int EventTypeId { get; set; }

    [JsonPropertyName("displayOrder")]
    public int DisplayOrder { get; set; }

    [JsonPropertyName("isPriority")]
    public bool IsPriority { get; set; }

    [JsonPropertyName("fromDt")]
    public string FromDt { get; set; } = string.Empty;

    [JsonPropertyName("toDt")]
    public string ToDt { get; set; } = string.Empty;

    [JsonPropertyName("displayGroupIds")]
    public string DisplayGroupIds { get; set; } = string.Empty;

    [JsonPropertyName("layoutId")]
    public int? LayoutId { get; set; }

    [JsonPropertyName("campaignId")]
    public int? CampaignId { get; set; }

    [JsonPropertyName("commandId")]
    public int? CommandId { get; set; }

    [JsonPropertyName("eventName")]
    public string EventName { get; set; } = string.Empty;
}

public class ScheduleDataResponse
{
    [JsonPropertyName("success")]
    public int Success { get; set; }

    [JsonPropertyName("result")]
    public List<Agendamento> Result { get; set; } = new();
}

    public class ScheduleResponse
    {
        public bool Success { get; set; }
        public int Total { get; set; }
        public List<Agendamento> Data { get; set; } = new();
    }
}
