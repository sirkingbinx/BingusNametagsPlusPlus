using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx;
using BingusNametagsPlusPlus.Classes;
using BingusNametagsPlusPlus.Interfaces;
using BingusNametagsPlusPlus.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BingusNametagsPlusPlus;

[BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
public class Main : BaseUnityPlugin
{
	public static Main? Instance;

	internal static GameObject? NametagDefault;
    internal static Action? UpdateNametags;

    internal static bool PluginEnabled = true;

    internal static Dictionary<IBaseNametag, Dictionary<VRRig, PlayerNametag>> Nametags = new();

	private void Start()
    {
        try { LogManager.CreateLog(); }
        catch (Exception ex)
        {
            ex.Report();
        }

        LogManager.Log("Loading assetbundle [1/4]");

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
            LogManager.LogLine();
            LogManager.Log($"Plugins loaded: {PluginManager.Plugins.Count}");
            LogManager.LogLine();

            if (Constants.Channel != ReleaseChannel.Stable)
            {
                LogManager.Log("WARNING!!! This is a beta build of BingusNametags++.\nBugs are to be expected.");
                LogManager.LogLine();
            }
        }

        // report errors
        {
            if (!PluginManager.PluginFailures.Any())
                return;

            LogManager.Log("Some errors occured, we have logged them to the console and displayed them");

            UIManager.Ask(
                $"There were errors loading some nametags.\n\n{PluginManager.PluginFailures.Zip("\n- ")}\n\nIf you are a user, please report these messages to the developer(s) of the nametag.",
                ["OK"],
                (ans) => { }
            );
        }
    }

	private void Update()
	{
		UIManager.Update();
        UpdateNametags?.Invoke();
    }

    public void OnEnable()
    {
        PluginEnabled = true;
    }

    public void OnDisable()
    {
        PluginEnabled = false;
        ConfigManager.SavePrefs();
	}

	private void OnGUI() => UIManager.OnGUI();
	private void LateUpdate() => NetworkingManager.SetNetworkedProperties();

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