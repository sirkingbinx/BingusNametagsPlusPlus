using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEngine;

#pragma warning disable CS8600
#pragma warning disable CS8602

namespace BingusNametagsPlusPlus.Utilities;

public static class AutoUpdater
{
    private static readonly HttpClient httpClient = new();

    private const string updateUrl = "https://monkeforge.org/api/v1/mods/dev.sirkingbinx.bingusnametagsplusplus/releases";
    private static string? downloadUrl;

#if BEPINEX5
    private const string loader = "bepinex5";
#elif BEPINEX6_MONO
    private const string loader = "bepinex6";
#elif MELONLOADER
    private const string loader = "melonloader";
#else
    private const string loader = "";
#endif

    internal static string? GetLoaderDownload(JToken release)
    {
        if (string.IsNullOrEmpty(loader))
            return null;

        return release["downloads"]?
            .FirstOrDefault(download =>
                string.Equals((string?)download["loader"], loader, StringComparison.OrdinalIgnoreCase) &&
                string.Equals((string?)download["kind"], "dll", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace((string?)download["url"]))?["url"]?.Value<string>();
    }

    public static (string, string) GetVersionData()
    {
        downloadUrl = null;
        httpClient.DefaultRequestHeaders.Remove("User-Agent");
        httpClient.DefaultRequestHeaders.Add("User-Agent", $"BingusNametags++/{Constants.Version} (.NET CLR {Environment.Version})");

        try
        {
            var versionData = JObject.Parse(httpClient.GetStringAsync(updateUrl).Result);

            var release = versionData["items"]?.FirstOrDefault();
            var latestVersion = (string?)release?["version"];
            downloadUrl = release == null ? null : GetLoaderDownload(release);

            if (string.IsNullOrWhiteSpace(latestVersion) || string.IsNullOrWhiteSpace(downloadUrl))
            {
                LogManager.LogLine($"[AutoUpdater] No compatible DLL update is available for {loader}.");
                downloadUrl = null;
                return ("", "");
            }

            LogManager.LogLine("[AutoUpdater] Latest version: " + latestVersion + ", available at \"" + downloadUrl + "\".");

            return (latestVersion.ToString() ?? "0.0.0", downloadUrl ?? "");
        }
        catch (Exception ex)
        {
            LogManager.Log($"Auto-update failed ({ex.GetType().Name}): {ex.Message}");
            LogManager.LogException(ex);
        }

        return ("", "");
    }

    public static void Invoke()
    {
        if (Config.Current.AutoUpdateMode == 2)
            return;

        try
        {
            var (latestVersion, downloadUrl) = GetVersionData();
            if (string.IsNullOrWhiteSpace(downloadUrl))
                return;

            var currentVersion = new Version(Constants.Version);

            if (currentVersion < new Version(latestVersion))
            {
                if (Config.Current.AutoUpdateMode == 0)
                    Update("Update");
                if (Config.Current.AutoUpdateMode == 1)
                    UIManager.Ask($"A new version of BingusNametags++ is available!\n\n{currentVersion} -> {latestVersion}\n\nBingusNametags++ can automatically install the update. Would you like to update?", ["Decline", "Update"], Update);
            }
        }
        catch (Exception ex)
        {
            LogManager.Log($"Auto-update failed ({ex.GetType().Name}): {ex.Message}");
            LogManager.LogException(ex);
        }
    }

    private static void Update(string result)
    {
        if (result == "Decline" || string.IsNullOrWhiteSpace(downloadUrl))
            return;
        
        // move current assembly
        var currentAssemblyName = Assembly.GetExecutingAssembly().Location;
        var oldAssemblyName = Path.ChangeExtension(currentAssemblyName, ".dll.old");

        File.Move(currentAssemblyName, oldAssemblyName);

        try
        {
            // start update
            using var resp = httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead).Result;

            resp.EnsureSuccessStatusCode(); // safe here since we have error handling

            using var dataStream = resp.Content.ReadAsStreamAsync().Result;
            using var outputStream = new FileStream(currentAssemblyName, FileMode.Create, FileAccess.Write);

            dataStream.CopyTo(outputStream);
        }
        catch (Exception ex)
        {
            // move back the current assembly since update failed
            File.Move(oldAssemblyName, currentAssemblyName);

            LogManager.Log($"Auto-update failed ({ex.GetType().Name}): {ex.Message}");
            LogManager.LogException(ex);
        }

        LogManager.Log("Auto-update was a success!");
        File.Delete(oldAssemblyName); // clear old dll

        LogManager.Log("Loading new assembly..");

        Assembly newVersion = Assembly.Load(File.ReadAllBytes(currentAssemblyName));

        Main.Instance?.gameObject.Destroy();

        var newMainType = newVersion.GetType("BingusNametagsPlusPlus.Main");
        new GameObject("BingusNametags++").AddComponent(newMainType);
    }
}
