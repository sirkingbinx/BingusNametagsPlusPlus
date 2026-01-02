using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace BingusNametagsPlusPlus.Utilities;

public static class Config
{
	public static bool ShowingNametags = true;

	public static bool ShowingName = true;
	public static bool ShowingPlatform = true;

	public static bool ShowInFirstPerson = true;
	public static bool ShowInThirdPerson = true;
	public static bool UserCustomIcons = true;
	public static bool GlobalIconsEnabled = true;

    public static bool ShowPersonalTag = false;

	public static TMP_FontAsset? CustomFont;

	public static float NametagScale = 5f;
	public static float NametagYOffset = 1f;

	public static bool CustomNametags = true;
	public static string NametagColor = "ffffff";

	public static bool NetworkBold = false;
	public static bool NetworkUnderline = false;
	public static bool NetworkItalic = false;

	public static void SavePrefs()
	{
		PlayerPrefs.SetInt("BG++_FPE", ShowInFirstPerson ? 1 : 0);
		PlayerPrefs.SetInt("BG++_TPE", ShowInThirdPerson ? 1 : 0);
        PlayerPrefs.SetInt("BG++_PersonalNametag", ShowPersonalTag ? 1 : 0);

        PlayerPrefs.SetInt("BG++_NameEnabled", ShowingName ? 1 : 0);

		PlayerPrefs.SetInt("BG++_AllIcons", GlobalIconsEnabled ? 1 : 0);
		PlayerPrefs.SetInt("BG++_PlatformEnabled", ShowingPlatform ? 1 : 0);
		PlayerPrefs.SetInt("BG++_ShowPlayerIcons", UserCustomIcons ? 1 : 0);

		PlayerPrefs.SetInt("BG++_DoNetworking", CustomNametags ? 1 : 0);
        PlayerPrefs.SetFloat("BG++_DoNetworkingSelf", Networking.DoNetworking ? 1 : 0);
        PlayerPrefs.SetString("BG++_NetworkNametagColor", NametagColor);

		PlayerPrefs.SetFloat("BG++_NametagScale", NametagScale);
		PlayerPrefs.SetFloat("BG++_NametagY", NametagYOffset);
	}

	public static void LoadPrefs()
	{
		ShowInFirstPerson = PlayerPrefs.GetInt("BG++_FPE", 1) == 1;
		ShowInThirdPerson = PlayerPrefs.GetInt("BG++_TPE", 1) == 1;
		ShowPersonalTag = PlayerPrefs.GetInt("BG++_PersonalNametag", 1) == 1;

        ShowingName = PlayerPrefs.GetInt("BG++_NameEnabled", 1) == 1;

		GlobalIconsEnabled = PlayerPrefs.GetInt("BG++_AllIcons", 1) == 1;
		ShowingPlatform = PlayerPrefs.GetInt("BG++_PlatformEnabled", 1) == 1;
		UserCustomIcons = PlayerPrefs.GetInt("BG++_ShowPlayerIcons", 1) == 1;

		Networking.DoNetworking = PlayerPrefs.GetInt("BG++_DoNetworkingSelf", 1) == 1;
        CustomNametags = PlayerPrefs.GetInt("BG++_DoNetworking", 1) == 1;
		NametagColor = PlayerPrefs.GetString("BG++_NetworkNametagColor", NametagColor);

		NametagScale = PlayerPrefs.GetFloat("BG++_NametagScale", NametagScale);
		NametagYOffset = PlayerPrefs.GetFloat("BG++_NametagY", NametagYOffset);

		var fontFile =
			Directory.EnumerateFiles(Constants.AssemblyDirectory, "*.ttf", SearchOption.TopDirectoryOnly)
				.FirstOrDefault()
			?? Directory.EnumerateFiles(Constants.AssemblyDirectory, "*.otf", SearchOption.TopDirectoryOnly)
				.FirstOrDefault();

		if (!fontFile.IsNullOrWhiteSpace())
			CustomFont = TMP_FontAsset.CreateFontAsset(new Font(fontFile));
	}

	public static bool ValidHexCode(string hexCode)
	{
		return !hexCode.IsNullOrWhiteSpace() && Regex.IsMatch(hexCode, @"^#?([0-9a-fA-F]{6}|[0-9a-fA-F]{3})$");
	}
}