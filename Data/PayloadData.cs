using System.Text.Json.Serialization;

namespace HytaleDownloader.Data;

public class Payload
{
    [JsonPropertyName("url")] public string? Url { get; set; } = null;
    [JsonPropertyName("sha256")] public string Sha256 { get; set; } = "";
}
