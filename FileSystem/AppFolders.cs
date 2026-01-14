using System.Security.Cryptography;
using HytaleDownloader.Configuration;
using HytaleDownloader.Enums;

namespace HytaleDownloader.FileSystem;

public static class AppFolders
{
    public static readonly string APP_DATA_FOLDER = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HytaleDownloader");
    public static readonly string DOWNLOAD_FOLDER = Path.Combine(APP_DATA_FOLDER, "Downloads"); // separate the origin to the extraction (installation) folder

    public static void Initialize()
    {
        CreateIfMissing(APP_DATA_FOLDER);
        CreateIfMissing(DOWNLOAD_FOLDER);
    }

    public static void CreateIfMissing(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    public static bool IsInstalled(PayloadTarget target)
    {
        switch (target)
        {
            case PayloadTarget.Hytale:
                string hytaleExec = Path.Combine(GetPathOfInterest(PayloadTarget.Hytale), "Client", "HytaleClient.exe");
                bool htlExists = File.Exists(hytaleExec);
                if (!htlExists)
                    return false;

                // just in case the size of the file is 0kb meaning that 7z is still uncompressing the file
                FileStream hytaleExecStream = File.OpenRead(hytaleExec);
                bool hytaleUnc = hytaleExecStream.Length > 0;
                hytaleExecStream.Close();

                return hytaleUnc;

            case PayloadTarget.Jre:
                string javaExec = Path.Combine(GetPathOfInterest(PayloadTarget.Jre), "java.exe");
                bool javaExists = File.Exists(javaExec);
                if (!javaExists)
                    return false;

                FileStream javaExecStream = File.OpenRead(javaExec);
                bool javaUnc = javaExecStream.Length > 0;
                javaExecStream.Close();

                return javaUnc;
        }

        return false;
    }

    public static string GetPathOfInterest(PayloadTarget target) => target switch
    {
        PayloadTarget.Hytale => Path.Combine(Config.BackedConfig.HytaleLocation!, "latest"),
        PayloadTarget.Jre => Path.Combine(Config.BackedConfig.JreLocation!, "latest", "bin"),
        _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
    };

    public static string? PickFolder(string description = "", string? initial = null)
    {
        using var dialog = new FolderBrowserDialog();
        dialog.Description = description;
        dialog.UseDescriptionForTitle = true;
        dialog.InitialDirectory = initial ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        return dialog.ShowDialog() == DialogResult.OK ? dialog.SelectedPath : null;
    }

    public static string ComputeSha256(string filePath)
    {
        using FileStream stream = File.OpenRead(filePath);
        using SHA256 sha = SHA256.Create();

        byte[] hash = sha.ComputeHash(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
