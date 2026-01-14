using HytaleDownloader.Enums;

namespace HytaleDownloader.Events;

public class StartDownloadEvent(PayloadTarget payloadTarget, Button targetButton, string onDownload = "") : Event
{
    public PayloadTarget PayloadTarget = payloadTarget;
    public Button TargetButton = targetButton;
    public string OnDownload = onDownload;
}
