/* InstantVR Head Movements
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.5.0
 * date: July 29, 2016
 * 
 * - added HeadUtils
 */
using UnityEngine;

using IVR;

public static class HeadUtils {

    public static Vector3 GetNeckEyeDelta(InstantVR ivr) {
        Animator animator = ivr.characterTransform.GetComponent<Animator>();
        if (animator != null) {
            Transform neckBone = animator.GetBoneTransform(HumanBodyBones.Neck);
            Transform leftEyeBone = animator.GetBoneTransform(HumanBodyBones.LeftEye);
            Transform rightEyeBone = animator.GetBoneTransform(HumanBodyBones.RightEye);

            if (neckBone != null && leftEyeBone != null && rightEyeBone != null) {
                Vector3 centerEyePosition = (leftEyeBone.transform.position + rightEyeBone.transform.position) / 2;
                Vector3 worldNeckEyeDelta = (centerEyePosition - neckBone.position);
                Vector3 localNeckEyeDelta = ivr.headTarget.InverseTransformDirection(worldNeckEyeDelta);
                return localNeckEyeDelta;
            }

            Camera camera = ivr.headTarget.GetComponentInChildren<Camera>();
            if (camera != null)
                return camera.transform.localPosition;

        }
        return Vector3.zero;
    }

    public static Vector3 GetHeadEyeDelta(InstantVR ivr) {
        Animator animator = ivr.characterTransform.GetComponent<Animator>();
        if (animator != null) {
            Transform neckBone = animator.GetBoneTransform(HumanBodyBones.Neck);
            Transform leftEyeBone = animator.GetBoneTransform(HumanBodyBones.LeftEye);
            Transform rightEyeBone = animator.GetBoneTransform(HumanBodyBones.RightEye);
            if (neckBone != null && leftEyeBone != null && rightEyeBone != null) {
                Vector3 centerEyePosition = (leftEyeBone.position + rightEyeBone.position) / 2;
                Vector3 worldHeadEyeDelta = (centerEyePosition - neckBone.position);
                Vector3 localHeadEyeDelta = ivr.headTarget.InverseTransformDirection(worldHeadEyeDelta);
                return localHeadEyeDelta;
            }
        }

        Camera camera = ivr.headTarget.GetComponentInChildren<Camera>();
        if (camera != null) {
            return camera.transform.localPosition;
        }

        return Vector3.zero;
    }

    public static Vector3 CalculateNeckPosition(Vector3 eyePosition, Quaternion eyeRotation, Vector3 eye2neck) {
        Vector3 neckPosition = eyePosition + eyeRotation * eye2neck;
        return neckPosition;
    }

}
