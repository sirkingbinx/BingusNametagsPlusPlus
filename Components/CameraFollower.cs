using BingusNametagsPlusPlus.Utilities;
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace BingusNametagsPlusPlus.Components;

public class CameraFollower : MonoBehaviour
{
    public bool lookingAtThirdPerson = false;

    private Vector3 baseScale;
    private Camera thirdPersonCam = null!;

    private void Awake() => baseScale = transform.localScale;

    private void OnEnable() => RenderPipelineManager.beginCameraRendering += OnCameraPreCull;
    private void OnDisable() => RenderPipelineManager.beginCameraRendering -= OnCameraPreCull;

    private void OnCameraPreCull(ScriptableRenderContext context, Camera cam)
    {
        try
        {
            thirdPersonCam ??= GorillaTagger.Instance.thirdPersonCamera.transform.Find("Shoulder Camera").GetComponent<Camera>();

            if (cam == thirdPersonCam)
            {
                transform.rotation = cam.transform.rotation;

                // Claude wrote this detection to tell if it's mirrored or not
                Vector3 s = cam.transform.lossyScale;
                bool mirrored = (s.x * s.y * s.z) < 0f || cam.projectionMatrix.m00 < 0f;

                transform.localScale = new Vector3(
                    mirrored ? -Mathf.Abs(baseScale.x) : Mathf.Abs(baseScale.x),
                    baseScale.y,
                    baseScale.z);
            }
            else
            {
                // FP works with no extra code
                transform.rotation = cam.transform.rotation;
                transform.localScale = baseScale;
            }
        } catch (Exception ex)
        {
            LogManager.LogException(ex);
        }
    }
}