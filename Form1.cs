using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;

namespace HytaleDownloader;

// TODO: Better error management and logging?
public partial class Form1 : Form
{
    private const string savefilename = "config.json";
    private readonly string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HytaleDownloader");
    private readonly JsonSettings jsonSettings;

    public Form1()
    {
        InitializeComponent();

        if (!Directory.Exists(appDataFolder))
            Directory.CreateDirectory(appDataFolder);

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
        DynPayloadJson? payloadInfo = fetchPayloadInfo();
        if (payloadInfo == null)
        {
            MessageBox.Show("Failed to fetch payload info", "Fatal failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }

        // uhh save cache? not for now lolz

        usernameb.Text = jsonSettings.Name;
        uuid_shower.Text = jsonSettings.Uuid.ToString();

        // validating
        if (string.IsNullOrWhiteSpace(usernameb.Text))
            return;

        // uuid is always gonna be present but we gonna ignore that lmao

        if (jsonSettings.HytaleLocation == null)
            return;
        hytale_location_show.Text = jsonSettings.HytaleLocation;

        if (jsonSettings.JreLocation == null)
            return;
        jre_location_show.Text = jsonSettings.JreLocation;

        progressBar1.Value = 100;
        playbtn.Enabled = true;
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
        string hytaleInstallation = Path.Combine(jsonSettings.HytaleLocation!, "latest");
        string jreFolder = Path.Combine(jsonSettings.JreLocation!, "latest", "bin");

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

    private static string computeSha256(string filePath)
    {
        using FileStream stream = File.OpenRead(filePath);
        using SHA256 sha = SHA256.Create();

        byte[] hash = sha.ComputeHash(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
