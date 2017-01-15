/* Kinematic physics
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 1.0.2
 * date: June 5, 2016
 *
 * - fixed first collision
 */


//#define DEBUG_FORCE
//#define DEBUG_TORQUE
//#define IMPULSE

using UnityEngine;

namespace KinematicPhysics {

    [RequireComponent(typeof(Rigidbody))]
    public class KinematicPhysics : MonoBehaviour {

        public Transform target;

        public int maxStrength = 100;
        public int maxAcceleration = 200;

        public bool continuousNK = false;

        [HideInInspector]
        private TransformPlus target2;
        [HideInInspector]
        private Rigidbody kinematicRigidbody;
        [HideInInspector]
        private bool hasCollided = false;

        public KinematicPhysics(Transform target) {
            this.target = target;
        }

        void Awake() {
            if (target != null) {
                Initialize(target);
            }
        }

        void FixedUpdate() {
            if (target2 != null) {
                Core.UpdateRigidbody(kinematicRigidbody, target2, maxStrength, maxAcceleration, hasCollided, continuousNK);
            } else {
                if (target != null) {
                    Initialize(target);
                }
            }
        }

        private void Initialize(Transform target) {
            target2 = new TransformPlus(target);
            if (enabled) {
                kinematicRigidbody = this.GetComponent<Rigidbody>();
                Core.Kinematize(kinematicRigidbody, continuousNK);
                kinematicRigidbody.maxAngularVelocity = 20;
            }
        }

        public void OnTriggerEnter(Collider collider) {
            bool otherHasKinematicPhysics = false;
            bool otherIsIVR = false;

            Rigidbody otherRigidbody = collider.attachedRigidbody;
            if (otherRigidbody != null) {
                KinematicPhysics kp = otherRigidbody.GetComponent<KinematicPhysics>();
                otherHasKinematicPhysics = (kp != null);
                IVR.InstantVR ivr = otherRigidbody.GetComponent<IVR.InstantVR>();
                otherIsIVR = (ivr != null);
            }

            if (kinematicRigidbody != null && kinematicRigidbody.isKinematic && (!collider.isTrigger || otherHasKinematicPhysics) && !otherIsIVR) {
                hasCollided = true;
                Core.SetKinematic(kinematicRigidbody, false);
                Core.ProcessFirstCollision(kinematicRigidbody, collider);
            }
        }

        public void OnCollisionExit(Collision collision) {
            if (kinematicRigidbody != null && !kinematicRigidbody.useGravity) {
                hasCollided = false;
            }
        }
    }


    class Core {
        public static void Kinematize(Rigidbody rigidbody, bool continuousNK) {
            if (rigidbody != null) {
                if (rigidbody.useGravity || continuousNK)
                    SetKinematic(rigidbody, false);
                else
                    SetKinematic(rigidbody, true);
            }
        }

        public static void Unkinematize(Rigidbody rigidbody) {
            SetKinematic(rigidbody, false);
        }

        public static void UpdateRigidbody(Rigidbody rigidbody, TransformPlus target, int maxStrength, int maxAcceleration, bool hasCollided, bool continuousNK) {
            if (rigidbody == null || target == null || target.transform == null)
                return;

            target.Update();

            bool hasJoint = false;
            if (rigidbody.GetComponentInChildren<Joint>() != null) {
                hasJoint = true;
                SetKinematic(rigidbody, false);
            }

            if (rigidbody != null) {
                Vector3 d = target.transform.position - rigidbody.position;

                Quaternion rot = Quaternion.Inverse(rigidbody.rotation) * target.transform.rotation;
                float angle;
                Vector3 axis;
                rot.ToAngleAxis(out angle, out axis);
                IVR.Angles.Normalize(angle);

                if (rigidbody.isKinematic == false) {
                    Vector3 torque = CalculateMyAngularAcceleration(rigidbody, target, maxAcceleration);
                    if (!float.IsNaN(torque.magnitude))
                        rigidbody.AddTorque(torque, ForceMode.Acceleration);

                    Vector3 force = CalculateMyForce(rigidbody, target, maxStrength, hasJoint);
                    if (!float.IsNaN(force.magnitude))
                        rigidbody.AddForce(force);
                } else {
                    rigidbody.MovePosition(rigidbody.position + d);
                    if (angle != 0)
                        rigidbody.MoveRotation(rigidbody.rotation * Quaternion.AngleAxis(angle, axis));
                }

                if (!rigidbody.isKinematic) {
                    if (!hasJoint && !hasCollided && !rigidbody.useGravity && !continuousNK) {
                        SetKinematic(rigidbody, true);
                    }
                }
            }
        }

        private static Vector3 CalculateMyForce(Rigidbody rigidbody, TransformPlus target, int maxStrength, bool hasJoint) {
            // calculate the acceleration needed to reach the target with a speed difference 0
#if DEBUG_FORCE
            Vector3 momentum = rigidbody.mass * rigidbody.velocity;
            Debug.DrawRay(rigidbody.position, momentum, Color.yellow);
#endif

            Vector3 locationDifference = target.transform.position - rigidbody.position;
            //Debug.DrawRay(rigidbody.position, locationDifference, Color.magenta);

            Vector3 speedDifference = target.velocity - rigidbody.velocity;

            Vector3 speedDifferenceP = Vector3.Project(speedDifference, locationDifference);
            //Debug.DrawRay(rigidbody.position, speedDifferenceP / 10, Color.cyan);
            Vector3 speedDifferenceR = speedDifference - speedDifferenceP;
            //Debug.DrawRay(rigidbody.position, speedDifferenceR / 10, Color.blue);

            Vector3 force = speedDifferenceR * rigidbody.mass * (maxStrength / 2);
            //Debug.DrawRay(rigidbody.position, force / 10, Color.white);

            float time2momentum0 = 0;                                              // time it takes to reach a momentum of 0 towards target
            if (locationDifference.magnitude > 0) {
                float momentumDifferenceP_magnitude = (speedDifferenceP * rigidbody.mass).magnitude;
                time2momentum0 = momentumDifferenceP_magnitude / (locationDifference.magnitude * maxStrength);

                // average deceleration during time2momentum0
                float avgForce = locationDifference.magnitude * (maxStrength / 2);

                // distance it takes to reach a speed difference of 0
                float distance2speed0 = 0.5f * -avgForce * (time2momentum0 * time2momentum0) + momentumDifferenceP_magnitude * time2momentum0;

                // if the distance to target is bigger than the distance to reach speed = 0,
                // we can still accelerate towards the goal otherwise we need to decelerate
                if (distance2speed0 < locationDifference.magnitude) {
                    force += locationDifference * maxStrength;
#if DEBUG_FORCE
                Debug.DrawRay(rigidbody.position, force / 10, Color.green);
#endif
                } else if (time2momentum0 > 0) {
                    force += (0.5f * speedDifferenceP.magnitude * speedDifferenceP.magnitude / locationDifference.magnitude) * speedDifferenceP.normalized;
#if DEBUG_FORCE
                Debug.DrawRay(rigidbody.position, force / 10, Color.red);
#endif
                }
            }

            // force added because of acceleration of the target
            force += rigidbody.mass * target.acceleration;

            return force;
        }

        private static Vector3 CalculateMyAngularAcceleration(Rigidbody rigidbody, TransformPlus target, int maxAcceleration) {
            Quaternion dRot = target.transform.rotation * Quaternion.Inverse(rigidbody.rotation);

#if DEBUG_TORQUE
        Debug.DrawRay(rigidbody.position, rigidbody.angularVelocity, Color.yellow);
#endif

            float angle;
            Vector3 axis;
            dRot.ToAngleAxis(out angle, out axis);
            IVR.Angles.Normalize(angle);

            Vector3 angleDifference = axis.normalized * (angle * Mathf.Deg2Rad);
            Vector3 speedDifference = target.angularVelocity - rigidbody.angularVelocity;

            Vector3 speedDifferenceP = Vector3.Project(speedDifference, angleDifference);
            Vector3 speedDifferenceR = speedDifference - speedDifferenceP;

            Vector3 acceleration = -speedDifferenceR;

            float time2speed0 = 0;                                             // time (in seconds) it takes to reach speed = 0 towards target
            if (angleDifference.magnitude > 0) {
                float speedDifferenceP_magnitude = speedDifferenceP.magnitude;
                time2speed0 = speedDifferenceP_magnitude / (angleDifference.magnitude * maxAcceleration);

                // average acceleration during time2speed0
                float avgAcceleration = angleDifference.magnitude * (maxAcceleration / 2);
                // angle it takes to reach a speed difference of 0
                float angle2speed0 = 0.5f * -avgAcceleration * (time2speed0 * time2speed0) + speedDifferenceP_magnitude * time2speed0;

                // if the angle difference is bigger than the angle to reach speed == 0
                // we can still accelerate towards the goal otherwise we need to decelerate
                if (angle2speed0 < angleDifference.magnitude) {
                    //acceleration += d.normalized * d.magnitude * maxAcceleration;
                    acceleration += angleDifference * maxAcceleration;
#if DEBUG_TORQUE
		    	Debug.DrawRay(rigidbody.position, acceleration / 10, Color.green);
#endif
                } else if (time2speed0 > 0) {
                    acceleration += (0.5f * speedDifferenceP.magnitude * speedDifferenceP.magnitude / angleDifference.magnitude) * speedDifferenceP.normalized;
#if DEBUG_TORQUE
                Debug.DrawRay(rigidbody.position, acceleration / 10, Color.red);
#endif
                }
            }

            // acceleration added because of acceleration of the target
            acceleration += rigidbody.mass * target.angularAcceleration;

            return acceleration;
        }

        public static void ProcessFirstCollision(Rigidbody rigidbody, Collider otherCollider) {
            
            Vector3 lastTranslation = rigidbody.velocity * Time.deltaTime;
            Vector3 sweepDirection = lastTranslation.normalized;
            float sweepDistance = lastTranslation.magnitude;

            rigidbody.transform.Translate(-lastTranslation);

            RaycastHit[] hits = rigidbody.SweepTestAll(sweepDirection, sweepDistance);
            float minDistance = float.MaxValue;
#if IMPULSE
        Rigidbody otherRigidbody = null;
		Vector3 collisionPoint = Vector3.zero;
#endif
            for (int i = 0; i < hits.Length; i++) {
                if (hits[i].distance < minDistance) {
                    minDistance = hits[i].distance;
#if IMPULSE
                otherRigidbody = hits[i].rigidbody;
				collisionPoint = hits[i].point;
#endif
                }
            }
            
            if (minDistance < float.MaxValue) {
                rigidbody.transform.Translate(sweepDirection * minDistance * 1.1F);
            }
            
#if IMPULSE
		CalculateCollisionImpuls(rigidbody, otherRigidbody, collisionPoint);
#endif
        }

#if IMPULSE
	private static void CalculateCollisionImpuls(Rigidbody rigidbody, Rigidbody otherRigidbody, Vector3 collisionPoint) {
		if (otherRigidbody != null) {
			Vector3 myImpuls = (rigidbody.mass / 10) * rigidbody.velocity;
			otherRigidbody.AddForceAtPosition(myImpuls, collisionPoint, ForceMode.Impulse);
		}
	}
#endif

        public static void SetKinematic(Rigidbody rigidbody, bool b) {
            GameObject obj = rigidbody.gameObject;
            if (obj.isStatic == false) {
                rigidbody.isKinematic = b;
                SetColliderToTrigger(obj, b);
            }
        }

        private static void SetColliderToTrigger(GameObject obj, bool b) {
            Collider[] colliders = obj.GetComponentsInChildren<Collider>();
            for (int j = 0; j < colliders.Length; j++)
                colliders[j].isTrigger = b;
        }
    }

    class TransformPlus {
        public Transform transform;

        public Vector3 velocity = Vector3.zero;
        public Vector3 acceleration = Vector3.zero;
        public Vector3 angularVelocity = Vector3.zero;
        public Vector3 angularAcceleration = Vector3.zero;

        public TransformPlus(Transform transform) {
            this.transform = transform;
        }

        private Vector3 lastPosition = Vector3.zero;
        private Vector3 lastVelocity = Vector3.zero;
        private Quaternion lastRotation = Quaternion.identity;
        private Quaternion lastRotationalVelocity = Quaternion.identity;

        private bool velocityOK = false;
        private bool accelerationOK = false;

        public void Update() {
            if (transform == null)
                return;

            if (velocityOK) {
                velocity = (transform.position - lastPosition) / Time.fixedDeltaTime;

                float angle;
                Vector3 axis;
                Quaternion rotationalVelocity = transform.rotation * Quaternion.Inverse(lastRotation);
                rotationalVelocity.ToAngleAxis(out angle, out axis);
                IVR.Angles.Normalize(angle);
                angularVelocity = (angle * Mathf.Deg2Rad * axis.normalized) / Time.fixedDeltaTime;

                if (accelerationOK) {
                    acceleration = (velocity - lastVelocity) / Time.fixedDeltaTime;

                    Quaternion rotationalAcceleration = rotationalVelocity * Quaternion.Inverse(lastRotationalVelocity);
                    rotationalAcceleration.ToAngleAxis(out angle, out axis);
                    IVR.Angles.Normalize(angle);
                    angularAcceleration = (angle * Mathf.Deg2Rad * axis.normalized) / Time.fixedDeltaTime;
                }
                lastVelocity = velocity;
                lastRotationalVelocity = rotationalVelocity;

                accelerationOK = true;
            }
            lastPosition = transform.position;
            lastRotation = transform.rotation;

            velocityOK = true;
        } 
    }
}