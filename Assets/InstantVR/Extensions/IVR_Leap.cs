/* InstantVR Leap extension
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.4.6
 * date: April 27, 2016
 * 
 * - Using Leap Orion v4.0.2
 */

using System.Collections.Generic;
using UnityEngine;

namespace IVR {

    [HelpURL("http://serrarens.nl/passervr/support/instantvr-support/instantvr-extensions/leap-motion/")]
    public class IVR_Leap : IVR_Extension {
#if UNITY_STANDALONE_WIN && IVR_LEAP
        public bool IsHeadMounted = true;

        Leap.Controller controller;

        [HideInInspector]
        private Transform headcam;

        [HideInInspector]
        private IVR_LeapHand leapLeftHand, leapRightHand;

        public override void StartExtension(InstantVR ivr) {
            base.StartExtension(ivr);

            headcam = ivr.GetComponentInChildren<Camera>().transform;

            controller = new Leap.Controller();
            if (controller != null) {
                if (controller.IsConnected) {
                    if (IsHeadMounted)
                        controller.SetPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
                    else
                        controller.ClearPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
                }

                leapLeftHand = ivr.leftHandTarget.GetComponent<IVR_LeapHand>();
                leapRightHand = ivr.rightHandTarget.GetComponent<IVR_LeapHand>();
            }
        }

        public override void UpdateExtension() {
            base.UpdateExtension();
            
            if (controller != null) {
                Leap.Frame frame = controller.Frame();
                List<Leap.Hand> hands = frame.Hands;

                leapLeftHand.leapHand = null;
                leapRightHand.leapHand = null;

                for (int i = 0; i < hands.Count; i++) {
                    if (hands[i].IsLeft) {
                        leapLeftHand.leapHand = hands[i];
                        break;
                    }
                }

                for (int i = 0; i < hands.Count; i++) {
                    if (hands[i].IsRight) {
                        leapRightHand.leapHand = hands[i];
                        break;
                    }
                }
            }

            if (IsHeadMounted) {
                Quaternion headcamRotation = Quaternion.Inverse(ivr.transform.rotation) * headcam.rotation;
                Vector3 headcamPosition = headcam.position - ivr.transform.position;
                trackerPosition = Quaternion.Inverse(ivr.transform.rotation) * (headcamPosition + headcamRotation * new Vector3(0, 0, 0.09F));
                trackerEulerAngles = (headcamRotation * Quaternion.Euler(270, 0, 180)).eulerAngles;
            }

        }

        void OnApplicationQuit() {
            StopLeap();
        }

        void OnDestroy() {
            StopLeap();
        }

        private void StopLeap() {
            if (controller != null) {
                controller.StopConnection();
            }
        }
#endif
    }
}