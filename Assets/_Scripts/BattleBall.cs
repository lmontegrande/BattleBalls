using UnityEngine;
using System.Collections;
using System;

public class BattleBall : Photon.MonoBehaviour, IPunObservable {

    private Rigidbody _rigidbody;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    [PunRPC]
    void RPCTest()
    {
        Vector3 randomVectorForce = new Vector3(UnityEngine.Random.Range(-10, 10), UnityEngine.Random.Range(-10, 10), UnityEngine.Random.Range(-10, 10));
        _rigidbody.AddForce(randomVectorForce);
    }

    void OnCollisionEnter(Collision other)
    {
        //if (!other.gameObject.GetComponent<PhotonView>().isMine) return;
        //CollisionHandle();
        
    }

    public void CollisionHandle(Collision other, Vector3 directionVector, float forceModifier)
    {
        Vector3 vel = _rigidbody.velocity * forceModifier;
        // Vector3 vel = other.relativeVelocity;
        // Vector3 vel = _rigidbody.velocity + (forceModifier * directionVector);
        Vector3 pos = transform.position;

        GetComponent<PhotonView>().RPC(
            "SynchronizeCollision",
            PhotonTargets.All,
            new System.Object[] { vel.x, vel.y, vel.z, pos.x, pos.y, pos.z });
    }

    [PunRPC]
    public void SynchronizeCollision(float x_vel, float y_vel, float z_vel, float x_pos, float y_pos, float z_pos)
    {
        Vector3 ballVelocity = new Vector3(x_vel, y_vel, z_vel);
        Vector3 ballPosition = new Vector3(x_pos, y_pos, z_pos);

        Debug.Log("ball velocity: " + ballVelocity);
        Debug.Log("ball position: " + ballPosition);
        _rigidbody.velocity = ballVelocity;
        transform.position = ballPosition;
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (true) return;

        if (stream.isWriting)
        {
            //Debug.Log("Host: " + _rigidbody.velocity);
            stream.SendNext(transform.position);
            stream.SendNext(_rigidbody.velocity);
        } else {
            //Debug.Log("Guest: " + _rigidbody.velocity);
            transform.position = (Vector3)stream.ReceiveNext();
            _rigidbody.velocity = (Vector3) stream.ReceiveNext();
        }      
    }
}
