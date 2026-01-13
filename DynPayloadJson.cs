using System.Text.Json.Serialization;

namespace HytaleDownloader;

public class DynPayloadJson
{
    [JsonPropertyName("hytale")]
    public Payload? Hytale { get; set; }

    [JsonPropertyName("jre")]
    public Payload? Jre { get; set; }
}

public class Payload
{
    [JsonPropertyName("url")] public string? Url { get; set; } = null;
    [JsonPropertyName("sha256")] public string Sha256 { get; set; } = "";
}
