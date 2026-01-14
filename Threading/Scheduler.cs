using System.Diagnostics;
using HytaleDownloader.Extensions;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable LocalizableElement

namespace HytaleDownloader.Threading;

// https://github.com/ppy/osu-framework/blob/0c8bac9b65bbbbb5e1f2f2b5ea4bed59baa4b620/osu.Framework/Threading/Scheduler.cs
public static class Scheduler
{
    private const string thread_name = "HytaleDownloader Scheduler Thread";
    private const int log_excesssive_queue_length_interval = 1000;

    private static readonly Stopwatch stopwatch = new();
    private static SynchronizationContext synchronizationContext = null!;
    private static readonly Thread thread = new(update)
    {
        IsBackground = true,
        Name = thread_name,
    };

    private static readonly Queue<ScheduledDelegate> run_queue = new();
    private static readonly List<ScheduledDelegate> timed_tasks = [];
    private static readonly List<ScheduledDelegate> per_update_tasks = [];

    private static double currentTime => stopwatch.ElapsedMilliseconds;
    private static readonly object queue_lock = new();

    /// <summary>
    /// Whether there are any tasks queued to run (including delayed tasks in the future).
    /// </summary>
    public static bool HasPendingTasks => TotalPendingTasks > 0;

    /// <summary>
    /// The total tasks this scheduler instance has run.
    /// </summary>
    public static int TotalTasksRun { get; private set; }

    /// <summary>
    /// The total number of <see cref="ScheduledDelegate"/>s tracked by this instance for future execution.
    /// </summary>
    internal static int TotalPendingTasks => run_queue.Count + timed_tasks.Count + per_update_tasks.Count;

    private static readonly List<ScheduledDelegate> tasks_to_schedule = [];
    private static readonly List<ScheduledDelegate> tasks_to_remove = [];

    public static void Initialize()
    {
        synchronizationContext = SynchronizationContext.Current!;
        thread.Start();
        stopwatch.Start();
    }

    /// <summary>
    /// Run any pending work tasks.
    /// </summary>
    private static void update()
    {
        // running in the dedicated thread
        // this is probably not the best way, but im not looking to implement the clocks stuff n shi
        // and the auto reset event wasnt working properly soo thread sleep itll be lol
        while (true)
        {
            bool hasTimedTasks = timed_tasks.Count > 0;
            bool hasPerUpdateTasks = per_update_tasks.Count > 0;

            if (hasTimedTasks || hasPerUpdateTasks) // avoid taking out a lock if there are no items.
            {
                lock (queue_lock)
                {
                    queueTimedTasks();
                    queuePerUpdateTasks();
                }
            }

            int countToRun = run_queue.Count;

            if (countToRun == 0)
            {
                Thread.Sleep(1);
                continue; // avoid taking out a lock via getNextTask() if there are no items.
            }

            int countRun = 0;

            while (getNextTask(out ScheduledDelegate? sd))
            {
                if (sd == null)
                    break;

                sd.RunTaskInternal();

                TotalTasksRun++;

                if (++countRun == countToRun)
                    break;
            }

            Thread.Sleep(1);
        }

        // ReSharper disable once FunctionNeverReturns
    }

    private static void queueTimedTasks()
    {
        // Already checked before this method is called, but helps with path prediction?
        if (timed_tasks.Count != 0)
        {
            double currentTimeLocal = currentTime;

            foreach (var sd in timed_tasks)
            {
                if (sd.ExecutionTime <= currentTimeLocal)
                {
                    tasks_to_remove.Add(sd);

                    if (sd.Cancelled) continue;

                    if (sd.RepeatInterval == 0)
                    {
                        // handling of every-frame tasks is slightly different to reduce overhead.
                        per_update_tasks.Add(sd);
                        continue;
                    }

                    if (sd.RepeatInterval > 0)
                    {
                        if (timed_tasks.Count > log_excesssive_queue_length_interval)
                            throw new ArgumentException("Too many timed tasks are in the queue!");

                        // schedule the next repeat of the task.
                        sd.SetNextExecution(currentTimeLocal);
                        tasks_to_schedule.Add(sd);
                    }

                    if (!sd.Completed) enqueue(sd);
                }
            }

            foreach (var t in tasks_to_remove)
                timed_tasks.Remove(t);

            tasks_to_remove.Clear();

            foreach (var t in tasks_to_schedule)
                timed_tasks.AddInPlace(t);

            tasks_to_schedule.Clear();
        }
    }

    private static void queuePerUpdateTasks()
    {
        // Already checked before this method is called, but helps with path prediction?
        if (per_update_tasks.Count != 0)
        {
            for (int i = 0; i < per_update_tasks.Count; i++)
            {
                ScheduledDelegate task = per_update_tasks[i];

                task.SetNextExecution(null);

                if (task.Cancelled)
                {
                    per_update_tasks.RemoveAt(i--);
                    continue;
                }

                enqueue(task);
            }
        }
    }

    private static bool getNextTask(out ScheduledDelegate? task)
    {
        lock (queue_lock)
        {
            if (run_queue.Count > 0)
            {
                task = run_queue.Dequeue();
                return true;
            }
        }

        task = null;
        return false;
    }

    /// <summary>
    /// Cancel any pending work tasks.
    /// </summary>
    public static void CancelDelayedTasks()
    {
        lock (queue_lock)
        {
            foreach (var t in timed_tasks)
                t.Cancel();
            timed_tasks.Clear();
        }
    }

    /// <summary>
    /// Add a task to be scheduled.
    /// </summary>
    /// <remarks>If scheduled, the task will be run on the next <see cref="Update"/> independent of the current clock time.</remarks>
    /// <param name="task">The work to be done.</param>
    /// <param name="data">The data to be passed to the task.</param>
    /// <param name="forceScheduled">If set to false, the task will be executed immediately if we are on the main thread.</param>
    /// <returns>The scheduled task, or <c>null</c> if the task was executed immediately.</returns>
    public static ScheduledDelegate? Add<T>(Action<T> task, T data, bool forceScheduled = true)
    {
        if (!forceScheduled)
        {
            // :+1:
            synchronizationContext.Post(d => task((T)d!), data);
            return null;
        }

        var del = new ScheduledDelegateWithData<T>(task, data);
        enqueue(del);
        return del;
    }

    /// <summary>
    /// Add a task to be scheduled.
    /// </summary>
    /// <remarks>If scheduled, the task will be run on the next <see cref="update"/> independent of the current clock time.</remarks>
    /// <param name="task">The work to be done.</param>
    /// <param name="forceScheduled">If set to false, the task will be executed immediately if we are on the main thread.</param>
    /// <returns>The scheduled task, or <c>null</c> if the task was executed immediately.</returns>
    public static ScheduledDelegate? Add(Action task, bool forceScheduled = true)
    {
        if (!forceScheduled)
        {
            synchronizationContext.Post(_ => task(), null);
            return null;
        }

        var del = new ScheduledDelegate(task);
        enqueue(del);
        return del;
    }

    /// <summary>
    /// Add a task to be scheduled.
    /// </summary>
    /// <remarks>The task will be run on the next <see cref="update"/> independent of the current clock time.</remarks>
    /// <param name="task">The scheduled delegate to add.</param>
    /// <exception cref="InvalidOperationException">Thrown when attempting to add a scheduled delegate that has been already completed.</exception>
    public static void Add(ScheduledDelegate task)
    {
        if (task.Completed)
            throw new InvalidOperationException($"Can not add a {nameof(ScheduledDelegate)} that has been already {nameof(ScheduledDelegate.Completed)}");

        lock (queue_lock)
        {
            timed_tasks.AddInPlace(task);

            if (timed_tasks.Count % log_excesssive_queue_length_interval == 0)
            {
                Console.WriteLine($"{nameof(Scheduler)} has {timed_tasks.Count} timed tasks pending");
                Console.WriteLine($"- First task: {timed_tasks.First()}");
                Console.WriteLine($"- Last task: {timed_tasks.Last()}");
            }
        }
    }

    /// <summary>
    /// Add a task which will be run after a specified delay from the current clock time.
    /// </summary>
    /// <param name="task">The work to be done.</param>
    /// <param name="data">The data to be passed to the task.</param>
    /// <param name="timeUntilRun">Milliseconds until run.</param>
    /// <param name="repeat">Whether this task should repeat.</param>
    /// <returns>Whether this is the first queue attempt of this work.</returns>
    public static ScheduledDelegate AddDelayed<T>(Action<T> task, T data, double timeUntilRun, bool repeat = false)
    {
        // We are locking here already to make sure we have no concurrent access to currentTime
        lock (queue_lock)
        {
            ScheduledDelegate del = new ScheduledDelegateWithData<T>(task, data, currentTime + timeUntilRun, repeat ? timeUntilRun : -1);
            Add(del);
            return del;
        }
    }

    /// <summary>
    /// Add a task which will be run after a specified delay from the current clock time.
    /// </summary>
    /// <param name="task">The work to be done.</param>
    /// <param name="timeUntilRun">Milliseconds until run.</param>
    /// <param name="repeat">Whether this task should repeat.</param>
    /// <returns>The scheduled task.</returns>
    public static ScheduledDelegate AddDelayed(Action task, double timeUntilRun, bool repeat = false)
    {
        // We are locking here already to make sure we have no concurrent access to currentTime
        lock (queue_lock)
        {
            ScheduledDelegate del = new ScheduledDelegate(task, currentTime + timeUntilRun, repeat ? timeUntilRun : -1);
            Add(del);
            return del;
        }
    }

    /// <summary>
    /// Adds a task which will only be run once per frame, no matter how many times it was scheduled in the previous frame.
    /// </summary>
    /// <remarks>The task will be run on the next <see cref="Update"/> independent of the current clock time.</remarks>
    /// <param name="task">The work to be done.</param>
    /// <param name="data">The data to be passed to the task. Note that duplicate schedules may result in previous data never being run.</param>
    /// <returns>Whether this is the first queue attempt of this work.</returns>
    public static bool AddOnce<T>(Action<T> task, T data)
    {
        lock (queue_lock)
        {
            var existing = run_queue.OfType<ScheduledDelegateWithData<T>>().SingleOrDefault(sd => sd.Task == task);

            if (existing != null)
            {
                // ensure the single queued instance always has the most recent data.
                existing.Data = data;
                return false;
            }

            enqueue(new ScheduledDelegateWithData<T>(task, data));
        }

        return true;
    }

    /// <summary>
    /// Adds a task which will only be run once per frame, no matter how many times it was scheduled in the previous frame.
    /// </summary>
    /// <remarks>The task will be run on the next <see cref="update"/> independent of the current clock time.</remarks>
    /// <param name="task">The work to be done. Avoid using inline delegates as they may not be cached, bypassing the once-per-frame guarantee.</param>
    /// <returns>Whether this is the first queue attempt of this work.</returns>
    public static bool AddOnce(Action task)
    {
        lock (queue_lock)
        {
            if (run_queue.Any(sd => sd.Task == task))
                return false;

            enqueue(new ScheduledDelegate(task));
        }

        return true;
    }

    private static void enqueue(ScheduledDelegate task)
    {
        lock (queue_lock)
        {
            run_queue.Enqueue(task);
            if (run_queue.Count % log_excesssive_queue_length_interval == 0)
                Console.WriteLine($"{nameof(Scheduler)} has {run_queue.Count} tasks pending");
        }
    }
}
