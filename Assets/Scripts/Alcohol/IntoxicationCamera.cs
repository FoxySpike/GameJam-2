using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>Opt gameplay into the volume and draw the UI layer afterwards without post-processing.</summary>
[DisallowMultipleComponent, RequireComponent(typeof(Camera))]
public sealed class IntoxicationCamera : MonoBehaviour
{
    private UniversalAdditionalCameraData data;
    private LayerMask previousMask;
    private bool previousPostProcessing;
    private bool configured;
    private Camera sourceCamera;
    private Camera uiCamera;
    private int uiMask;

    public static void Ensure(Camera camera)
    {
        if (camera != null && camera.TryGetComponent(out UniversalAdditionalCameraData existing) &&
            existing.renderType == CameraRenderType.Overlay) return;
        if (camera != null && camera.GetComponent<IntoxicationCamera>() == null)
            camera.gameObject.AddComponent<IntoxicationCamera>();
    }

    private void OnEnable()
    {
        int layer = LayerMask.NameToLayer(DrunkenVision.VolumeLayer);
        if (layer < 0) return;
        sourceCamera = GetComponent<Camera>();
        data = sourceCamera.GetUniversalAdditionalCameraData();
        previousMask = data.volumeLayerMask;
        previousPostProcessing = data.renderPostProcessing;
        data.volumeLayerMask = previousMask.value | (1 << layer);
        data.renderPostProcessing = true;
        CreateUICamera();
        configured = true;
    }

    private void CreateUICamera()
    {
        // Screen Space Overlay HUD already bypasses camera effects. This also protects
        // World Space canvases (the park sign) and Screen Space Camera UI on the UI layer.
        uiMask = sourceCamera.cullingMask & (1 << LayerMask.NameToLayer("UI"));
        if (uiMask == 0 || data.renderType != CameraRenderType.Base) return;
        var stack = data.cameraStack;
        if (stack == null) return;
        var uiObject = new GameObject("UI without intoxication");
        uiObject.transform.SetParent(transform, false);
        uiCamera = uiObject.AddComponent<Camera>();
        CopyCameraSettings();
        UniversalAdditionalCameraData uiData = uiCamera.GetUniversalAdditionalCameraData();
        uiData.renderType = CameraRenderType.Overlay;
        uiData.renderPostProcessing = false;
        uiData.volumeLayerMask = 0;
        // URP 17 exposes clearDepth without a setter. Set its serialized backing
        // field so world-space UI still respects the gameplay camera's depth.
        JsonUtility.FromJsonOverwrite("{\"m_ClearDepth\":false}", uiData);
        uiData.renderShadows = false;
        stack.Add(uiCamera);
        sourceCamera.cullingMask &= ~uiMask;
    }

    private void LateUpdate()
    {
        if (uiCamera != null) CopyCameraSettings();
    }

    private void CopyCameraSettings()
    {
        uiCamera.CopyFrom(sourceCamera);
        uiCamera.cullingMask = uiMask;
        uiCamera.clearFlags = CameraClearFlags.Nothing;
        uiCamera.enabled = true;
    }

    private void OnDisable()
    {
        if (!configured || data == null) return;
        data.volumeLayerMask = previousMask;
        data.renderPostProcessing = previousPostProcessing;
        if (uiCamera != null)
        {
            var stack = data.cameraStack;
            if (stack != null) stack.Remove(uiCamera);
            uiCamera.enabled = false;
            Destroy(uiCamera.gameObject);
            uiCamera = null;
            sourceCamera.cullingMask |= uiMask;
        }
        configured = false;
    }
}
