using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BingusNametagsPlusPlus.Components;
using BingusNametagsPlusPlus.Utilities;
using UnityEngine;

using NConfig = BingusNametagsPlusPlus.Utilities.Config;
using Object = UnityEngine.Object;

namespace BingusNametagsPlusPlus;

[BepInPlugin("bingus.nametagspp", "BingusNametags++", "1.1.0")]
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

			var prefix = "";
			
			if (NConfig.UserCustomIcons)
			{
				if (rig.OwningNetPlayer.UserId == "DEFC9810769F1F55")
					prefix += "<sprite name=\"admin\"> ";
			}

			if (NConfig.ShowingPlatform)
				prefix += $"<sprite name=\"{GetPlatformString(rig)}\">";

			var ir = Nametags[rig];
			ir.SetText($"{prefix}{(NConfig.ShowingName ? rig.OwningNetPlayer.NickName : "")}");
		}
	}

	private void OnGUI() => UIManager.OnGUI();

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
	
	public static T Load<T>(string path, string name) where T : Object
	{
		var ab = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(path));
		var obj = ab.LoadAsset<T>(name);
		
		if (obj.Uninitialized())
			Debug.LogError($"Cannot load assetbundle \"{path}\" object \"{name}\" to type \"{typeof(T).FullName}.\nValid streams: \n\t{Assembly.GetExecutingAssembly().GetManifestResourceNames().Join("\n\t")}");
			
		ab.Unload(false);

		return obj;
	}
	
	public static string AssemblyDirectory
	{
		get
		{
			var codeBase = Assembly.GetExecutingAssembly().CodeBase;
			var uri = new UriBuilder(codeBase);
			var path = Uri.UnescapeDataString(uri.Path);
            
			return Path.GetDirectoryName(path);
		}
	}
}