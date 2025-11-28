using BingusNametagsPlusPlus.Components;
using GorillaExtensions;
using TMPro;
using UnityEngine;

namespace BingusNametagsPlusPlus.Utilities;

public class NametagIR // Nametag Immediate Representation
{
	public GameObject FirstPersonTag;
	public GameObject ThirdPersonTag;

	public void SetText(string text)
	{
		FirstPersonTag.GetComponent<TextMeshPro>().text = text;
		ThirdPersonTag.GetComponent<TextMeshPro>().text = text;
		
		// update settings here cuz y not
		FirstPersonTag.GetComponent<TextMeshPro>().fontSize = Config.NametagScale;
		ThirdPersonTag.GetComponent<TextMeshPro>().fontSize = Config.NametagScale;
		
		FirstPersonTag.SetActive(Config.ShowInFirstPerson);
		ThirdPersonTag.SetActive(Config.ShowInThirdPerson);
	}

	public void Destroy()
	{
		FirstPersonTag.Destroy();
		ThirdPersonTag.Destroy();
	}

	public NametagIR(GameObject firstPerson, GameObject thirdPerson)
	{
		FirstPersonTag = firstPerson;
		ThirdPersonTag = thirdPerson;
	}
}

public static class Nametags
{
	public static GameObject CreateNametag(VRRig owner, string name, string layerName, string platformSpriteName)
	{
		var parent = owner.transform.Find("Body") ?? owner.transform;
		var tagObject = GameObject.Instantiate(Main.NametagDefault, parent, false);

		tagObject.gameObject.layer = LayerMask.NameToLayer(layerName);
		tagObject.transform.localPosition = new Vector3(0f, 0.8f, 0f);
		tagObject.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
		
		var tmPro = tagObject.GetComponent<TextMeshPro>();
		tmPro.text = $"<sprite name=\"{platformSpriteName}\">{name}";
		
		if (!Config.CustomFont.IsNull())
			tmPro.font = Config.CustomFont;
		
		// Fixed shaders because URP is love URP is life
		tmPro.fontMaterial.shader = Shader.Find("TextMeshPro/Mobile/Distance Field");
		tmPro.spriteAsset.material.shader = Shader.Find("UI/Default");

		var looker = tagObject.AddComponent<CameraFollower>();
		looker.Following = owner;
		looker.TMPObject = tmPro;
		
		return tagObject;
	}

	public static NametagIR CreateNametagIR(VRRig owner) =>
		new NametagIR(
			CreateNametag(owner, "Loading", "FirstPersonOnly", "meta"),
			CreateNametag(owner, "Loading", "MirrorOnly", "meta")
		);
}