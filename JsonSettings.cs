namespace HytaleDownloader;

public class JsonSettings
{
    public string Name { get; set; } = "";
    public string? HytaleLocation { get; set; } = null;
    public string? JreLocation { get; set; } = null;
    public Guid Uuid { get; set; } = Guid.NewGuid();
}
