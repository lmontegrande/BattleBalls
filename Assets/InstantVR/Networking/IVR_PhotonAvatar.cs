/* InstantVR Photon Avatar
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.5.2
 * date: August 7, 2016
 *
 * - Reduced glitches in movement
 */

#if IVR_PHOTON
using UnityEngine;

namespace IVR {

    [RequireComponent(typeof(PhotonView))]
    public class IVR_PhotonAvatar : Photon.MonoBehaviour{

        public GameObject avatarFP;
        public GameObject avatarTP;
        private bool isFirstPerson;

        protected InstantVR instantiatedAvatar = null;

        protected IVR_HandMovements leftHandMovements, rightHandMovements;

        private NetworkedTarget[] targets;

        private enum Modes {
            Uninitialized,
            Player
        }

        void OnPhotonInstantiate(PhotonMessageInfo info) {
            if (photonView.isMine) {
                InstantiateFirstPerson(avatarFP);
            } else {
                InstantiateThirdPerson(avatarTP);
            }

            InitializeNetworkedTargets(instantiatedAvatar);
        }

        private void InitializeNetworkedTargets(InstantVR instantiatedAvatar) {
            targets = new NetworkedTarget[7];
            targets[0] = new NetworkedTarget(instantiatedAvatar.transform);
            targets[1] = new NetworkedTarget(instantiatedAvatar.headTarget);
            targets[2] = new NetworkedTarget(instantiatedAvatar.leftHandTarget);
            targets[3] = new NetworkedTarget(instantiatedAvatar.rightHandTarget);
            targets[4] = new NetworkedTarget(instantiatedAvatar.hipTarget);
            targets[5] = new NetworkedTarget(instantiatedAvatar.leftFootTarget);
            targets[6] = new NetworkedTarget(instantiatedAvatar.rightFootTarget);
        }

        protected virtual void InstantiateFirstPerson(GameObject avatar) {
            GameObject instantiatedObject = (GameObject) Instantiate(avatar, transform.position, transform.rotation);
            instantiatedAvatar = instantiatedObject.GetComponent<InstantVR>();

            instantiatedObject.transform.parent = this.transform;

            this.gameObject.name = avatar.name;

            if (instantiatedAvatar != null) {
                leftHandMovements = instantiatedAvatar.leftHandTarget.GetComponent<IVR_HandMovements>();
                rightHandMovements = instantiatedAvatar.rightHandTarget.GetComponent<IVR_HandMovements>();
            }
        }

        protected virtual void InstantiateThirdPerson(GameObject avatar) {
            //Debug.Log("InstantiateThirdPerson: " + avatar);
            if (avatar != null) {
                GameObject instantiatedObject = (GameObject) Instantiate(avatar, this.transform.position, this.transform.rotation);

                instantiatedAvatar = instantiatedObject.GetComponent<InstantVR>();
                instantiatedAvatar.collisions = false;

                instantiatedObject.transform.parent = this.transform;

                this.gameObject.name = avatar.name;

                if (instantiatedAvatar != null) {
                    leftHandMovements = instantiatedAvatar.leftHandTarget.GetComponent<IVR_HandMovements>();
                    rightHandMovements = instantiatedAvatar.rightHandTarget.GetComponent<IVR_HandMovements>();
                }
            }
        }

        void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
            if (stream.isWriting) {
                WriteAvatarPose(stream, instantiatedAvatar, leftHandMovements, rightHandMovements);
            } else {
                ReadAvatarPose(stream, instantiatedAvatar, leftHandMovements, rightHandMovements);
            }
        }

        void Update() {
            if (!photonView.isMine) {
                for (int i = 0; i < targets.Length; i++) {
                    UpdateTarget(targets[i]);
                }
            }
        }

        int poseNr;

        public void WriteAvatarPose(PhotonStream stream, InstantVR ivr, IVR_HandMovements leftHandMovements, IVR_HandMovements rightHandMovements) {
            stream.SendNext(Time.time);

            stream.SendNext(poseNr++);

            for (int i = 0; i < targets.Length; i++) {
                SendTarget(stream, targets[i]);
            }

            WriteAvatarHandPose(stream, leftHandMovements);
            WriteAvatarHandPose(stream, rightHandMovements);
        }

        private void WriteAvatarHandPose(PhotonStream stream, IVR_HandMovements handMovements) {
            if (handMovements != null) {
                stream.SendNext(true);
                stream.SendNext(handMovements.thumbCurl);
                stream.SendNext(handMovements.indexCurl);
                stream.SendNext(handMovements.middleCurl);
                stream.SendNext(handMovements.ringCurl);
                stream.SendNext(handMovements.littleCurl);
            } else {
                stream.SendNext(false);
            }
        }

        float lastTime;
        float deltaTime;
        int lastPostNr;

        private void ReadAvatarPose(PhotonStream reader, InstantVR ivr, IVR_HandMovements leftHandMovements, IVR_HandMovements rightHandMovements) {
            float poseTime = (float) reader.ReceiveNext();
            deltaTime = poseTime - lastTime;
            lastTime = poseTime;

            int poseNr = (int) reader.ReceiveNext();
            if (poseNr != lastPostNr + 1) {
                Debug.LogWarning("Message order is incorrect");
            }
            lastPostNr = poseNr;

            for (int i = 0; i < targets.Length; i++) {
                ReceiveTarget(reader, targets[i]);
            }

            bool leftHandIncluded = (bool) reader.ReceiveNext();
            if (leftHandIncluded) {
                ReadAvatarHandPose(reader, leftHandMovements);
            }

            bool rightHandIncluded = (bool) reader.ReceiveNext();
            if (rightHandIncluded) {
                ReadAvatarHandPose(reader, rightHandMovements);
            }
        }

        private void ReadAvatarHandPose(PhotonStream reader, IVR_HandMovements handMovements) {
            float thumbCurl = (float) reader.ReceiveNext();
            float indexCurl = (float) reader.ReceiveNext();
            float middleCurl = (float) reader.ReceiveNext();
            float ringCurl = (float) reader.ReceiveNext();
            float littleCurl = (float) reader.ReceiveNext();

            if (handMovements != null) {
                handMovements.thumbCurl = thumbCurl;
                handMovements.indexCurl = indexCurl;
                handMovements.middleCurl = middleCurl;
                handMovements.ringCurl = ringCurl;
                handMovements.littleCurl = littleCurl;
            }
        }

        class NetworkedTarget {
            public Transform transform;

            public int validLevel = 0; // 1 = position, 2 = velocity, 3 = acceleration

            //public float lastTime = Time.time;
            public Quaternion lastRotation;
            public Vector3 lastPosition;

            public Vector3 positionalVelocity;
            public float angularVelocity;
            public Vector3 velocityAxis;

            public NetworkedTarget(Transform _transform) {
                transform = _transform;
            }
        };

        private void SendTarget(PhotonStream stream, NetworkedTarget target) {
            stream.SendNext(target.transform.position);
            stream.SendNext(target.transform.rotation);
        }

        private void ReceiveTarget(PhotonStream reader, NetworkedTarget target) {
            target.transform.position = (Vector3) reader.ReceiveNext();
            target.transform.rotation = (Quaternion) reader.ReceiveNext();

            CalculateVelocity(ref target);
        }

        private void CalculateVelocity(ref NetworkedTarget target) {
            //float deltaTime = Time.time - target.lastTime;
            if (deltaTime > 0) {
                if (target.validLevel < 3)
                    target.validLevel++;

                Vector3 newPositionalVelocity = (target.transform.position - target.lastPosition) / deltaTime;

                float angle = 0;
                Quaternion rotationalChange = Quaternion.Inverse(target.lastRotation) * target.transform.rotation;

                Vector3 newVelocityAxis;
                rotationalChange.ToAngleAxis(out angle, out newVelocityAxis);
                if (angle == 0)
                    newVelocityAxis = Vector3.one;

                target.positionalVelocity = newPositionalVelocity;
                target.angularVelocity = angle / deltaTime;
                target.velocityAxis = newVelocityAxis;

                target.lastPosition = target.transform.position;
                target.lastRotation = target.transform.rotation;
                //target.lastTime = Time.time;
            }
        }

        private void UpdateTarget(NetworkedTarget target) {
            if (target.validLevel == 3) {
                //float deltaTime = Time.time - target.lastTime;

                if (deltaTime < 0.2F) { //extrapolate for 0.5s maximum
                    target.transform.position = target.lastPosition + target.positionalVelocity * deltaTime;
                    target.transform.rotation = target.lastRotation * Quaternion.AngleAxis(target.angularVelocity * deltaTime, target.velocityAxis);
                }
            }
        }

        public void PunGrab(GameObject obj, bool leftHanded) {
            if (photonView.isMine) {
                //Debug.Log("PunGrab " + obj);
                PhotonView objView = obj.GetComponent<PhotonView>();
                if (objView == null) {
                    Debug.LogWarning("Grabbed object does not have a PhotonView");
                    LocalGrab(obj, leftHanded);
                } else {
                    photonView.RPC("PunRpcGrab", PhotonTargets.All, objView.viewID, leftHanded);
                }
            }
        }

        public void PunLetGo(GameObject obj, bool leftHanded) {
            if (photonView.isMine) {
                //Debug.Log("PunLetGo" + obj);
                PhotonView objView = obj.GetComponent<PhotonView>();
                if (objView == null) {
                    Debug.LogWarning("Let go object does not have a PhotonView");
                    LocalLetGo(obj, leftHanded);
                } else {
                    photonView.RPC("PunRpcLetGo", PhotonTargets.All, objView.viewID, leftHanded);
                }
            }
        }
        
        [PunRPC]
        public void PunRpcGrab(int objViewID, bool leftHanded) {
            GameObject obj = PhotonView.Find(objViewID).gameObject;
            Debug.Log("PunRpcGrab " + obj);
            LocalGrab(obj, leftHanded);
        }
        
        private void LocalGrab(GameObject obj, bool leftHanded) {
            if (obj != null) {
                IVR_HandMovements handMovements;
                if (leftHanded)
                    handMovements = instantiatedAvatar.leftHandTarget.GetComponent<IVR_HandMovements>();
                else
                    handMovements = instantiatedAvatar.rightHandTarget.GetComponent<IVR_HandMovements>();
                if (handMovements != null) {
                    handMovements.LocalGrab(obj);
                }
            }
        }
        
        [PunRPC]
        public void PunRpcLetGo(int objViewID, bool leftHanded) {
            GameObject obj = PhotonView.Find(objViewID).gameObject;
            //Debug.Log("PunRpcLetGo " + obj);
            LocalLetGo(obj, leftHanded);
        }

        private void LocalLetGo(GameObject obj, bool leftHanded) {
            if (obj != null) {
                IVR_HandMovements handMovements;
                if (leftHanded)
                    handMovements = instantiatedAvatar.leftHandTarget.GetComponent<IVR_HandMovements>();
                else
                    handMovements = instantiatedAvatar.rightHandTarget.GetComponent<IVR_HandMovements>();
                if (handMovements != null) {
                    handMovements.LocalLetGo(obj);
                }
            }
        }
    }
}
#endif