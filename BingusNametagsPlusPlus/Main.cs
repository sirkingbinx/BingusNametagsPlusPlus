using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BingusNametagsPlusPlus.Classes;
using BingusNametagsPlusPlus.Components;
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
    internal static Action UpdateNametags = delegate { };

    internal static bool PluginEnabled = true;

    internal static Dictionary<IBaseNametag, Dictionary<VRRig, PlayerNametag>> Nametags = new();

	private void Start()
    {
        Debug.Log("Loading assetbundle.");

		NametagDefault = Load<GameObject>(@"BingusNametagsPlusPlus.Resources.nametags", "Nametag");
        Instance = this;

        GorillaTagger.OnPlayerSpawned(() =>
        {
            try { OnPlayerSpawned(); }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        });
    }

    private static void OnPlayerSpawned()
    {
        Debug.Log("[BG++] Loading nametags [1/2 AppDomain]..");
        PluginManager.LoadNametagsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

        Debug.Log("[BG++] Loading nametags [2/2 nametags Folder]..");
        PluginManager.LoadFromDefaultFolder();

        Debug.Log("[BG++] Loading configuration...");
        ConfigManager.LoadPrefs();

        Debug.Log("[BG++] Nametags have been loaded. yay");

        if (!PluginManager.PluginFailures.Any())
            return;

        Debug.Log("[BG++]: Some errors occured, we have logged them to the console and displayed them");

        UIManager.Ask(
            $"There were errors loading some nametags.\n\n{PluginManager.PluginFailures.Zip("\n- ")}\n\nIf you are a user, please report these messages to the developer(s) of the nametag.",
            ["OK"],
            (ans) => { }
        );
    }

	private void Update()
	{
		UIManager.Update();
        UpdateNametags();
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
	private void LateUpdate() => Networking.SetNetworkedProperties();

	private static T Load<T>(string path, string name) where T : Object
	{
		var ab = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(path));
		var obj = ab.LoadAsset<T>(name);

		if (obj.Uninitialized())
			Debug.Log(
				$"Cannot load assetbundle \"{path}\" object \"{name}\" to type \"{typeof(T).FullName}.\nValid streams: \n\t{Assembly.GetExecutingAssembly().GetManifestResourceNames().Join("\n\t")}");

		ab.Unload(false);

		return obj;
	}
}