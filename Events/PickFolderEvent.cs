using HytaleDownloader.Enums;

namespace HytaleDownloader.Events;

public class PickFolderEvent(PayloadTarget payloadTarget, Button targetButton, string pickDescription, string onLocate) : Event
{
    public PayloadTarget PayloadTarget = payloadTarget;
    public Button TargetButton = targetButton;
    public string PickDescription = pickDescription;
    public string OnLocate = onLocate;
}
