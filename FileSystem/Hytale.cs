using System.Diagnostics;
using System.Text;
using HytaleDownloader.Configuration;
using HytaleDownloader.Enums;
using HytaleDownloader.Threading;
// ReSharper disable MemberCanBePrivate.Global

namespace HytaleDownloader.FileSystem;

public static class Hytale
{
    public static readonly string HYTALE_FOLDER = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Hytale");
    public static readonly string HYTALE_USER_DATA = Path.Combine(HYTALE_FOLDER, "UserData");

    public static void Run()
    {
        // yea we doin last minute checks here bruh
        string? validation = Config.Validate();
        if (validation != null)
        {
            MessageBox.Show(validation, "Validation error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        AppFolders.CreateIfMissing(HYTALE_FOLDER);
        AppFolders.CreateIfMissing(HYTALE_USER_DATA);

        // forced but matches the archives sooo sorry lol
        string hytaleInstallation = AppFolders.GetPathOfInterest(PayloadTarget.Hytale);
        string jreInstallation = Path.Combine(AppFolders.GetPathOfInterest(PayloadTarget.Jre), "java.exe");

        // god I LOVE STRING BUILDERS
        StringBuilder argBuilder = new StringBuilder();
        argBuilder.Append($"--app-dir {hytaleInstallation} ");
        argBuilder.Append($"--user-dir {HYTALE_USER_DATA} ");
        argBuilder.Append($"--java-exec {jreInstallation} ");
        argBuilder.Append("--auth-mode offline ");
        argBuilder.Append($"--uuid {Config.BackedConfig.Uuid.ToString()} ");
        argBuilder.Append($"--name {Config.BackedConfig.Name}");
        string arguments = argBuilder.ToString();

        /*
         * .\HytaleClient.exe
         * --app-dir C:\Users\WDAGUtilityAccount\Desktop\latest
         * --user-dir C:\Users\WDAGUtilityAccount\AppData\Roaming\Hytale\UserData
         * --java-exec C:\Users\WDAGUtilityAccount\Desktop\jre\latest\bin\java.exe
         * --auth-mode offline
         * --uuid $uuid
         * --name $name
         */
        ProcessStartInfo psi = new ProcessStartInfo()
        {
            UseShellExecute = true,
            FileName = Path.Combine(hytaleInstallation, "Client", "HytaleClient.exe"),
            Arguments = arguments
        };

        try
        {
            Process.Start(psi);
            Scheduler.AddDelayed(Application.Exit, 2000);
        }
        catch (Exception e)
        {
            DialogResult res = MessageBox.Show(e.Message, $"Failed to start {Path.GetFileName(psi.FileName)}", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
            switch (res)
            {
                case DialogResult.Retry:
                    Run();
                    break;

                case DialogResult.Cancel:
                    // do nothing lol
                    break;
            }
        }
    }
}
