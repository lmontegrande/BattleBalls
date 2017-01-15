/* InstantVR VicoVR head controller
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.5.0
 * date: March 27, 2016
 * 
 */

using UnityEngine;

namespace IVR {

    public class IVR_VicoVRHead : IVR_Controller {
#if UNITY_ANDROID
        public bool headRotation = true;

        [HideInInspector]
        private IVR_VicoVR vicoVR;
        [HideInInspector]
        private IVR_UnityVRHead gearVRHead;
        [HideInInspector]
        private IVR_CardboardHead cardboardHead;

        void Start() {
            // This dummy code is here to ensure the checkbox is present in editor
        }

        public override void StartController(InstantVR ivr) {
            vicoVR = ivr.GetComponent<IVR_VicoVR>();
            extension = vicoVR;
            base.StartController(ivr);

            extrapolation = false;

            gearVRHead = GetComponent<IVR_UnityVRHead>();
            cardboardHead = GetComponent<IVR_CardboardHead>();
        }

        float lastFrameTimestamp;

        public override void UpdateController() {
            float currentFrameTimestamp = vicoVR.GetFrameTimestamp();
            if (!vicoVR.present || !enabled || currentFrameTimestamp <= lastFrameTimestamp)
                return;

            lastFrameTimestamp = currentFrameTimestamp;
            
            if (vicoVR.BoneTracking(IVR_Kinect2.BoneType.ShoulderCenter)) {
                Vector3 neckPos = vicoVR.GetBonePosition(IVR_Kinect2.BoneType.ShoulderCenter);
                if (neckPos != Vector3.zero) {
                    controllerPosition = neckPos;

                    if (headRotation) {
                        Vector3 headPos = vicoVR.GetBonePosition(IVR_Kinect2.BoneType.Head);

                        Vector3 direction = headPos - neckPos;
                        controllerRotation = Quaternion.LookRotation(Vector3.forward, direction);
                    }

                    tracking = vicoVR.Tracking;

                    if (!selected) {
                        if (gearVRHead != null && gearVRHead.isSelected() && gearVRHead.positionalTracking)
                            CalibrateWithSelected();
                        if (cardboardHead != null && cardboardHead.isSelected())
                            CalibrateWithSelected();
                    }

                    base.UpdateController();
                }
            } else
                tracking = false;
        }

        private void CalibrateWithSelected() {
            extension.trackerPosition = transform.position - ivr.transform.position - controllerPosition;
        }
#endif
    }
}