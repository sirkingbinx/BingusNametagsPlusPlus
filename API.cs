using System;
using System.Linq;
using System.Reflection;
using BingusNametagsPlusPlus.APIClasses;
using BingusNametagsPlusPlus.Attributes;
using BingusNametagsPlusPlus.Classes;
using BingusNametagsPlusPlus.Interfaces;
using BingusNametagsPlusPlus.Utilities;

namespace BingusNametagsPlusPlus;

/// <summary>
/// The BingusNametags++ API to allow implementation of nametag features by external mods.
/// </summary>
public static class API
{
    /// <summary>
    /// Register a new nametag with BingusNametags++ and return a ManagedNametag instance for management.
    /// </summary>
    /// <param name="nametag">Your nametag casted into an IBaseNametag.</param>
    /// <returns>A ManagedNametag instance.</returns>
    public static ManagedNametag CreateNametag(IBaseNametag nametag)
    {
        if (PluginManager.Plugins.Contains(nametag))
            throw new ArgumentException("This plugin was already initialized.");
        
        Type nametagType = nametag.GetType();

        const BindingFlags everything = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        var allNametagsInPlugin = nametagType.GetMethods(everything)
            .Where(f => f.IsDefined(typeof(BingusNametagsNametag), false))
            .ToList();

        var metadata = nametagType.GetCustomAttribute<BingusNametagsPlugin>();

        allNametagsInPlugin.ForEach(nametagInfo =>
        {
            var attribute = nametagInfo.GetCustomAttribute(typeof(BingusNametagsNametag), false) as BingusNametagsNametag;
            var updateFunc = (Action<PlayerNametag>)Delegate.CreateDelegate(typeof(Action<PlayerNametag>), nametagInfo);

            if (attribute != null)
                metadata.Nametags.Add(attribute, updateFunc);
        });

        PluginManager.PluginMetadata.Add(nametag, metadata);

        if (metadata == null)
            throw new ArgumentNullException("Your plugin {nametagType.Name} is missing a BingusNametagsPlugin attribute, which defines crucial metadata for your nametag.");

        LogManager.Log($"Loaded nametag {nametag.Metadata.Name}");

        PluginManager.Plugins.Add(nametag);

        return new ManagedNametag()
        {
            Nametag = nametag
        };
    }

    public static ManagedNametag CreateNametag<T>() where T : IBaseNametag
        => CreateNametag(Activator.CreateInstance<T>());
}
