using UnityEngine;

public class IVR_KinematicPhysics : IVR_Kinematics {
    public float strength = 100;

    public bool continuousNK = false;

    public override void Kinematize(Rigidbody rigidbody) {
        KinematicPhysics.KinematicPhysics kp = rigidbody.gameObject.GetComponent<KinematicPhysics.KinematicPhysics>();
        if (kp == null) {
            kp = rigidbody.gameObject.AddComponent<KinematicPhysics.KinematicPhysics>();
        }

        kp.target = this.target;
        kp.maxStrength = (int) this.strength;
        kp.maxAcceleration = (int) this.strength;
        kp.continuousNK = continuousNK;
    }

    public override void Join(GameObject obj, Transform handPalm) {
    }

    public override void UpdateJoinedObject(Transform target) {
    }
}
