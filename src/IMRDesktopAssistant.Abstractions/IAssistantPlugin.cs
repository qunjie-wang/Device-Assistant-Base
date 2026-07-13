using System.Windows;

namespace IMRDesktopAssistant.Abstractions;

public interface IAssistantPlugin : IDisposable
{
    string Id { get; }
    string DisplayName { get; }
    FrameworkElement View { get; }

    Task StartAsync(CancellationToken cancellationToken);
    Task RefreshAsync();
    Task StopAsync();
}
