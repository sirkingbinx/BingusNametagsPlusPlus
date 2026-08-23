using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BingusNametagsPlusPlus.Attributes;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

namespace BingusNametagsPlusPlus.Utilities;

public class Config
{
    public static string ConfigFilePath =>
        Path.Combine(Constants.BingusNametagsData, "config.json");

    public static Config Current = new();

    // Name
    public bool Nametags = true;
    public bool FirstPersonEnabled = true;
    public bool ThirdPersonEnabled = true;
    public bool SanitizeNicknames = true;
    public float Scale = 5f;
    public float Offset = 0.65f;
    public bool GFriendsIntegration = true;

    // Icons
    public bool Icons = true;
    public bool PlatformIcons = true;
    public bool UserIcons = true;

    // Networking
    public bool CustomNametags = false;
    public bool ViewOtherCustomStyles = true;

    public string NetworkColor = "ffffff";
    public bool NetworkBold = false;
    public bool NetworkUnderline = false;
    public bool NetworkItalic = false;

    public int AutoUpdateMode = 0;

    public int SetLanguage = 0;

    // Plugins
    public string EnabledPlugins = "Default";

    public Dictionary<string, float> PluginOffsets = new();

    // Misc
    public TMP_FontAsset? CustomFont;

	public static void SavePrefs()
    {
        string configText = JsonConvert.SerializeObject(Current, Formatting.Indented);
        File.WriteAllText(ConfigFilePath, configText);
	}

	public static void LoadPrefs()
	{
        try
        {
            if (!File.Exists(ConfigFilePath))
                SavePrefs();

            if (JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigFilePath)) is Config now)
                Current = now;
        } catch
        {
            return;
        }
    }

    public static void ProcessPrefs()
    {
        if (Current.EnabledPlugins == "")
            Current.EnabledPlugins = "Default";

        LocalizationManager.SetLanguage(Current.SetLanguage);

        var fontFile =
            Directory.EnumerateFiles(Constants.BingusNametagsData, "*.ttf", SearchOption.AllDirectories)
                .FirstOrDefault()
            ?? Directory.EnumerateFiles(Constants.BingusNametagsData, "*.otf", SearchOption.AllDirectories)
                .FirstOrDefault();

        if (!fontFile.IsNullOrWhiteSpace())
            Current.CustomFont = TMP_FontAsset.CreateFontAsset(new Font(fontFile));
    }

    public static void SaveNametagOffset(BingusNametagsPlugin plugin, BingusNametagsNametag nametag, float offset) =>
        Current.PluginOffsets[$"{plugin.Author}.{plugin.Name}.{nametag.Name}"] = offset;

    public static float GetNametagOffset(BingusNametagsPlugin plugin, BingusNametagsNametag nametag) =>
        Current.PluginOffsets[$"{plugin.Author}.{plugin.Name}.{nametag.Name}"];

	public static bool ValidHexCode(string hexCode) =>
        !hexCode.IsNullOrWhiteSpace() && Regex.IsMatch(hexCode, @"^#?([0-9a-fA-F]{6}|[0-9a-fA-F]{3})$");
}