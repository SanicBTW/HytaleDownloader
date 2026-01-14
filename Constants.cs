using HytaleDownloader.Enums;

namespace HytaleDownloader;

public static class Constants
{
    /// <summary>
    /// The config save file name
    /// </summary>
    public const string CONFIG_FILENAME = "configv1.json";

    /// <summary>
    /// The file name of the downloaded Hytale Payload
    /// <remarks>
    /// It may not be the latest version available on the official launcher but the one I published
    /// </remarks>
    /// </summary>
    public const string HYTALE_PAYLOAD_NAME = "hytaleLatest.7z";

    /// <summary>
    /// The file name of the downloaded JRE Payload
    /// <remarks>
    /// This is the same JRE folder provided by the Hytale launcher
    /// </remarks>
    /// </summary>
    public const string JRE_PAYLOAD_NAME = "jreLatest.7z"; // the bundled jre with hytale really

    public static string GetPayloadName(PayloadTarget target) => target switch
    {
        PayloadTarget.Hytale => HYTALE_PAYLOAD_NAME,
        PayloadTarget.Jre => JRE_PAYLOAD_NAME,
        _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
    };
}
