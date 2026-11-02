using System;
using System.Reflection;
using System.Resources;

namespace BingusNametagsPlusPlus.Utilities;

public static class LocalizationManager
{
    public static int CurrentLanguageIndex = 0;
    public static string[] Languages = ["English", "Nederlands", "Deutsch", "Suomi", "Français", "Español", "Norsk", "Polski", "Português", "Русский", "Svenska", "中国人"];
    public static string[] LanguageCode = ["en", "nl", "de", "fi", "fr", "es", "nb", "pl", "pt", "ru", "sv", "zn"];
    private static readonly ResourceManager English = new ResourceManager("BingusNametagsPlusPlus.Localization.UIStrings-en", Assembly.GetExecutingAssembly());
    private static ResourceManager rm = new ResourceManager("BingusNametagsPlusPlus.Localization.UIStrings-en", Assembly.GetExecutingAssembly());

    public static void SetLanguage(int languageIndex)
    {
        CurrentLanguageIndex = languageIndex >= 0 && languageIndex < LanguageCode.Length ? languageIndex : 0;
        rm = new ResourceManager("BingusNametagsPlusPlus.Localization.UIStrings-" + LanguageCode[CurrentLanguageIndex], Assembly.GetExecutingAssembly());
        Config.Current.SetLanguage = CurrentLanguageIndex;
    }

    public static string GetString(string translationName, int itemIndex = 0)
    {
        string? str;
        try
        {
            str = rm.GetString(translationName);
        }
        catch (MissingManifestResourceException)
        {
            str = null;
        }

        str ??= English.GetString(translationName) ?? translationName;
        // Only the legacy t-keys use "label - tooltip" values. New keys can contain hyphens.
        if (!translationName.StartsWith("t", StringComparison.Ordinal))
            return str;
        int separator = str.IndexOf(" - ", StringComparison.Ordinal);
        if (separator < 0)
            return itemIndex == 0 ? str : "";
        return itemIndex == 0 ? str.Substring(0, separator).Trim() : str.Substring(separator + 3).Trim();
    }
}
