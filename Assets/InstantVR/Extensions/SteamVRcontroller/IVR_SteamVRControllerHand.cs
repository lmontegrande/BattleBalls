/* InstantVR SteamVR hand controller
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.5.3
 * date: August 29, 2016
 * 
 * - mapped button[0] to SteamVR controller trigger
 */

using UnityEngine;
using UnityEngine.VR;

namespace IVR {

    public class IVR_SteamVRControllerHand : IVR_HandController {

        [HideInInspector]
        private GameObject controller;
        [HideInInspector]
        private SteamVR_TrackedObject steamTracker;

        private Vector3 palm2Wrist;

        public override void StartController(InstantVR ivr) {
            extension = ivr.GetComponent<IVR_SteamVR>();
            base.StartController(ivr);

            IVR_UnityVRHead unityVRhead = ivr.headTarget.GetComponent<IVR_UnityVRHead>();

            if (unityVRhead != null &&  unityVRhead.steamManager != null) {
                steamTracker = CreateSteamController(ivr, transform);

                if (transform == ivr.leftHandTarget) {
                    unityVRhead.steamManager.left = steamTracker.gameObject;
                    palm2Wrist = new Vector3(-0.03F, 0.06F, -0.15F);
                } else {
                    unityVRhead.steamManager.left = steamTracker.gameObject;
                    palm2Wrist = new Vector3(0.03F, 0.06F, -0.15F);
                }

                SteamVR_Utils.Event.Listen("device_connected", OnDeviceConnected);

                SetBodyRotation();
            } else
                enabled = false;

            controllerInput = Controllers.GetController(0);
        }

        public static SteamVR_TrackedObject CreateSteamController(InstantVR ivr, Transform transform) {
            IVR_UnityVRHead unityVRhead = ivr.headTarget.GetComponent<IVR_UnityVRHead>();

            GameObject controller = new GameObject();
            if (transform == ivr.leftHandTarget) {
                controller.name = "Steam Controller Left";
            } else {
                controller.name = "Steam Controller Right";
            }

            controller.transform.parent = unityVRhead.cameraRoot.transform;
            controller.transform.position = transform.position;
            controller.transform.rotation = transform.rotation;

            SteamVR_TrackedObject steamTracker = controller.AddComponent<SteamVR_TrackedObject>();

            return steamTracker;
        }

        private void OnDeviceConnected(params object[] args) {
            int i = (int) args[0];
            bool connected = (bool) args[1];

            IVR_UnityVRHead unityVRhead = ivr.headTarget.GetComponent<IVR_UnityVRHead>();

            Valve.VR.ETrackedDeviceClass deviceClass = SteamVR.instance.hmd.GetTrackedDeviceClass((uint) i);

            bool isController = (deviceClass == Valve.VR.ETrackedDeviceClass.Controller);
            if (isController && connected) {
                bool isLeftController = (i == SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost));
                if (transform == ivr.leftHandTarget && isLeftController) {
                    if (steamTracker.index != 0) {
                        SwitchSteamTracker(steamTracker.index, true);
                    }
                    steamTracker.index = (SteamVR_TrackedObject.EIndex) i;
                    unityVRhead.steamManager.left = steamTracker.gameObject;
                } else if (transform == ivr.rightHandTarget && !isLeftController) {
                    if (steamTracker.index != 0) {
                        SwitchSteamTracker(steamTracker.index, false);
                    }
                    
                    steamTracker.index = (SteamVR_TrackedObject.EIndex) i;
                    unityVRhead.steamManager.right = steamTracker.gameObject;
                }
            }
        }

        private void SwitchSteamTracker(SteamVR_TrackedObject.EIndex index, bool isLeft) {
            IVR_UnityVRHead unityVRhead = ivr.headTarget.GetComponent<IVR_UnityVRHead>();

            Transform otherSteamTrackerTransform;
            if (isLeft) {
                otherSteamTrackerTransform = unityVRhead.cameraRoot.transform.FindChild("Steam Controller Right");
                unityVRhead.steamManager.right = otherSteamTrackerTransform.gameObject;
            } else {
                otherSteamTrackerTransform = unityVRhead.cameraRoot.transform.FindChild("Steam Controller Left");
                unityVRhead.steamManager.left = otherSteamTrackerTransform.gameObject;
            }
            SteamVR_TrackedObject otherSteamTracker = otherSteamTrackerTransform.GetComponent<SteamVR_TrackedObject>();
            otherSteamTracker.index = index;
        }

        private void SetBodyRotation() {
            IVR_AnimatorHip hipAnimator = ivr.hipTarget.GetComponent<IVR_AnimatorHip>();
            if (hipAnimator != null) {
                if (hipAnimator.rotationMethod == IVR_AnimatorHip.Rotations.Auto) {
                    hipAnimator.rotationMethod = IVR_AnimatorHip.Rotations.HandRotation;
                }
            }
        }

        public override void UpdateController() {
            if (enabled) {
                UpdateTransform();
                UpdateInput();
            }
        }

        private void UpdateTransform() {
            if (steamTracker.isValid) {
                tracking = true;
                Vector3 controllerPosition = steamTracker.transform.position + ivr.transform.rotation * new Vector3(0, 0.1333F, 0.1333F); //strange correction needed to get controller at right position
                Quaternion controllerRotation = steamTracker.transform.rotation;

                Quaternion localHandRotation = Quaternion.Inverse(ivr.transform.rotation) * steamTracker.transform.rotation;

                if (transform == ivr.leftHandTarget)
                    transform.rotation = ivr.transform.rotation * localHandRotation * Quaternion.Euler(45, 0, 90);
                else
                    transform.rotation = ivr.transform.rotation * localHandRotation * Quaternion.Euler(45, 0, -90);

                transform.position = controllerPosition  + controllerRotation * palm2Wrist;
            }
        }

        /*
        private void UpdateTransform() {
            tracking = steamTracker != null && steamTracker.isValid && (steamTracker.index != SteamVR_TrackedObject.EIndex.Hmd);
            if (tracking) {
                Vector3 localPalmPosition = controller.transform.position - ivr.transform.position;
                Quaternion localHandRotation = Quaternion.Inverse(ivr.transform.rotation) * controller.transform.rotation;

                if (transform == ivr.leftHandTarget)
                    transform.rotation = ivr.transform.rotation * localHandRotation * Quaternion.Euler(45, 0, 90);
                else
                    transform.rotation = ivr.transform.rotation * localHandRotation * Quaternion.Euler(45, 0, -90);

                transform.position = ivr.transform.position + localPalmPosition + transform.rotation * palm2Wrist;
            }
        }
        */

        private ControllerInput controllerInput;

        private void UpdateInput() {
            if (transform == ivr.leftHandTarget)
                SetControllerInput(controllerInput.left);
            else
                SetControllerInput(controllerInput.right);
        }

        public Valve.VR.VRControllerState_t controllerState;

        private void SetControllerInput(ControllerInputSide controllerInputSide) {
            SteamVR_Controller.Device device = SteamVR_Controller.Input((int) steamTracker.index);

            controllerInputSide.stickHorizontal += device.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad).x;
            controllerInputSide.stickVertical += device.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad).y;

            controllerInputSide.trigger += device.GetPress(Valve.VR.EVRButtonId.k_EButton_Grip) ? 1 : 0;
            controllerInputSide.bumper += device.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger).x;
            controllerInputSide.buttons[0] = device.GetPress(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger);

            controllerInputSide.stickButton |= device.GetPress(SteamVR_Controller.ButtonMask.Touchpad);
            controllerInputSide.option |= device.GetPress(SteamVR_Controller.ButtonMask.ApplicationMenu);

            if (controllerInputSide.stickButton) {
                controllerInputSide.stickHorizontal += device.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad).x;
                controllerInputSide.stickVertical += device.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad).y;
            }

            controllerInputSide.up |= (controllerInputSide.stickButton && controllerInputSide.stickVertical > 0.3F);
            controllerInputSide.down |= (controllerInputSide.stickButton && controllerInputSide.stickVertical < -0.3F);
            controllerInputSide.left |= (controllerInputSide.stickButton && controllerInputSide.stickHorizontal < -0.3F);
            controllerInputSide.right |= (controllerInputSide.stickButton && controllerInputSide.stickHorizontal > 0.3F);
        }

    }
}
