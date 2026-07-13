namespace IMRDesktopAssistant.Models;

public sealed record DeviceInfo(string ComputerName, string IpAddress, string MacAddress)
{
    public static DeviceInfo Empty { get; } = new("--", "--", "--");
}
