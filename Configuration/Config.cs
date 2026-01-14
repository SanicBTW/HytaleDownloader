using System.Text.Json;
using HytaleDownloader.Enums;
using HytaleDownloader.FileSystem;
using HytaleDownloader.Threading;

// ReSharper disable MemberCanBePrivate.Global

namespace HytaleDownloader.Configuration;

public static class Config
{
    public static readonly string CONFIG_PATH = Path.Combine(AppFolders.APP_DATA_FOLDER, Constants.CONFIG_FILENAME);

    public static ConfigStruct BackedConfig { get; private set; } = null!;
    private static ScheduledDelegate? lastSave;

    public static void Initialize()
    {
        if (File.Exists(CONFIG_PATH))
        {
            using StreamReader streamReader = File.OpenText(CONFIG_PATH);
            string content = streamReader.ReadToEnd();
            BackedConfig = JsonSerializer.Deserialize<ConfigStruct>(content)!;
            return;
        }

        // write for the first time
        BackedConfig = new ConfigStruct();
        ScheduleSave();
    }

    // just like osu!frameworK?!?!?!?
    public static void ScheduleSave()
    {
        if (lastSave is not null && lastSave is { Cancelled: false })
        {
            lastSave.Cancel();
            lastSave = null;
        }

        lastSave = Scheduler.AddDelayed(applyChanges, 500);
    }

    // has too much functionality...
    public static string? Validate()
    {
        if (string.IsNullOrWhiteSpace(BackedConfig.Name))
            return "Can't play without a username!";

        if (string.IsNullOrWhiteSpace(BackedConfig.Uuid.ToString()))
            return "Can't play without a UUID!";

        if (string.IsNullOrWhiteSpace(BackedConfig.HytaleLocation))
            return "Can't play without a Hytale location!";

        if (!AppFolders.IsInstalled(PayloadTarget.Hytale))
            return "Couldn't find HytaleClient.exe or the app is still uncompressing the payload, " +
                   "please re-install the game if this error keeps showing up.";

        if (string.IsNullOrWhiteSpace(BackedConfig.JreLocation))
            return "Can't play without the JRE location!";

        if (!AppFolders.IsInstalled(PayloadTarget.Jre))
            return "Couldn't find java.exe or the app is still uncompressing the payload, " +
                   "please re-install the Java Runtime Environment provided by the app if this error keeps showing up.\n" +
                   "This is needed to play offline worlds since it runs on the server made in Java.";

        // nothing to cry about, continue ahead
        return null;
    }

    // should use an open stream to write without issues but i should be fine
    private static void applyChanges()
    {
        string content = JsonSerializer.Serialize(BackedConfig, new JsonSerializerOptions() { WriteIndented = true });
        using StreamWriter streamWriter = File.CreateText(CONFIG_PATH);
        streamWriter.Write(content);
        streamWriter.Close();
    }
}
