namespace HytaleDownloader.Events;

public class IntValueEvent(int number) : Event
{
    public int Number = number;
}
