// Note: to change what mod loader to build for, see Directory.Build.props

using BingusNametagsPlusPlus;
using BingusNametagsPlusPlus.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

#if MELONLOADER
using MelonLoader;

[assembly: MelonInfo(typeof(MLPlugin), BingusNametagsPlusPlus.Constants.Name, BingusNametagsPlusPlus.Constants.Version, BingusNametagsPlusPlus.Constants.Author)]
[assembly: MelonGame("Another Axiom", "Gorilla Tag")]

#elif BEPINEX
using BepInEx;
#endif

namespace BingusNametagsPlusPlus;

#if MELONLOADER
public class MLPlugin : MelonMod
{
    public void OnInitializeMelon() => new GameObject("BingusNametags++").AddComponent<Plugin>();
    public void OnDeinitializeMelon() => Plugin.Instance?.OnDisable();

    public void OnEnable() => Plugin.Instance?.OnEnable();
    public void OnDisable() => Plugin.Instance?.OnDisable();
}

#elif BEPINEX
[BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
public class BPlugin : BaseUnityPlugin
{
    public void Start() => new GameObject("BingusNametags++").AddComponent<Plugin>();

    public void OnEnable() => Plugin.Instance?.OnEnable();
    public void OnDisable() => Plugin.Instance?.OnDisable();
}
#endif

public class Plugin
{
    public static Plugin? Instance;

    internal static GameObject? NametagDefault;
    internal static Action? UpdateNametags;

    internal static bool PluginEnabled = true;

    public void Start()
    {
        try { LogManager.CreateLog(); }
        catch (Exception ex)
        {
            ex.Report();
        }

        LogManager.LogLine("Loading assetbundle item [1/4]");

        NametagDefault = Load<GameObject>("BingusNametagsPlusPlus.Resources.nametags", "Nametag");
        Instance = this;

        PluginEnabled = true;

        GorillaTagger.OnPlayerSpawned(() =>
        {
            try { OnPlayerSpawned(); }
            catch (Exception ex)
            {
                ex.Report();
            }
        });
    }

    private static void OnPlayerSpawned()
    {
        {
            LogManager.Log("Applying assetbundle shaders [1/4]");

            var tmPro = NametagDefault?.GetComponent<TextMeshPro>();
            tmPro?.fontMaterial.shader = Shader.Find("TextMeshPro/Mobile/Distance Field");
            tmPro?.spriteAsset.material.shader = Shader.Find("UI/Default");
        }
        // load stuff
        {
            LogManager.Log("Loading nametags [2/4]");
            Task.Run(PluginManager.LoadNametags).Wait();

            LogManager.Log("Loading configuration [3/4]");
            ConfigManager.LoadPrefs();

            LogManager.Log("Nametags have been loaded. yay [4/4]");
        }

        // summary
        {
            LogManager.LogDivider();
            LogManager.Log($"Plugins loaded: {PluginManager.Plugins.Count}");
            LogManager.LogDivider();

#pragma warning disable CS0162
            if (Constants.Channel != ReleaseChannel.Stable)
            {
                LogManager.Log("WARNING!!! This is a beta build of BingusNametags++.\nBugs are to be expected.");
                LogManager.LogDivider();
            }
#pragma warning restore CS0162
        }

        // report errors
        {
            if (!PluginManager.PluginFailures.Any())
                return;

            LogManager.Log("Some errors occured, we have logged them to the console and displayed them");

            UIManager.Ask(
                $"There were errors loading some nametags.\n\n{PluginManager.PluginFailures.Zip("\n- ")}\n\nIf you are a user, please report these messages to the developer(s) of the nametag.",
                ["OK"],
                (_) => { }
            );
        }
    }

    public void Update()
    {
        UIManager.Update();
        UpdateNametags?.Invoke();
    }

    public void LateUpdate()
    {
        NetworkingManager.SetNetworkedProperties();
    }

    public void OnEnable()
    {
        PluginEnabled = true;
    }

    public override void OnDisable()
    {
        PluginEnabled = false;
        UpdateNametags?.Invoke(); // turn yourselves off
        ConfigManager.SavePrefs();
    }

    public override void OnGUI()
    {
        UIManager.OnGUI();
    }

    private static T Load<T>(string path, string name) where T : UnityEngine.Object
    {
        var ab = AssetBundle.LoadFromStream(typeof(Plugin).Assembly.GetManifestResourceStream(path));
        var obj = ab.LoadAsset<T>(name);

        if (obj.Uninitialized())
            LogManager.Log(
                $"Cannot load assetbundle \"{path}\" object \"{name}\" to type \"{typeof(T).FullName}.\nValid streams: \n\t{typeof(Plugin).Assembly.GetManifestResourceNames().Join("\n\t")}");

        ab.Unload(false);

        return obj;
    }
}