using System.Collections;
using ReadyPlayerMe.AvatarLoader;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.MagicLeap;
using InputDevice = UnityEngine.XR.InputDevice;

namespace ML2_RPM
{
    public class AvatarEyeTracking : MonoBehaviour
    {
    
        private const float EyeBlinkMultiplier = 1f;
        private const string EYE_BLINK_LEFT_BLEND_SHAPE_NAME = "eyeBlinkLeft";
        private const string EYE_BLINK_RIGHT_BLEND_SHAPE_NAME = "eyeBlinkRight";
        
        private bool hasBlinkBlendShapes;
    
        private SkinnedMeshRenderer headMesh;
        public int eyeBlinkLeftBlendShapeIndex = -1;
        public int eyeBlinkRightBlendShapeIndex = -1;

        private InputActionProperty _rightEyeInputRotation;
        private InputActionProperty _leftEyeInputRotation;
    
        private Transform _eyesFixationPoint;

        // Used to get ml inputs.
        private MagicLeapInputs _mlInputs;

        // Used to get eyes action data.
        private MagicLeapInputs.EyesActions _eyesActions;

        // Used to get other eye data
        private InputDevice _eyesDevice;

        // Was EyeTracking permission granted by user
        private bool _permissionGranted = false;
        private readonly MLPermissions.Callbacks _permissionCallbacks = new MLPermissions.Callbacks();
    
        public Transform leftEyeBone;
        public Transform rightEyeBone;
        

        void Awake()
        {
            //Request eye tracking permission
            _permissionCallbacks.OnPermissionGranted += OnPermissionGranted;
            _permissionCallbacks.OnPermissionDenied += OnPermissionDenied;
            _permissionCallbacks.OnPermissionDeniedAndDontAskAgain += OnPermissionDenied;
            
            //Get the head mesh
            headMesh = gameObject.GetMeshRenderer(MeshType.HeadMesh);
            hasBlinkBlendShapes = HasBlinkBlendshapes();
        
        }
    
        private IEnumerator Start()
        {
            _mlInputs = new MagicLeapInputs();
            _mlInputs.Enable();
            
            while (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                //Waiting for the microphone request from the Voice Handler script attached to the avatar prefab
                yield return null;
            }
            
            
            MLPermissions.RequestPermission(MLPermission.EyeTracking, _permissionCallbacks);
        }

        private void OnDestroy()
        {
            _permissionCallbacks.OnPermissionGranted -= OnPermissionGranted;
            _permissionCallbacks.OnPermissionDenied -= OnPermissionDenied;
            _permissionCallbacks.OnPermissionDeniedAndDontAskAgain -= OnPermissionDenied;

            _mlInputs.Disable();
            _mlInputs.Dispose();

            InputSubsystem.Extensions.MLEyes.StopTracking();
        
        }
    
        private void Update()
        {
            if (!_permissionGranted)
            {
                return;
            }

            if (!_eyesDevice.isValid)
            {
                this._eyesDevice = InputSubsystem.Utils.FindMagicLeapDevice(InputDeviceCharacteristics.EyeTracking | InputDeviceCharacteristics.TrackedDevice);
                return;
            }

            // Eye data provided by the engine for all XR devices.
            // Used here only to update the status text. The 
            // left/right eye centers are moved to their respective positions &
            // orientations using InputSystem's TrackedPoseDriver component.
            var eyes = _eyesActions.Data.ReadValue<UnityEngine.InputSystem.XR.Eyes>();
            

            // Eye data specific to Magic Leap
            InputSubsystem.Extensions.TryGetEyeTrackingState(_eyesDevice, out var trackingState);
            

            //Tracking the eye blinks
            if (trackingState.RightBlink)
            {
                Debug.Log($"Eye Tracking Blink Registered Right Eye Blink: {true}" );
                headMesh.SetBlendShapeWeight(eyeBlinkRightBlendShapeIndex, EyeBlinkMultiplier);
                 
            }
            
            else if (!trackingState.RightBlink)
            {
                headMesh.SetBlendShapeWeight(eyeBlinkRightBlendShapeIndex, 0); 
            }

            if (trackingState.LeftBlink)
            {
                Debug.Log($"Eye Tracking Blink Registered Left Eye Blink: {true}");
                headMesh.SetBlendShapeWeight(eyeBlinkLeftBlendShapeIndex, EyeBlinkMultiplier);
                
            }
            else if (!trackingState.LeftBlink)
            {
                headMesh.SetBlendShapeWeight(eyeBlinkLeftBlendShapeIndex, 0);
            }


            //Rotating the model's eyes
            var rightRotation = eyes.rightEyeRotation.eulerAngles;
            var leftRotation = eyes.leftEyeRotation.eulerAngles;
            rightEyeBone.rotation = Quaternion.Euler(rightRotation)* Quaternion.Euler(-90,180,0);
            leftEyeBone.rotation = Quaternion.Euler(leftRotation)*Quaternion.Euler(-90,180,0);
            Debug.Log(leftEyeBone.rotation);
            Debug.Log(rightEyeBone.rotation);

        }

        private void OnPermissionDenied(string permission)
        {
            MLPluginLog.Error($"{permission} denied, example won't function.");
            Debug.Log($"{permission} denied, example won't function.");
        }

        private void OnPermissionGranted(string permission)
        {
            InputSubsystem.Extensions.MLEyes.StartTracking();
            _eyesActions = new MagicLeapInputs.EyesActions(_mlInputs);
            _permissionGranted = true;
            Debug.Log("Permissions granted");
      
        }
        
        private bool HasBlinkBlendshapes()
        {
            eyeBlinkLeftBlendShapeIndex = headMesh.sharedMesh.GetBlendShapeIndex(EYE_BLINK_LEFT_BLEND_SHAPE_NAME);
            eyeBlinkRightBlendShapeIndex = headMesh.sharedMesh.GetBlendShapeIndex(EYE_BLINK_RIGHT_BLEND_SHAPE_NAME);
            return eyeBlinkLeftBlendShapeIndex > -1 && eyeBlinkRightBlendShapeIndex > -1;
        }



    }
}