using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.MagicLeap;
using InputDevice = UnityEngine.XR.InputDevice;

internal class HandTrackingEnable : MonoBehaviour
{
    private void Start()
    {
        // HAND_TRACKING is a normal permission, so we don't request it at runtime. It is auto-granted if included in the app manifest.
        // If it's missing from the manifest, the permission is not available.
        if (MLPermissions.CheckPermission(MLPermission.HandTracking).IsOk)
        {
            //Start Hand Tracking
            InputSubsystem.Extensions.MLHandTracking.StartTracking();
        }
    }
}