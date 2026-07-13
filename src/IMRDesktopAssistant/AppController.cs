using System.Drawing;
using System.Windows;
using System.Windows.Threading;
using IMRDesktopAssistant.Abstractions;
using IMRDesktopAssistant.Models;
using IMRDesktopAssistant.Services;
using WinForms = System.Windows.Forms;

namespace IMRDesktopAssistant;

public sealed class AppController : IDisposable
{
    private readonly SettingsService _settingsService = new();
    private readonly DeviceInfoService _deviceInfoService = new();
    private readonly PluginLoader _pluginLoader = new();
    private readonly CancellationTokenSource _pluginCancellation = new();

    private AppSettings _settings = new();
    private MainWindow? _window;
    private WinForms.NotifyIcon? _trayIcon;
    private DispatcherTimer? _deviceTimer;
    private IReadOnlyList<IAssistantPlugin> _plugins = Array.Empty<IAssistantPlugin>();
    private bool _disposed;

    public void Start()
    {
        _settings = _settingsService.Load();
        if (_settings.AutoStart && !StartupService.IsEnabled())
        {
            StartupService.SetEnabled(true);
        }

        _window = new MainWindow();
        _window.SetDeviceInfo(_deviceInfoService.GetCurrent());

        _plugins = _pluginLoader.Load(_settingsService.PluginsDirectory);
        _window.SetPlugins(_plugins);

        foreach (var plugin in _plugins)
        {
            _ = plugin.StartAsync(_pluginCancellation.Token);
        }

        CreateTrayIcon();
        CreateDeviceRefreshTimer();
    }

    private void CreateTrayIcon()
    {
        var menu = new WinForms.ContextMenuStrip();
        menu.Items.Add("显示 / 隐藏", null, (_, _) => ToggleWindow());
        menu.Items.Add("立即刷新", null, async (_, _) => await RefreshAllAsync());

        var autoStartItem = new WinForms.ToolStripMenuItem("开机自动启动")
        {
            Checked = StartupService.IsEnabled(),
            CheckOnClick = true
        };
        autoStartItem.CheckedChanged += (_, _) =>
        {
            StartupService.SetEnabled(autoStartItem.Checked);
            _settings.AutoStart = autoStartItem.Checked;
            _settingsService.Save(_settings);
        };
        menu.Items.Add(autoStartItem);
        menu.Items.Add(new WinForms.ToolStripSeparator());
        menu.Items.Add("退出", null, (_, _) => Application.Current.Shutdown());

        _trayIcon = new WinForms.NotifyIcon
        {
            Icon = SystemIcons.Information,
            Text = "IMR 设备助手",
            Visible = true,
            ContextMenuStrip = menu
        };

        _trayIcon.MouseClick += (_, args) =>
        {
            if (args.Button == WinForms.MouseButtons.Left)
            {
                ToggleWindow();
            }
        };
    }

    private void CreateDeviceRefreshTimer()
    {
        _deviceTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(Math.Max(10, _settings.DeviceRefreshSeconds))
        };
        _deviceTimer.Tick += (_, _) => RefreshDeviceInfo();
        _deviceTimer.Start();
    }

    private void ToggleWindow()
    {
        if (_window is null)
        {
            return;
        }

        if (_window.IsVisible)
        {
            _window.Hide();
            return;
        }

        RefreshDeviceInfo();
        PositionWindow();
        _window.Show();
        _window.Topmost = true;
    }

    private void PositionWindow()
    {
        if (_window is null)
        {
            return;
        }

        _window.UpdateLayout();
        var workArea = SystemParameters.WorkArea;
        var width = _window.ActualWidth > 0 ? _window.ActualWidth : _window.Width;
        var height = _window.ActualHeight > 0 ? _window.ActualHeight : 160;

        _window.Left = workArea.Right - width - _settings.RightMargin;
        _window.Top = workArea.Bottom - height - _settings.BottomMargin;
    }

    private void RefreshDeviceInfo()
    {
        _window?.SetDeviceInfo(_deviceInfoService.GetCurrent());
    }

    private async Task RefreshAllAsync()
    {
        RefreshDeviceInfo();
        foreach (var plugin in _plugins)
        {
            try
            {
                await plugin.RefreshAsync();
            }
            catch
            {
                // 插件刷新失败不影响主程序。
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _deviceTimer?.Stop();
        _pluginCancellation.Cancel();

        foreach (var plugin in _plugins)
        {
            try
            {
                plugin.StopAsync().GetAwaiter().GetResult();
            }
            catch
            {
                // 退出时忽略插件释放异常。
            }
            plugin.Dispose();
        }

        if (_trayIcon is not null)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }

        _window?.Close();
        _pluginCancellation.Dispose();
    }
}
