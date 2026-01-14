namespace HytaleDownloader.Events;

public class Event
{
    public bool IsCancelled { get; private set; } = false;

    public virtual void Cancel()
    {
        IsCancelled = true;
    }

    public virtual void Adjust() { }
}
