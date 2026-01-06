using UnityEngine;

namespace BingusNametagsPlusPlus.Components;

public class CameraFollower : MonoBehaviour
{
	private void Update()
	{
		transform.LookAt(GorillaTagger.Instance.mainCamera.transform);
		transform.Rotate(new Vector3(180, 0, 180));
	}
}