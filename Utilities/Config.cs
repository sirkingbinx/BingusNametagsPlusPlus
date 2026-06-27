using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
	public bool   NetworkBold = false;
	public bool   NetworkUnderline = false;
	public bool   NetworkItalic = false;

    public int AutoUpdateMode = 0;

    // Plugins
    public List<string> EnabledPlugins = [ "Default" ];

    // Misc
    public TMP_FontAsset? CustomFont;

	public static void SavePrefs()
    {
        string configText = JsonConvert.SerializeObject(Current, Formatting.Indented);
        File.WriteAllText(ConfigFilePath, configText);
	}

	public static void LoadPrefs()
	{
        if (Current.EnabledPlugins.Count == 0)
            Current.EnabledPlugins.Add("Default");

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

		var fontFile =
			Directory.EnumerateFiles(Constants.BingusNametagsData, "*.ttf", SearchOption.AllDirectories)
				.FirstOrDefault()
			?? Directory.EnumerateFiles(Constants.BingusNametagsData, "*.otf", SearchOption.AllDirectories)
				.FirstOrDefault();

		if (!fontFile.IsNullOrWhiteSpace())
			Current.CustomFont = TMP_FontAsset.CreateFontAsset(new Font(fontFile));

        if (Current.EnabledPlugins.Count == 0)
            Current.EnabledPlugins.Add("Default");

        PluginManager.Plugins.ForEach(plugin =>
        {
            if (Current.EnabledPlugins.Contains(plugin.Metadata.Name))
                PluginManager.EnablePlugin(plugin);
            else
                PluginManager.DisablePlugin(plugin);
        });
    }

	public static bool ValidHexCode(string hexCode) =>
        !hexCode.IsNullOrWhiteSpace() && Regex.IsMatch(hexCode, @"^#?([0-9a-fA-F]{6}|[0-9a-fA-F]{3})$");
}