using System.Reflection;
using IMRDesktopAssistant.Abstractions;

namespace IMRDesktopAssistant.Services;

public sealed class PluginLoader
{
    public IReadOnlyList<IAssistantPlugin> Load(string pluginsDirectory)
    {
        Directory.CreateDirectory(pluginsDirectory);
        var plugins = new List<IAssistantPlugin>();

        foreach (var dllPath in Directory.EnumerateFiles(
                     pluginsDirectory,
                     "IMRDesktopAssistant.Plugin.*.dll",
                     SearchOption.AllDirectories))
        {
            try
            {
                var assembly = Assembly.LoadFrom(dllPath);
                var pluginTypes = assembly.GetTypes()
                    .Where(type =>
                        !type.IsAbstract &&
                        typeof(IAssistantPlugin).IsAssignableFrom(type) &&
                        type.GetConstructor(Type.EmptyTypes) is not null);

                foreach (var pluginType in pluginTypes)
                {
                    if (Activator.CreateInstance(pluginType) is IAssistantPlugin plugin)
                    {
                        plugins.Add(plugin);
                    }
                }
            }
            catch
            {
                // 单个插件损坏时不阻止主程序启动。
            }
        }

        return plugins;
    }
}
