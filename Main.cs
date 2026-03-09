using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BingusNametagsPlusPlus.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

#if BEPINEX
using BepInEx;
#elif MELONLOADER
using MelonLoader;
using BingusNametagsPlusPlus;

[assembly: MelonInfo(typeof(BingusNametagsPlusPlus.Main), BingusNametagsPlusPlus.Constants.Name, BingusNametagsPlusPlus.Constants.Version, BingusNametagsPlusPlus.Constants.Author)]
[assembly: MelonGame("Another Axiom", "Gorilla Tag")]
#endif

namespace BingusNametagsPlusPlus;

#if BEPINEX
[BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
public class Main : BaseUnityPlugin
#elif MELONLOADER
public class Main : MelonMod
#endif
{
    public static Main? Instance;

    internal static GameObject? NametagDefault;
    internal static Action? UpdateNametags;

    internal static bool PluginEnabled = true;

#if BEPINEX
    private void Start()
    {
#elif MELONLOADER
    public override void OnSceneWasLoaded(int buildindex, string sceneName)
    {
        if (Instance != null) return;
#endif
        try { LogManager.CreateLog(); }
        catch (Exception ex)
        {
            ex.Report();
        }

        LogManager.LogLine("Loading assetbundle item [1/4]");

        NametagDefault = Load<GameObject>("BingusNametagsPlusPlus.Resources.nametags", "Nametag");
        Instance = this;

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

#if BEPINEX
    public void Update()
#elif MELONLOADER
    public override void OnUpdate()
#endif
    {
        if (Keyboard.current.rightShiftKey.wasPressedThisFrame)
            UIManager.ShowingUI = !UIManager.ShowingUI;

        UpdateNametags?.Invoke();
    }

#if BEPINEX
    public void OnEnable()
    {
        PluginEnabled = true;
    }
#endif

#if BEPINEX
    public void OnDisable()
#elif MELONLOADER
    public override void OnDeinitializeMelon()
#endif
    {
        PluginEnabled = false;
        UpdateNametags?.Invoke(); // turn yourselves off
        ConfigManager.SavePrefs();
    }

#if BEPINEX
    public void OnGUI()
#elif MELONLOADER
    public override void OnGUI()
#endif
    {
        UIManager.OnGUI();
    }

#if BEPINEX
    public void LateUpdate()
#elif MELONLOADER
    public override void OnLateUpdate()
#endif
    {
        NetworkingManager.SetNetworkedProperties();
    }

    private static T Load<T>(string path, string name) where T : Object
    {
        var ab = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(path));
        var obj = ab.LoadAsset<T>(name);

        if (obj.Uninitialized())
            LogManager.Log(
                $"Cannot load assetbundle \"{path}\" object \"{name}\" to type \"{typeof(T).FullName}.\nValid streams: \n\t{Assembly.GetExecutingAssembly().GetManifestResourceNames().Join("\n\t")}");

        ab.Unload(false);

        return obj;
    }
}