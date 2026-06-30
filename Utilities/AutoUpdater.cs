using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using UnityEngine;

namespace BingusNametagsPlusPlus.Utilities;

public static class AutoUpdater
{
    private static readonly HttpClient httpClient = new();
    private const string updateVersionUrl = "https://files.sirkingbinx.dev/bingusNametagsPlusPlus/version.txt";
    private const string updateFileUrl = "https://files.sirkingbinx.dev/bingusNametagsPlusPlus/latest.dll";

    public static void Invoke()
    {
        httpClient.DefaultRequestHeaders.Add("User-Agent", $"BingusNametags++/{Constants.Version} ({System.Environment.Version.ToString()} System.Net.Http.HttpClient)"); 
        
        if (Config.Current.AutoUpdateMode == 2) // auto-update off
            return;

        try
        {
            var currentVersion = new Version(Constants.Version);
            var latestVersion = new Version(httpClient.GetStringAsync(updateVersionUrl).Result);

            if (currentVersion < latestVersion)
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
        if (result == "Decline")
            return;
        
        // move current assembly
        var currentAssemblyName = Assembly.GetExecutingAssembly().Location;
        var oldAssemblyName = Path.ChangeExtension(currentAssemblyName, ".dll.old");

        File.Move(currentAssemblyName, oldAssemblyName);

        try
        {
            // start update
            using var resp = httpClient.GetAsync(updateFileUrl, HttpCompletionOption.ResponseHeadersRead).Result;

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
