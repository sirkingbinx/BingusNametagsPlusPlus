using UnityEngine;

namespace BingusNametagsPlusPlus.Utilities;

public static class Config
{
	public static bool ShowingNametags = true;

	public static bool ShowingName = true;
	public static bool ShowingPlatform = true;

	public static bool ShowInFirstPerson = true;
	public static bool ShowInThirdPerson = true;

	public static float NametagScale = 5f;

	public static float NametagYOffset = 1f;
	
	public static void SavePrefs()
	{
		PlayerPrefs.SetInt("bgn++_FPE", ShowInFirstPerson ? 1 : 0);
		PlayerPrefs.SetInt("bgn++_TPE", ShowInThirdPerson ? 1 : 0);
		
		PlayerPrefs.SetInt("bgn++_Nametags", ShowingNametags ? 1 : 0);
		PlayerPrefs.SetInt("bgn++_NameEnabled", ShowingName ? 1 : 0);
		PlayerPrefs.SetInt("bgn++_PlatformEnabled", ShowingPlatform ? 1 : 0);
		
		PlayerPrefs.SetFloat("bgn++_NametagScale", NametagScale);
		PlayerPrefs.SetFloat("bgn++_NametagY", NametagYOffset);
	}

	public static void LoadPrefs()
	{
		ShowInFirstPerson = PlayerPrefs.GetInt("bgn++_FPE", 1) == 1;
		ShowInThirdPerson = PlayerPrefs.GetInt("bgn++_TPE", 1) == 1;
		
		ShowingNametags = PlayerPrefs.GetInt("bgn++_Nametags", 1) == 1;
		ShowingName = PlayerPrefs.GetInt("bgn++_NameEnabled", 1) == 1;
		ShowingPlatform = PlayerPrefs.GetInt("bgn++_PlatformEnabled", 1) == 1;
		
		NametagScale = PlayerPrefs.GetFloat("bgn++_NametagScale", NametagScale);
		NametagYOffset = PlayerPrefs.GetFloat("bgn++_NametagY", NametagYOffset);
	}
}