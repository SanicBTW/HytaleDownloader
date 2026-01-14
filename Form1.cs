using System.Diagnostics;
using HytaleDownloader.Configuration;
using HytaleDownloader.Threading;
using HytaleDownloader.Enums;
using HytaleDownloader.Events;
using HytaleDownloader.FileSystem;
using HytaleDownloader.Managers;

namespace HytaleDownloader;

// TODO: Better error management and logging?
// TODO: Improve the progress check
// hey uhh sanco here, I DID NOT need the scheduler to check EVERY SECOND, i think a filesystem watcher was more than enough?
public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();

        Scheduler.Initialize();

        AppFolders.Initialize();
        Config.Initialize();

        DownloadManager.Initialize();
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        // should make a quick wrapper for wrapping the event calls into the main thread but uhhhh
        EventManager.Register<IntValueEvent>(EventConstants.UPDATE_PROGRESS_BAR, (ev) =>
            Scheduler.Add(evn => progressBar1.Value = evn.Number, ev, false));
        EventManager.Register<ColorChangeEvent>(EventConstants.CHANGE_PROGRESS_BAR_COLOR, (ev) =>
            Scheduler.Add(evn => progressBar1.ForeColor = evn.Color, ev, false));
        EventManager.Register<UpdateLocationTextBoxEvent>(EventConstants.UPDATE_LOCATION_TEXTBOX, (ev) =>
            Scheduler.Add(updateLocations, ev, false));
        EventManager.Register<BoolEvent>(EventConstants.STATE_TOGGLEABLES, (ev) =>
            Scheduler.Add(refreshUi, ev, false));
        EventManager.Register<Event>(EventConstants.CHECK_PLAY_AVAILABILITY, _ => Scheduler.Add(canPlay, false));
        EventManager.Register<Event>(EventConstants.APP_READY, _ => Scheduler.Add(validateConfig, false));

        PayloadsManager.Initialize();
    }

    private void validateConfig()
    {
        usernameb.Enabled = genuuid.Enabled = hytale_location_selector.Enabled = jre_location_selector.Enabled = true;

        usernameb.Text = Config.BackedConfig.Name;
        uuid_shower.Text = Config.BackedConfig.Uuid.ToString();

        hytale_location_selector.Enabled = jre_location_selector.Enabled = true;
        hytale_location_selector.Tag = jre_location_selector.Tag = PayloadButtonState.Locate;

        bool hasHytalePath = Config.BackedConfig.HytaleLocation != null;
        bool hasJrePath = Config.BackedConfig.JreLocation != null;

        // validating
        // uuid is always gonna be present so we gonna ignore that check lmao
        // once its installed you arent able to change it anymore, unless the json file is modified, should change this however...
        if (hasHytalePath)
        {
            hytale_location_show.Text = Config.BackedConfig.HytaleLocation;
            updateButtonState(PayloadTarget.Hytale, hytale_location_selector, "Hytale");
        }

        if (hasJrePath)
        {
            jre_location_show.Text = Config.BackedConfig.JreLocation;
            updateButtonState(PayloadTarget.Jre, jre_location_selector, "JRE");
        }

        canPlay();
    }

    private void genuuid_Click(object sender, EventArgs e)
    {
        Config.BackedConfig.Uuid = Guid.NewGuid();
        uuid_shower.Text = Config.BackedConfig.Uuid.ToString();
        Config.ScheduleSave();
    }

    private void usernameb_TextChanged(object sender, EventArgs e)
    {
        Config.BackedConfig.Name = usernameb.Text;
        Config.ScheduleSave();
    }

    private void buylink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        buylink.Visible = true;
        Process.Start(new ProcessStartInfo("https://store.hytale.com/") { UseShellExecute = true });
    }

    private void PlaybtnOnClick(object? sender, EventArgs e) => Hytale.Run();

    private void Hytale_location_selectorOnClick(object? sender, EventArgs e)
    {
        handleBtnClick(PayloadTarget.Hytale, hytale_location_selector,
            "Select the folder where you want to install Hytale (separate the folders for clarity between JRE and Hytale)",
            "Download Hytale", "Install Hytale", "Re-install Hytale");
    }

    private void Jre_location_selectorOnClick(object? sender, EventArgs e)
    {
        handleBtnClick(PayloadTarget.Jre, jre_location_selector,
            "Select the folder where you want to install the JRE (separate the folders for clarity between JRE and Hytale)",
            "Download JRE", "Install JRE", "Re-install JRE");
    }

    private void updateButtonState(PayloadTarget target, Button targetBtn, string content)
    {
        bool isDownloaded = DownloadManager.IsDownloaded(target);
        bool isInstalled = AppFolders.IsInstalled(target);

        if (!isDownloaded && !isInstalled)
        {
            targetBtn.Text = $"Download {content}";
            targetBtn.Tag = PayloadButtonState.Download;
        }
        else if (isDownloaded && !isInstalled)
        {
            string payloadName = Constants.GetPayloadName(target);
            string payloadPath = Path.Combine(AppFolders.DOWNLOAD_FOLDER, payloadName);
            EventManager.TriggerEvent(EventConstants.READY_FOR_EXTRACTION, new ReadyForExtractionEvent(target, payloadPath));

            targetBtn.Text = $"Install {content}";
            targetBtn.Tag = PayloadButtonState.Install;
        }
        else if (isDownloaded && isInstalled)
        {
            targetBtn.Text = $"Re-install {content}";
            targetBtn.Tag = PayloadButtonState.ReInstall;
        }
    }

    private void updateLocations(UpdateLocationTextBoxEvent ev)
    {
        switch (ev.Target)
        {
            case PayloadTarget.Hytale:
                hytale_location_show.Text = Config.BackedConfig.HytaleLocation;
                break;

            case PayloadTarget.Jre:
                jre_location_show.Text = Config.BackedConfig.JreLocation;
                break;
        }
    }

    private void refreshUi(BoolEvent ev)
    {
        hytale_location_selector.Enabled = ev.State;
        jre_location_selector.Enabled = ev.State;
        genuuid.Enabled = ev.State;
        usernameb.Enabled = ev.State;
    }

    private void canPlay()
    {
        // we only need to follow these for the 2 cases where this is gonna get called
        // on the beginning when validating the config and the payload installation (extraction)
        PayloadButtonState hytaleBtnState = (PayloadButtonState)hytale_location_selector.Tag!;
        PayloadButtonState jreBtnState = (PayloadButtonState)jre_location_selector.Tag!;

        if (hytaleBtnState != PayloadButtonState.ReInstall || jreBtnState != PayloadButtonState.ReInstall)
        {
            playbtn.Enabled = false;
            return;
        }

        playbtn.Enabled = true;
        progressBar1.Value = 100;
    }

    // the names can be confusing i know,
    // on locate its used when the folder is located, on download is when the payload is downloaded and on install is when the payload is installed (extracted)
    private void handleBtnClick(PayloadTarget payloadTarget, Button targetBtn,
        string pickDesc, string onLocate = "", string onDownload = "", string onInstall = "")
    {
        PayloadButtonState state = (PayloadButtonState)targetBtn.Tag!;
        switch (state)
        {
            case PayloadButtonState.Locate:
                EventManager.TriggerEvent(EventConstants.PICK_FOLDER,
                    new PickFolderEvent(payloadTarget, targetBtn, pickDesc, onLocate));
                break;

            case PayloadButtonState.Download:
                EventManager.TriggerEvent(EventConstants.START_DOWNLOAD,
                    new StartDownloadEvent(payloadTarget, targetBtn, onDownload));
                break;

            case PayloadButtonState.Install:
                EventManager.TriggerEvent(EventConstants.START_EXTRACTION,
                    new StartExtractionEvent(payloadTarget, targetBtn, onInstall));
                break;

            case PayloadButtonState.ReInstall:
                MessageBox.Show("If willing to reinstall the game, remove the extracted content in the selected folder and re-open the app.", "WIP", MessageBoxButtons.OK, MessageBoxIcon.Information);
                break;
        }
    }
}
