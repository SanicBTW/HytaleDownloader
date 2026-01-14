// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
namespace HytaleDownloader.Threading;

internal class ScheduledDelegateWithData<T>(
    Action<T> task,
    T data,
    double executionTime = 0,
    double repeatInterval = -1)
    : ScheduledDelegate(executionTime, repeatInterval)
{
    public new readonly Action<T> Task = task;

    public T Data = data;

    protected override void InvokeTask() => Task(Data);

    public override string ToString() => $"method \"{Task.Method}\" targeting \"{Task.Target}\" with data \"{Data}\" executing at {ExecutionTime:N0} with repeat {RepeatInterval}";
}
