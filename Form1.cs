using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using SevenZipExtractor;

namespace HytaleDownloader;

internal enum PayloadTarget
{
    Hytale,
    Jre
}

internal enum PayloadBtnState
{
    Locate,
    Install,
    ReInstall
}

// TODO: Better error management and logging?
// TODO: Avoid duplicate code!!
// TODO: Make blocking parts multi threaded (HttpClient and stuff)
// TODO: Improve the progress check
// TODO: Do not enable the play button until the HytaleClient is found
public partial class Form1 : Form
{
    private const string savefilename = "config.json";
    private const string hytalepayloadname = "hytaleLatest.7z"; // it may not be the latest but the latest version available that i published
    private const string jrepayloadname = "jreLatest.7z"; // the bundled jre with hytale really

    private readonly string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HytaleDownloader");
    private string downloadFolder => Path.Combine(appDataFolder, "downloads"); // separate the origin to the extraction (installation) folder
    private readonly JsonSettings jsonSettings;

    private DynPayloadJson payloadJson = new(); // its never gonna be empty anyways

    public Form1()
    {
        InitializeComponent();

        if (!Directory.Exists(appDataFolder))
            Directory.CreateDirectory(appDataFolder);

        if (!Directory.Exists(downloadFolder))
            Directory.CreateDirectory(downloadFolder);

        string configPath = Path.Combine(appDataFolder, savefilename);
        if (File.Exists(configPath))
        {
            using StreamReader streamReader = File.OpenText(configPath);
            string content = streamReader.ReadToEnd();
            jsonSettings = JsonSerializer.Deserialize<JsonSettings>(content)!;
        }
        else
        {
            // write for the first time
            jsonSettings = new JsonSettings();
            writeSettings();
        }
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        // uhh save cache? not for now lolz
        DynPayloadJson? payloadInfo = fetchPayloadInfo();
        if (payloadInfo == null)
        {
            MessageBox.Show("Failed to fetch payload info", "Fatal failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
            return;
        }
        payloadJson = payloadInfo;

        usernameb.Text = jsonSettings.Name;
        uuid_shower.Text = jsonSettings.Uuid.ToString();
        hytale_location_selector.Tag = PayloadBtnState.Locate;
        jre_location_selector.Tag = PayloadBtnState.Locate;

        bool hasHytalePath = jsonSettings.HytaleLocation != null;
        bool hasJrePath = jsonSettings.JreLocation != null;
        bool isReady = (hasHytalePath & hasJrePath);
        // validating
        // uuid is always gonna be present but we gonna ignore that lmao
        // once its installed you arent able to change it anymore, unless the json file is modified, should change this however...
        if (hasHytalePath)
        {
            hytale_location_show.Text = jsonSettings.HytaleLocation;

            if (!checkInstalled(PayloadTarget.Hytale))
            {
                hytale_location_selector.Text = "Install Hytale";
                hytale_location_selector.Tag = PayloadBtnState.Install;
                isReady = false;
            }
            else
            {
                hytale_location_selector.Text = "Re-install Hytale";
                hytale_location_selector.Tag = PayloadBtnState.ReInstall;
            }
        }

        if (hasJrePath)
        {
            jre_location_show.Text = jsonSettings.JreLocation;

            if (!checkInstalled(PayloadTarget.Jre))
            {
                jre_location_selector.Text = "Install Jre";
                jre_location_selector.Tag = PayloadBtnState.Install;
                isReady = false;
            }
            else
            {
                jre_location_selector.Text = "Re-install Jre";
                jre_location_selector.Tag = PayloadBtnState.ReInstall;
            }
        }

        if (isReady)
        {
            progressBar1.Value = 100;
            playbtn.Enabled = true;
        }
    }

    private void genuuid_Click(object sender, EventArgs e)
    {
        jsonSettings.Uuid = Guid.NewGuid();
        uuid_shower.Text = jsonSettings.Uuid.ToString();
        writeSettings();
    }

    private void usernameb_TextChanged(object sender, EventArgs e)
    {
        jsonSettings.Name = usernameb.Text;
        writeSettings();
    }

    // should use an open stream to write without issues but i should be fine
    private void writeSettings()
    {
        string content = JsonSerializer.Serialize(jsonSettings, new JsonSerializerOptions() { WriteIndented = true });
        using StreamWriter streamWriter = File.CreateText(Path.Combine(appDataFolder, savefilename));
        streamWriter.Write(content);
        streamWriter.Close();
    }

    private void buylink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        buylink.Visible = true;
        Process.Start(new ProcessStartInfo("https://store.hytale.com/") { UseShellExecute = true });
    }

    private void PlaybtnOnClick(object? sender, EventArgs e)
    {
        // yea we doin last minute checks here bruh
        string hytaleFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Hytale");
        if (!Directory.Exists(hytaleFolder))
            Directory.CreateDirectory(hytaleFolder);

        string userDataFolder = Path.Combine(hytaleFolder, "UserData");
        if (!Directory.Exists(userDataFolder))
            Directory.CreateDirectory(userDataFolder);

        // forced but matches the archives sooo sorry lol
        string hytaleInstallation = getPathOfInterest(PayloadTarget.Hytale);
        string jreFolder = Path.Combine(getPathOfInterest(PayloadTarget.Jre), "java.exe");

        /*
         * .\HytaleClient.exe
         * --app-dir C:\Users\WDAGUtilityAccount\Desktop\latest
         * --user-dir C:\Users\WDAGUtilityAccount\AppData\Roaming\Hytale\UserData
         * --java-exec C:\Users\WDAGUtilityAccount\Desktop\jre\latest\bin\java.exe
         * --auth-mode offline
         * --uuid $name
         * --name $name
         */
        ProcessStartInfo psi = new ProcessStartInfo()
        {
            UseShellExecute = true,
            FileName = Path.Combine(hytaleInstallation, "Client", "HytaleClient.exe"),
            Arguments = $"--app-dir {hytaleInstallation} --user-dir {userDataFolder} --java-exec {jreFolder} --auth-mode offline --uuid {jsonSettings.Uuid.ToString()} --name {jsonSettings.Name}"
        };

        try
        {
            Process.Start(psi);
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message, "Failed to start HytaleClient.exe", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Application.Exit();
        }
    }

    private void Hytale_location_selectorOnClick(object? sender, EventArgs e)
    {
        initDownload(PayloadTarget.Hytale, hytale_location_selector,
            "Select the folder where you want to install Hytale (separate the folders for clarity between JRE and Hytale)",
            "Install Hytale", "Re-install Hytale");
    }

    private void Jre_location_selectorOnClick(object? sender, EventArgs e)
    {
        initDownload(PayloadTarget.Jre, jre_location_selector,
            "Select the folder where you want to install the JRE (separate the folders for clarity between JRE and Hytale)",
            "Install JRE", "Re-install JRE");
    }

    private string getPathOfInterest(PayloadTarget target)
    {
        return target switch
        {
            PayloadTarget.Hytale => Path.Combine(jsonSettings.HytaleLocation!, "latest"),
            PayloadTarget.Jre => Path.Combine(jsonSettings.JreLocation!, "latest", "bin"),
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
        };
    }

    private bool checkInstalled(PayloadTarget target)
    {
        return target switch
        {
            PayloadTarget.Hytale =>
                Path.Exists(
                    Path.Combine(getPathOfInterest(PayloadTarget.Hytale), "Client", "HytaleClient.exe")
                    ),
            PayloadTarget.Jre =>
                Path.Exists(
                    Path.Combine(getPathOfInterest(PayloadTarget.Jre), "java.exe")
                    ),
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
        };
    }

    private void setChangeablesState(bool state)
    {
        PayloadBtnState hytaleBtnState = (PayloadBtnState)hytale_location_selector.Tag!;
        PayloadBtnState jreBtnState = (PayloadBtnState)jre_location_selector.Tag!;

        hytale_location_selector.Enabled = hytaleBtnState != PayloadBtnState.ReInstall && state;
        jre_location_selector.Enabled = jreBtnState != PayloadBtnState.ReInstall && state;

        genuuid.Enabled = state;
        usernameb.Enabled = state;

        if (hytaleBtnState == PayloadBtnState.ReInstall && jreBtnState == PayloadBtnState.ReInstall)
            playbtn.Enabled = true;
    }

    // TODO: Move to another class
    private static string? pickFolder(string description = "", string? initial = null)
    {
        using var dialog = new FolderBrowserDialog();
        dialog.Description = description;
        dialog.UseDescriptionForTitle = true;
        dialog.SelectedPath = initial ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        return dialog.ShowDialog() == DialogResult.OK ? dialog.SelectedPath : null;
    }

    private static string computeSha256(string filePath)
    {
        using FileStream stream = File.OpenRead(filePath);
        using SHA256 sha = SHA256.Create();

        byte[] hash = sha.ComputeHash(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    // async someday, im sorry folks!
    private DynPayloadJson? fetchPayloadInfo()
    {
        using HttpClient client = new HttpClient();
        try
        {
            // hardcoded mb!!
            string json = client.GetStringAsync(
                    "https://pub-f3aea920c9fb44f28d610fd4d1435731.r2.dev/Cracks/hytalv1/PayloadDefinitions.json")
                .GetAwaiter()
                .GetResult();

            return JsonSerializer.Deserialize<DynPayloadJson>(json)!;
        }
        catch
        {
            progressBar1.ForeColor = Color.Red;
            progressBar1.Value = 100;
        }

        return null;
    }

    // should handle errors bruh
    private void initDownload(PayloadTarget targetPayload, Button targetBtn, string pickDesc, string onLocate = "", string onInstall = "")
    {
        PayloadBtnState state = (PayloadBtnState)targetBtn.Tag!;

        switch (state)
        {
            case PayloadBtnState.Locate:
                string? folderPick = pickFolder(pickDesc, appDataFolder);
                if (folderPick == null)
                    return;

                switch (targetPayload)
                {
                    case PayloadTarget.Hytale:
                        jsonSettings.HytaleLocation = folderPick;
                        hytale_location_show.Text = folderPick;
                        break;

                    case PayloadTarget.Jre:
                        jsonSettings.JreLocation = folderPick;
                        jre_location_show.Text = folderPick;
                        break;
                }
                writeSettings();

                targetBtn.Text = onLocate;
                targetBtn.Tag = PayloadBtnState.Install;
                break;

            case PayloadBtnState.Install:
                progressBar1.Value = 0;
                setChangeablesState(false);

                Payload payload = targetPayload switch
                {
                    PayloadTarget.Hytale => payloadJson.Hytale!,
                    PayloadTarget.Jre => payloadJson.Jre!,
                    _ => throw new ArgumentOutOfRangeException(nameof(targetPayload), targetPayload, null)
                };

                string payloadName = targetPayload switch
                {
                    PayloadTarget.Hytale => hytalepayloadname,
                    PayloadTarget.Jre => jrepayloadname,
                    _ => throw new ArgumentOutOfRangeException(nameof(targetPayload), targetPayload, null)
                };

                string payloadPath = Path.Combine(downloadFolder, payloadName);
                try
                {
                    download(payload.Url!, payloadPath);

                    if (computeSha256(payloadPath) != payload.Sha256)
                    {
                        File.Delete(payloadPath);
                        MessageBox.Show("Corrupted file", "Failed downloading the payload", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    decompress(targetPayload, payloadPath);

                    targetBtn.Text = onInstall;
                    targetBtn.Enabled = false;
                    targetBtn.Tag = PayloadBtnState.ReInstall;

                    setChangeablesState(true);
                    progressBar1.Value = 100;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                break;

            case PayloadBtnState.ReInstall:
                MessageBox.Show("If willing to reinstall the game, remove the extracted content in the selected folder and re-open the app.", "WIP", MessageBoxButtons.OK, MessageBoxIcon.Information);
                break;
        }
    }

    private void download(string url, string outputPath)
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

        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            output.Write(buffer, 0, read);
            readTotal += read;

            if (total.HasValue)
            {
                int percent = (int)Math.Min(readTotal * 100 / total.Value, 100);
                progressBar1.Invoke(() => progressBar1.Value = percent);
            }
        }
    }

    private void decompress(PayloadTarget targetPayload, string payloadPath)
    {
        string outPath = targetPayload switch
        {
            PayloadTarget.Hytale => jsonSettings.HytaleLocation!,
            PayloadTarget.Jre => jsonSettings.JreLocation!,
            _ => throw new ArgumentOutOfRangeException(nameof(targetPayload), targetPayload, null)
        };

        Thread thread = new Thread(() =>
        {
            using FileStream streamRead = File.OpenRead(payloadPath);
            using ArchiveFile archiveFile = new ArchiveFile(streamRead);
            archiveFile.Extract(outPath, true);
        })
        {
            IsBackground = true
        };
        thread.Start();
    }
}
