using UnityEngine;
using System.Collections;

public class TargetBehavior : MonoBehaviour {

    //create array of materials to find material
    public Material[] materials;
    private Renderer rend;
    public GameObject[] neighbor;

    // Use this for initialization
    void Start()
    {
        //check if materials exist
        if (materials.Length == 0)
            return;

        //get material of this object
        rend = GetComponent<Renderer>();
        //enable material
        rend.enabled = true;
    }

    void OnCollisionEnter (Collision other)
    {
        if(other.gameObject.name == "volleyBall")
        {
            //Debug.Log("hit using OnCollisionEnter");
            rend.sharedMaterial = materials[0];
        }
    }

    void SendEventToNeighbors()
    {

    }

    void ReceiveFromNeighbors()
    {
    }


    void Update()
    {

    }
}
