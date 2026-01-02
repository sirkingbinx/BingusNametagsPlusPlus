using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BingusNametagsPlusPlus.Components;
using BingusNametagsPlusPlus.Utilities;
using UnityEngine;
using NConfig = BingusNametagsPlusPlus.Utilities.Config;
using Object = UnityEngine.Object;

namespace BingusNametagsPlusPlus;

[BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
public class Main : BaseUnityPlugin
{
	public static Main? Instance;
	private static readonly Dictionary<VRRig, NametagObject> Nametags = new();

	internal static GameObject? NametagDefault;

	private void Start()
	{
		NametagDefault = Load<GameObject>(@"BingusNametagsPlusPlus.Resources.nametags", "Nametag");
		Instance = this;
		NConfig.LoadPrefs();
	}

	private void Update()
	{
		UIManager.Update();

		if (!GorillaParent.hasInstance || !NConfig.ShowingNametags)
			return;

		foreach (var pair in Nametags.Where(p => !GorillaParent.instance.vrrigs.Contains(p.Key)))
		{
			pair.Value.Destroy();
			Nametags.Remove(pair.Key);
		}

		foreach (var rig in GorillaParent.instance.vrrigs.Where(rig => !NConfig.ShowPersonalTag || rig != GorillaTagger.Instance.offlineVRRig))
		{
			if (!Nametags.ContainsKey(rig))
				Nametags.Add(rig, Utilities.Nametags.CreateNametagObject(rig));

			var prefix = "";

			if (NConfig.GlobalIconsEnabled)
			{
				if (NConfig.UserCustomIcons &&
				    Constants.SpecialBadgeIds.TryGetValue(rig.OwningNetPlayer.UserId.ToLower(), out var n))
				{
					var adding = "";
					n.Split(",").ForEach(sprite => adding += $"<sprite name=\"{sprite}\"> ");
					prefix += adding;
				}

				if (NConfig.ShowingPlatform)
					prefix += $"<sprite name=\"{GetPlatformString(rig)}\">";
			}

			var ir = Nametags[rig];
			ir.SetText($"{prefix}{(NConfig.ShowingName ? rig.OwningNetPlayer.NickName : "")}");
		}
	}

	public void OnDisable()
	{
		NConfig.ShowingNametags = false;

		Nametags.ForEach(pair => pair.Value.Destroy());
		Nametags.Clear();

		NConfig.SavePrefs();
	}

	private void OnGUI() => UIManager.OnGUI();
	private void LateUpdate() => Networking.SetNetworkedProperties();
	public void OnEnable() => NConfig.ShowingNametags = true;

	private static string GetPlatformString(VRRig player)
	{
		var cosmetics = player.concatStringOfCosmeticsAllowed.ToLower();
		var properties = player.OwningNetPlayer.GetPlayerRef().CustomProperties.Count;

		return cosmetics.Contains("s. first login") ? "steam" : (cosmetics.Contains("first login") || cosmetics.Contains("game-purchase") || properties > 1) ? "oculus" : "meta";
	}

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