using UnityEngine;
using System.Collections;

public class GameManager : Photon.PunBehaviour {

    public static GameManager instance;

    public GameObject[] spawns;

    public void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        spawns = GameObject.FindGameObjectsWithTag("Spawn");
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        Debug.Log("Player Connected");
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        Debug.Log("Player Disconnected");
    }
}
