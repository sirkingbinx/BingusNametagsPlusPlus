using UnityEngine;

namespace BingusNametagsPlusPlus.Components;

public class CameraFollower : MonoBehaviour
{
    public bool lookingAtThirdPerson = false;
	public Transform trackingTransform = null!;
	public Vector3 rotateEulers;


    private void Start()
	{
		trackingTransform = lookingAtThirdPerson ? GorillaTagger.Instance.thirdPersonCamera.transform.Find("Shoulder Camera") : GorillaTagger.Instance.mainCamera.transform;
        rotateEulers = lookingAtThirdPerson ? new Vector3(0, 180, 0) : new Vector3(180, 0, 180);
    }

	private void Update()
	{
        transform.LookAt(trackingTransform);
		transform.Rotate(rotateEulers);
	}
}