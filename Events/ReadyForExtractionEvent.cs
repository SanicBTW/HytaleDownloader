using HytaleDownloader.Enums;

namespace HytaleDownloader.Events;

public class ReadyForExtractionEvent(PayloadTarget payloadTarget, string extractPath) : Event
{
    public PayloadTarget PayloadTarget = payloadTarget;
    public string ExtractPath = extractPath;
}
