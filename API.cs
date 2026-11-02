using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BingusNametagsPlusPlus.Attributes;
using BingusNametagsPlusPlus.Classes;
using BingusNametagsPlusPlus.Classes.API;
using BingusNametagsPlusPlus.Interfaces;
using BingusNametagsPlusPlus.Utilities;
using UnityEngine;

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
            {
                metadata.Nametags.Add(attribute, updateFunc);
                Config.Current?.PluginOffsets.TryAdd($"{metadata.Author}.{metadata.Name}.{attribute.Name}", attribute.Offset);
            }
        });

        PluginManager.PluginMetadata.Add(nametag, metadata);

        if (metadata == null)
            throw new ArgumentNullException($"Your plugin {nametagType.Name} is missing a BingusNametagsPlugin attribute, which defines crucial metadata for your nametag.");

        LogManager.Log($"Loaded nametag {nametag.Metadata.Name}");

        PluginManager.Plugins.Add(nametag);

        if (Config.Current.EnabledPlugins.Split(";").Contains(nametag.Metadata.Name))
            PluginManager.EnablePlugin(nametag, true);

        return new ManagedNametag()
        {
            Nametag = nametag
        };
    }
    
    private static Dictionary<string, Platform> _cachedPlatforms = [ ];

    /// <summary>
    /// Gets the Platform enum of a VRRig.
    /// Note that this check is not 100% accurate. It is not possible to 100% guarantee the platform of another player, but you can get pretty close.
    /// </summary>
    /// <param name="rig">The rig to detect the platform of.</param>
    /// <returns>The Platform enum of rig.</returns>
    public static Platform GetPlatform(VRRig rig)
    {
        var platform = Platform.Unknown;
        
        if (!rig.InitializedCosmetics)
        {
            platform = Platform.Unknown;
            goto end;
        }
        
        if (_cachedPlatforms.TryGetValue(rig.Creator.UserId, out var cachedPlatform))
            return cachedPlatform;

        var properties = rig.Creator.GetPlayerRef().CustomProperties;
        var cosmetics = rig._playerOwnedCosmetics.Select(n => n.ToLower()).ToList();

        // Why did Gorilla Tag network this? No clue.
        // Yes, it's real. Check player properties.
        if (properties["platform"] is string platformString)
        {
            // 99% of games will use this code. Look at this part first
            if (platformString.ToLower() == "steam")
                platform = Platform.SteamVR;
            else if (platformString.ToLower() == "pc")
                platform = Platform.OculusRift;
            else if (platformString.ToLower() == "quest")
                platform = Platform.Quest;
            else
                platform = Platform.PCBasedPlatform;
            
            goto end;
        }

        // We still leave the old system here for a fallback
        if (rig.currentRankedSubTierPC > 0 || properties.Count > 1)
            platform = Platform.PCBasedPlatform;

        if (rig.currentRankedSubTierQuest > 0)
        {
            platform = Platform.Quest;
            goto end;
        }

        if (cosmetics.Contains("s. first login"))
            platform = Platform.SteamVR;
        
        if (cosmetics.Contains("first login"))
            platform = Platform.OculusRift;

        end:
        
        if (platform != Platform.Unknown)
            _cachedPlatforms[rig.Creator.UserId] = platform;
        
        return platform;
    }

    /// <summary>
    /// Clears the platform cache, forcing every new platform detection to refresh.
    /// </summary>
    public static void ClearPlatformCache()
    {
        _cachedPlatforms.Clear();
    }


    /// <summary>Legacy API retained for plugin compatibility. User icons are now assigned by the icon server.</summary>
    public static Badge[] GetBadgeData(VRRig rig) => [];

    /// <summary>Legacy compatibility hook. Server icons refresh automatically.</summary>
    public static void ClearBadgeCache() { }

    /// <summary>
    /// Represents a platform of a player.
    /// </summary>
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

    private static string GetPlatformSpriteId(Platform platform) => platform switch
    {
        API.Platform.Quest => "<sprite name=\"meta\">",
        API.Platform.OculusRift => "<sprite name=\"oculus\">",
        API.Platform.SteamVR => "<sprite name=\"steam\">",
        API.Platform.PCBasedPlatform => "<sprite name=\"oculus\">? ", // ≈ denotes "almost"
        _ => "? "
    };

    /// <summary>
    /// Badges owned by a user. These are displayed as icons on the default nametag.
    /// </summary>
    public enum Badge
    {
        /// <summary>
        /// The user is a developer of BingusNametags++. Exclusively for Bingus.
        /// </summary>
        Developer,

        /// <summary>
        /// The user is a beta tester for BingusNametags++.
        /// </summary>
        BetaTester
    }

    /// <summary>
    /// Gets the badge sprite ID of a badge for usage with the default nametag sprites.
    /// </summary>
    /// <param name="badge">The badge to get the sprite ID of.</param>
    /// <returns>A string to be used as a sprite ID. Use it with &lt;sprite name="[NAME HERE]"&gt;</returns>
    public static string GetBadgeSpriteId(Badge badge) => badge switch
    {
        Badge.Developer => "bingus",
        Badge.BetaTester => "beta",
        _ => ""
    };
    
    /// <summary>
    /// Returns the text that would be used by the Default nametag for the specified VRRig. This also accepts overrides for easy changes.
    /// </summary>
    /// <param name="rig">The rig which will be used for the nametag. If you would like to manually set all of the parameters, remove this argument.</param>
    /// <param name="overrideName">Override the name used on the nametag.</param>
    /// <param name="overridePlatform">Override the platform shown on the nametag.</param>
    /// <param name="overrideBadges">Override the badges displayed on the nametag.</param>
    /// <returns>A string to apply to a nametag's Text property, which will match the look of the default nametag.</returns>
    public static string GetDefaultNametagText(VRRig rig, string overrideName = "", Platform overridePlatform = Platform.Unknown, Badge[]? overrideBadges = null)
    {
        if (overrideName.IsNullOrEmpty())
            overrideName = Config.Current.SanitizeNicknames ? rig.playerText1.text : rig.Creator.NickName;
        
        if (overridePlatform == Platform.Unknown)
            overridePlatform = GetPlatform(rig);
        
        if (overrideBadges == null)
            overrideBadges = GetBadgeData(rig);

        return GetDefaultNametagText(name: overrideName, platform: overridePlatform, badges: overrideBadges);
    }

    /// <summary>
    /// Returns the text that would be used by the Default nametag with the specified parameters.
    /// </summary>
    /// <param name="name">The name to use.</param>
    /// <param name="platform">The platform of the player.</param>
    /// <param name="badges">The badges of the player.</param>
    /// <returns>A string to apply to a nametag's Text property, which will match the look of the default nametag.</returns>
    public static string GetDefaultNametagText(string name, Platform platform, Badge[] badges)
    {
        var prefix = "";

        if (Config.Current.Icons)
        {
            if (Config.Current.UserIcons) badges
                    .Select(badge => GetBadgeSpriteId(badge))
                    .ForEach(sprite => prefix += $"<sprite name=\"{sprite}\">");

            if (Config.Current.PlatformIcons)
                prefix += GetPlatformSpriteId(platform);
        }

        return prefix + name;
    }

    /// <summary>
    /// Create a ManagedNametag with the specified nametag.
    /// </summary>
    /// <typeparam name="T">The nametag to initialize. Must implement IBaseNametag.</typeparam>
    /// <returns>A ManagedNametag for the type parameter T.</returns>
    public static ManagedNametag CreateNametag<T>() where T : IBaseNametag
        => CreateNametag(Activator.CreateInstance<T>());
}
