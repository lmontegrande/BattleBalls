/* InstantVR Leap extension editor
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.6.0
 * date: September 23, 2016
 * 
 * - added warning for missing Leap SDK
 * - removed check for IVR_ADVANCED, is not part of IVR_Advanced_Editor
 */

using UnityEngine;
 using UnityEditor;

namespace IVR {

    [CustomEditor(typeof(IVR_Leap))]
    public class IVR_Leap_Editor : IVR_Extension_Editor {

        private InstantVR ivr;
        private IVR_Leap ivrLeap;
        private IVR_LeapHand leapLeftHand, leapRightHand;

        void OnDestroy() {
            if (ivr != null && ivrLeap == null) {
                if (ivr.leftHandTarget != null) {
                    leapLeftHand = ivr.leftHandTarget.GetComponent<IVR_LeapHand>();
                    if (leapLeftHand != null)
                        DestroyImmediate(leapLeftHand, true);
                }

                if (ivr.rightHandTarget != null) {
                    leapRightHand = ivr.rightHandTarget.GetComponent<IVR_LeapHand>();
                    if (leapRightHand != null)
                        DestroyImmediate(leapRightHand, true);
                }
            }
        }

        void OnEnable() {
            ivrLeap = (IVR_Leap)target;
            if (!ivrLeap)
                return;

            ivr = ivrLeap.GetComponent<InstantVR>();

            leapLeftHand = ivr.leftHandTarget.GetComponent<IVR_LeapHand>();
            if (leapLeftHand == null) {
                leapLeftHand = ivr.leftHandTarget.gameObject.AddComponent<IVR_LeapHand>();
                leapLeftHand.extension = ivrLeap;
            }

            leapRightHand = ivr.rightHandTarget.GetComponent<IVR_LeapHand>();
            if (leapRightHand == null) {
                leapRightHand = ivr.rightHandTarget.gameObject.AddComponent<IVR_LeapHand>();
                leapRightHand.extension = ivrLeap;
            }

            IVR_Extension[] extensions = ivr.GetComponents<IVR_Extension>();
            if (ivrLeap.priority == -1)
                ivrLeap.priority = extensions.Length - 1;
            for (int i = 0; i < extensions.Length; i++) {
                if (ivrLeap == extensions[i]) {
                    while (i < ivrLeap.priority) {
                        MoveUp(leapLeftHand);
                        MoveUp(leapRightHand);
                        ivrLeap.priority--;
                        //Debug.Log ("Leap Move up to : " + i + " now: " + ivrLeap.priority);
                    }
                    while (i > ivrLeap.priority) {
                        MoveDown(leapLeftHand);
                        MoveDown(leapRightHand);
                        ivrLeap.priority++;
                        //Debug.Log ("Leap Move down to : " + i + " now: " + ivrLeap.priority);
                    }
                }
            }
        }

        public override void OnInspectorGUI() {
#if !IVR_LEAP
            EditorGUILayout.HelpBox("Leap Motion Core Assets are not available. Please download the Core Assets using the button below and import them into this project.", MessageType.Warning, true);
            if (GUILayout.Button("Download Leap Motion Unity Core Assets"))
                Application.OpenURL("https://developer.leapmotion.com/unity");
#endif
            base.OnInspectorGUI();
        }
    }
}