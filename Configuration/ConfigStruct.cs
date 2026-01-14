// ReSharper disable PropertyCanBeMadeInitOnly.Global
namespace HytaleDownloader.Configuration;

public class ConfigStruct
{
    /// <summary>
    /// The in-game username
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// The location of the Hytale installation
    /// </summary>
    public string? HytaleLocation { get; set; }

    /// <summary>
    /// The location of the JRE installation
    /// </summary>
    public string? JreLocation { get; set; }

    /// <summary>
    /// The necessary UUID v4 for the Sentry setup (even offline)
    /// </summary>
    public Guid Uuid { get; set; } = Guid.NewGuid();
}
