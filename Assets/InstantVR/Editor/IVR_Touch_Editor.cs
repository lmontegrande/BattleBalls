/* InstantVR Oculus Touch extension editor
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.6.0
 * date: September 26, 2016
 *
 */

using UnityEditor;

namespace IVR {

    [CustomEditor(typeof(IVR_Touch))]
    public class IVR_Touch_Editor : IVR_Extension_Editor {

        private InstantVR ivr;
        private IVR_Touch ivrTouch;

        private IVR_TouchHand touchLeftHand, touchRightHand;

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
        }

        void OnDestroy() {
            if (ivrTouch == null && ivr != null) {
                touchLeftHand = ivr.leftHandTarget.GetComponent<IVR_TouchHand>();
                if (touchLeftHand != null)
                    DestroyImmediate(touchLeftHand, true);

                touchRightHand = ivr.rightHandTarget.GetComponent<IVR_TouchHand>();
                if (touchRightHand != null)
                    DestroyImmediate(touchRightHand, true);
            }
        }

        void OnEnable() {

            ivrTouch = (IVR_Touch) target;
            ivr = ivrTouch.GetComponent<InstantVR>();

            if (ivr != null) {
                touchLeftHand = ivr.leftHandTarget.GetComponent<IVR_TouchHand>();
                if (touchLeftHand == null) {
                    touchLeftHand = ivr.leftHandTarget.gameObject.AddComponent<IVR_TouchHand>();
                    touchLeftHand.extension = ivrTouch;
                }

                touchRightHand = ivr.rightHandTarget.GetComponent<IVR_TouchHand>();
                if (touchRightHand == null) {
                    touchRightHand = ivr.rightHandTarget.gameObject.AddComponent<IVR_TouchHand>();
                    touchRightHand.extension = ivrTouch;
                }

                IVR_Extension[] extensions = ivr.GetComponents<IVR_Extension>();
                if (ivrTouch.priority == -1)
                    ivrTouch.priority = extensions.Length - 1;
                for (int i = 0; i < extensions.Length; i++) {
                    if (ivrTouch == extensions[i]) {
                        while (i < ivrTouch.priority) {
                            MoveUp(touchLeftHand);
                            MoveUp(touchRightHand);
                            ivrTouch.priority--;
                            //Debug.Log ("Touch Move up to : " + i + " now: " + ivrRift.priority);
                        }
                        while (i > ivrTouch.priority) {
                            MoveDown(touchLeftHand);
                            MoveDown(touchRightHand);
                            ivrTouch.priority++;
                            //Debug.Log ("Touch Move down to : " + i + " now: " + ivrRift.priority);
                        }
                    }
                }
            }
        }
    }
}