using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

#if MELONLOADER
using MelonLoader;
#elif BEPINEX
using BepInEx.Configuration;
#endif

namespace BingusNametagsPlusPlus.Utilities;

public static class ConfigManager
{
    public static string ConfigFilePath =>
        Path.Combine(Constants.BingusNametagsData, $"config.cfg");

    // Name
    public static bool Nametags = true;
    public static bool FirstPersonEnabled = true;
    public static bool ThirdPersonEnabled = true;
    public static bool SanitizeNicknames = true;
    public static float Scale = 5f;
    public static float Offset = 0.65f;
    public static bool GFriendsIntegration = true;

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
    public static TMP_FontAsset? CustomFont;

#if MELONLOADER
    // Categories
    public static MelonPreferences_Category NametagsCategory;
    public static MelonPreferences_Category IconsCategory;
    public static MelonPreferences_Category NetworkingCategory;
    public static MelonPreferences_Category PluginsCategory;

    // the big list of things
    public static Dictionary<string, MelonPreferences_Entry> ConfigEntries = new();

    public static void SetupConfig()
    {
        NametagsCategory = MelonPreferences.CreateCategory("Nametags");
        IconsCategory = MelonPreferences.CreateCategory("Icons");
        NetworkingCategory = MelonPreferences.CreateCategory("Networking");
        PluginsCategory = MelonPreferences.CreateCategory("Plugins");

        NametagsCategory.SetFilePath(ConfigFilePath);
        IconsCategory.SetFilePath(ConfigFilePath);
        NetworkingCategory.SetFilePath(ConfigFilePath);
        PluginsCategory.SetFilePath(ConfigFilePath);

        NametagsCategory.LoadFromFile();
        IconsCategory.LoadFromFile();
        NetworkingCategory.LoadFromFile();
        PluginsCategory.LoadFromFile();

        // Nametags
        ConfigEntries["ShowNametags"] = NametagsCategory.CreateEntry("Show", Nametags, description: "Show nametags");
        ConfigEntries["FirstPersonEnabled"] = NametagsCategory.CreateEntry("FirstPersonEnabled", FirstPersonEnabled, description: "Show the nametags in first person");
        ConfigEntries["ThirdPersonEnabled"] = NametagsCategory.CreateEntry("ThirdPersonEnabled", ThirdPersonEnabled, description: "Show the nametags in third person");
        ConfigEntries["SanitizeNicknames"] = NametagsCategory.CreateEntry("SanitizeNicknames", SanitizeNicknames, description: "\"Clean\" usernames, filtering out any bad usernames");
        ConfigEntries["Scale"] = NametagsCategory.CreateEntry("Scale", Scale, description: "Change how large nametags are");
        ConfigEntries["Offset"] = NametagsCategory.CreateEntry("Offset", Offset, description: "Change where nametags are positioned");

        // Icons
        ConfigEntries["ShowIcons"] = IconsCategory.CreateEntry("Show", Icons, description: "Show icons on the default nametag");
        ConfigEntries["SpecialUserIcons"] = IconsCategory.CreateEntry("SpecialUserIcons", UserIcons, description: "Show custom icons for developers and beta testers");
        ConfigEntries["PlatformIcons"] = IconsCategory.CreateEntry("PlatformIcons", PlatformIcons, description: "Show icons representing what platform users play on");

        // Networking
        ConfigEntries["CustomNametags"] = NetworkingCategory.CreateEntry("CustomNametags", CustomNametags, description: "Allow yourself to customize how your nametag looks to other people [WARNING! Please see the warning box from the in-game UI (Right Shift) before enabling this.");
        ConfigEntries["ViewOtherCustomStyles"] = NetworkingCategory.CreateEntry("ViewOtherCustomStyles", ViewOtherCustomStyles, description: "View the custom nametags of other BingusNametags++ users");
        ConfigEntries["Color"] = NetworkingCategory.CreateEntry("Color", NetworkColor, description: "Hex code of your nametag");
        ConfigEntries["Bold"] = NetworkingCategory.CreateEntry("Bold", NetworkBold, description: "Bold nametag");
        ConfigEntries["Italics"] = NetworkingCategory.CreateEntry("Italics", NetworkItalic, description: "Italic nametag");
        ConfigEntries["Underlined"] = NetworkingCategory.CreateEntry("Underlined", NetworkUnderline, description: "Underline nametag");

        // Plugins
        ConfigEntries["EnabledPlugins"] = PluginsCategory.CreateEntry("EnabledPlugins", "Default", description: "Plugins that are enabled on startup, seperated by commas (whitespace is optional). By default, it is set to what plugins were enabled before Gorilla Tag closed.");
    }

	public static void SavePrefs()
    {
        // Nametags
        ConfigEntries["ShowNametags"].BoxedValue = Nametags;
        ConfigEntries["FirstPersonEnabled"].BoxedValue = FirstPersonEnabled;
        ConfigEntries["ThirdPersonEnabled"].BoxedValue = ThirdPersonEnabled;
        ConfigEntries["SanitizeNicknames"].BoxedValue = SanitizeNicknames;
        ConfigEntries["Scale"].BoxedValue = Scale;
        ConfigEntries["Offset"].BoxedValue = Offset;

        // Icons
        ConfigEntries["ShowIcons"].BoxedValue = Icons;
        ConfigEntries["SpecialUserIcons"].BoxedValue = UserIcons;
        ConfigEntries["PlatformIcons"].BoxedValue = PlatformIcons;

        // Networking
        ConfigEntries["CustomNametags"].BoxedValue = CustomNametags;
        ConfigEntries["ViewOtherCustomStyles"].BoxedValue = ViewOtherCustomStyles;
        ConfigEntries["Color"].BoxedValue = NetworkColor;
        ConfigEntries["Bold"].BoxedValue = NetworkBold;
        ConfigEntries["Italics"].BoxedValue = NetworkItalic;
        ConfigEntries["Underlined"].BoxedValue = NetworkUnderline;

        // Plugins
        ConfigEntries["EnabledPlugins"].BoxedValue = string.Join(", ", PluginManager.EnabledPlugins.Select(plugin => plugin.Metadata.Name));

        NametagsCategory.SaveToFile();
        IconsCategory.SaveToFile();
        NetworkingCategory.SaveToFile();
        PluginsCategory.SaveToFile();
	}

	public static void LoadPrefs()
	{
        SetupConfig();

		var fontFile =
			Directory.EnumerateFiles(Constants.BingusNametagsData, "*.ttf", SearchOption.AllDirectories)
				.FirstOrDefault()
			?? Directory.EnumerateFiles(Constants.BingusNametagsData, "*.otf", SearchOption.AllDirectories)
				.FirstOrDefault();

		if (!fontFile.IsNullOrWhiteSpace())
			CustomFont = TMP_FontAsset.CreateFontAsset(new Font(fontFile));

        var enabledPluginsString = (string)ConfigEntries["EnabledPlugins"].BoxedValue;
        var enabledPlugins = enabledPluginsString
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(name => name.Trim());

        PluginManager.Plugins.ForEach(plugin =>
        {
            if (enabledPlugins.Contains(plugin.Metadata.Name))
                PluginManager.EnablePlugin(plugin);
            else
                PluginManager.DisablePlugin(plugin);
        });
    }
#elif BEPINEX
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
        cfgFile.Get<string>("Plugins", "EnabledPlugins").BoxedValue = string.Join(", ", PluginManager.EnabledPlugins.Select(plugin => plugin.Metadata.Name));

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
            if (enabledPlugins.Contains(plugin.Metadata.Name))
                PluginManager.EnablePlugin(plugin);
            else
                PluginManager.DisablePlugin(plugin);
        });
    }
#endif
	public static bool ValidHexCode(string hexCode) =>
        !hexCode.IsNullOrWhiteSpace() && Regex.IsMatch(hexCode, @"^#?([0-9a-fA-F]{6}|[0-9a-fA-F]{3})$");
	
}