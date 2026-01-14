using System.Collections.Concurrent;
using HytaleDownloader.Events;

namespace HytaleDownloader;

// the event manager from Aether Framework, ported to this project
// https://github.com/SanicBTW/Aether-Framework/blob/master/AetherFramework/EventManager.cs
public static class EventManager
{
    private static readonly ConcurrentDictionary<string, HashSet<Delegate>> global_events = [];

    private static readonly Action<string> logger = msg => Console.WriteLine($"{nameof(EventManager)} | {msg}");

    public static void Register<T>(string eventName, Action<T> handler) where T : Event
    {
        ArgumentNullException.ThrowIfNull(handler);

        // Add or update the global event handlers dictionary.
        global_events.AddOrUpdate(eventName,
            _ => [handler],
            (_, bag) =>
            {
                lock (bag)
                {
                    bag.Add(handler);
                }
                return bag;
            });

        logger.Invoke($"Registered global event handler for {eventName}.");
    }

    public static void Unregister<T>(string eventName, Action<T> handler) where T : Event
    {
        ArgumentNullException.ThrowIfNull(handler);

        if (!global_events.TryGetValue(eventName, out var handlers))
            return;

        lock (handlers)
        {
            handlers.Remove(handler);

            if (handlers.Count == 0)
                global_events.TryRemove(eventName, out _);
        }

        logger.Invoke($"Unregistered global event handler for {eventName}.");
    }

    public static void TriggerEvent(string eventName, Event eventInstance)
    {
        ArgumentNullException.ThrowIfNull(eventInstance);

        // Check if there are handlers registered for this event type
        if (!global_events.TryGetValue(eventName, out var handlers))
            return;

        CallOnHandlers(handlers, eventInstance);
    }

    private static void CallOnHandlers(HashSet<Delegate> handlers, Event eventInstance)
    {
        foreach (Delegate handler in handlers)
        {
            try
            {
                handler.DynamicInvoke(eventInstance);

                // If the event has been cancelled, stop further propagation
                if (eventInstance.IsCancelled)
                    break;
            }
            catch (Exception ex)
            {
                logger?.Invoke($"Exception while dispatching handlers for {eventInstance.GetType().Name}: {ex}");
            }
        }
    }
}
