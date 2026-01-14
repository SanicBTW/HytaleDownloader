using System.Text.Json.Serialization;

namespace HytaleDownloader.Data;

// ugghhhh i should name this differently
public class DynamicPayloadJson
{
    [JsonPropertyName("hytale")]
    public Payload? Hytale { get; set; }

    [JsonPropertyName("jre")]
    public Payload? Jre { get; set; }
}
