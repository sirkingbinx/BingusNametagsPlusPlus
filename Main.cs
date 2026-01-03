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
using NConfig = BingusNametagsPlusPlus.Utilities.Config;
using Object = UnityEngine.Object;

namespace BingusNametagsPlusPlus;

[BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
public class Main : BaseUnityPlugin
{
	public static Main? Instance;

	internal static GameObject? NametagDefault;
    internal static Action UpdateNametags = delegate { };

    internal static List<IBaseNametag> Plugins = new();
    internal static Dictionary<IBaseNametag, Dictionary<VRRig, PlayerNametag>> Nametags = new();

	private void Start()
	{
		NametagDefault = Load<GameObject>(@"BingusNametagsPlusPlus.Resources.nametags", "Nametag");
		Instance = this;
		NConfig.LoadPrefs();

		Debug.Log("Loading nametags...");

        var nametagTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(IBaseNametag).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);

        foreach (var nametagType in nametagTypes)
        {
            if (Activator.CreateInstance(nametagType) is not IBaseNametag nametag)
                return;

			Debug.Log($"Loaded nametag {nametag.Name}");

			Plugins.Add(nametag);

            UpdateNametags += () =>
            {
                Nametags.TryAdd(nametag, new Dictionary<VRRig, PlayerNametag>());
				nametag.Update(Nametags[nametag], nametag.Offset);
            };
        }
    }

	private void Update()
	{
		UIManager.Update();
        UpdateNametags();
    }

	public void OnDisable()
    {
        NConfig.ShowingNametags = false;
		NConfig.SavePrefs();
	}

	private void OnGUI() => UIManager.OnGUI();
	private void LateUpdate() => Networking.SetNetworkedProperties();
	public void OnEnable() => NConfig.ShowingNametags = true;

	private static T Load<T>(string path, string name) where T : Object
	{
		var ab = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(path));
		var obj = ab.LoadAsset<T>(name);

		if (obj.Uninitialized())
			Debug.LogError(
				$"Cannot load assetbundle \"{path}\" object \"{name}\" to type \"{typeof(T).FullName}.\nValid streams: \n\t{Assembly.GetExecutingAssembly().GetManifestResourceNames().Join("\n\t")}");

		ab.Unload(false);

		return obj;
	}
}