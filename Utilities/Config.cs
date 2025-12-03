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
		PlayerPrefs.SetInt("bgn++_FPE", ShowInFirstPerson ? 1 : 0);
		PlayerPrefs.SetInt("bgn++_TPE", ShowInThirdPerson ? 1 : 0);

		PlayerPrefs.SetInt("bgn++_NameEnabled", ShowingName ? 1 : 0);

		PlayerPrefs.SetInt("bgn++_AllIcons", GlobalIconsEnabled ? 1 : 0);
		PlayerPrefs.SetInt("bgn++_PlatformEnabled", ShowingPlatform ? 1 : 0);
		PlayerPrefs.SetInt("bgn++_ShowPlayerIcons", UserCustomIcons ? 1 : 0);

		PlayerPrefs.SetInt("bgn++_DoNetworking", CustomNametags ? 1 : 0);
		PlayerPrefs.SetString("bgn++_NetworkNametagColor", NametagColor);

		PlayerPrefs.SetFloat("bgn++_NametagScale", NametagScale);
		PlayerPrefs.SetFloat("bgn++_NametagY", NametagYOffset);
	}

	public static void LoadPrefs()
	{
		ShowInFirstPerson = PlayerPrefs.GetInt("bgn++_FPE", 1) == 1;
		ShowInThirdPerson = PlayerPrefs.GetInt("bgn++_TPE", 1) == 1;

		ShowingName = PlayerPrefs.GetInt("bgn++_NameEnabled", 1) == 1;

		GlobalIconsEnabled = PlayerPrefs.GetInt("bgn++_AllIcons", 1) == 1;
		ShowingPlatform = PlayerPrefs.GetInt("bgn++_PlatformEnabled", 1) == 1;
		UserCustomIcons = PlayerPrefs.GetInt("bgn++_ShowPlayerIcons", 1) == 1;

		CustomNametags = PlayerPrefs.GetInt("bgn++_DoNetworking", 1) == 1;
		NametagColor = PlayerPrefs.GetString("bgn++_NetworkNametagColor", NametagColor);

		NametagScale = PlayerPrefs.GetFloat("bgn++_NametagScale", NametagScale);
		NametagYOffset = PlayerPrefs.GetFloat("bgn++_NametagY", NametagYOffset);

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