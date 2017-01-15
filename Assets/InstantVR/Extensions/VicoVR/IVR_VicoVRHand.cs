/* InstantVR VicoVR hand controller
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.4.6
 * date: April 30, 2016
 * 
 * - added check for presence HandMovements
 */

using UnityEngine;

namespace IVR {

    public class IVR_VicoVRHand : IVR_HandController {
#if UNITY_ANDROID
        public bool handTracking = true;

        [HideInInspector]
        private IVR_VicoVR vicoVR;
        [HideInInspector]
        private IVR_Kinect2.BoneType wristBone, handBone;

        private IVR_HandMovements handMovements;

        void Start() {
            // This dummy code is here to ensure the checkbox is present in editor
        }

        public override void StartController(InstantVR ivr) {
            vicoVR = ivr.GetComponent<IVR_VicoVR>();
            extension = vicoVR;
            base.StartController(ivr);

            if (transform == ivr.leftHandTarget)
                startRotation = Quaternion.AngleAxis(90, Vector3.forward);
            else
                startRotation = Quaternion.AngleAxis(270, Vector3.forward);

            extrapolation = false;

            if (transform == ivr.leftHandTarget) {
                wristBone = IVR_Kinect2.BoneType.WristLeft;
                handBone = IVR_Kinect2.BoneType.HandLeft;
            } else {
                wristBone = IVR_Kinect2.BoneType.WristRight;
                handBone = IVR_Kinect2.BoneType.HandRight;
            }

            handMovements = gameObject.GetComponent<IVR_HandMovements>();
        }

        float lastFrameTimestamp;

        public override void UpdateController() {
            float currentFrameTimestamp = vicoVR.GetFrameTimestamp();
            if (!vicoVR.present || !enabled || currentFrameTimestamp <= lastFrameTimestamp)
                return;

            lastFrameTimestamp = currentFrameTimestamp;

            if (vicoVR.BoneTracking(wristBone)) {
                Vector3 wristPos = vicoVR.GetBonePosition(wristBone);
                Vector3 handPos = vicoVR.GetBonePosition(handBone);

                Vector3 direction = handPos - wristPos;
                if (direction.magnitude > 0) {
                    if (transform == ivr.leftHandTarget)
                        controllerRotation = Quaternion.LookRotation(direction, Vector3.left);
                    else
                        controllerRotation = Quaternion.LookRotation(direction, Vector3.right);
                } else
                    controllerRotation = Quaternion.identity;

                controllerPosition = wristPos;

                tracking = vicoVR.Tracking;

                base.UpdateController();
                
                float handInput;
                if (transform == ivr.leftHandTarget)
                    handInput = vicoVR.GetHandPose(IVR_Kinect2.BoneType.HandLeft);
                else
                    handInput = vicoVR.GetHandPose(IVR_Kinect2.BoneType.HandRight);

                if (handMovements != null) {
                    handMovements.thumbCurl = handInput;
                    handMovements.indexCurl = handInput;
                    handMovements.middleCurl = handInput;
                    handMovements.ringCurl = handInput;
                    handMovements.littleCurl = handInput;
                }
                
            } else
                tracking = false;
        }
#endif
    }
}