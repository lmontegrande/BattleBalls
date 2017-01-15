/* InstantVR Leap controller
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.5.2
 * date: August 7, 2016
 *
 * - fixed thumbs up pose
 */

using System.Collections.Generic;
using UnityEngine;

namespace IVR {

    public class IVR_LeapHand : IVR_HandController {

#if UNITY_STANDALONE_WIN && IVR_LEAP
        public enum LeapMode {
            HandTracking,
            FingerTracking
        }

        public LeapMode mode = LeapMode.FingerTracking;

        [HideInInspector]
        private IVR_Leap ivrLeap;
        [HideInInspector]
        public Leap.Hand leapHand;
        [HideInInspector]
        private Leap.Controller controller;

        private long lastTimeStamp = 0;

        [HideInInspector]
        private IVR_HandMovements handMovements;

        public override void StartController(InstantVR ivr) {
            ivrLeap = ivr.GetComponent<IVR_Leap>();
            extension = ivrLeap;

            base.StartController(ivr);

            extrapolation = false;

            controller = new Leap.Controller();

            GetFingerBones();

            if (ivrLeap != null)
                isHeadMounted = ivrLeap.IsHeadMounted;

            handMovements = GetComponent<IVR_HandMovements>();
        }

        [HideInInspector]
        public bool isHeadMounted;

        public override void UpdateController() {
            if (leapHand == null)
                tracking = false;
            else {
                Leap.Frame frame;
                frame = controller.Frame();
                if (frame.Timestamp > lastTimeStamp) {
                    lastTimeStamp = frame.Timestamp;

                    controllerPosition = ToVector3(leapHand.WristPosition) / 1000;

                    Vector3 palmNormal = ToVector3(leapHand.PalmNormal);
                    Vector3 handDirection = ToVector3(leapHand.Direction);
                    controllerRotation = Quaternion.LookRotation(handDirection, -palmNormal);

                    base.UpdateController();


                    List<Leap.Finger> fingerList = leapHand.Fingers;
                    for (int i = 0; i < fingerList.Count; i++) {
                        Leap.Finger finger = fingerList[i];
                        Leap.Bone proximal = finger.Bone(Leap.Bone.BoneType.TYPE_PROXIMAL);
                        Leap.Bone distal = finger.Bone(Leap.Bone.BoneType.TYPE_DISTAL);

                        if (selected) {

                            switch (mode) {
                                case LeapMode.FingerTracking:
                                    float angle = proximal.Direction.AngleTo(distal.Direction);
                                    Vector3 axis = ToVector3(proximal.Direction.Cross(distal.Direction));
                                    float dot = proximal.Direction.Dot(distal.Direction);
                                    float value = Mathf.Clamp01((1.0f - dot) - 0.5f);
                                    switch (finger.Type) {
                                        case Leap.Finger.FingerType.TYPE_THUMB:
                                            if ((transform == ivr.leftHandTarget && Vector3.Angle(axis, palmNormal) < 90) ||
                                                (transform == ivr.rightHandTarget && Vector3.Angle(axis, palmNormal) > 90))
                                                angle = -angle * 2F;
                                            handMovements.thumbCurl = angle * Mathf.Rad2Deg / 60;
                                            break;
                                        case Leap.Finger.FingerType.TYPE_INDEX:
                                            handMovements.indexCurl = value;
                                            break;
                                        case Leap.Finger.FingerType.TYPE_MIDDLE:
                                            handMovements.middleCurl = value;
                                            break;
                                        case Leap.Finger.FingerType.TYPE_RING:
                                            handMovements.ringCurl = value;
                                            break;
                                        case Leap.Finger.FingerType.TYPE_PINKY:
                                            handMovements.littleCurl = value;
                                            break;
                                    }
                                    break;
                                case LeapMode.HandTracking:
                                    handMovements.thumbCurl = leapHand.GrabStrength;
                                    handMovements.indexCurl = leapHand.GrabStrength;
                                    handMovements.middleCurl = leapHand.GrabStrength;
                                    handMovements.ringCurl = leapHand.GrabStrength;
                                    handMovements.littleCurl = leapHand.GrabStrength;
                                    break;
                            }
                        }
                    }
                    tracking = true;
                }
            }
        }
        

        private static Vector3 ToVector3(Leap.Vector vector) {
            return new Vector3(vector.x, vector.y, -vector.z);
        }

        private void SetFingerJointRotation(Transform bone, Leap.Bone leapBone) {
            Vector3 directionFwd = ToVector3(leapBone.Direction);

            Vector3 directionUp = Vector3.Cross(directionFwd, this.transform.right);
            bone.rotation = Quaternion.LookRotation(directionFwd, directionUp) * fromNormFinger;
        }

        private void SetThumbJointRotation(Transform bone, Leap.Bone leapBone) {
            Vector3 directionFwd = ToVector3(leapBone.Direction);

            Vector3 directionUp = Vector3.Cross(directionFwd, this.transform.right);
            bone.rotation = Quaternion.LookRotation(directionFwd, directionUp) * fromNormThumb;
        }

        private Transform thumb1, thumb2;
        private Transform middle1, middle3;

        private Quaternion fromNormThumb;
        private Quaternion fromNormFinger;

        private void GetFingerBones() {
            if (ivr.characterTransform == null)
                return;

            Animator animator = ivr.characterTransform.GetComponent<Animator>();
            if (animator == null)
                return;

            if (this.transform == ivr.leftHandTarget) {
                thumb1 = animator.GetBoneTransform(HumanBodyBones.LeftThumbProximal);
                thumb2 = animator.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);

                middle1 = animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
                middle3 = animator.GetBoneTransform(HumanBodyBones.LeftMiddleDistal);
            } else {
                thumb1 = animator.GetBoneTransform(HumanBodyBones.RightThumbProximal);
                thumb2 = animator.GetBoneTransform(HumanBodyBones.RightThumbIntermediate);

                middle1 = animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
                middle3 = animator.GetBoneTransform(HumanBodyBones.RightMiddleDistal);

            }

            if (thumb1 != null && thumb2 != null)
                fromNormThumb = Quaternion.Inverse(Quaternion.LookRotation(thumb2.position - thumb1.position, this.transform.up)) * thumb1.rotation;
            if (middle1 != null && middle3 != null)
                fromNormFinger = Quaternion.Inverse(Quaternion.LookRotation(middle3.position - middle1.position, this.transform.up)) * middle1.rotation;
        }
#endif
    }
}