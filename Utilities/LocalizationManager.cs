using System.Reflection;
using System.Resources;

namespace BingusNametagsPlusPlus.Utilities;

public static class LocalizationManager
{
    public static int CurrentLanguageIndex = 0;
    public static string[] Languages = [ "English", "Nederlands", "Deutsch", "Suomi", "Français"];
    public static string[] LanguageCode = [ "en", "nl", "de", "fi", "fr" ];

    private static ResourceManager rm = new ResourceManager("BingusNametagsPlusPlus.Localization.UIStrings-en", Assembly.GetExecutingAssembly());

    public static void SetLanguage(int languageIndex)
    {
        CurrentLanguageIndex = languageIndex;
        rm = new ResourceManager("BingusNametagsPlusPlus.Localization.UIStrings-" + LanguageCode[CurrentLanguageIndex], Assembly.GetExecutingAssembly());
        Config.Current.SetLanguage = CurrentLanguageIndex;
    }

    public static string GetString(string translationName, int itemIndex = 0)
    {
        if (rm == null) return "";
        var str = rm.GetString(translationName);
        return str.Split("-")[itemIndex].Trim();
    }
}