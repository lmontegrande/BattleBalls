/* InstantVR Unity VR head controller
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.6.0
 * date: September 23, 2016
 * 
 * - restored recalibrate when using floorlever tracking
 */

using UnityEngine;
using UnityEngine.VR;

namespace IVR {

    public class IVR_UnityVRHead : IVR_Controller {
#if (UNITY_STANDALONE_WIN || UNITY_ANDROID)

        [HideInInspector]
        public GameObject cameraRoot;
        [HideInInspector]
        private Transform cameraTransform;

        [HideInInspector]
        private Vector3 neck2eyes;
        [HideInInspector]
        private Vector3 head2eyes;

        [HideInInspector]
        public bool positionalTracking = false;

        private bool needsOVR = false;
        [HideInInspector]
        public OVRManager ovrManager;

        private bool needsSteamVR = false;
        [HideInInspector]
        public SteamVR_ControllerManager steamManager;
        [HideInInspector]
        private SteamVR_TrackedObject steamTracker;

#if INSTANTVR_ADVANCED
        [HideInInspector]
        private IVR_Kinect2Head kinect2Head;
        [HideInInspector]
        private IVR_VicoVRHead vicoVRHead;
#endif

        void Start() {
            // This dummy code is here to ensure the checkbox is present in editor
        }

        public override void StartController(InstantVR ivr) {
            if (extension == null)
                extension = ivr.GetComponent<IVR_UnityVR>();

            if (VRSettings.enabled) {
                extension.present = VRDevice.isPresent;

                Camera camera = CheckCamera();
                if (camera != null) {
                    cameraTransform = camera.transform;
                    neck2eyes = HeadUtils.GetNeckEyeDelta(ivr);
                    head2eyes = HeadUtils.GetHeadEyeDelta(ivr);

                    DeterminePlatform();

                    cameraRoot = new GameObject("UnityVR Root");
                    cameraRoot.transform.parent = ivr.transform;
#if UNITY_STANDALONE_WIN
                    if (needsOVR || needsSteamVR)
                        cameraRoot.transform.position = ivr.transform.position;
                    else
#endif
                        cameraRoot.transform.position = transform.position;

                    cameraRoot.transform.rotation = ivr.transform.rotation;

                    cameraTransform.SetParent(cameraRoot.transform, false);

                    base.StartController(ivr);

                    if (needsOVR) {
                        ovrManager = transform.gameObject.AddComponent<OVRManager>();
                        if (ovrManager != null) {
                            ovrManager.resetTrackerOnLoad = true;
#if UNITY_STANDALONE_WIN
                            ovrManager.trackingOriginType = OVRManager.TrackingOrigin.FloorLevel;
                            cameraTransform.localPosition = Vector3.zero;
#else
                            ovrManager.trackingOriginType = OVRManager.TrackingOrigin.EyeLevel;
#endif
                        }
                    }

                    if (needsSteamVR) {
                        cameraTransform.gameObject.AddComponent<SteamVR_Camera>();
                        steamManager = cameraRoot.gameObject.AddComponent<SteamVR_ControllerManager>();
                        cameraTransform.localPosition = Vector3.zero;
                    }

#if INSTANTVR_ADVANCED
                    kinect2Head = GetComponent<IVR_Kinect2Head>();
                    vicoVRHead = GetComponent<IVR_VicoVRHead>();
#endif
                }
            }
        }

        private Camera CheckCamera() {
            Camera camera = transform.GetComponentInChildren<Camera>();
            if (camera == null) {
                GameObject cameraObj = new GameObject("First Person Camera");
                cameraObj.transform.SetParent(transform);
                camera = cameraObj.AddComponent<Camera>();
                camera.nearClipPlane = 0.05F;
            }

            return camera;
        }

        private void DeterminePlatform() {
#if UNITY_5_4
            switch (VRSettings.loadedDeviceName) {
                case "OpenVR":
                    needsSteamVR = true;
                    break;
                case "Oculus":
                    needsOVR = true;
                    break;
                default:
                    break;
            }
#elif UNITY_5_3
            if (VRSettings.enabled)
                needsOVR = true;
#endif
        }

        public override void UpdateController() {
            if (extension.present && enabled) {
                UpdateUnityVR();
            } else {
                tracking = false;
            }
        }

        private void UpdateUnityVR() {
            tracking = CheckTracking();
            CalculateCameraRoot();
            SetHeadTargets();
        }

        private bool CheckTracking() {
            if (needsOVR && OVRManager.tracker != null) {

#if UNITY_STANDALONE_WIN
                if (OVRManager.tracker.isPositionTracked) {
                    if (extension.trackerPosition.magnitude < 0.0001F && extension.trackerEulerAngles.magnitude < 0.0001F) {
                        extension.trackerPosition = OVRManager.tracker.GetPose().position + ivr.transform.position;
                        extension.trackerRotation = ivr.transform.rotation * OVRManager.tracker.GetPose().orientation * Quaternion.AngleAxis(-180, Vector3.up);
                        extension.trackerEulerAngles = extension.trackerRotation.eulerAngles;
                    }
                    positionalTracking = true;
                    return true;
                } else
#endif
                {
                    transform.position = cameraTransform.position + cameraTransform.rotation * -neck2eyes;
                    positionalTracking = false;
                    return true;
                }
            }

            if (needsSteamVR) {
                if (steamTracker != null) {
                    positionalTracking = steamTracker.isValid;
                    return steamTracker.isValid;
                } else {
                    steamTracker = cameraTransform.GetComponentInChildren<SteamVR_TrackedObject>();
                    //return false;
                    // Does not work at the moment, because the Headcam (head) GameObject is disabled and cannot be enabled without errors
                    positionalTracking = true;
                    return true;
                }
            }

            positionalTracking = false;
            return true;
        }

        private void CalculateCameraRoot() {
            if (tracking && needsOVR) {
#if UNITY_STANDALONE_WIN
                if (positionalTracking) {// && ovrManager.trackingOriginType == OVRManager.TrackingOrigin.EyeLevel) {
                    Vector3 ivrTrackerPosition = extension.trackerPosition;
                    Vector3 ovrTrackerPosition = OVRManager.tracker.GetPose().position;
                    Vector3 deltaPosition = ivrTrackerPosition - ovrTrackerPosition;
                    cameraRoot.transform.localPosition = deltaPosition;
                }
#endif
                Quaternion ivrTrackerRotation = extension.trackerRotation * Quaternion.AngleAxis(180, Vector3.up);
                Quaternion ovrTrackerRotation = OVRManager.tracker.GetPose().orientation;
                Quaternion deltaRotation = Quaternion.Inverse(ovrTrackerRotation) * ivrTrackerRotation;
                cameraRoot.transform.localRotation = deltaRotation;
            }
        }

        private void SetHeadTargets() {
            if (tracking) {
                transform.rotation = cameraTransform.rotation;
                if (positionalTracking)
                    transform.position = cameraTransform.position + cameraTransform.rotation * -neck2eyes;
#if INSTANTVR_ADVANCED
                else {
                    if (kinect2Head != null && kinect2Head.isTracking()) {
                        transform.position = kinect2Head.position;
                    } else if (vicoVRHead != null && vicoVRHead.isTracking()) {
                        transform.position = vicoVRHead.position;
                    }
                }
#endif
            }
        }

        private void UpdateHeadTarget() {
            if (OVRManager.tracker.isPositionTracked) {
                if (extension.trackerPosition.magnitude < 0.0001F && extension.trackerEulerAngles.magnitude < 0.0001F) {
                    extension.trackerPosition = OVRManager.tracker.GetPose().position;
                    extension.trackerRotation = OVRManager.tracker.GetPose().orientation;
                    extension.trackerEulerAngles = extension.trackerRotation.eulerAngles;
                }

                positionalTracking = true;

            } else {
                positionalTracking = false;

#if INSTANTVR_ADVANCED
                if (kinect2Head != null && kinect2Head.isTracking()) {
                    transform.position = kinect2Head.position;
                } else if (vicoVRHead != null && vicoVRHead.isTracking()) {
                    transform.position = vicoVRHead.position;
                }
#endif
            }

            CalculateCameraRoot();

            if (selected) {
                if (positionalTracking) {
                    Vector3 neckPosition = HeadUtils.CalculateNeckPosition(cameraTransform.position, cameraTransform.rotation, -head2eyes);
                    transform.position = neckPosition;
                }

                Quaternion neckRotation = cameraTransform.rotation;
                transform.rotation = neckRotation;
            }

            tracking = true;
        }

        public override void OnTargetReset() {
            InputTracking.Recenter();
            extension.trackerPosition = Vector3.zero;
            extension.trackerEulerAngles = Vector3.zero;

            positionalTracking = false;
        }
#endif
    }
}