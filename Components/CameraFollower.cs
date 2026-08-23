using UnityEngine;

namespace BingusNametagsPlusPlus.Components;

public class CameraFollower : MonoBehaviour
{
    public bool lookingAtThirdPerson = false;
	public Transform? trackingTransform;

	private void Update()
	{
		trackingTransform ??= lookingAtThirdPerson ? GorillaTagger.Instance.thirdPersonCamera.transform.Find("Shoulder Camera") : GorillaTagger.Instance.mainCamera.transform;

        transform.LookAt(trackingTransform);
		transform.Rotate(lookingAtThirdPerson ? new Vector3(0, 180, 0) : new Vector3(180, 0, 180));
	}
}