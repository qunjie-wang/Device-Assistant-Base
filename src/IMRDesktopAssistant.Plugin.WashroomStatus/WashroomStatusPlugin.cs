using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using IMRDesktopAssistant.Abstractions;
using IMRDesktopAssistant.Plugin.WashroomStatus.Models;
using IMRDesktopAssistant.Plugin.WashroomStatus.Services;

namespace IMRDesktopAssistant.Plugin.WashroomStatus;

public sealed class WashroomStatusPlugin : IAssistantPlugin
{
    private readonly WashroomStatusView _view = new();
    private readonly WashroomApiClient _client = new();
    private CancellationTokenSource? _linkedCancellation;
    private Task? _backgroundTask;
    private WashroomSettings _settings = new();

    public string Id => "washroom-status";
    public string DisplayName => "洗手间状态";
    public FrameworkElement View => _view;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _settings = LoadSettings();
        _linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _backgroundTask = RunLoopAsync(_linkedCancellation.Token);
        return Task.CompletedTask;
    }

    public async Task RefreshAsync()
    {
        await RefreshInternalAsync(CancellationToken.None);
    }

    public async Task StopAsync()
    {
        if (_linkedCancellation is null)
        {
            return;
        }

        _linkedCancellation.Cancel();
        if (_backgroundTask is not null)
        {
            try
            {
                await _backgroundTask;
            }
            catch (OperationCanceledException)
            {
            }
        }
    }

    private async Task RunLoopAsync(CancellationToken cancellationToken)
    {
        await RefreshInternalAsync(cancellationToken);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(Math.Max(1, _settings.RefreshSeconds)));
        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            await RefreshInternalAsync(cancellationToken);
        }
    }

    private async Task RefreshInternalAsync(CancellationToken cancellationToken)
    {
        IReadOnlyDictionary<int, StallState> states;
        try
        {
            states = await _client.GetStatesAsync(_settings.ApiUrl, cancellationToken);
        }
        catch
        {
            states = Enumerable.Range(1, 4)
                .ToDictionary(index => index, _ => StallState.Offline);
        }

        await Application.Current.Dispatcher.InvokeAsync(() => _view.UpdateStates(states));
    }

    private static WashroomSettings LoadSettings()
    {
        var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            ?? AppContext.BaseDirectory;
        var path = Path.Combine(assemblyDirectory, "washroom.json");

        if (!File.Exists(path))
        {
            var defaults = new WashroomSettings();
            File.WriteAllText(path, JsonSerializer.Serialize(defaults, new JsonSerializerOptions
            {
                WriteIndented = true
            }));
            return defaults;
        }

        try
        {
            return JsonSerializer.Deserialize<WashroomSettings>(
                       File.ReadAllText(path),
                       new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                   ?? new WashroomSettings();
        }
        catch
        {
            return new WashroomSettings();
        }
    }

    public void Dispose()
    {
        _linkedCancellation?.Dispose();
        _client.Dispose();
    }
}
