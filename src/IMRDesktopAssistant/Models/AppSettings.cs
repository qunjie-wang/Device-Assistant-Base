namespace IMRDesktopAssistant.Models;

public sealed class AppSettings
{
    public bool AutoStart { get; set; } = true;
    public int RightMargin { get; set; } = 16;
    public int BottomMargin { get; set; } = 16;
    public int DeviceRefreshSeconds { get; set; } = 60;

    public bool HasCustomPosition { get; set; }
    public double WindowLeft { get; set; }
    public double WindowTop { get; set; }

    public double IdleOpacity { get; set; } = 0.42;
    public double HoverOpacity { get; set; } = 1.0;
    public double IdleBlurRadius { get; set; } = 1.4;
}
