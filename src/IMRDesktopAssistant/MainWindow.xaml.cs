using System.Windows;
using IMRDesktopAssistant.Abstractions;
using IMRDesktopAssistant.Models;

namespace IMRDesktopAssistant;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public void SetDeviceInfo(DeviceInfo info)
    {
        ComputerNameText.Text = info.ComputerName;
        ComputerNameText.ToolTip = info.ComputerName;
        IpAddressText.Text = info.IpAddress;
        IpAddressText.ToolTip = info.IpAddress;
        MacAddressText.Text = info.MacAddress;
        MacAddressText.ToolTip = info.MacAddress;
    }

    public void SetPlugins(IEnumerable<IAssistantPlugin> plugins)
    {
        PluginHost.Children.Clear();

        foreach (var plugin in plugins)
        {
            PluginHost.Children.Add(plugin.View);
        }

        PluginHost.Visibility = PluginHost.Children.Count > 0
            ? Visibility.Visible
            : Visibility.Collapsed;
    }
}
