namespace HytaleDownloader.Enums;

public enum RunState
{
    /// <summary>
    /// Waiting to run. Potentially not the first run if on a repeating schedule.
    /// </summary>
    Waiting,

    /// <summary>
    /// Currently running.
    /// </summary>
    Running,

    /// <summary>
    /// Running completed for a final time.
    /// </summary>
    Complete,

    /// <summary>
    /// Task manually cancelled.
    /// </summary>
    Cancelled
}
