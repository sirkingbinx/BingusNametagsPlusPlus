using System.Reflection;
using System.Resources;

namespace BingusNametagsPlusPlus.Utilities;

public static class LocalizationManager
{
    public static int CurrentLanguageIndex = 0;
    public static string[] Languages = [ "English", "Francais", "Nederlands", "Deutsch" ];
    public static string[] LanguageCode = [ "en", "fr", "nl", "de" ];

    private static ResourceManager rm;

    public static void SetLanguage(int languageIndex)
    {
        CurrentLanguageIndex = languageIndex;
        rm = new ResourceManager("BingusNametagsPlusPlus.Localization.UIStrings-" + LanguageCode[CurrentLanguageIndex], Assembly.GetExecutingAssembly());
    }

    public static string GetString(string translationName, int itemIndex = 0)
    {
        if (rm == null) return "";
        var str = rm.GetString(translationName);
        return str.Split("-")[itemIndex].Trim();
    }
}