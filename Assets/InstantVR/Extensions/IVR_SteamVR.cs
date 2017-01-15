/* InstantVR SteamVR extension
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.4.0
 * date: March 18, 2016
 * 
 */

using UnityEngine;

namespace IVR {

    [HelpURL("http://serrarens.nl/passervr/support/instantvr-support/instantvr-extensions/htc-vive/")]
    public class IVR_SteamVR : IVR_Extension {

        public override void StartExtension(InstantVR ivr) {
            base.StartExtension(ivr);

            SteamVR_ControllerManager controllerManager = ivr.gameObject.AddComponent<SteamVR_ControllerManager>();
            controllerManager.left = ivr.leftHandTarget.gameObject;
            controllerManager.right = ivr.rightHandTarget.gameObject;
        }
    }
}