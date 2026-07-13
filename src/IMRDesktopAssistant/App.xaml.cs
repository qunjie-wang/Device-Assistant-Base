using System.Threading;
using System.Windows;

namespace IMRDesktopAssistant;

public partial class App : Application
{
    private Mutex? _singleInstanceMutex;
    private AppController? _controller;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _singleInstanceMutex = new Mutex(true, "IMRDesktopAssistant.SingleInstance", out var createdNew);
        if (!createdNew)
        {
            Shutdown();
            return;
        }

        _controller = new AppController();
        _controller.Start();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _controller?.Dispose();
        _singleInstanceMutex?.Dispose();
        base.OnExit(e);
    }
}
