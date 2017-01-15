using UnityEngine;
using System.Collections;

public class BallPlayer : Photon.MonoBehaviour {

    public float forceModifier = 2f;

    public void Start()
    {
        if (photonView.isMine)
        {
            transform.position = GameManager.instance.spawns[PhotonNetwork.countOfPlayers % 2].transform.position;
            transform.rotation = GameManager.instance.spawns[PhotonNetwork.countOfPlayers % 2].transform.rotation;

            SetUpChildren(transform);
        }
    }

    public void SetUpChildren(Transform t)
    {
        foreach (Transform childTransform in t)
        {
            if (childTransform.gameObject.GetComponent<Collider>() != null)
            {
                ChildClass childClass = childTransform.gameObject.AddComponent<ChildClass>();
                Rigidbody rigidBody = childTransform.gameObject.AddComponent<Rigidbody>();
                rigidBody.useGravity = false;
                rigidBody.isKinematic = true;
                childClass.TouchDelegate = OnCollisionChild;
            }
            SetUpChildren(childTransform);
        }
    }

    public void OnCollisionChild(Collision other)
    { 
        BattleBall ball = GameObject.FindWithTag("Ball").GetComponent<BattleBall>();
        ball.CollisionHandle(other, new Vector3(1f, 0f, 0f), forceModifier);
    }

    public class ChildClass : MonoBehaviour
    {
        public delegate void OnTouchedDelegate(Collision other);

        public OnTouchedDelegate TouchDelegate;

        public void OnCollisionExit(Collision other)
        {
            if (other.gameObject.tag != "Ball") return;

            Debug.Log(other.relativeVelocity);
            TouchDelegate.Invoke(other);
        }
    }
}