using System;
using System.Collections.Generic;
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
    /// <param name="nametag">Your nametag cast into an IBaseNametag.</param>
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
            throw new ArgumentNullException($"Your plugin {nametagType.Name} is missing a BingusNametagsPlugin attribute, which defines crucial metadata for your nametag.");

        LogManager.Log($"Loaded nametag {nametag.Metadata.Name}");

        PluginManager.Plugins.Add(nametag);

        return new ManagedNametag()
        {
            Nametag = nametag
        };
    }
    
    private static Dictionary<VRRig, Platform> _cachedPlatforms = [ ];

    public static Platform GetPlatform(VRRig rig)
    {
        var cosmetics = rig.cosmeticSet.ToDisplayNameArray().Select(n => n.ToLower()).ToList();
        var platform = Platform.Unknown;
        
        if (!rig.InitializedCosmetics)
        {
            platform = Platform.Unknown;
            goto end;
        }
        
        if (_cachedPlatforms.TryGetValue(rig, out var cachedPlatform))
            return cachedPlatform;

        if (rig.currentRankedSubTierPC > 0)
            platform = Platform.PCBasedPlatform;

        if (rig.currentRankedSubTierQuest > 0)
        {
            platform = Platform.Quest;
            goto end;
        }
        
        var properties = rig.Creator.GetPlayerRef().CustomProperties.Count;

        if (cosmetics.Contains("s. first login"))
            platform = Platform.SteamVR;
        
        if (cosmetics.Contains("first login") || properties > 1)
            platform = Platform.OculusRift;

        end:
        
        if (platform != Platform.Unknown)
            _cachedPlatforms[rig] = platform;
        
        return platform;
    }

    public static void ClearPlatformCache()
    {
        _cachedPlatforms.Clear();
    }
    
    public enum Platform
    {
        /// <summary>
        /// A Steam player.
        /// </summary>
        SteamVR,
        
        /// <summary>
        /// An Oculus Rift player.
        /// </summary>
        OculusRift,
        
        /// <summary>
        /// Unknown which platform, but is a PC-based platform. This will be presented as Oculus Rift to default nametag users.
        /// </summary>
        PCBasedPlatform,
        
        /// <summary>
        /// Meta Quest users.
        /// </summary>
        Quest,
        
        /// <summary>
        /// Unknown platform. May be waiting for initialization.
        /// </summary>
        Unknown = -1,
    }

    public static ManagedNametag CreateNametag<T>() where T : IBaseNametag
        => CreateNametag(Activator.CreateInstance<T>());
}
