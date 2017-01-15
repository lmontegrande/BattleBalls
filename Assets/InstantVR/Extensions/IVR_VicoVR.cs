/* InstantVR VicoVR extension
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.4.6
 * date: April 30, 2016
 * 
 * - fixes in hand pose code
 */

using System;
using UnityEngine;

namespace IVR {

    [HelpURL("http://serrarens.nl/passervr/support/instantvr-support/instantvr-extensions/vicovr/")]
    public class IVR_VicoVR : IVR_Extension {

        protected bool tracking;
        public bool Tracking { get { return tracking; } }

#if UNITY_ANDROID
        public TextMesh textMesh;
        public override void StartExtension(InstantVR ivr) {
            base.StartExtension(ivr);

           try {              
                NuitrackInitState state = NuitrackLoader.InitNuitrackLibraries();
                if (state != NuitrackInitState.INIT_OK) {
                    textMesh.text +=  "Nuitrack native libraries initialization error. ";
                }
            }
            catch (Exception e) {
                textMesh.text = "Exception: " + e.Message;
                Debug.LogWarning(e.Message + " VicoVR cannot be tested in the editor.");
                present = false;
            }
        }

        void Start() {
            try {
                nuitrack.Nuitrack.Init();

                skeletonTracker = nuitrack.SkeletonTracker.Create();
                skeletonTracker.SetAutoTracking(true);
                skeletonTracker.OnSkeletonUpdateEvent += SkeletonUpdate;

                handTracker = nuitrack.HandTracker.Create();
                handTracker.OnUpdateEvent += HandsUpdate;

                nuitrack.Nuitrack.Run();

                present = true;
            }
            catch (Exception e) {
                //textMesh.text = "Exception: " + e.Message;
                Debug.LogWarning(e.Message + " VicoVR cannot be tested in the editor.");
                present = false;
            }
        }

        void OnDestroy() {
            if (present)
                nuitrack.Nuitrack.Release();
        }

        public override void UpdateExtension() {
            if (present)
                nuitrack.Nuitrack.Update();
        }

        private nuitrack.Skeleton skeleton;
        private nuitrack.UserHands hands;

        private nuitrack.SkeletonTracker skeletonTracker;
        private nuitrack.HandTracker handTracker;

        private nuitrack.SkeletonData skeletonData;
        private nuitrack.HandTrackerData handTrackerData;

#endif

        public bool BoneTracking(IVR_Kinect2.BoneType boneType) {
            return true;
            /*
            if (skeleton != null) {
                int i = (int)Bone2JointType(boneType);
                nuitrack.Joint joint = skeleton.Joints[i];
                return (joint.Confidence > 0);
            } else
                return false;
            */
        }

        public  Vector3 GetBonePosition(IVR_Kinect2.BoneType boneType) {
#if UNITY_ANDROID
            Vector3 position = Vector3.zero;
            nSkeletonUpdates = 0;
            nHandsUpdates = 0;
            if (skeleton != null) {
                int i = (int)Bone2JointType(boneType);
                nuitrack.Joint joint = skeleton.Joints[i];

                position = new Vector3(0 - joint.Real.X / 1000, joint.Real.Y / 1000, 0 - joint.Real.Z / 1000);
            }
            return position;
#else
            return Vector3.zero;
#endif
        }

        public Quaternion GetBoneRotation(IVR_Kinect2.BoneType boneType) {
#if UNITY_ANDROID
            Quaternion rotation = Quaternion.identity;
            if (skeleton != null) {
                int i = (int)Bone2JointType(boneType);
                nuitrack.Joint joint = skeleton.Joints[i];

                Vector3 jointUp = new Vector3(joint.Orient.Matrix[1], joint.Orient.Matrix[4], joint.Orient.Matrix[7]);
                Vector3 jointForward = new Vector3(joint.Orient.Matrix[2], joint.Orient.Matrix[5], joint.Orient.Matrix[8]);

                rotation = Quaternion.LookRotation(jointForward, jointUp);
            }
            return rotation;
#else
            return Quaternion.identity;
#endif
        }
#if UNITY_ANDROID
        private float frameTimestamp;

        public float GetFrameTimestamp() {
            return frameTimestamp;
        }

        public float GetHandPose(IVR_Kinect2.BoneType boneType) {
            switch (boneType) {
                case IVR_Kinect2.BoneType.HandLeft:
                    if (hands != null && hands.LeftHand != null)
                        return hands.LeftHand.Value.Click ? 1 : 0;
                    else
                        return 0;
                case IVR_Kinect2.BoneType.HandRight:
                    if (hands != null && hands.RightHand != null)
                        return hands.RightHand.Value.Click ? 1 : 0;
                    else
                        return 0;
                default:
                    return 0;
            }
        }

        int nSkeletonUpdates;
        float lastFrameTimestamp;
        void SkeletonUpdate(nuitrack.SkeletonData _skeletonData) {
            if (present) {
                skeletonData = _skeletonData;

                if (skeletonData != null) {
                    textMesh.text = "NumUsers: " + skeletonData.NumUsers.ToString();
                    tracking = (skeletonData.NumUsers > 0);

                    if (tracking) {
                        nSkeletonUpdates++;
                        // no support for multiple skeletons yet
                        skeleton = skeletonData.Skeletons[0];
                        frameTimestamp = Time.realtimeSinceStartup;
                        textMesh.text += " tracking " + (frameTimestamp - lastFrameTimestamp);
                        lastFrameTimestamp = frameTimestamp;
                    } else {
                        skeleton = null;
                    }
                }
            }
        }

        int nHandsUpdates;
        void HandsUpdate(nuitrack.HandTrackerData _handTrackerData) {
            if (present) {
                handTrackerData = _handTrackerData;

                if (handTrackerData != null && handTrackerData.NumUsers > 0) {
                    nHandsUpdates++;
                    hands = handTrackerData.GetUserHandsByID(skeleton.ID);
                }  else
                    hands = null;
            }
        }

        private nuitrack.JointType Bone2JointType(IVR_Kinect2.BoneType boneType) {
            return jointTypes[(int)boneType];
        }

        private nuitrack.JointType[] jointTypes = {
            nuitrack.JointType.Waist,
            nuitrack.JointType.Torso,
            nuitrack.JointType.Neck,
            nuitrack.JointType.Head,
            nuitrack.JointType.LeftShoulder,
            nuitrack.JointType.LeftElbow,
            nuitrack.JointType.LeftWrist,
            nuitrack.JointType.LeftHand,
            nuitrack.JointType.RightShoulder,
            nuitrack.JointType.RightElbow,
            nuitrack.JointType.RightWrist,
            nuitrack.JointType.RightHand,
            nuitrack.JointType.LeftHip,
            nuitrack.JointType.LeftKnee,
            nuitrack.JointType.LeftAnkle,
            nuitrack.JointType.LeftFoot,
            nuitrack.JointType.RightHip,
            nuitrack.JointType.RightKnee,
            nuitrack.JointType.RightAnkle,
            nuitrack.JointType.RightFoot,
            nuitrack.JointType.None,
            nuitrack.JointType.LeftFingertip,
            nuitrack.JointType.None,
            nuitrack.JointType.RightFingertip,
            nuitrack.JointType.None
        };        
#endif
    }
}