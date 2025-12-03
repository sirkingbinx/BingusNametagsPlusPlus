using System;
using System.Collections.Generic;
using BingusNametagsPlusPlus.Components;
using GorillaExtensions;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BingusNametagsPlusPlus.Utilities;

public class NametagObject // Nametag Immediate Representation
{
	private readonly GameObject _firstPersonTag;
	private readonly GameObject _thirdPersonTag;

	private readonly NetPlayer _user;

	public NametagObject(NetPlayer player, GameObject firstPerson, GameObject thirdPerson)
	{
		_firstPersonTag = firstPerson;
		_thirdPersonTag = thirdPerson;
		_user = player;
	}

	public void SetText(string text)
	{
		var tmpFirst = _firstPersonTag.GetComponent<TextMeshPro>();
		var tmpThird = _thirdPersonTag.GetComponent<TextMeshPro>();

		tmpFirst.text = text;
		tmpThird.text = text;

		tmpFirst.fontSize = Config.NametagScale;
		tmpThird.fontSize = Config.NametagScale;

		_firstPersonTag.transform.localPosition = new Vector3(0f, Config.NametagYOffset, 0f);
		_thirdPersonTag.transform.localPosition = new Vector3(0f, Config.NametagYOffset, 0f);

		_firstPersonTag.SetActive(Config.ShowInFirstPerson);
		_thirdPersonTag.SetActive(Config.ShowInThirdPerson);

		if (Config.CustomNametags)
		{
			if (!_user.GetPlayerRef().CustomProperties.TryGetValue("BingusNametags++", out var rawData))
				return;

			var data = (Dictionary<string, object>)rawData;
			if (data == null)
				return;

			var color = (string)data["Color"];
			var bold = (bool)data["isBold"];
			var italic = (bool)data["isItalic"];
			var underlined = (bool)data["isUnderlined"];

			if (Config.ValidHexCode(color))
			{
				tmpFirst.text = $"<color=#{color}>{tmpFirst.text}</color>";
				tmpThird.text = $"<color=#{color}>{tmpThird.text}</color>";
			}

			if (bold)
			{
				tmpFirst.text = $"<b>{tmpFirst.text}</b>";
				tmpThird.text = $"<b>{tmpThird.text}</b>";
			}

			if (italic)
			{
				tmpFirst.text = $"<i>{tmpFirst.text}</i>";
				tmpThird.text = $"<i>{tmpThird.text}</i>";
			}

			if (underlined)
			{
				tmpFirst.text = $"<u>{tmpFirst.text}</u>";
				tmpThird.text = $"<u>{tmpThird.text}</u>";
			}
		}
	}

	public void Destroy()
	{
		_firstPersonTag.Destroy();
		_thirdPersonTag.Destroy();
	}
}

public static class Nametags
{
	private static GameObject CreateNametag(VRRig owner, string name, string layerName, string platformSpriteName)
	{
		var parent = owner.transform.Find("Body") ?? owner.transform;
		var tagObject = Object.Instantiate(Main.NametagDefault, parent, false);

		tagObject?.gameObject.layer = LayerMask.NameToLayer(layerName);
		tagObject?.transform.localPosition = new Vector3(0f, Config.NametagYOffset, 0f);
		tagObject?.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

		tagObject?.AddComponent<CameraFollower>();

		var tmPro = tagObject?.GetComponent<TextMeshPro>(); // follow the camera around
		tmPro?.text = $"<sprite name=\"{platformSpriteName}\">{name}";

		if (!Config.CustomFont.IsNull())
			tmPro?.font = Config.CustomFont;

		// Fixed shaders because URP is love URP is life
		tmPro?.fontMaterial.shader = Shader.Find("TextMeshPro/Mobile/Distance Field");
		tmPro?.spriteAsset.material.shader = Shader.Find("UI/Default");

		return tagObject ?? throw new Exception("Missing AB");
	}

	public static NametagObject CreateNametagObject(VRRig owner)
	{
		return new NametagObject(
			owner.OwningNetPlayer,
			CreateNametag(owner, "Loading", "FirstPersonOnly", "meta"),
			CreateNametag(owner, "Loading", "MirrorOnly", "meta"));
	}
}