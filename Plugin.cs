using BepInEx;
using SirKingBinx;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BingusNametagsPlusPlus;

[BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
#if RELEASE
        if (!IntegrityCheck.TestIntegrityStrongName("0d26e2837731d963").Result)
        {
            ShowMessageDialog("SN integrity test failed", "Your release version of BingusNametags++ has been modified and game startup has stopped to prevent damage to your system. Please run a virus check or reinstall all mods.");
            Environment.Exit(0);
            return;
        }
#endif

        GameObject bingusNametagsGameObject = new GameObject("BingusNametagsPlusPlus");
        bingusNametagsGameObject.AddComponent<Main>();
        DontDestroyOnLoad(bingusNametagsGameObject);
    }

    public static void ShowMessageDialog(string title, string content)
    {
        IntPtr hwnd = GetActiveWindow();
        MessageBox(hwnd, content, title, 0x00000000 | 0x00000060);
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);
}
