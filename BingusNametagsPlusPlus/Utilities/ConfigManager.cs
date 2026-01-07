using System.Collections.Generic;
using BepInEx.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace BingusNametagsPlusPlus.Utilities;

public static class ConfigManager
{
    public static string ConfigFilePath =>
        Path.Combine(Constants.BingusNametagsData, "config.cfg");

    public static bool ShowingNametags = true;

	public static bool ShowingName = true;
	public static bool ShowingPlatform = true;

	public static bool ShowInFirstPerson = true;
	public static bool ShowInThirdPerson = true;
	public static bool UserCustomIcons = true;
	public static bool GlobalIconsEnabled = true;

    public static bool UseSanitizedNickName = true;

	public static TMP_FontAsset? CustomFont;

	public static float NametagScale = 5f;
	public static float NametagYOffset = 0.65f;

	public static bool CustomNametags = true;
	public static string NametagColor = "ffffff";

	public static bool NetworkBold = false;
	public static bool NetworkUnderline = false;
	public static bool NetworkItalic = false;

    public static ConfigEntry<T> Get<T>(ConfigFile file, string section, string name)
    {
        file.TryGetEntry(section, name, out ConfigEntry<T> thing);
        return thing;
    }

    public static ConfigFile GenerateConfig()
    {
        var config = new ConfigFile(ConfigFilePath, true);

        // Nametags
        config.Bind("Nametags", "Show", ShowingNametags, "Show nametags");
        config.Bind("Nametags", "FirstPersonEnabled", ShowInFirstPerson, "Show the nametags in first person");
        config.Bind("Nametags", "ThirdPersonEnabled", ShowInThirdPerson, "Show the nametags in third person");
        config.Bind("Nametags", "SanitizeNicknames", UseSanitizedNickName, "\"Clean\" usernames, filtering out any bad usernames");
        config.Bind("Nametags", "Scale", NametagScale, "Change how large nametags are");
        config.Bind("Nametags", "Offset", NametagYOffset, "Change where nametags are positioned");

		// Icons
        config.Bind("Icons", "Show", GlobalIconsEnabled, "Show icons on the default nametag");
        config.Bind("Icons", "SpecialUserIcons", UserCustomIcons, "Show custom icons for developers and beta testers");
        config.Bind("Icons", "PlatformIcons", ShowingPlatform, "Show icons representing what platform users play on");

		// Networking
        config.Bind("Networking", "CustomNametags", Networking.DoNetworking, "Allow yourself to customize how your nametag looks to other people [WARNING! Please see the warning box from the in-game UI (Right Shift) before enabling this.");
        config.Bind("Networking", "ViewOtherCustomStyles", CustomNametags, "View the custom nametags of other BingusNametags++ users");
        config.Bind("Networking", "Color", NametagColor, "Hex code of your nametag");
        config.Bind("Networking", "Bold", NetworkBold, "Bold nametag");
        config.Bind("Networking", "Italics", NetworkItalic, "Italic nametag");
        config.Bind("Networking", "Underlined", NetworkUnderline, "Underline nametag");

        return config;
    }

	public static void SavePrefs()
    {
        var cfgFile = GenerateConfig();

        // Nametags
        cfgFile.Get<bool>("Nametags", "Show").BoxedValue = ShowingNametags;
        cfgFile.Get<bool>("Nametags", "FirstPersonEnabled").BoxedValue = ShowInFirstPerson;
        cfgFile.Get<bool>("Nametags", "ThirdPersonEnabled").BoxedValue = ShowInThirdPerson;
        cfgFile.Get<bool>("Nametags", "SanitizeNicknames").BoxedValue = UseSanitizedNickName;
        cfgFile.Get<float>("Nametags", "Scale").BoxedValue = NametagScale;
        cfgFile.Get<float>("Nametags", "Offset").BoxedValue = NametagYOffset;

        // Icons
        cfgFile.Get<bool>("Icons", "Show").BoxedValue = GlobalIconsEnabled;
        cfgFile.Get<bool>("Icons", "SpecialUserIcons").BoxedValue = UserCustomIcons;
        cfgFile.Get<bool>("Icons", "PlatformIcons").BoxedValue = ShowingPlatform;

        // Networking
        cfgFile.Get<bool>("Networking", "CustomNametags").BoxedValue = Networking.DoNetworking;
        cfgFile.Get<bool>("Networking", "ViewOtherCustomStyles").BoxedValue = CustomNametags;
        cfgFile.Get<string>("Networking", "Color").BoxedValue = NametagColor;
        cfgFile.Get<bool>("Networking", "Bold").BoxedValue = NetworkBold;
        cfgFile.Get<bool>("Networking", "Italics").BoxedValue = NetworkItalic;
        cfgFile.Get<bool>("Networking", "Underlined").BoxedValue = NetworkUnderline;

        cfgFile.Save();

        var enabledPlugins = PluginManager.Plugins.Where(tag => tag.Enabled).Select(tag => tag.Name);
		PlayerPrefs.SetString("BG++_EnabledPlugins", string.Join("&", enabledPlugins));
	}

	public static void LoadPrefs()
	{
        var cfgFile = GenerateConfig();

        // Nametags
        ShowingNametags = cfgFile.Get<bool>("Nametags", "Show").Value;
        ShowInFirstPerson = cfgFile.Get<bool>("Nametags", "FirstPersonEnabled").Value;
        ShowInThirdPerson = cfgFile.Get<bool>("Nametags", "ThirdPersonEnabled").Value;
        UseSanitizedNickName = cfgFile.Get<bool>("Nametags", "SanitizeNicknames").Value;
        NametagScale = cfgFile.Get<float>("Nametags", "Scale").Value;
        NametagYOffset = cfgFile.Get<float>("Nametags", "Offset").Value;

        // Icons
        GlobalIconsEnabled = cfgFile.Get<bool>("Icons", "Show").Value;
        UserCustomIcons = cfgFile.Get<bool>("Icons", "SpecialUserIcons").Value;
        ShowingPlatform = cfgFile.Get<bool>("Icons", "PlatformIcons").Value;

        // Networking
        Networking.DoNetworking = cfgFile.Get<bool>("Networking", "CustomNametags").Value;
        CustomNametags = cfgFile.Get<bool>("Networking", "ViewOtherCustomStyles").Value;
        NametagColor = cfgFile.Get<string>("Networking", "Color").Value;
        NetworkBold = cfgFile.Get<bool>("Networking", "Bold").Value;
        NetworkItalic = cfgFile.Get<bool>("Networking", "Italics").Value;
        NetworkUnderline = cfgFile.Get<bool>("Networking", "Underlined").Value;

        var enabledPluginsString = PlayerPrefs.GetString("BG++_EnabledPlugins", "Default");
        var enabledPlugins = enabledPluginsString.Split("&");

        PluginManager.Plugins.ForEach(nametag =>
        {
            if (enabledPlugins.Contains(nametag.Name))
                PluginManager.EnablePlugin(nametag);
            else
                PluginManager.DisablePlugin(nametag);
        });

		var fontFile =
			Directory.EnumerateFiles(Constants.AssemblyDirectory, "*.ttf", SearchOption.TopDirectoryOnly)
				.FirstOrDefault()
			?? Directory.EnumerateFiles(Constants.AssemblyDirectory, "*.otf", SearchOption.TopDirectoryOnly)
				.FirstOrDefault();

		if (!fontFile.IsNullOrWhiteSpace())
			CustomFont = TMP_FontAsset.CreateFontAsset(new Font(fontFile));
	}

	public static bool ValidHexCode(string hexCode) =>
        !hexCode.IsNullOrWhiteSpace() && Regex.IsMatch(hexCode, @"^#?([0-9a-fA-F]{6}|[0-9a-fA-F]{3})$");
	
}