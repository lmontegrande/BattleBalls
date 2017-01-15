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

    public class IVR_SteamVRHead : IVR_Controller {

        [HideInInspector]
        private Transform headcam;
        [HideInInspector]
        public GameObject headcamRoot;
        [HideInInspector]
        private SteamVR_TrackedObject steamTracker;

        [HideInInspector]
        private Vector3 localNeckOffset;

        public override void StartController(InstantVR ivr) {
            extension = ivr.GetComponent<IVR_SteamVR>();
            base.StartController(ivr);

            Camera camera = ivr.GetComponentInChildren<Camera>();
            if (camera != null) {
                headcam = camera.transform;
                localNeckOffset = HeadUtils.GetHeadEyeDelta(ivr);
                headcam.localPosition = localNeckOffset;

                headcamRoot = new GameObject("HeadcamRoot");
                headcamRoot.transform.parent = ivr.transform;
                headcamRoot.transform.position = new Vector3(transform.position.x, ivr.transform.position.y, transform.position.z) + localNeckOffset;
                headcamRoot.transform.rotation = transform.rotation;

                headcam.parent = headcamRoot.transform;
                headcam.position = this.transform.position + localNeckOffset;

                camera.gameObject.AddComponent<SteamVR_Camera>();
                steamTracker = headcam.parent.GetComponent<SteamVR_TrackedObject>();
                headcam.position = transform.position + localNeckOffset;
            }
        }

        public override void UpdateController() {
            if (steamTracker == null)
                steamTracker = headcam.parent.GetComponent<SteamVR_TrackedObject>();

            if (steamTracker != null)
                tracking = steamTracker.isValid;
            if (tracking) {
                Vector3 localEyePosition = headcam.position - ivr.transform.position;
                Quaternion localEyeRotation = Quaternion.Inverse(ivr.transform.rotation) * headcam.rotation;

                Vector3 localNeckPosition = localEyePosition - (ivr.transform.rotation * localEyeRotation * localNeckOffset);
                Quaternion localNeckRotation = localEyeRotation;

                transform.position = ivr.transform.position + localNeckPosition;
                transform.rotation = ivr.transform.rotation * localNeckRotation;
            }
        }
    }
}