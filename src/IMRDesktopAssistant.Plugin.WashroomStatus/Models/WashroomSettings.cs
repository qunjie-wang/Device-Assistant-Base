namespace IMRDesktopAssistant.Plugin.WashroomStatus.Models;

public sealed class WashroomSettings
{
    public string ApiUrl { get; set; } = "http://192.168.4.196:3002/api/wc_status";
    public int RefreshSeconds { get; set; } = 3;
}
