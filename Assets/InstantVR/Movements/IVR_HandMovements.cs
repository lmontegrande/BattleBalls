/* InstantVR Hand Movements
 * author: Pascal Serrarnes
 * email: support@passervr.com
 * version: 3.4.9
 * date: June 2, 2016
 * 
 * - Fixed hands colliding with body
 * - Included kinematic physics switch for InstantVR Edge
 */

using UnityEngine;
using System.Collections;

namespace IVR {

    public class IVR_HandMovements : IVR_HandMovementsBase {

#if INSTANTVR_EDGE
        public bool kinematicPhysics = true;
        public float strength = 100;
#endif
        public bool bodyPull = false;

        [HideInInspector]
        public Animator animator;
        [HideInInspector]
        private GameObject hand;
        [HideInInspector]
        private Rigidbody handRigidbody;
        [HideInInspector]
        private Transform forearm;
        [HideInInspector]
        private Transform handTarget;
        [HideInInspector]
        private IVR_BodyMovements bm;

        private Thumb thumb = null;
        private Finger indexFinger = null;
        private Finger middleFinger = null;
        private Finger ringFinger = null;
        private Finger littleFinger = null;

        private Digit[] digits = null;

        public float thumbCurl;
        public float indexCurl;
        public float middleCurl;
        public float ringCurl;
        public float littleCurl;

        [HideInInspector]
        public IVR_Kinematics kr = null;

        [HideInInspector]
        private Collider collidedObject;

        [HideInInspector]
        public Transform handPalm;
        [HideInInspector]
        private GameObject handObj;
        [HideInInspector]
        private Vector3 palmOffset;

        [HideInInspector]
        private Vector3 handRightAxis, handRightAxisThumb;

        private enum LeftHandBones {
            ThumbProximal = 24,
            ThumbIntermediate = 25,
            ThumbDistal = 26,
            IndexProximal = 27,
            IndexIntermediate = 28,
            IndexDistal = 29,
            MiddleProximal = 30,
            MiddleIntermediate = 31,
            MiddleDistal = 32,
            RingProximal = 33,
            RingIntermediate = 34,
            RingDistal = 35,
            LittleProximal = 36,
            LittleIntermediate = 37,
            LittleDistal = 38
        };
        private enum RightHandBones {
            ThumbProximal = 39,
            ThumbIntermediate = 40,
            ThumbDistal = 41,
            IndexProximal = 42,
            IndexIntermediate = 43,
            IndexDistal = 44,
            MiddleProximal = 45,
            MiddleIntermediate = 46,
            MiddleDistal = 47,
            RingProximal = 48,
            RingIntermediate = 49,
            RingDistal = 50,
            LittleProximal = 51,
            LittleIntermediate = 52,
            LittleDistal = 53
        };

        public override void StartMovements(InstantVR ivr) {
            this.ivr = ivr;

            handTarget = this.transform;

            hand = this.gameObject;
            thumb = new Thumb();
            indexFinger = new Finger();
            middleFinger = new Finger();
            ringFinger = new Finger();
            littleFinger = new Finger();

            animator = ivr.GetComponentInChildren<Animator>();
            bm = ivr.GetComponent<IVR_BodyMovements>();

            if (this.transform == ivr.leftHandTarget) {
                hand = animator.GetBoneTransform(HumanBodyBones.LeftHand).gameObject;

                thumb.transform = animator.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);
                indexFinger.transform = animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal);
                middleFinger.transform = animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
                ringFinger.transform = animator.GetBoneTransform(HumanBodyBones.LeftRingProximal);
                littleFinger.transform = animator.GetBoneTransform(HumanBodyBones.LeftLittleProximal);

                forearm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);

                //leapHand = ivr.leftHandTarget.GetComponent<IVR.IVR_LeapHand>();
            } else {
                hand = animator.GetBoneTransform(HumanBodyBones.RightHand).gameObject;

                thumb.transform = animator.GetBoneTransform(HumanBodyBones.RightThumbIntermediate);
                indexFinger.transform = animator.GetBoneTransform(HumanBodyBones.RightIndexProximal);
                middleFinger.transform = animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
                ringFinger.transform = animator.GetBoneTransform(HumanBodyBones.RightRingProximal);
                littleFinger.transform = animator.GetBoneTransform(HumanBodyBones.RightLittleProximal);

                forearm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            }

            DeterminePalmPosition();

            if (indexFinger.transform != null && littleFinger.transform != null) {
                handRightAxisThumb = thumb.transform.InverseTransformDirection(littleFinger.transform.position - indexFinger.transform.position);
                handRightAxis = indexFinger.transform.InverseTransformDirection(littleFinger.transform.position - indexFinger.transform.position);
            } else if (indexFinger.transform != null && middleFinger.transform != null) {
                handRightAxisThumb = thumb.transform.InverseTransformDirection(middleFinger.transform.position - indexFinger.transform.position);
                handRightAxis = indexFinger.transform.InverseTransformDirection(middleFinger.transform.position - indexFinger.transform.position);
            } else {
                handRightAxisThumb = -ivr.characterTransform.right;
                handRightAxis = -ivr.characterTransform.right;
            }
            if (this.transform == ivr.leftHandTarget) {
                handRightAxisThumb = -handRightAxisThumb;
                handRightAxis = -handRightAxis;
            }

            handObj = DetachHand();

            kr = this.GetComponent<IVR_Kinematics>();
            if (kr == null) {
#if INSTANTVR_EDGE
                if (kinematicPhysics) {
                    IVR_KinematicPhysics kp = this.gameObject.AddComponent<IVR_KinematicPhysics>();
                    kp.strength = this.strength;
                    kr = kp;
                } else
#endif
                    kr = this.gameObject.AddComponent<IVR_Kinematics>();
            }

            kr.target = handTarget;
            kr.Kinematize(handObj.GetComponent<Rigidbody>());

            IVR_HandColliderHandler handCH = handObj.AddComponent<IVR_HandColliderHandler>();
            handCH.Initialize(this.transform == ivr.leftHandTarget, this);

            digits = new Digit[5];
            digits[0] = thumb;
            digits[1] = indexFinger;
            digits[2] = middleFinger;
            digits[3] = ringFinger;
            digits[4] = littleFinger;

            thumb.characterTransform = animator.transform;
            thumb.Init(hand.transform, handRightAxis, hand.transform.position - forearm.position, this.transform == ivr.leftHandTarget, 0);
            for (int i = 1; i < digits.Length; i++)
                digits[i].Init(hand.transform, handRightAxis, hand.transform.position - forearm.position, this.transform == ivr.leftHandTarget, i);

            if (bm != null) {
                if (this.transform == ivr.leftHandTarget)
                    bm.SetLeftHandTarget(handObj.transform);
                else
                    bm.SetRightHandTarget(handObj.transform);
            }

        }

        private GameObject DetachHand() {
            handObj = new GameObject();
            handObj.transform.position = hand.transform.position;

            if (this.transform == ivr.leftHandTarget) {
                handObj.name = "Left hand";
                handObj.transform.rotation = hand.transform.rotation * bm.leftArm.fromNormHand;
            } else {
                handObj.name = "Right hand";
                handObj.transform.rotation = hand.transform.rotation * bm.rightArm.fromNormHand;
            }
            hand.transform.parent = handObj.transform;

            //handObj.transform.position = handTarget.position;
            //handObj.transform.rotation = handTarget.rotation;

            handRigidbody = handObj.GetComponent<Rigidbody>();
            if (handRigidbody == null)
                handRigidbody = handObj.AddComponent<Rigidbody>();
            handRigidbody.mass = 1F;
            handRigidbody.drag = 0; //5
            handRigidbody.angularDrag = 7;
            handRigidbody.useGravity = false;
            handRigidbody.isKinematic = true;
            handRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            handRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            return handObj;
        }

        private void DeterminePalmPosition() {
            GameObject handPalmObj = new GameObject("Hand Palm");
            handPalm = handPalmObj.transform;
            handPalm.parent = hand.transform;

            // Determine position
            if (indexFinger.transform)
                palmOffset = (indexFinger.transform.position - hand.transform.position) * 0.9f;
            else if (middleFinger.transform)
                palmOffset = (middleFinger.transform.position - hand.transform.position) * 0.9f;
            else
                palmOffset = new Vector3(0.1f, 0, 0);

            handPalm.position = hand.transform.position + palmOffset;

            // Determine rotation
            if (indexFinger.transform)
                handPalm.LookAt(indexFinger.transform, Vector3.up);
            else if (middleFinger.transform)
                handPalm.LookAt(middleFinger.transform, Vector3.up);
            else if (this.transform == ivr.leftHandTarget)
                handPalm.LookAt(handPalm.position - ivr.characterTransform.right, Vector3.up);
            else
                handPalm.LookAt(handPalm.position + ivr.characterTransform.right, Vector3.up);

            // Now get it in the palm

            handPalm.rotation *= Quaternion.Euler(50, 0, 0); // * handPalm.rotation;
            if (transform == ivr.leftHandTarget) {
                handPalm.position += handPalm.rotation * new Vector3(0.02f, -0.02f, 0);
            } else {
                handPalm.position += handPalm.rotation * new Vector3(-0.02f, -0.02f, 0);
            }
        }
        bool collisionsIgnored = false;

        private void IgnoreStaticCollisions(Rigidbody myHand, GameObject obj, bool ignore = true) {
            Collider[] myHandColliders = myHand.GetComponentsInChildren<Collider>();
            Collider[] objColliders = obj.GetComponentsInChildren<Collider>();

            for (int i = 0; i < objColliders.Length; i++) {
                for (int j = 0; j < myHandColliders.Length; j++) {

                    Physics.IgnoreCollision(objColliders[i], myHandColliders[j], ignore);
                }
            }
        }

        private void IgnoreRigidbodyCollisions(Rigidbody myBody, Rigidbody myHand) {
            Collider[] myBodyColliders = myBody.GetComponentsInChildren<Collider>();
            Collider[] myHandColliders = myHand.GetComponentsInChildren<Collider>();

            for (int i = 0; i < myBodyColliders.Length; i++) {
                for (int j = 0; j < myHandColliders.Length; j++) {
                    Physics.IgnoreCollision(myBodyColliders[i], myHandColliders[j]);
                }
            }
        }

        private void IgnoreHandBodyCollisions() {
            Rigidbody hipRigidbody = ivr.GetComponent<Rigidbody>();
            if (hipRigidbody != null) {
                Rigidbody handRigidbody = handObj.GetComponent<Rigidbody>();
                IgnoreRigidbodyCollisions(hipRigidbody, handRigidbody);
            }
        }

        IEnumerator TmpDisableCollisions(Rigidbody handRigidbody, GameObject grabbedObj) {
            SetAllColliders(handObj, false);
            yield return new WaitForSeconds(0.2f);
            SetAllColliders(handObj, true);
        }

        public override void UpdateMovements() {
            if (!collisionsIgnored) {
                IgnoreHandBodyCollisions();
                collisionsIgnored = true;
            }
            HandUpdate();
            CheckHandMessages();
        }

        private void HandUpdate() {
            kr.UpdateKR();

            if (thumb != null && digits != null) {
                thumb.Update(thumbCurl);
                digits[1].Update(indexCurl);
                digits[2].Update(middleCurl);
                digits[3].Update(ringCurl);
                digits[4].Update(littleCurl);
                if (grabbedObject != null) {
                    kr.UpdateJoinedObject(handPalm);
                }
            }
        }

#region HandMessages
        private bool fingersClosed = false;

        private void CheckHandMessages() {
            if (fingersClosed) {
                if (indexFinger.input < 0.5f && middleFinger.input < 0.5f && ringFinger.input < 0.5f & littleFinger.input < 0.5f) {
                    fingersClosed = false;
                    ivr.gameObject.SendMessage("OnFingersOpened", this, SendMessageOptions.DontRequireReceiver);
                }
            } else {
                if (indexFinger.input > 0.5f || middleFinger.input > 0.5f || ringFinger.input > 0.5f || littleFinger.input > 0.5f) {
                    fingersClosed = true;
                    ivr.gameObject.SendMessage("OnFingersClosed", this, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
#endregion

        public bool grabbing = false;
        public bool lettingGo = false;

        void FixedUpdate() {
            DetermineVelocity(handRigidbody);
            if (grabbedObject != null && !lettingGo) {
                float totalCurl = indexCurl + middleCurl + ringCurl + littleCurl;
                if (totalCurl < 0.15F) {
                    lettingGo = false;
                    LetGo(grabbedObject);
                }
            }
        }

        [HideInInspector]
        private Vector3 lastPosition;
        private Quaternion lastRotation;
        private void DetermineVelocity(Rigidbody rigidbody) {
            // velocity is normally not calculated for kinematic rigidbodies :-|
            if (rigidbody != null && rigidbody.isKinematic) {
                rigidbody.velocity = (rigidbody.position - lastPosition) / Time.fixedDeltaTime;
                lastPosition = rigidbody.position;

                rigidbody.angularVelocity = (Quaternion.Inverse(lastRotation) * rigidbody.rotation).eulerAngles / Time.fixedDeltaTime;
                lastRotation = rigidbody.rotation;
                //Debug.Log(rigidbody.velocity * 1000 + " / " + rigidbody.angularVelocity * 1000);
            }
        }

        public void Grab(GameObject obj) {
#if INSTANTVR_EDGE
            if (ivr.transform.parent != null) {
                IVR_UnetAvatar networkAvatar = ivr.transform.parent.GetComponent<IVR_UnetAvatar>();
#if IVR_PHOTON
                IVR_PhotonAvatar punAvatar = ivr.transform.parent.GetComponent<IVR_PhotonAvatar>();
#endif
                if (networkAvatar != null) {
                    if (networkAvatar.isLocalPlayer) {
                        networkAvatar.CmdGrab(obj, transform == ivr.leftHandTarget);
                    }
#if IVR_PHOTON
                } else if (punAvatar != null) {
                    punAvatar.PunGrab(obj, transform == ivr.leftHandTarget);
#endif
                } else
                    LocalGrab(obj);
            } else
#endif
                    LocalGrab(obj);
        }

        public void LocalGrab(GameObject obj) {
            Rigidbody objRigidbody = obj.GetComponent<Rigidbody>();
#if INSTANTVR_EDGE
            KinematicPhysics.KinematicPhysics kp = obj.GetComponent<KinematicPhysics.KinematicPhysics>();
            if (kp != null) {
                grabbing = false;
                return; //Grabbing of KP objects is not yet supported
            }
#endif
            IVR_Handle handle = obj.GetComponentInChildren<IVR_Handle>();

            if (handle != null) {
                SetAllColliders(handObj, false);
                handle.Grab(handObj, hand.transform.parent, handPalm);
                kr.Join(obj, handPalm);
                grabbedObject = obj;
            } else {
                if (objRigidbody != null) {
                    SetAllColliders(handObj, false);
                    GrabNoHandle(obj.GetComponent<Rigidbody>());
                    grabbedObject = obj;
                }
            }
            /*
            if (objRigidbody != null)
                GrabMassRedistribution(handObj.GetComponent<Rigidbody>(), objRigidbody);
            */

            grabbing = false;

            obj.SendMessage("OnGrabbed", this, SendMessageOptions.DontRequireReceiver);
            SendMessage("OnGrabbing", obj, SendMessageOptions.DontRequireReceiver);
        }

        StoredRigidbody grabbedRBdata;
        Transform originalParent;

        private void GrabNoHandle(Rigidbody rigidbody) {
            if (rigidbody != null) {
                grabbedRBdata = new StoredRigidbody(rigidbody);
                Destroy(rigidbody);
            } else {
                grabbedRBdata = null;
            }

            originalParent = rigidbody.transform.parent;
            rigidbody.transform.parent = hand.transform.parent;
        }

        public static void HandGrabPosition(Transform wrist, Transform handPalm, Vector3 targetPosition, Quaternion targetRotation, out Vector3 handPosition, out Quaternion handRotation) {
            Quaternion palm2handRot = Quaternion.Inverse(handPalm.rotation) * wrist.rotation;
            handRotation = targetRotation * palm2handRot;

            Vector3 localPalmPosition = wrist.InverseTransformPoint(handPalm.position);
            handPosition = targetPosition - handRotation * localPalmPosition;
        }

        public void HandGrabPosition(Vector3 targetPosition, Quaternion targetRotation, out Vector3 handPosition, out Quaternion handRotation) {
            HandGrabPosition(handObj.transform, handPalm, targetPosition, targetRotation, out handPosition, out handRotation);
        }

        public void LetGo(GameObject obj) {
#if INSTANTVR_EDGE
            if (ivr.transform.parent != null) {
                IVR_UnetAvatar networkAvatar = ivr.transform.parent.GetComponent<IVR_UnetAvatar>();
#if IVR_PHOTON
                IVR_PhotonAvatar punAvatar = ivr.transform.parent.GetComponent<IVR_PhotonAvatar>();
#endif
                if (networkAvatar != null) {
                    if (networkAvatar.isLocalPlayer && !lettingGo) {
                        lettingGo = true;
                        networkAvatar.CmdLetGo(grabbedObject, transform == ivr.leftHandTarget);
                    }
#if IVR_PHOTON
                } else if (punAvatar != null) {
                    punAvatar.PunLetGo(obj, transform == ivr.leftHandTarget);
#endif
                } else
                    LocalLetGo(grabbedObject);
            } else
#endif
                    LocalLetGo(grabbedObject);
        }

        public void LocalLetGo(GameObject obj) {
            kr.Unjoin();

            IVR_Handle handle = obj.GetComponentInChildren<IVR_Handle>();

            if (handle != null)
                handle.LetGo(handObj);
            else
                LetGoNoHandle(obj);
            /*
            if (objRigidbody != null)
                GrabMassRestoration(handObj.GetComponent<Rigidbody>(), objRigidbody);
            */
            IgnoreStaticCollisions(handObj.GetComponent<Rigidbody>(), obj, false);
            SetColliderToTrigger(obj, false);
            
            StartCoroutine(TmpDisableCollisions(handObj.GetComponent<Rigidbody>(), grabbedObject));

            grabbedObject = null;
            lettingGo = false;

            obj.SendMessage("OnLetGo", this, SendMessageOptions.DontRequireReceiver);
            SendMessage("OneLetGo", obj, SendMessageOptions.DontRequireReceiver);
        }



        private void LetGoNoHandle(GameObject obj) {
            Joint joint = handObj.GetComponent<Joint>();
            if (joint != null) {
                Destroy(joint);
            }

            Rigidbody rigidbody = obj.GetComponent<Rigidbody>();
            if (rigidbody == null)
                rigidbody = obj.AddComponent<Rigidbody>();
            if (grabbedRBdata != null)
                grabbedRBdata.CopyToRigidbody(rigidbody);

            if (originalParent != null) {
                obj.transform.parent = originalParent;
                originalParent = null;
            }

            Rigidbody handRigidbody = handObj.GetComponent<Rigidbody>();
            if (handRigidbody != null) {
                rigidbody.velocity = handRigidbody.velocity;
                rigidbody.angularVelocity = handRigidbody.angularVelocity;
            }
        }

        private void GrabMassRedistribution(Rigidbody handRigidbody, Rigidbody grabbedRigidbody) {
            handRigidbody.mass = grabbedRigidbody.mass;
            grabbedRigidbody.mass *= 0.01f;
        }

        private void GrabMassRestoration(Rigidbody handRigidbody, Rigidbody grabbedRigidbody) {
            grabbedRigidbody.mass += handRigidbody.mass;
            handRigidbody.mass = 1f;
        }

        void LateUpdate() {
            if (bodyPull)
                BodyPull();
        }

        private void SetAllColliders(GameObject obj, bool enabled) {
            Collider[] colliders = obj.GetComponentsInChildren<Collider>();
            foreach (Collider c in colliders)
                c.enabled = enabled;
        }

        private static void SetColliderToTrigger(GameObject obj, bool b) {
            Collider[] colliders = obj.GetComponentsInChildren<Collider>();
            for (int j = 0; j < colliders.Length; j++)
                colliders[j].isTrigger = b;
        }

        private void BodyPull() {
            if (grabbedObject != null && grabbedObject.isStatic) {
                Vector3 handPull = hand.transform.position - handTarget.position;
                Debug.DrawLine(hand.transform.position, handTarget.position, Color.blue);

                ivr.Move(handPull, true);
            }
        }
    }

    public class IVR_HandColliderHandler : MonoBehaviour {

        //private Transform thumb;
        private Transform[] digits;

        private IVR_HandMovements handMovements;

        public void Initialize(bool isLeftHand, IVR_HandMovements handMovements) {
            this.handMovements = handMovements;

            digits = new Transform[5];
            if (isLeftHand) {
                //thumb = handTarget.animator.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);
                digits[1] = handMovements.animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal);
                digits[2] = handMovements.animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
                digits[3] = handMovements.animator.GetBoneTransform(HumanBodyBones.LeftRingProximal);
                digits[4] = handMovements.animator.GetBoneTransform(HumanBodyBones.LeftLittleProximal);
            } else {
                //thumb = handTarget.animator.GetBoneTransform(HumanBodyBones.RightThumbIntermediate);
                digits[1] = handMovements.animator.GetBoneTransform(HumanBodyBones.RightIndexProximal);
                digits[2] = handMovements.animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
                digits[3] = handMovements.animator.GetBoneTransform(HumanBodyBones.RightRingProximal);
                digits[4] = handMovements.animator.GetBoneTransform(HumanBodyBones.RightLittleProximal);
            }

        }
        /* We just do simple grabbing at the momennt
            void OnCollisionStay (Collision otherCollider) {
                Transform thisTransform, otherTransform;

                if (handTarget.grabbedObject == null) {
                    bool fingersCollided = false;
                    bool thumbCollided = false;

                    int ncontacts = otherCollider.contacts.Length;
                    for (int i = 0; i < ncontacts; i++ ) {
                        thisTransform = otherCollider.contacts[i].thisCollider.transform;
                        otherTransform = otherCollider.contacts[i].otherCollider.transform;
                        if (thisTransform == thumb || otherTransform == thumb)
                            thumbCollided = true;
                        for (int j = 1; j < digits.Length; j++) {
                            Transform finger = digits[j];
                            if (thisTransform == finger || otherTransform == finger) {
                                fingersCollided = true;
                            }
                        }
                    }

                    bool grabbed = false;
                    // We are touching it both sides
                    if (fingersCollided || thumbCollided) { // || = easy grab, && = realistic grab
                        if (handTarget.indexInput + handTarget.middleInput + handTarget.ringInput + handTarget.littleInput > 0)
                        if (!grabbed) {
                            handTarget.Grab(otherCollider.gameObject);
                            grabbed = true;
                        }
                    }
                }
            }
        */

        void OnKinematicCollisionEnter(GameObject gameObject) {
            GrabOnCollision(gameObject);
        }

        void OnCollisionEnter(Collision collision) {
            GrabOnCollision(collision.gameObject);
        }

        void OnCollisionStay(Collision collision) {
            GrabOnCollision(collision.gameObject);
        }

        public void GrabOnCollision(GameObject grabbedGameObject) {
            if (handMovements.grabbedObject == null && !handMovements.grabbing) {
                if (handMovements.indexCurl + handMovements.middleCurl + handMovements.ringCurl + handMovements.littleCurl >= 1f) {
                    handMovements.grabbing = true;
                    handMovements.Grab(grabbedGameObject);
                }
            }
        }
    }
}