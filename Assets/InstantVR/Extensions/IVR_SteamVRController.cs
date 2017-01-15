/* InstantVR SteamVRcontroller extension
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.5.0
 * date: July 25, 2016
 *
 * - Unity 5.4 support
 */

using UnityEngine;

namespace IVR {

    [HelpURL("http://serrarens.nl/passervr/support/instantvr-support/instantvr-extensions/htc-vive/")]
    public class IVR_SteamVRController : IVR_Extension {

        public override void StartExtension(InstantVR ivr) {
            base.StartExtension(ivr);
            /*
            SteamVR_ControllerManager controllerManager = ivr.gameObject.AddComponent<SteamVR_ControllerManager>();
            controllerManager.left = ivr.leftHandTarget.gameObject;
            controllerManager.right = ivr.rightHandTarget.gameObject;
            */
        }
    }
}