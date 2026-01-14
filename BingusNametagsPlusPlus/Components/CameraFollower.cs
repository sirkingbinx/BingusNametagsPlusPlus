using UnityEngine;

namespace BingusNametagsPlusPlus.Components;

public class CameraFollower : MonoBehaviour
{
    public bool LookingAtThirdPerson = false;

	private void Update()
	{
		transform.LookAt((LookingAtThirdPerson ? GorillaTagger.Instance.thirdPersonCamera : GorillaTagger.Instance.mainCamera).transform);
		transform.Rotate(new Vector3(180, 0, 180));
	}
}