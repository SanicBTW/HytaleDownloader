namespace HytaleDownloader.Events;

public class ColorChangeEvent(Color newColor) : Event
{
    public Color Color = newColor;
}
