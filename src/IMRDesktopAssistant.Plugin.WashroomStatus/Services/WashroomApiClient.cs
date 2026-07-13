using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using IMRDesktopAssistant.Plugin.WashroomStatus.Models;

namespace IMRDesktopAssistant.Plugin.WashroomStatus.Services;

public sealed class WashroomApiClient : IDisposable
{
    private readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(5)
    };

    public async Task<IReadOnlyDictionary<int, StallState>> GetStatesAsync(
        string apiUrl,
        CancellationToken cancellationToken)
    {
        var json = await _httpClient.GetStringAsync(apiUrl, cancellationToken);
        using var document = JsonDocument.Parse(json);

        var result = Enumerable.Range(1, 4)
            .ToDictionary(index => index, _ => StallState.Offline);

        var list = FindList(document.RootElement);
        if (list.ValueKind != JsonValueKind.Array)
        {
            return result;
        }

        foreach (var item in list.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var idText = GetText(item, "stallId", "stall_id", "id", "name");
            var id = ParseStallId(idText);
            if (id is < 1 or > 4)
            {
                continue;
            }

            var statusText = GetText(item, "status", "state", "occupancy", "value");
            result[id.Value] = NormalizeStatus(statusText);
        }

        return result;
    }

    private static JsonElement FindList(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            return root;
        }

        if (root.ValueKind != JsonValueKind.Object)
        {
            return default;
        }

        foreach (var key in new[] { "stalls", "data", "result", "items", "list" })
        {
            if (root.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.Array)
            {
                return value;
            }
        }

        return default;
    }

    private static string GetText(JsonElement item, params string[] names)
    {
        foreach (var name in names)
        {
            if (!item.TryGetProperty(name, out var value))
            {
                continue;
            }

            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString() ?? string.Empty,
                JsonValueKind.Number => value.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => value.ToString()
            };
        }

        return string.Empty;
    }

    private static int? ParseStallId(string value)
    {
        if (int.TryParse(value, out var direct))
        {
            return direct;
        }

        var match = Regex.Match(value ?? string.Empty, @"\d+");
        return match.Success && int.TryParse(match.Value, out var parsed)
            ? parsed
            : null;
    }

    private static StallState NormalizeStatus(string value)
    {
        var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();

        return normalized switch
        {
            "idle" or "free" or "off" or "false" or "0" or "clear" or
                "vacant" or "available" or "无人" or "空闲" => StallState.Free,

            "occupied" or "busy" or "on" or "true" or "1" or "presence" or
                "present" or "detected" or "有人" or "占用" => StallState.Occupied,

            _ => StallState.Offline
        };
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
