/* InstantVR Razer Hydra editor
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.3.0
 * date: February 5, 2016
 *
 * - added namespace
 */

using UnityEditor;

namespace IVR {
    [CustomEditor(typeof(IVR_Hydra))]
    public class IVR_Hydra_Editor : IVR_Extension_Editor {

        private InstantVR ivr;
        private IVR_Hydra ivrHydra;

        private IVR_HydraHand hydraLeftHand, hydraRightHand;

        void OnDestroy() {
            if (ivrHydra == null && ivr != null) {
                hydraLeftHand = ivr.leftHandTarget.GetComponent<IVR_HydraHand>();
                if (hydraLeftHand != null)
                    DestroyImmediate(hydraLeftHand, true);

                hydraRightHand = ivr.rightHandTarget.GetComponent<IVR_HydraHand>();
                if (hydraRightHand != null)
                    DestroyImmediate(hydraRightHand, true);
            }
        }

        void OnEnable() {
            ivrHydra = (IVR_Hydra)target;
            ivr = ivrHydra.GetComponent<InstantVR>();

            if (ivr != null) {
                hydraLeftHand = ivr.leftHandTarget.GetComponent<IVR_HydraHand>();
                if (hydraLeftHand == null) {
                    hydraLeftHand = ivr.leftHandTarget.gameObject.AddComponent<IVR_HydraHand>();
                    hydraLeftHand.extension = ivrHydra;
                }

                hydraRightHand = ivr.rightHandTarget.GetComponent<IVR_HydraHand>();
                if (hydraRightHand == null) {
                    hydraRightHand = ivr.rightHandTarget.gameObject.AddComponent<IVR_HydraHand>();
                    hydraRightHand.extension = ivrHydra;
                }
            }

            IVR_Extension[] extensions = ivr.GetComponents<IVR_Extension>();
            if (ivrHydra.priority == -1)
                ivrHydra.priority = extensions.Length - 1;
            for (int i = 0; i < extensions.Length; i++) {
                if (ivrHydra == extensions[i]) {
                    while (i < ivrHydra.priority) {
                        MoveUp(hydraLeftHand);
                        MoveUp(hydraRightHand);
                        ivrHydra.priority--;
                        //Debug.Log ("Hydra Move up to : " + i + " now: " + ivrHydra.priority);
                    }
                    while (i > ivrHydra.priority) {
                        MoveDown(hydraLeftHand);
                        MoveDown(hydraRightHand);
                        ivrHydra.priority++;
                        //Debug.Log ("Hydra Move down to : " + i + " now: " + ivrHydra.priority);
                    }
                }
            }
        }
    }
}