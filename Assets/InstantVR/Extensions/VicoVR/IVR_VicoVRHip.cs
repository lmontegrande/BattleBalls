/* InstantVR VicoVR hip controller
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.3.0
 * date: February 5, 2016
 *
 * - added namespace
 */

using UnityEngine;

namespace IVR {

    public class IVR_VicoVRHip : IVR_Controller {
#if UNITY_ANDROID
        [HideInInspector]
        private IVR_VicoVR vicoVR;

        void Start() {
            // This dummy code is here to ensure the checkbox is present in editor
        }

        public override void StartController(InstantVR ivr) {
            vicoVR = ivr.GetComponent<IVR_VicoVR>();
            extension = vicoVR;
            base.StartController(ivr);

            extrapolation = false;
        }

        float lastFrameTimestamp;

        public override void UpdateController() {
            float currentFrameTimestamp = vicoVR.GetFrameTimestamp();
            if (!vicoVR.present || !enabled || currentFrameTimestamp <= lastFrameTimestamp)
                return;

            lastFrameTimestamp = currentFrameTimestamp;

            if (vicoVR.BoneTracking(IVR_Kinect2.BoneType.HipCenter)) {
                Vector3 hipsPos = vicoVR.GetBonePosition(IVR_Kinect2.BoneType.HipCenter);
                Vector3 hipLeftPos = vicoVR.GetBonePosition(IVR_Kinect2.BoneType.HipLeft);
                Vector3 hipRightPos = vicoVR.GetBonePosition(IVR_Kinect2.BoneType.HipRight);

                this.controllerPosition = hipsPos;
                Vector3 direction = hipRightPos - hipLeftPos;
                if (direction.magnitude > 0) {
                    controllerRotation = Quaternion.FromToRotation(Vector3.right, direction);

                    tracking = vicoVR.Tracking;

                    base.UpdateController();
                }
            } else
                tracking = false;
        }
#endif
    }
}