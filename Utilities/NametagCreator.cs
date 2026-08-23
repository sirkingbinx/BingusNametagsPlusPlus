using System;
using BingusNametagsPlusPlus.Classes;
using BingusNametagsPlusPlus.Components;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BingusNametagsPlusPlus.Utilities;

public static class NametagCreator
{
	private static GameObject CreateNametag(VRRig owner, string layer)
	{
		var parent = owner.transform.Find("Body") ?? owner.transform;
		var tagObject = Object.Instantiate(Main.NametagDefault, parent, false);

		tagObject?.transform.localPosition = new Vector3(0f, Config.Current.Offset, 0f);
		tagObject?.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

		tagObject?.layer = LayerMask.NameToLayer(layer);

        var cf = tagObject?.AddComponent<CameraFollower>();
		cf?.lookingAtThirdPerson = (layer == "MirrorOnly");

        var tmPro = tagObject?.GetComponent<TextMeshPro>();
		tmPro?.text = "...";

		if (Config.Current.CustomFont is TMP_FontAsset f)
			tmPro?.font = f;

        return tagObject ?? throw new Exception("Missing AB");
	}

	public static PlayerNametag CreateNametagObject(VRRig owner) =>
		new PlayerNametag(owner, CreateNametag(owner, "FirstPersonOnly"), CreateNametag(owner, "MirrorOnly"));
}
