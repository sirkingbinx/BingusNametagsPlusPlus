using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BingusNametagsPlusPlus.Components;
using BingusNametagsPlusPlus.Utilities;
using GorillaExtensions;
using UnityEngine;
using NConfig = BingusNametagsPlusPlus.Utilities.Config;
using Object = UnityEngine.Object;

namespace BingusNametagsPlusPlus;

[BepInPlugin("bingus.nametagspp", "BingusNametags++", "1.0.0")]
public class Main : BaseUnityPlugin
{
	public static Main Instance;
	private static readonly Dictionary<VRRig, NametagIR> Nametags = new();
	
	internal static GameObject? NametagDefault;

	private void Start()
	{
		NametagDefault = Load<GameObject>(@"BingusNametagsPlusPlus.Resources.nametags", "Nametag");
		Instance = this;
		NConfig.LoadPrefs();
	}

	public void OnEnable() => NConfig.ShowingNametags = true;
	public void OnDisable()
	{
		NConfig.ShowingNametags = false;
		NConfig.SavePrefs();
	}

	private void Update()
	{
		UIManager.Update();
		
		if (!GorillaParent.hasInstance)
			return;

		foreach (var pair in Nametags.Where(p => !NConfig.ShowingNametags || !GorillaParent.instance.vrrigs.Contains(p.Key)))
		{
			pair.Value.Destroy();
			Nametags.Remove(pair.Key);
		}

		foreach (var rig in GorillaParent.instance.vrrigs.Where(rig => NConfig.ShowingNametags && rig != GorillaTagger.Instance.offlineVRRig))
		{
			if (!Nametags.ContainsKey(rig))
				Nametags.Add(rig, Utilities.Nametags.CreateNametagIR(rig));

			var ir = Nametags[rig];
			ir.SetText($"{(NConfig.ShowingPlatform ? $"<sprite name=\"{GetPlatformString(rig)}\">" : "")}{(NConfig.ShowingName ? rig.OwningNetPlayer.NickName : "")}");
		}
	}

	private void OnGUI() => UIManager.OnGUI();

	private static T Load<T>(string path, string name) where T : Object
	{
		var ab = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(path));
		T? obj = ab.LoadAsset<T>(name);
		
		if (obj.IsNull())
			Debug.LogError($"Cannot load assetbundle \"{path}\" object \"{name}\" to type \"{typeof(T).FullName}.\nValid streams: \n\t{Assembly.GetExecutingAssembly().GetManifestResourceNames().Join("\n\t")}");
			
		ab.Unload(false);

		return obj;
	}

	private static string GetPlatformString(VRRig player)
	{
		var cosmetics = player.concatStringOfCosmeticsAllowed;
		var properties = player.OwningNetPlayer.GetPlayerRef().CustomProperties.Count;

		if (cosmetics.Contains("S. FIRST LOGIN"))
			return "steam";
		if (properties > 1)
			return "oculus";

		return "meta";
	}
}