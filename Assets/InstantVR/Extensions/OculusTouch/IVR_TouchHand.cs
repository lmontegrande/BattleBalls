/* InstantVR Oculus Touch hand controller
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.6.0
 * date: August 12, 2016
 * 
 */

using UnityEngine;
using UnityEngine.VR;

namespace IVR {

    public class IVR_TouchHand : IVR_HandController {

        [HideInInspector]
        private GameObject controller;

        [HideInInspector]
        private OVRInput.Controller touchControllerID;

        private bool positionalTracking;

        private Vector3 palm2Wrist;

        public override void StartController(InstantVR ivr) {
            extension = ivr.GetComponent<IVR_Touch>();
            base.StartController(ivr);

            if (transform == ivr.leftHandTarget) {
                touchControllerID = OVRInput.Controller.LTouch;
                palm2Wrist = new Vector3(-0.04F, 0.02F, -0.13F);
            } else {
                touchControllerID = OVRInput.Controller.RTouch;
                palm2Wrist = new Vector3(0.04F, 0.02F, -0.13F);
            }

            CreateController();
            SetBodyRotation();

            controllerInput = Controllers.GetController(0);
        }

        GameObject controllerObject;
        private void CreateController() {
            IVR_UnityVRHead ivrUnityHead = ivr.headTarget.GetComponent<IVR_UnityVRHead>();
            if (ivrUnityHead == null && ivrUnityHead.cameraRoot == null)
                return;

            controllerObject = new GameObject();
            controllerObject.transform.parent = ivrUnityHead.cameraRoot.transform;
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
            if (!enabled)
                return;

            if ((OVRInput.GetConnectedControllers() & touchControllerID) != touchControllerID)
                return;

            UpdateTransform();
            UpdateInput();
        }

        private void UpdateTransform() {
            controllerObject.transform.localPosition = OVRInput.GetLocalControllerPosition(touchControllerID);
            controllerObject.transform.localRotation = OVRInput.GetLocalControllerRotation(touchControllerID);

            controllerPosition = controllerObject.transform.position;
            controllerRotation = controllerObject.transform.rotation;

            if (OVRInput.GetControllerPositionTracked(touchControllerID)) {
                positionalTracking = true;
            } else {
                positionalTracking = false;
            }

            if (selected) {
                transform.rotation = CalculateHandRotation(controllerObject.transform.rotation);
                if (positionalTracking)
                    transform.position = CalculateHandPosition(controllerObject.transform.position, controllerObject.transform.rotation);
            }

            tracking = true;
        }

        private Vector3 CalculateHandPosition(Vector3 controllerPosition, Quaternion controllerRotation) {
            return controllerPosition + transform.rotation * palm2Wrist;
        }

        private Quaternion CalculateHandRotation(Quaternion controllerRotation) {
            if (transform == ivr.leftHandTarget) {
                return controllerRotation * Quaternion.Euler(0, 0, 90);
            } else {
                return controllerRotation * Quaternion.Euler(0, 0, -90);
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
            controllerInputSide.stickHorizontal = Mathf.Clamp(controllerInputSide.stickHorizontal + OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, touchControllerID).x, -1, 1);
            controllerInputSide.stickVertical = Mathf.Clamp(controllerInputSide.stickVertical + OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, touchControllerID).y, -1, 1);
            controllerInputSide.stickButton |= OVRInput.Get(OVRInput.Button.PrimaryThumbstick, touchControllerID);
            controllerInputSide.stickTouch = OVRInput.Get(OVRInput.NearTouch.PrimaryThumbButtons, touchControllerID);

            controllerInputSide.up |= (controllerInputSide.stickVertical > 0.3F);
            controllerInputSide.down |= (controllerInputSide.stickVertical < -0.3F);
            controllerInputSide.left |= (controllerInputSide.stickHorizontal < -0.3F);
            controllerInputSide.right |= (controllerInputSide.stickHorizontal > 0.3F);

            controllerInputSide.bumper = Mathf.Max(controllerInputSide.bumper, OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, touchControllerID) * 0.8F + 0.2F);
            controllerInputSide.bumper = OVRInput.Get(OVRInput.NearTouch.PrimaryIndexTrigger, touchControllerID) ? controllerInputSide.bumper : 0;
            controllerInputSide.trigger = Mathf.Max(controllerInputSide.trigger, OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, touchControllerID));

            controllerInputSide.buttons[0] |= OVRInput.Get(OVRInput.Button.One, touchControllerID);
            controllerInputSide.buttons[1] |= OVRInput.Get(OVRInput.Button.Two, touchControllerID);
        }

    }
}
