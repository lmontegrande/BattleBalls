/* InstantVR Network Avatar
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.5.3
 * date: May 24, 2016
 *
 * - added check on handMovements existence
 */

using UnityEngine;
using UnityEngine.Networking;

namespace IVR {
    [RequireComponent(typeof(NetworkIdentity))]
    public class IVR_UnetAvatar : NetworkBehaviour {

        public GameObject avatarFP;
        public GameObject avatarTP;
        private bool isFirstPerson;

        protected InstantVR instantiatedAvatar = null;

        protected IVR_HandMovements leftHandMovements, rightHandMovements;

        protected NetworkManager nwManager;
        protected NetworkIdentity identity;

        private enum Modes {
            Uninitialized,
            Player
        }
        [SyncVar]
        private Modes mode = Modes.Uninitialized;

        public override void OnStartServer() {
            RegisterNetworkingHandlers();

            OnStartClient();
        }

        public override void OnStartClient() {
            nwManager = FindObjectOfType<NetworkManager>();
            identity = GetComponent<NetworkIdentity>();
            if (!identity.localPlayerAuthority)
                Debug.LogWarning("Network Avatar Identity need to have Local Player Authority = true");

            switch (mode) {
                case Modes.Player:
                    InstantiateThirdPerson(avatarTP);
                    break;
                default:
                    break;
            }
        }

        public override void OnStartLocalPlayer() {
            identity = GetComponent<NetworkIdentity>();

            if (identity.hasAuthority) {
                mode = Modes.Player;
                NetworkStartPosition[] startPositions = FindObjectsOfType<NetworkStartPosition>();

                switch (mode) {
                    case Modes.Player:
                        if (startPositions.Length > 0) {
                            transform.position = startPositions[0].transform.position;
                            transform.rotation = startPositions[0].transform.rotation;
                        }
                        InstantiateFirstPerson(avatarFP);
                        CmdInstantiateThirdPersonOnClients(mode);
                        break;
                    default:
                        break;
                }
            }
        }

        void FixedUpdate() {
            if (identity != null) {
                if (identity.isLocalPlayer) {
                    if (instantiatedAvatar != null) {
                        SyncClient2Server(identity, instantiatedAvatar, leftHandMovements, rightHandMovements);
                    }
                }
            }
        }

        private void SyncClient2Server(NetworkIdentity identity, InstantVR instantiatedAvatar, IVR_HandMovements leftHandMovements, IVR_HandMovements rightHandMovements) {
            short msgType = MsgType.Highest + 1;

            NetworkWriter writer = new NetworkWriter();
            writer.StartMessage(msgType);
            {
                writer.Write(identity.netId);
                WriteAvatarPose(writer, instantiatedAvatar, leftHandMovements, rightHandMovements);
            }
            writer.FinishMessage();

            identity.connectionToServer.SendWriter(writer, Channels.DefaultReliable);
        }

        #region Server

        [Server]
        protected void RegisterNetworkingHandlers() {
            short msgType = MsgType.Highest + 1;

            NetworkServer.RegisterHandler(msgType, OnAvatarPose);

        }

        [ServerCallback]
        public void OnAvatarPose(NetworkMessage msg) {
            NetworkReader reader = msg.reader;

            NetworkInstanceId netId = reader.ReadNetworkId();
            GameObject obj = NetworkServer.FindLocalObject(netId);
            InstantVR ivrFP = obj.GetComponentInChildren<InstantVR>();
            if (ivrFP != null) {
                IVR_HandMovements leftHandMovements = ivrFP.leftHandTarget.GetComponent<IVR_HandMovements>();
                IVR_HandMovements rightHandMovements = ivrFP.rightHandTarget.GetComponent<IVR_HandMovements>();

                ReadAvatarPose(reader, ivrFP, leftHandMovements, rightHandMovements);

                short msgType = MsgType.Highest + 2;
                NetworkWriter writer = new NetworkWriter();
                writer.StartMessage(msgType);
                {
                    writer.Write(netId);
                    WriteAvatarPose(writer, ivrFP, leftHandMovements, rightHandMovements);
                }
                writer.FinishMessage();

                NetworkServer.SendWriterToReady(null, writer, Channels.DefaultUnreliable);
            }
        }

        protected void WriteAvatarPose(NetworkWriter writer, InstantVR ivr, IVR_HandMovements leftHandMovements, IVR_HandMovements rightHandMovements) {
            writer.Write(ivr.transform.position);
            writer.Write(ivr.transform.rotation);

            writer.Write(ivr.headTarget.position);
            writer.Write(ivr.headTarget.rotation);

            writer.Write(ivr.leftHandTarget.position);
            writer.Write(ivr.leftHandTarget.rotation);

            writer.Write(ivr.rightHandTarget.position);
            writer.Write(ivr.rightHandTarget.rotation);

            writer.Write(ivr.hipTarget.position);
            writer.Write(ivr.hipTarget.rotation);

            writer.Write(ivr.leftFootTarget.position);
            writer.Write(ivr.leftFootTarget.rotation);

            writer.Write(ivr.rightFootTarget.position);
            writer.Write(ivr.rightFootTarget.rotation);

            WriteAvatarHandPose(writer, leftHandMovements);
            WriteAvatarHandPose(writer, rightHandMovements);
        }

        private void WriteAvatarHandPose(NetworkWriter writer, IVR_HandMovements handMovements) {
            if (handMovements != null) {
                writer.Write(true);
                writer.Write(handMovements.thumbCurl);
                writer.Write(handMovements.indexCurl);
                writer.Write(handMovements.middleCurl);
                writer.Write(handMovements.ringCurl);
                writer.Write(handMovements.littleCurl);
            } else {
                writer.Write(false);
            }
        }

        #endregion

        #region Client First Person
        [Client]
        protected void RegisterClientHandlers(InstantVR ivr, NetworkManager nwManager) {
            short msgType = MsgType.Highest + 2;

            nwManager.client.RegisterHandler(msgType, msg => OnAvatarTPPose(msg, instantiatedAvatar));
        }

        protected virtual void InstantiateFirstPerson(GameObject avatar) {
            GameObject instantiatedObject = (GameObject)Instantiate(avatar, transform.position, transform.rotation);
            instantiatedAvatar = instantiatedObject.GetComponent<InstantVR>();

            instantiatedObject.transform.parent = this.transform;

            this.gameObject.name = instantiatedObject.name;

            if (instantiatedAvatar != null) {
                leftHandMovements = instantiatedAvatar.leftHandTarget.GetComponent<IVR_HandMovements>();
                rightHandMovements = instantiatedAvatar.rightHandTarget.GetComponent<IVR_HandMovements>();

                if (identity.isLocalPlayer)
                    RegisterClientHandlers(instantiatedAvatar, nwManager);
            }
        }

        #endregion

        #region Client Third Person
        [Command]
        private void CmdInstantiateThirdPersonOnClients(Modes mode) {
            //Debug.Log("CmdInstantiateThirdPersonOnClients: " + avatar);
            RpcInstantiateThirdPerson(mode);
        }

        [ClientRpc]
        private void RpcInstantiateThirdPerson(Modes mode) {
            //Debug.Log("RpcInstantiateThirdPerson: " + avatar);
            NetworkIdentity identity = GetComponent<NetworkIdentity>();
            if (identity == null)
                avatarTP.AddComponent<NetworkIdentity>();

            if (!identity.hasAuthority) {
                switch (mode) {
                    case Modes.Player:
                        InstantiateThirdPerson(avatarTP);
                        break;
                    default:
                        break;
                }
            }
        }

        protected virtual void InstantiateThirdPerson(GameObject avatar) {
            //Debug.Log("InstantiateThirdPerson: " + avatar);
            if (avatar != null) {
                GameObject instantiatedObject = (GameObject)Instantiate(avatar, this.transform.position, this.transform.rotation);

                instantiatedAvatar = instantiatedObject.GetComponent<InstantVR>();

                instantiatedObject.transform.parent = this.transform;

                this.gameObject.name = instantiatedObject.name;

                if (instantiatedAvatar != null) {
                    leftHandMovements = instantiatedAvatar.leftHandTarget.GetComponent<IVR_HandMovements>();
                    rightHandMovements = instantiatedAvatar.rightHandTarget.GetComponent<IVR_HandMovements>();

                    if (!identity.isLocalPlayer)
                        RegisterClientHandlers(instantiatedAvatar, nwManager);
                }
            }
        }

        [ClientCallback]
        public void OnAvatarTPPose(NetworkMessage msg, InstantVR tpAvatar) {
            NetworkReader reader = msg.reader;

            NetworkInstanceId netId = reader.ReadNetworkId();

            GameObject obj = ClientScene.FindLocalObject(netId);
            NetworkIdentity id = obj.GetComponent<NetworkIdentity>();
            if (id.hasAuthority == false) {
                InstantVR ivrTP = obj.GetComponentInChildren<InstantVR>();
                IVR_HandMovements leftHandMovements = ivrTP.leftHandTarget.GetComponent<IVR_HandMovements>();
                IVR_HandMovements rightHandMovements = ivrTP.rightHandTarget.GetComponent<IVR_HandMovements>();
                ReadAvatarPose(reader, ivrTP, leftHandMovements, rightHandMovements);
            }
        }
        #endregion


        private void ReadAvatarPose(NetworkReader reader, InstantVR ivr, IVR_HandMovements leftHandMovements, IVR_HandMovements rightHandMovements) {
            ivr.transform.position = reader.ReadVector3();
            ivr.transform.rotation = reader.ReadQuaternion();

            ivr.headTarget.position = reader.ReadVector3();
            ivr.headTarget.rotation = reader.ReadQuaternion();

            ivr.leftHandTarget.position = reader.ReadVector3();
            ivr.leftHandTarget.rotation = reader.ReadQuaternion();

            ivr.rightHandTarget.position = reader.ReadVector3();
            ivr.rightHandTarget.rotation = reader.ReadQuaternion();

            ivr.hipTarget.position = reader.ReadVector3();
            ivr.hipTarget.rotation = reader.ReadQuaternion();

            ivr.leftFootTarget.position = reader.ReadVector3();
            ivr.leftFootTarget.rotation = reader.ReadQuaternion();

            ivr.rightFootTarget.position = reader.ReadVector3();
            ivr.rightFootTarget.rotation = reader.ReadQuaternion();

            bool leftHandIncluded = (bool) reader.ReadBoolean();
            if (leftHandIncluded) {
                ReadAvatarHandPose(reader, leftHandMovements);
            }

            bool rightHandIncluded = (bool) reader.ReadBoolean();
            if (rightHandIncluded) {
                ReadAvatarHandPose(reader, rightHandMovements);
            }
        }

        private void ReadAvatarHandPose(NetworkReader reader, IVR_HandMovements handMovements) {
            float thumbCurl = (float) reader.ReadSingle();
            float indexCurl = (float) reader.ReadSingle();
            float middleCurl = (float) reader.ReadSingle();
            float ringCurl = (float) reader.ReadSingle();
            float littleCurl = (float) reader.ReadSingle();

            if (handMovements != null) {
                handMovements.thumbCurl = thumbCurl;
                handMovements.indexCurl = indexCurl;
                handMovements.middleCurl = middleCurl;
                handMovements.ringCurl = ringCurl;
                handMovements.littleCurl = littleCurl;
            }
        }


        void OnDisconnectedFromServer(NetworkDisconnection info) {
            Destroy(this.gameObject);
        }

        void OnDestroy() {
            if (instantiatedAvatar != null)
                Destroy(instantiatedAvatar.gameObject);
        }

        [Command]
        public void CmdGrab(GameObject obj, bool leftHanded) {
            //Debug.Log("CmdGrab " + obj);
            NetworkIdentity nwIdentity = obj.GetComponent<NetworkIdentity>();
            if (nwIdentity == null) {
                Debug.LogWarning("Grabbed object does not have a network identity. Its transform will not be synced across the network.");
                LocalClientGrab(obj, leftHanded);
            } else {
                RpcClientGrab(obj, leftHanded);
            }
        }

        [Command]
        public void CmdLetGo(GameObject obj, bool leftHanded) {
            //Debug.Log("CmdLetGo" + obj);
            NetworkIdentity nwIdentity = obj.GetComponent<NetworkIdentity>();
            if (nwIdentity == null) {
                Debug.LogWarning("Let go object does not have a network identity. Its transform will not be synced across the network.");
                LocalClientLetGo(obj, leftHanded);
            } else {
                RpcClientLetGo(obj, leftHanded);
            }
        }

        [ClientRpc]
        public void RpcClientGrab(GameObject obj, bool leftHanded) {
            //Debug.Log("RpcClientGrab " + obj);
            LocalClientGrab(obj, leftHanded);
        }

        private void LocalClientGrab(GameObject obj, bool leftHanded) {
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

        [ClientRpc]
        public void RpcClientLetGo(GameObject obj, bool leftHanded) {
            //Debug.Log("RpcClientLetGo " + obj);
            LocalClientLetGo(obj, leftHanded);
        }

        private void LocalClientLetGo(GameObject obj, bool leftHanded) {
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