using System;
using System.Collections.Generic;
using BingusNametagsPlusPlus.Classes;
using BingusNametagsPlusPlus.Components;
using GorillaExtensions;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BingusNametagsPlusPlus.Utilities;

public static class Nametags
{
	private static GameObject CreateNametag(VRRig owner, string name, string layerName, string platformSpriteName)
	{
		var parent = owner.transform.Find("Body") ?? owner.transform;
		var tagObject = Object.Instantiate(Main.NametagDefault, parent, false);

		tagObject?.gameObject.layer = LayerMask.NameToLayer(layerName);
		tagObject?.transform.localPosition = new Vector3(0f, Config.NametagYOffset, 0f);
		tagObject?.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

		tagObject?.AddComponent<CameraFollower>(); // follow the camera around

        var tmPro = tagObject?.GetComponent<TextMeshPro>();
		tmPro?.text = $"<sprite name=\"{platformSpriteName}\">{name}";

		if (!Config.CustomFont.IsNull())
			tmPro?.font = Config.CustomFont;

		// Fixed shaders because URP is love URP is life
		tmPro?.fontMaterial.shader = Shader.Find("TextMeshPro/Mobile/Distance Field");
		tmPro?.spriteAsset.material.shader = Shader.Find("UI/Default");

		return tagObject ?? throw new Exception("Missing AB");
	}

	public static PlayerNametag CreateNametagObject(VRRig owner)
	{
        return new PlayerNametag(
			owner,
			CreateNametag(owner, "Loading", "FirstPersonOnly", "meta"),
			CreateNametag(owner, "Loading", "MirrorOnly", "meta")
        );
	}
}
