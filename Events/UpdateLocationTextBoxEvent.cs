using HytaleDownloader.Enums;

namespace HytaleDownloader.Events;

// bruv
public class UpdateLocationTextBoxEvent(PayloadTarget target) : Event
{
    public PayloadTarget Target = target;
}
