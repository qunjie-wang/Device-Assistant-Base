using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using IMRDesktopAssistant.Models;

namespace IMRDesktopAssistant.Services;

public sealed class DeviceInfoService
{
    public DeviceInfo GetCurrent()
    {
        var computerName = Environment.MachineName;

        var adapter = NetworkInterface.GetAllNetworkInterfaces()
            .Where(IsUsable)
            .OrderByDescending(HasIpv4Gateway)
            .ThenByDescending(n => n.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
            .ThenByDescending(n => n.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
            .FirstOrDefault();

        if (adapter is null)
        {
            return new DeviceInfo(computerName, "未连接", "未连接");
        }

        var properties = adapter.GetIPProperties();
        var ipv4 = properties.UnicastAddresses
            .Select(item => item.Address)
            .FirstOrDefault(address => address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(address));

        var macBytes = adapter.GetPhysicalAddress().GetAddressBytes();
        var mac = macBytes.Length == 0
            ? "未获取"
            : string.Join("-", macBytes.Select(value => value.ToString("X2")));

        return new DeviceInfo(
            computerName,
            ipv4?.ToString() ?? "未获取",
            mac);
    }

    private static bool IsUsable(NetworkInterface adapter)
    {
        if (adapter.OperationalStatus != OperationalStatus.Up)
        {
            return false;
        }

        if (adapter.NetworkInterfaceType is NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel)
        {
            return false;
        }

        return adapter.GetIPProperties().UnicastAddresses.Any(item =>
            item.Address.AddressFamily == AddressFamily.InterNetwork &&
            !IPAddress.IsLoopback(item.Address));
    }

    private static bool HasIpv4Gateway(NetworkInterface adapter)
    {
        return adapter.GetIPProperties().GatewayAddresses.Any(item =>
            item.Address.AddressFamily == AddressFamily.InterNetwork &&
            !item.Address.Equals(IPAddress.Any));
    }
}
