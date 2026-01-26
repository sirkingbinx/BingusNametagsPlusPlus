using BingusNametagsPlusPlus.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx.Bootstrap;
using BingusNametagsPlusPlus.Attributes;
using BingusNametagsPlusPlus.Classes;

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
    /// All currently loaded and enabled IBaseNametags.
    /// </summary>
    public static List<IBaseNametag> EnabledPlugins => [.. Plugins.Where(plugin => plugin.Metadata.Enabled)];

    /// <summary>
    /// The metadata associated with every plugin. This can be accessed directly with IBaseNametag.Metadata.
    /// </summary>
    public static Dictionary<IBaseNametag, BingusNametagsPlugin> PluginMetadata = [];

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
        var allEnabledPlugins = Plugins.Where(a => a.Metadata.Enabled);
        var allEnabledNames = allEnabledPlugins.Select(a => a.Metadata.Name);

        var unsupportedList = "";
        var unsupported = plugin.Metadata.Unsupported.Where(unsupportedName => allEnabledNames.Contains(unsupportedName)).ForEach(unsupportedPlugin => unsupportedList += (unsupportedList == "" ? unsupportedPlugin : $", {unsupportedPlugin}"));

        var disableStuff = true;

        if (unsupported.Any())
        {
            UIManager.Ask(
                $"The nametag you want to enable is incompatable with the following <i>enabled</i> nametags:\n\n{unsupportedList}\n\nAre you sure you want to enable this nametag?",
                ["Yes", "No"],
                answer => { disableStuff = (answer == "Yes"); }
            );
        }

        if (!disableStuff || (Main.UpdateNametags?.GetInvocationList().Contains(plugin.Update) ?? false))
            return;

        plugin.Metadata.Enabled = true;
        Main.UpdateNametags += plugin.Update;

        foreach (var badPlugin in allEnabledPlugins.Where(a => unsupported.Contains(a.Metadata.Name)))
            DisablePlugin(badPlugin);
    }

    /// <summary>
    /// Disable the specified IBaseNametag.
    /// </summary>
    /// <param name="plugin">The plugin to disable.</param>
    public static void DisablePlugin(IBaseNametag plugin)
    {
        plugin.Metadata.Enabled = false;

        if (Main.UpdateNametags?.GetInvocationList().Contains(plugin.Update) ?? false)
        {
            try { Main.UpdateNametags -= plugin.Update; }
            catch (Exception ex)
            {
                ex.Report();
            }

            Main.Nametags[plugin].ForEach(rig => rig.Value.Destroy());
            Main.Nametags[plugin].Clear();
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
                LoadNametagsFromAssembly(assembly);
            }
            catch (Exception ex)
            {
                ex.Report();
                LogManager.Log($"The file {file} could not be loaded correctly: {ex.Message}");
                PluginFailures.Add($"[{file}]: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Load all detectable nametags from the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to load plugins from.</param>
    public static void LoadNametagsFromAssembly(Assembly assembly)
    {
        var nametagTypes = assembly.GetTypes()
            .Where(type => typeof(IBaseNametag).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);

        foreach (var nametagType in nametagTypes)
        {
            try
            {
                if (Activator.CreateInstance(nametagType) is not IBaseNametag nametag)
                    return;

                const BindingFlags everything = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
                var allNametagsInPlugin = nametagType.GetMethods(everything)
                    .Where(f => f.IsDefined(typeof(BingusNametagsNametag), false))
                    .ToList();

                var metadata = nametagType.GetCustomAttribute<BingusNametagsPlugin>();

                allNametagsInPlugin.ForEach(nametagMI =>
                {
                    var attribute = nametagMI.GetCustomAttribute(typeof(BingusNametagsNametag), false) as BingusNametagsNametag;
                    var updateFunc = (Action<PlayerNametag>)Delegate.CreateDelegate(typeof(Action<PlayerNametag>), nametagMI);
                    metadata.Nametags.Add(attribute, updateFunc);
                });

                PluginMetadata.Add(nametag, metadata);

                if (metadata == null)
                {
                    LogManager.Log($"Error: Your plugin {nametagType.Name} is missing a BingusNametagsPlugin attribute, which defines crucial metadata for your nametag.");
                    return;
                }

                LogManager.Log($"Loaded nametag {nametag.Metadata.Name}");

                Plugins.Add(nametag);
            }
            catch (Exception ex)
            {
                ex.Report();
                PluginFailures.Add($"[{nametagType.Name}]: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Load all detectable nametags from the specified assemblies.
    /// </summary>
    /// <param name="assemblies">List of plugins to load nametags from.</param>
    public static void LoadNametagsFromAssemblies(Assembly[] assemblies) =>
        assemblies.ForEach(LoadNametagsFromAssembly);

    /// <summary>
    /// Loads both nametag contexts at the same time, which saves about 0 seconds
    /// </summary>
    /// <returns></returns>
    public static async Task LoadNametags()
    {
        var usNametags = Task.Run(() =>
        {
            LogManager.Log("Loading nametags from BG++");
            // bepinex assemblies
            LoadNametagsFromAssembly(Assembly.GetExecutingAssembly());
            LogManager.Log("Loaded nametags from BG++");
        });

        var bepinexNametags = Task.Run(() =>
        {
            try
            {
                LogManager.Log("Loading nametags from BepInEx");
                // bepinex assemblies
                LoadNametagsFromAssemblies(Chainloader.PluginInfos.Values.Select(info => info.GetType().Assembly).AsArray());
                LogManager.Log("Loaded nametags from BepInEx");
            }
            catch (Exception ex)
            {
                LogManager.Log($"BepInEx nametag loading failed: {ex.Message}");
                ex.Report();
            }
        });

        var managedNametags = Task.Run(() =>
        {
            LogManager.Log("Loading nametags from data folder");
            LoadFromDefaultFolder();
            LogManager.Log("Loaded nametags from data folder");
        });

        await Task.WhenAll(usNametags, bepinexNametags, managedNametags);

        bepinexNametags.Dispose();
        managedNametags.Dispose();
    }
}