using UnityEngine;

namespace BingusNametagsPlusPlus.Components;

public class CameraFollower : MonoBehaviour
{
    public bool lookingAtThirdPerson = false;

	private void Update()
	{ 
		transform.LookAt((/*lookingAtThirdPerson ? GorillaTagger.Instance.thirdPersonCamera :*/ GorillaTagger.Instance.mainCamera).transform);
		transform.Rotate(/*lookingAtThirdPerson ? new Vector3(0, 180, 180) :*/ new Vector3(180, 0, 180));
	}
}