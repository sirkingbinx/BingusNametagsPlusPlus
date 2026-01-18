using System;
using BingusNametagsPlusPlus.Classes;
using BingusNametagsPlusPlus.Components;
using GorillaExtensions;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BingusNametagsPlusPlus.Utilities;

public static class NametagCreator
{
	private static GameObject CreateNametag(VRRig owner, string layerName)
	{
		var parent = owner.transform.Find("Body") ?? owner.transform;
		var tagObject = Object.Instantiate(Main.NametagDefault, parent, false);

		tagObject?.gameObject.layer = LayerMask.NameToLayer(layerName);
		tagObject?.transform.localPosition = new Vector3(0f, ConfigManager.Offset, 0f);
		tagObject?.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

        tagObject?.AddComponent<CameraFollower>();

        var tmPro = tagObject?.GetComponent<TextMeshPro>();
		tmPro?.text = "...";

		if (!ConfigManager.CustomFont.IsNull())
			tmPro?.font = ConfigManager.CustomFont;

        // Fixed shaders because URP is love URP is life
        // Moved to main.cs during init

        return tagObject ?? throw new Exception("Missing AB");
	}

	public static PlayerNametag CreateNametagObject(VRRig owner)
	{
        return new PlayerNametag(
			owner,
			CreateNametag(owner, "FirstPersonOnly"),
			CreateNametag(owner, "MirrorOnly")
        );
	}
}
