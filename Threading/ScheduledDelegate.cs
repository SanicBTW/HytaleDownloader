using System.Diagnostics;
using HytaleDownloader.Enums;

namespace HytaleDownloader.Threading;

// https://github.com/ppy/osu-framework/blob/0c8bac9b65bbbbb5e1f2f2b5ea4bed59baa4b620/osu.Framework/Threading/ScheduledDelegate.cs
public class ScheduledDelegate
{
    /// <summary>
    /// The earliest ElapsedTime value at which this task will be executed via a <see cref="Scheduler"/>.
    /// </summary>
    public double ExecutionTime { get; internal set; }

    /// <summary>
    /// Time in milliseconds between repeats of this task. -1 means no repeats.
    /// </summary>
    public readonly double RepeatInterval;

    /// <summary>
    /// In the case of a repeating execution, setting this to true will allow the delegate to run more than once at already elapsed points in time in order to catch up to current.
    /// This will ensure a consistent number of runs over real-time, even if the <see cref="Scheduler"/> running the delegate is suspended.
    /// Setting to false will skip catch-up executions, ensuring a future time is used after each execution.
    /// </summary>
    public bool PerformRepeatCatchUpExecutions = true;

    /// <summary>
    /// Whether this task has finished running.
    /// </summary>
    public bool Completed => State == RunState.Complete;

    /// <summary>
    /// Whether this task has been cancelled.
    /// </summary>
    public bool Cancelled => State == RunState.Cancelled;

    public RunState State { get; private set; }

    /// <summary>
    /// The work task.
    /// </summary>
    internal Action? Task;

    private readonly object runLock = new();

    public ScheduledDelegate(Action task, double executionTime = 0, double repeatInterval = -1)
        : this(executionTime, repeatInterval)
    {
        Task = task;
    }

    protected ScheduledDelegate(double executionTime = 0, double repeatInterval = -1)
    {
        ExecutionTime = executionTime;
        RepeatInterval = repeatInterval;
    }

    /// <summary>
    /// Invokes the scheduled task.
    /// </summary>
    /// <remarks>
    /// This call may block if the task is currently being run on another thread.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when attempting to run a task that has been cancelled or already completed.</exception>
    public void RunTask()
    {
        lock (runLock)
        {
            if (Cancelled)
                throw new InvalidOperationException($"Can not run a {nameof(ScheduledDelegate)} that has been {nameof(Cancelled)}.");

            if (Completed)
                throw new InvalidOperationException($"Can not run a {nameof(ScheduledDelegate)} that has been already {nameof(Completed)}.");

            RunTaskInternal();
        }
    }

    /// <summary>
    /// Invokes the scheduled task without throwing on incorrect state.
    /// </summary>
    internal void RunTaskInternal()
    {
        lock (runLock)
        {
            if (State != RunState.Waiting)
                return;

            State = RunState.Running;

            InvokeTask();

            // task may have been cancelled during execution.
            if (State == RunState.Cancelled)
                return;

            Trace.Assert(State == RunState.Running);
            State = RunState.Complete;
        }
    }

    protected virtual void InvokeTask()
    {
        Debug.Assert(Task != null);
        Task();
    }

    /// <summary>
    /// Cancel a task.
    /// </summary>
    /// <remarks>
    /// This call may block if the task is currently being run on another thread.
    /// </remarks>
    public void Cancel()
    {
        lock (runLock)
        {
            State = RunState.Cancelled;
        }
    }

    internal void SetNextExecution(double? currentTime)
    {
        lock (runLock)
        {
            if (State == RunState.Cancelled)
                return;

            State = RunState.Waiting;

            if (currentTime != null)
            {
                ExecutionTime += RepeatInterval;

                if (ExecutionTime < currentTime && !PerformRepeatCatchUpExecutions)
                    ExecutionTime = currentTime.Value + RepeatInterval;
            }
        }
    }
}
