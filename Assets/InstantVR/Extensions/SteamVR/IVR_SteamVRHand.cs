/* InstantVR SteamVR head controller
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.4.9
 * date: June 2, 2016
 * 
 * - Removed check on VRSettings
 */

using UnityEngine;
using UnityEngine.VR;

namespace IVR {

    public class IVR_SteamVRHand : IVR_HandController {

        [HideInInspector]
        private GameObject controller;
        [HideInInspector]
        private int controllerIndex;
        [HideInInspector]
        private SteamVR_TrackedObject steamTracker;

        private Vector3 palm2Wrist;

        public override void StartController(InstantVR ivr) {
            extension = ivr.GetComponent<IVR_SteamVR>();
            base.StartController(ivr);

            controller = new GameObject("Steam Controller");
            if (transform == ivr.leftHandTarget) {
                controller.name = "Steam Controller Left";
                palm2Wrist = new Vector3(-0.07F, 0.03F, -0.15F);
            } else {
                controller.name = "Steam Controller Right";
                palm2Wrist = new Vector3(0.07F, 0.03F, -0.15F);
            }

            IVR_SteamVRHead steamHead = ivr.headTarget.GetComponent<IVR_SteamVRHead>();
            controller.transform.parent = steamHead.headcamRoot.transform;

            controller.transform.position = transform.position;
            controller.transform.rotation = transform.rotation;

            steamTracker = controller.AddComponent<SteamVR_TrackedObject>();

            controllerInput = Controllers.GetController(0);

            SteamVR_Utils.Event.Listen("device_connected", OnDeviceConnected);

            SetBodyRotation();
        }

        private void OnDeviceConnected(params object[] args) {
            int i = (int) args[0];
            bool connected = (bool) args[1];

            SteamVR vr = SteamVR.instance;
            bool isController = (vr.hmd.GetTrackedDeviceClass((uint) i) == Valve.VR.ETrackedDeviceClass.Controller);
            if (isController && connected) {
                bool isLeftController = (i == SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost));
                if (transform == ivr.leftHandTarget && isLeftController) {
                    controllerIndex = i;
                    steamTracker.index = (SteamVR_TrackedObject.EIndex) controllerIndex;
                } else if (transform == ivr.rightHandTarget && !isLeftController) {
                    controllerIndex = i;
                    steamTracker.index = (SteamVR_TrackedObject.EIndex) controllerIndex;
                }
            }
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

        private ControllerInput controllerInput;

        private void UpdateInput() {
            if (transform == ivr.leftHandTarget)
                SetControllerInput(controllerInput.left);
            else
                SetControllerInput(controllerInput.right);
        }

        public Valve.VR.VRControllerState_t controllerState;

        private void SetControllerInput(ControllerInputSide controllerInputSide) {
            SteamVR_Controller.Device device = SteamVR_Controller.Input(controllerIndex);

            controllerInputSide.stickHorizontal += device.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad).x;
            controllerInputSide.stickVertical += device.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad).y;

            controllerInputSide.trigger += device.GetPress(Valve.VR.EVRButtonId.k_EButton_Grip) ? 1 : 0;
            controllerInputSide.bumper += device.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger).x;

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
