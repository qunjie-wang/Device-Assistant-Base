using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using IMRDesktopAssistant.Plugin.WashroomStatus.Models;

namespace IMRDesktopAssistant.Plugin.WashroomStatus;

public partial class WashroomStatusView : UserControl
{
    private static readonly Brush FreeBrush = new SolidColorBrush(Color.FromRgb(105, 185, 137));
    private static readonly Brush OccupiedBrush = new SolidColorBrush(Color.FromRgb(216, 121, 121));
    private static readonly Brush OfflineBrush = new SolidColorBrush(Color.FromRgb(115, 123, 134));

    public WashroomStatusView()
    {
        InitializeComponent();
    }

    public void UpdateStates(IReadOnlyDictionary<int, StallState> states)
    {
        for (var id = 1; id <= 4; id++)
        {
            var state = states.TryGetValue(id, out var value) ? value : StallState.Offline;
            UpdateCircle(id, state);
        }
    }

    private void UpdateCircle(int id, StallState state)
    {
        var outer = (Ellipse)FindName($"Outer{id}");
        var inner = (Ellipse)FindName($"Inner{id}");
        var host = (Grid)outer.Parent;

        var brush = state switch
        {
            StallState.Free => FreeBrush,
            StallState.Occupied => OccupiedBrush,
            _ => OfflineBrush
        };

        outer.Stroke = brush;
        inner.Fill = brush;
        outer.Opacity = state == StallState.Offline ? 0.65 : 1;
        inner.Opacity = state == StallState.Offline ? 0.65 : 1;

        var text = state switch
        {
            StallState.Free => "空闲",
            StallState.Occupied => "占用",
            _ => "离线"
        };
        host.ToolTip = $"{id}号：{text}";
    }
}
