using System.Text.Json;
using HytaleDownloader.Configuration;
using HytaleDownloader.Data;
using HytaleDownloader.Enums;
using HytaleDownloader.Events;
using HytaleDownloader.FileSystem;
using HytaleDownloader.Threading;
using SevenZipExtractor;

namespace HytaleDownloader.Managers;

public static class PayloadsManager
{
    public static DynamicPayloadJson DynamicPayload { get; private set; } = new(); // its never gonna be empty anyways
    private static readonly Dictionary<PayloadTarget, string> extraction_ready = []; // this is crazy yo
    private static readonly Dictionary<PayloadTarget, ScheduledDelegate> queued_checks = [];

    // TODO: Log
    // async someday, im sorry folks!
    public static void Initialize()
    {
        using HttpClient client = new HttpClient();

        try
        {
            // uhh save cache? not for now lolz
            string json = client
                .GetStringAsync(
                    "https://pub-f3aea920c9fb44f28d610fd4d1435731.r2.dev/Cracks/hytalv1/PayloadDefinitions.json")
                .GetAwaiter()
                .GetResult();

            DynamicPayload = JsonSerializer.Deserialize<DynamicPayloadJson>(json)!;
            EventManager.TriggerEvent(EventConstants.APP_READY, new Event());
        }
        catch
        {
            EventManager.TriggerEvent(EventConstants.CHANGE_PROGRESS_BAR_COLOR, new ColorChangeEvent(Color.Red));
            EventManager.TriggerEvent(EventConstants.UPDATE_PROGRESS_BAR, new IntValueEvent(100));
            MessageBox.Show("Failed to fetch payload info", "Fatal failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }

        EventManager.Register<ReadyForExtractionEvent>(EventConstants.READY_FOR_EXTRACTION, saveRef);
        EventManager.Register<StartExtractionEvent>(EventConstants.START_EXTRACTION, startDecompress);
        EventManager.Register<StartExtractionEvent>(EventConstants.EXTRACTION_DONE, decompressDone);
    }

    private static void saveRef(ReadyForExtractionEvent ev)
        => extraction_ready[ev.PayloadTarget] = ev.ExtractPath;

    private static void startDecompress(StartExtractionEvent ev)
    {
        if (!extraction_ready.TryGetValue(ev.PayloadTarget, out string? payloadPath))
        {
            // should log something lol
            MessageBox.Show($"The {nameof(PayloadsManager)} expected {ev.PayloadTarget} to be in the Dictionary and it received nothing.", "An internal error has occured", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        string outPath = ev.PayloadTarget switch
        {
            PayloadTarget.Hytale => Config.BackedConfig.HytaleLocation!,
            PayloadTarget.Jre => Config.BackedConfig.JreLocation!,
            _ => throw new ArgumentOutOfRangeException(nameof(ev.PayloadTarget), ev.PayloadTarget, null)
        };

        EventManager.TriggerEvent(EventConstants.UPDATE_PROGRESS_BAR, new IntValueEvent(0));
        EventManager.TriggerEvent(EventConstants.STATE_TOGGLEABLES, new BoolEvent(false));

        Scheduler.Add(() => decompress(payloadPath, outPath));

        // uhhhh
        ScheduledDelegate sched = Scheduler.AddDelayed(target =>
        {
            bool isDone = AppFolders.IsInstalled(target);
            if (!isDone)
                return;

            // another check just in case the event gets triggered more times after getting called once (?)
            if (!extraction_ready.ContainsKey(target))
                return;

            EventManager.TriggerEvent(EventConstants.EXTRACTION_DONE, ev);
        }, ev.PayloadTarget, 1000, true);;
        queued_checks[ev.PayloadTarget] = sched;
    }

    private static void decompressDone(StartExtractionEvent ev)
    {
        extraction_ready.Remove(ev.PayloadTarget);

        queued_checks[ev.PayloadTarget].Cancel();
        queued_checks.Remove(ev.PayloadTarget);

        Scheduler.Add(btn =>
        {
            btn.Tag = PayloadButtonState.ReInstall;
            btn.Text = ev.OnInstall;
        }, ev.TargetButton, false);

        EventManager.TriggerEvent(EventConstants.UPDATE_PROGRESS_BAR, new IntValueEvent(100));
        EventManager.TriggerEvent(EventConstants.STATE_TOGGLEABLES, new BoolEvent(true));
        EventManager.TriggerEvent(EventConstants.CHECK_PLAY_AVAILABILITY, new Event());
    }

    private static void decompress(string payloadPath, string outPath)
    {
        using FileStream streamRead = File.OpenRead(payloadPath);
        using ArchiveFile archiveFile = new ArchiveFile(streamRead);
        archiveFile.Extract(outPath, true);
    }
}
