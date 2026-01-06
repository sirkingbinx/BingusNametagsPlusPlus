using BingusNametagsPlusPlus.Components;
using BingusNametagsPlusPlus.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Debug = UnityEngine.Debug;

namespace BingusNametagsPlusPlus.Utilities;

/// <summary>
/// A dependable class for loading, managing, and changing plugins.
/// </summary>
public static class PluginManager
{
    /// <summary>
    /// All currently loaded IBaseNametags.
    /// </summary>
    public static List<IBaseNametag> Plugins = [];

    /// <summary>
    /// Any failures caused while loading plugins.
    /// </summary>
    public static List<string> PluginFailures = [];

    /// <summary>
    /// Safely enable the specified IBaseNametag, warning the user about any unsupported nametags. The success of this function depends if there are any unsupported nametags enabled.
    /// </summary>
    /// <param name="plugin">The plugin to enable.</param>
    public static void EnablePlugin(IBaseNametag plugin)
    {
        var allEnabledPlugins = Plugins.Where(a => a.Enabled);
        var allEnabledNames = allEnabledPlugins.Select(a => a.Name);
        var unsupported = plugin.Unsupported.Where(unsupportedName => allEnabledNames.Contains(unsupportedName));

        if (!unsupported.Any())
        {
            plugin.Enabled = true;
            Main.UpdateNametags += plugin.Update;
            return;
        }

        var unsupportedList = "";
        unsupported.ForEach(unsupportedPlugin => unsupportedList += (unsupportedList == "" ? unsupportedPlugin : $", {unsupportedPlugin}"));

        UIManager.Ask(
            $"The nametag you want to enable is incompatable with the following <i>enabled</i> nametags:\n\n{unsupportedList}\n\nAre you sure you want to enable this nametag?",
            ["Yes", "No"],
            answer =>
            {
                if (answer == "No")
                    return;

                plugin.Enabled = true;
                Main.UpdateNametags += plugin.Update;

                foreach (var plugin in allEnabledPlugins.Where(a => unsupported.Contains(a.Name)))
                    plugin.Enabled = false;
            }
        );
    }

    /// <summary>
    /// Disable the specified IBaseNametag.
    /// </summary>
    /// <param name="plugin">The plugin to disable.</param>
    public static void DisablePlugin(IBaseNametag plugin)
    {
        plugin.Enabled = false;
        try { Main.UpdateNametags -= plugin.Update; }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    /// <summary>
    /// The path to the BingusNametags++ Nametags folder.
    /// </summary>
    public static string BingusNametagsPath =>
        Path.Combine(Constants.BingusNametagsData, "nametags");

    /// <summary>
    /// Load all nametags from the default folder.
    /// </summary>
    public static void LoadFromDefaultFolder() =>
        LoadNametagsFromFolder(BingusNametagsPath);

    /// <summary>
    /// Open the default nametags folder.
    /// </summary>
    public static void OpenNametagsFolder() =>
        Process.Start(new ProcessStartInfo
        {
            FileName = BingusNametagsPath,
            UseShellExecute = true,
            Verb = "open"
        });

    /// <summary>
    /// Load all nametags from the specified folder.
    /// </summary>
    /// <param name="folderPath">The path to the folder to load plugins from.</param>
    public static void LoadNametagsFromFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            return;
        }

        foreach (var file in Directory.GetFiles(folderPath, "*.dll", SearchOption.AllDirectories))
        {
            try
            {
                var assembly = Assembly.LoadFile(file);
                LoadNametagsFromAssemblies( [assembly] );
            }
            catch (Exception ex)
            {
                Debug.Log($"The file {file} could not be loaded correctly: {ex.Message}");
                PluginFailures.Add($"[{file}]: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Load all detectable nametags from the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to load plugins from.</param>
    public static void LoadNametagsFromAssembly(Assembly assembly) =>
        LoadNametagsFromAssemblies([assembly]);

    /// <summary>
    /// Load all detectable nametags from the specified assemblies.
    /// </summary>
    /// <param name="assemblies">List of plugins to load nametags from.</param>
    public static void LoadNametagsFromAssemblies(Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            var nametagTypes = assembly.GetTypes()
                .Where(type => typeof(IBaseNametag).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);

            foreach (var nametagType in nametagTypes)
            {
                try
                {
                    if (Activator.CreateInstance(nametagType) is not IBaseNametag nametag)
                        return;

                    Debug.Log($"Loaded nametag {nametag.Name}");

                    Plugins.Add(nametag);
                }
                catch (Exception ex)
                {
                    PluginFailures.Add($"[{nametagType.Name}]: {ex.Message}");
                }
            }
        }
    }
}