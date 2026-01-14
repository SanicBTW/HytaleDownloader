using HytaleDownloader.Enums;

namespace HytaleDownloader.Events;

public class StartExtractionEvent(PayloadTarget payloadTarget, Button targetButton, string onInstall = "") : Event
{
    public PayloadTarget PayloadTarget = payloadTarget;
    public Button TargetButton = targetButton;
    public string OnInstall = onInstall;
}
