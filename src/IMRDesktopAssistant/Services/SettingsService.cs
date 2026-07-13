using System.Text.Json;
using IMRDesktopAssistant.Models;

namespace IMRDesktopAssistant.Services;

public sealed class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public string AppDataDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "IMRDesktopAssistant");

    public string PluginsDirectory => Path.Combine(AppDataDirectory, "Plugins");

    private string SettingsPath => Path.Combine(AppDataDirectory, "config.json");

    public AppSettings Load()
    {
        Directory.CreateDirectory(AppDataDirectory);
        Directory.CreateDirectory(PluginsDirectory);

        if (!File.Exists(SettingsPath))
        {
            var defaults = new AppSettings();
            Save(defaults);
            return defaults;
        }

        try
        {
            return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(SettingsPath), JsonOptions)
                ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        Directory.CreateDirectory(AppDataDirectory);
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(settings, JsonOptions));
    }
}
