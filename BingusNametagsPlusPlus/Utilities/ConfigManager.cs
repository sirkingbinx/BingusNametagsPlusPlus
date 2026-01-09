using System;
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

    // Name
    public static bool Nametags = true;
    public static bool FirstPersonEnabled = true;
    public static bool ThirdPersonEnabled = true;
    public static bool SanitizeNicknames = true;
    public static float Scale = 5f;
    public static float Offset = 0.65f;

    // Icons
    public static bool Icons = true;
    public static bool PlatformIcons = true;
    public static bool UserIcons = true;

    // Networking
    public static bool CustomNametags = false;
	public static bool ViewOtherCustomStyles = true;

	public static string NetworkColor = "ffffff";
	public static bool   NetworkBold = false;
	public static bool   NetworkUnderline = false;
	public static bool   NetworkItalic = false;

    // Misc
    public static bool Default_ShowingName = true;
    public static TMP_FontAsset? CustomFont;

    public static ConfigEntry<T> Get<T>(ConfigFile file, string section, string name)
    {
        file.TryGetEntry(section, name, out ConfigEntry<T> thing);
        return thing;
    }

    public static ConfigFile GenerateConfig()
    {
        var config = new ConfigFile(ConfigFilePath, true);

        // Nametags
        config.Bind("Nametags", "Show", Nametags, "Show nametags");
        config.Bind("Nametags", "FirstPersonEnabled", FirstPersonEnabled, "Show the nametags in first person");
        config.Bind("Nametags", "ThirdPersonEnabled", ThirdPersonEnabled, "Show the nametags in third person");
        config.Bind("Nametags", "SanitizeNicknames", SanitizeNicknames, "\"Clean\" usernames, filtering out any bad usernames");
        config.Bind("Nametags", "Scale", Scale, "Change how large nametags are");
        config.Bind("Nametags", "Offset", Offset, "Change where nametags are positioned");

		// Icons
        config.Bind("Icons", "Show", Icons, "Show icons on the default nametag");
        config.Bind("Icons", "SpecialUserIcons", UserIcons, "Show custom icons for developers and beta testers");
        config.Bind("Icons", "PlatformIcons", PlatformIcons, "Show icons representing what platform users play on");

		// Networking
        config.Bind("Networking", "CustomNametags", CustomNametags, "Allow yourself to customize how your nametag looks to other people [WARNING! Please see the warning box from the in-game UI (Right Shift) before enabling this.");
        config.Bind("Networking", "ViewOtherCustomStyles", ViewOtherCustomStyles, "View the custom nametags of other BingusNametags++ users");
        config.Bind("Networking", "Color", NetworkColor, "Hex code of your nametag");
        config.Bind("Networking", "Bold", NetworkBold, "Bold nametag");
        config.Bind("Networking", "Italics", NetworkItalic, "Italic nametag");
        config.Bind("Networking", "Underlined", NetworkUnderline, "Underline nametag");

        // Plugins
        config.Bind("Plugins", "EnabledPlugins", "Default", "Plugins that are enabled on startup, seperated by commas (whitespace is optional). By default, it is set to what plugins were enabled before Gorilla Tag closed.");

        return config;
    }

	public static void SavePrefs()
    {
        var cfgFile = GenerateConfig();

        // Nametags
        cfgFile.Get<bool>("Nametags", "Show").BoxedValue = Nametags;
        cfgFile.Get<bool>("Nametags", "FirstPersonEnabled").BoxedValue = FirstPersonEnabled;
        cfgFile.Get<bool>("Nametags", "ThirdPersonEnabled").BoxedValue = ThirdPersonEnabled;
        cfgFile.Get<bool>("Nametags", "SanitizeNicknames").BoxedValue = SanitizeNicknames;
        cfgFile.Get<float>("Nametags", "Scale").BoxedValue = Scale;
        cfgFile.Get<float>("Nametags", "Offset").BoxedValue = Offset;

        // Icons
        cfgFile.Get<bool>("Icons", "Show").BoxedValue = Icons;
        cfgFile.Get<bool>("Icons", "SpecialUserIcons").BoxedValue = UserIcons;
        cfgFile.Get<bool>("Icons", "PlatformIcons").BoxedValue = PlatformIcons;

        // Networking
        cfgFile.Get<bool>("Networking", "CustomNametags").BoxedValue = CustomNametags;
        cfgFile.Get<bool>("Networking", "ViewOtherCustomStyles").BoxedValue = ViewOtherCustomStyles;
        cfgFile.Get<string>("Networking", "Color").BoxedValue = NetworkColor;
        cfgFile.Get<bool>("Networking", "Bold").BoxedValue = NetworkBold;
        cfgFile.Get<bool>("Networking", "Italics").BoxedValue = NetworkItalic;
        cfgFile.Get<bool>("Networking", "Underlined").BoxedValue = NetworkUnderline;

        // Plugins
        cfgFile.Get<string>("Plugins", "EnabledPlugins").BoxedValue = string.Join(", ", PluginManager.EnabledPlugins.Select(plugin => plugin.Name));

        cfgFile.Save();
	}

	public static void LoadPrefs()
	{
        var cfgFile = GenerateConfig();

        // Nametags
        Nametags = cfgFile.Get<bool>("Nametags", "Show").Value;
        FirstPersonEnabled = cfgFile.Get<bool>("Nametags", "FirstPersonEnabled").Value;
        ThirdPersonEnabled = cfgFile.Get<bool>("Nametags", "ThirdPersonEnabled").Value;
        SanitizeNicknames = cfgFile.Get<bool>("Nametags", "SanitizeNicknames").Value;
        Scale = cfgFile.Get<float>("Nametags", "Scale").Value;
        Offset = cfgFile.Get<float>("Nametags", "Offset").Value;

        // Icons
        Icons = cfgFile.Get<bool>("Icons", "Show").Value;
        UserIcons = cfgFile.Get<bool>("Icons", "SpecialUserIcons").Value;
        PlatformIcons = cfgFile.Get<bool>("Icons", "PlatformIcons").Value;

        // Networking
        CustomNametags = cfgFile.Get<bool>("Networking", "CustomNametags").Value;
        ViewOtherCustomStyles = cfgFile.Get<bool>("Networking", "ViewOtherCustomStyles").Value;
        NetworkColor = cfgFile.Get<string>("Networking", "Color").Value;
        NetworkBold = cfgFile.Get<bool>("Networking", "Bold").Value;
        NetworkItalic = cfgFile.Get<bool>("Networking", "Italics").Value;
        NetworkUnderline = cfgFile.Get<bool>("Networking", "Underlined").Value;

		var fontFile =
			Directory.EnumerateFiles(Constants.BingusNametagsData, "*.ttf", SearchOption.AllDirectories)
				.FirstOrDefault()
			?? Directory.EnumerateFiles(Constants.BingusNametagsData, "*.otf", SearchOption.AllDirectories)
				.FirstOrDefault();

		if (!fontFile.IsNullOrWhiteSpace())
			CustomFont = TMP_FontAsset.CreateFontAsset(new Font(fontFile));

        var enabledPluginsString = cfgFile.Get<string>("Plugins", "EnabledPlugins").Value.Trim();
        var enabledPlugins = enabledPluginsString
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(name => name.Trim());

        PluginManager.Plugins.ForEach(plugin =>
        {
            if (enabledPlugins.Contains(plugin.Name))
                PluginManager.EnablePlugin(plugin);
            else
                PluginManager.DisablePlugin(plugin);
        });
    }

	public static bool ValidHexCode(string hexCode) =>
        !hexCode.IsNullOrWhiteSpace() && Regex.IsMatch(hexCode, @"^#?([0-9a-fA-F]{6}|[0-9a-fA-F]{3})$");
	
}