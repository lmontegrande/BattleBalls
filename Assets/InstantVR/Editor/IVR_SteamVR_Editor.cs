/* InstantVR Steam VR extension editor
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.4.0
 * date: March 4, 2016
 *
 */

using UnityEditor;

namespace IVR {

    [CustomEditor(typeof(IVR_SteamVR))]
    public class IVR_SteamVR_Editor : IVR_Extension_Editor {

        private InstantVR ivr;
        private IVR_SteamVR ivrSteamVR;

        private IVR_SteamVRHead steamHead;
        private IVR_SteamVRHand steamLeftHand, steamRightHand;

        public override void OnInspectorGUI() {
            if (PlayerSettings.virtualRealitySupported == true)
                EditorGUILayout.HelpBox("VirtualRealitySupported needs to be DISabled in Player Settings for SteamVR support", MessageType.Warning, true);

            base.OnInspectorGUI();
            CheckIVRAdvancedDefine();
        }

        void OnDestroy() {
            if (ivrSteamVR == null && ivr != null) {
                steamHead = ivr.headTarget.GetComponent<IVR_SteamVRHead>();
                if (steamHead != null)
                    DestroyImmediate(steamHead, true);

                steamLeftHand = ivr.leftHandTarget.GetComponent<IVR_SteamVRHand>();
                if (steamLeftHand != null)
                    DestroyImmediate(steamLeftHand, true);

                steamRightHand = ivr.rightHandTarget.GetComponent<IVR_SteamVRHand>();
                if (steamRightHand != null)
                    DestroyImmediate(steamRightHand, true);
            }
        }

        void OnEnable() {

            ivrSteamVR = (IVR_SteamVR)target;
            ivr = ivrSteamVR.GetComponent<InstantVR>();

            if (ivr != null) {
                steamHead = ivr.headTarget.GetComponent<IVR_SteamVRHead>();
                if (steamHead == null) {
                    steamHead = ivr.headTarget.gameObject.AddComponent<IVR_SteamVRHead>();
                    steamHead.extension = ivrSteamVR;
                }

                steamLeftHand = ivr.leftHandTarget.GetComponent<IVR_SteamVRHand>();
                if (steamLeftHand == null) {
                    steamLeftHand = ivr.leftHandTarget.gameObject.AddComponent<IVR_SteamVRHand>();
                    steamLeftHand.extension = ivrSteamVR;
                }

                steamRightHand = ivr.rightHandTarget.GetComponent<IVR_SteamVRHand>();
                if (steamRightHand == null) {
                    steamRightHand = ivr.rightHandTarget.gameObject.AddComponent<IVR_SteamVRHand>();
                    steamRightHand.extension = ivrSteamVR;
                }

                IVR_Extension[] extensions = ivr.GetComponents<IVR_Extension>();
                if (ivrSteamVR.priority == -1)
                    ivrSteamVR.priority = extensions.Length - 1;
                for (int i = 0; i < extensions.Length; i++) {
                    if (ivrSteamVR == extensions[i]) {
                        while (i < ivrSteamVR.priority) {
                            MoveUp(steamHead);
                            MoveUp(steamLeftHand);
                            MoveUp(steamRightHand);
                            ivrSteamVR.priority--;
                            //Debug.Log ("Steam Move up to : " + i + " now: " + ivrRift.priority);
                        }
                        while (i > ivrSteamVR.priority) {
                            MoveDown(steamHead);
                            MoveDown(steamLeftHand);
                            MoveDown(steamRightHand);
                            ivrSteamVR.priority++;
                            //Debug.Log ("Steam Move down to : " + i + " now: " + ivrRift.priority);
                        }
                    }
                }
            }
        }

        private void CheckIVRAdvancedDefine() {
            string scriptDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (!scriptDefines.Contains("INSTANTVR_ADVANCED")) {
                string newScriptDefines = scriptDefines + " INSTANTVR_ADVANCED";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, newScriptDefines);
            }
        }

    }
}