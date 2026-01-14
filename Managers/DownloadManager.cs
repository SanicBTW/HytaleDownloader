using HytaleDownloader.Configuration;
using HytaleDownloader.Data;
using HytaleDownloader.Enums;
using HytaleDownloader.Events;
using HytaleDownloader.FileSystem;
using HytaleDownloader.Threading;

namespace HytaleDownloader.Managers;

// TODO: LOG
public static class DownloadManager
{
    public static void Initialize()
    {
        EventManager.Register<PickFolderEvent>(EventConstants.PICK_FOLDER, pickFolder);
        EventManager.Register<StartDownloadEvent>(EventConstants.START_DOWNLOAD, startDownload);
    }

    private static void pickFolder(PickFolderEvent ev)
    {
        string? folderPick = AppFolders.PickFolder(ev.PickDescription, AppFolders.APP_DATA_FOLDER);
        if (folderPick == null)
        {
            // uhhhhh??
            ev.Cancel();
            return;
        }

        // best way to not copy over the elements?
        PayloadTarget payloadTarget = ev.PayloadTarget;
        switch (payloadTarget)
        {
            case PayloadTarget.Hytale:
                Config.BackedConfig.HytaleLocation = folderPick;
                break;

            case PayloadTarget.Jre:
                Config.BackedConfig.JreLocation = folderPick;
                break;
        }
        Config.ScheduleSave();
        EventManager.TriggerEvent(EventConstants.UPDATE_LOCATION_TEXTBOX, new UpdateLocationTextBoxEvent(payloadTarget));

        Scheduler.Add(btn =>
        {
            btn.Tag = PayloadButtonState.Download;
            btn.Text = ev.OnLocate;
        }, ev.TargetButton, false);
    }

    // should handle errors bruh
    private static void startDownload(StartDownloadEvent ev)
    {
        EventManager.TriggerEvent(EventConstants.UPDATE_PROGRESS_BAR, new IntValueEvent(0));
        EventManager.TriggerEvent(EventConstants.STATE_TOGGLEABLES, new BoolEvent(false));

        Payload payload = ev.PayloadTarget switch
        {
            PayloadTarget.Hytale => PayloadsManager.DynamicPayload.Hytale!,
            PayloadTarget.Jre => PayloadsManager.DynamicPayload.Jre!,
            _ => throw new ArgumentOutOfRangeException(nameof(ev.PayloadTarget), ev.PayloadTarget, null)
        };

        string payloadName = Constants.GetPayloadName(ev.PayloadTarget);
        string payloadPath = Path.Combine(AppFolders.DOWNLOAD_FOLDER, payloadName);

        Scheduler.Add(() =>
        {
            try
            {
                download(payload.Url!, payloadPath);

                if (AppFolders.ComputeSha256(payloadPath) != payload.Sha256)
                {
                    File.Delete(payloadPath);
                    MessageBox.Show("Corrupted file", "Failed downloading the payload", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    EventManager.TriggerEvent(EventConstants.STATE_TOGGLEABLES, new BoolEvent(true)); // enable early in case of not matching
                    return;
                }

                // only set to install when matching
                Scheduler.Add(btn =>
                {
                    btn.Tag = PayloadButtonState.Install;
                    btn.Text = ev.OnDownload;
                }, ev.TargetButton, false);
                EventManager.TriggerEvent(EventConstants.STATE_TOGGLEABLES, new BoolEvent(true)); // enable after changing the content
                EventManager.TriggerEvent(EventConstants.UPDATE_PROGRESS_BAR, new IntValueEvent(100));
                EventManager.TriggerEvent(EventConstants.READY_FOR_EXTRACTION, new ReadyForExtractionEvent(ev.PayloadTarget, payloadPath));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        });
    }

    private static void download(string url, string outputPath)
    {
        using HttpClient client = new HttpClient();
        using HttpResponseMessage response = client.GetAsync(
                url,
                HttpCompletionOption.ResponseHeadersRead)
            .GetAwaiter()
            .GetResult();

        response.EnsureSuccessStatusCode();

        long? total = response.Content.Headers.ContentLength;
        using Stream input = response.Content
            .ReadAsStreamAsync()
            .GetAwaiter()
            .GetResult();

        using FileStream output = File.Create(outputPath);

        byte[] buffer = new byte[81920];
        long readTotal = 0;
        int read;

        // TODO: Show download speed xd

        // reuse the instance, although i dont know how that'll go lol
        IntValueEvent progressUpdate = new IntValueEvent(0);
        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            output.Write(buffer, 0, read);
            readTotal += read;

            if (total is > 0)
            {
                double percent = (double)readTotal / total.Value * 100.0;
                int value = (int)Math.Clamp(percent, 0, 100);

                progressUpdate.Number = value;
                EventManager.TriggerEvent(EventConstants.UPDATE_PROGRESS_BAR, progressUpdate);
            }
        }
    }

    public static bool IsDownloaded(PayloadTarget target) => target switch
    {
        PayloadTarget.Hytale => File.Exists(Path.Combine(AppFolders.DOWNLOAD_FOLDER, Constants.HYTALE_PAYLOAD_NAME)),
        PayloadTarget.Jre => File.Exists(Path.Combine(AppFolders.DOWNLOAD_FOLDER, Constants.JRE_PAYLOAD_NAME)),
        _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
    };
}
