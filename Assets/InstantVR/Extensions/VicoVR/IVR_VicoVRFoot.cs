/* InstantVR VicoVR foot controller
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.3.0
 * date: February 6, 2016
 * 
 * - added namespace
 */

using UnityEngine;

namespace IVR {

    public class IVR_VicoVRFoot : IVR_Controller {
#if UNITY_ANDROID
        [HideInInspector]
        private IVR_VicoVR vicoVR;
        [HideInInspector]
        private IVR_Kinect2.BoneType ankleBone;//, footBone;
        [HideInInspector]
        private IVR_VicoVRHip vicoHip;

        void Start() {
            // This dummy code is here to ensure the checkbox is present in editor
        }

        public override void StartController(InstantVR ivr) {
            vicoVR = ivr.GetComponent<IVR_VicoVR>();
            extension = vicoVR;
            base.StartController(ivr);

            extrapolation = false;

            if (transform == ivr.leftFootTarget) {
                ankleBone = IVR_Kinect2.BoneType.AnkleLeft;
            } else {
                ankleBone = IVR_Kinect2.BoneType.AnkleRight;
            }

            vicoHip = ivr.hipTarget.GetComponent<IVR_VicoVRHip>();
        }

        float lastFrameTimestamp;

        public override void UpdateController() {
            float currentFrameTimestamp = vicoVR.GetFrameTimestamp();
            if (!vicoVR.present || !enabled || currentFrameTimestamp <= lastFrameTimestamp)
                return;

            lastFrameTimestamp = currentFrameTimestamp;

            Vector3 anklePos = Vector3.zero;

            if (vicoVR.BoneTracking(ankleBone)) {
                anklePos = vicoVR.GetBonePosition(ankleBone);

                controllerRotation = Quaternion.identity;
                controllerPosition = anklePos;

                if (!tracking && vicoVR.Tracking)
                    tracking = true;

                base.UpdateController();
                transform.rotation = vicoHip.rotation;
            } else
                tracking = false;
        }
#endif
    }
}