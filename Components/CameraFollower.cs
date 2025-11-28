using TMPro;
using UnityEngine;

namespace BingusNametagsPlusPlus.Components;

public class CameraFollower : MonoBehaviour
{
	private void Update()
	{
		var f = Camera.main.transform.forward;
		f.y = 0f;
		f.Normalize();

		transform.rotation = Quaternion.LookRotation(f);
	}

	public VRRig Following;
	public TextMeshPro TMPObject;
}