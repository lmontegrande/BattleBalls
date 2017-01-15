/* InstantVR VicoVR extension editor
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.3.0
 * date: January 31, 2016
 * 
 */

using UnityEditor;
using IVR;

[CustomEditor(typeof(IVR_VicoVR))] 
public class IVR_VicoVR_Editor : IVR_Extension_Editor {
	
	private InstantVR ivr;
	private IVR_VicoVR ivrVicoVR;
	
	private IVR_VicoVRHead vicoVRHead;
	private IVR_VicoVRHand vicoVRLeftHand, vicoVRRightHand;
	private IVR_VicoVRHip vicoVRHip;
	private IVR_VicoVRFoot vicoVRLeftFoot, vicoVRRightFoot;
	
	void OnDestroy() {
		if (ivrVicoVR == null && ivr != null) {
			vicoVRHead = ivr.headTarget.GetComponent<IVR_VicoVRHead>();
			if (vicoVRHead != null)
				DestroyImmediate(vicoVRHead, true);
			
			vicoVRLeftHand = ivr.leftHandTarget.GetComponent<IVR_VicoVRHand>();
			if (vicoVRLeftHand != null)
				DestroyImmediate(vicoVRLeftHand, true);
			
			vicoVRRightHand = ivr.rightHandTarget.GetComponent<IVR_VicoVRHand>();
			if (vicoVRRightHand != null)
				DestroyImmediate(vicoVRRightHand, true);
		
			vicoVRHip = ivr.hipTarget.GetComponent<IVR_VicoVRHip>();
			if (vicoVRHip != null)
				DestroyImmediate(vicoVRHip, true);

			vicoVRLeftFoot = ivr.leftFootTarget.GetComponent<IVR_VicoVRFoot>();
			if (vicoVRLeftFoot != null)
				DestroyImmediate(vicoVRLeftFoot, true);
			
			vicoVRRightFoot = ivr.rightFootTarget.GetComponent<IVR_VicoVRFoot>();
			if (vicoVRRightFoot != null)
				DestroyImmediate(vicoVRRightFoot, true);
		}
	}
	
	void OnEnable() {
		ivrVicoVR = (IVR_VicoVR) target;
		ivr = ivrVicoVR.GetComponent<InstantVR>();
		
		if (ivr != null) {
			vicoVRHead = ivr.headTarget.GetComponent<IVR_VicoVRHead>();
			if (vicoVRHead == null) {
				vicoVRHead = ivr.headTarget.gameObject.AddComponent<IVR_VicoVRHead>();
				vicoVRHead.extension = ivrVicoVR;
			}
			
			vicoVRLeftHand = ivr.leftHandTarget.GetComponent<IVR_VicoVRHand>();
			if (vicoVRLeftHand == null) {
				vicoVRLeftHand = ivr.leftHandTarget.gameObject.AddComponent<IVR_VicoVRHand>();
				vicoVRLeftHand.extension = ivrVicoVR;
			}
			
			vicoVRRightHand = ivr.rightHandTarget.GetComponent<IVR_VicoVRHand>();
			if (vicoVRRightHand == null) {
				vicoVRRightHand = ivr.rightHandTarget.gameObject.AddComponent<IVR_VicoVRHand>();
				vicoVRRightHand.extension = ivrVicoVR;
			}
			
			vicoVRHip = ivr.hipTarget.GetComponent<IVR_VicoVRHip>();
			if (vicoVRHip == null) {
				vicoVRHip = ivr.hipTarget.gameObject.AddComponent<IVR_VicoVRHip>();
				vicoVRHip.extension = ivrVicoVR;
			}

			vicoVRLeftFoot = ivr.leftFootTarget.GetComponent<IVR_VicoVRFoot>();
			if (vicoVRLeftFoot == null) {
				vicoVRLeftFoot = ivr.leftFootTarget.gameObject.AddComponent<IVR_VicoVRFoot>();
				vicoVRLeftFoot.extension = ivrVicoVR;
			}
			
			vicoVRRightFoot = ivr.rightFootTarget.GetComponent<IVR_VicoVRFoot>();
			if (vicoVRRightFoot == null) {
				vicoVRRightFoot = ivr.rightFootTarget.gameObject.AddComponent<IVR_VicoVRFoot>();
				vicoVRRightFoot.extension = ivrVicoVR;
			}

			IVR_Extension[] extensions = ivr.GetComponents<IVR_Extension>();
			if (ivrVicoVR.priority == -1)
				ivrVicoVR.priority = extensions.Length - 1;
			for (int i = 0; i < extensions.Length; i++) {
				if (ivrVicoVR == extensions[i]) {
					while (i < ivrVicoVR.priority) {
						MoveUp(vicoVRHead);
						MoveUp(vicoVRLeftHand);
						MoveUp(vicoVRRightHand);
						MoveUp(vicoVRHip);
						MoveUp(vicoVRLeftFoot);
						MoveUp(vicoVRRightFoot);
						ivrVicoVR.priority--;
						//Debug.Log ("VicoVR Move up to : " + i + " now: " + ivrVicoVR.priority);
					}
					while (i > ivrVicoVR.priority) {
						MoveDown(vicoVRHead);
						MoveDown(vicoVRLeftHand);
						MoveDown(vicoVRRightHand);
						MoveDown(vicoVRHip);
						MoveDown(vicoVRLeftFoot);
						MoveDown(vicoVRRightFoot);
						ivrVicoVR.priority++;
						//Debug.Log ("VicoVR Move down to : " + i + " now: " + ivrVicoVR.priority);
					}
				}
			}
        }

    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        CheckIVRAdvancedDefine();
    }

    private void CheckIVRAdvancedDefine() {
        string scriptDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        if (!scriptDefines.Contains("INSTANTVR_ADVANCED")) {
            string newScriptDefines = scriptDefines + " INSTANTVR_ADVANCED";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, newScriptDefines);
        }
    }
}

[CustomEditor(typeof(IVR_VicoVRHead))] 
public class IVR_VicoVR2Head_Editor : IVR_Controller_Editor { }
[CustomEditor(typeof(IVR_VicoVRHand))] 
public class IVR_VicoVRHand_Editor : IVR_Controller_Editor { }
[CustomEditor(typeof(IVR_VicoVRHip))] 
public class IVR_VicoVRHip_Editor : IVR_Controller_Editor { }
[CustomEditor(typeof(IVR_VicoVRFoot))] 
public class IVR_VicoVRFoot_Editor : IVR_Controller_Editor { }