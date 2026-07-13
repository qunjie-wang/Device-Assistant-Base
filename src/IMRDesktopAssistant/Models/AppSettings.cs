namespace IMRDesktopAssistant.Models;

public sealed class AppSettings
{
    public bool AutoStart { get; set; } = true;
    public int RightMargin { get; set; } = 16;
    public int BottomMargin { get; set; } = 16;
    public int DeviceRefreshSeconds { get; set; } = 60;
}
