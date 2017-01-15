using UnityEngine;
using System.Collections;

public class TargetBehavior : MonoBehaviour {

    private const float GLOW_TIME = 0.200f;
    private const float PROPAGATE_TIME = 0.050f;

    //create array of materials to find material
    public Material[] hitMaterial;
    private Renderer rend;
    private TargetBehavior[] neighbor;
    private float lastNotifiedTime;
    private Material originalMaterial;
    private float lastColorChangeTime;
    private Collider[] colliderArray;

    // Use this for initialization
    void Start()
    {
        //check if materials exist
        if (hitMaterial.Length == 0)
            return;

        //get material of this object
        rend = GetComponent<Renderer>();
        originalMaterial = rend.material;
        //enable material
        rend.enabled = true;

        //if neighbors has no tiles or if the number of neighbors is less than 8, then Find All Neighbors
        if (neighbor == null || neighbor.Length < 8)
        {
            FindAllNeighbors();
        }

    }

    private void FindAllNeighbors()
    {
        //get all gameobjects within one unit from this tile
        colliderArray = Physics.OverlapSphere(transform.position, 1.0f);

        //Check size of array and log if null
        if (colliderArray.Length < 1)
        {
            Debug.LogError("OverlapSphere didn't find anything");
            neighbor = null;
            return;
        }

        neighbor = null;
        neighbor = new TargetBehavior[8];
        int n = 0;

        foreach(Collider c in colliderArray)
        {
            //if the found object is not null, and is a tile, and is not us, then add its targetBehavior component to the neighbor array
            TargetBehavior t = c.gameObject.GetComponent<TargetBehavior>();
            if (t != null && c.gameObject.tag == "Tile" && c.gameObject != this.gameObject)
            {
                if (n < 8)
                {
                    neighbor[n] = t;
                    n++;
                }
                else
                {
                    //Debug.LogError("Oh shit we have too many neighbors!");
                }
            }
        }

        if (n != 8)
        {

            TargetBehavior[] tmp = new TargetBehavior[n];

            for (int i = 0; i < n; i++)
            {
                tmp[i] = neighbor[i];
            }

            neighbor = null;
            neighbor = tmp;

            //Debug.LogError("Oh shit we only have "+ n + "neighbors");
        }
    }

    void OnCollisionEnter (Collision other)
    {
        if(other.gameObject.name == "volleyBall")
        {
            //Debug.Log("hit using OnCollisionEnter");
            ChangeColor(0);
        }
    }

    void ChangeColor(int distance)
    {
        //if we have not already been notified, then change color
        //this takes care of hitting distant neighbors only once
        if (Time.timeSinceLevelLoad - lastNotifiedTime > 1.0f)
        {
            //last time we were notified
            lastNotifiedTime = Time.timeSinceLevelLoad;
            //change the sender color
            rend.material = hitMaterial[0];
            lastColorChangeTime = lastNotifiedTime;

            //notify the neighbors of the color change
            if (distance <4 && neighbor.Length >0)
            {
                foreach (TargetBehavior t in neighbor)
                {
                    t.NotifyNeighbor(this, lastNotifiedTime, distance + 1);
                }
            }
        }   
    }

    void NotifyNeighbor(TargetBehavior sender, float time, int distance)
    {
        StartCoroutine(ColorChangeCoroutine(sender, time, distance));
    }

    void Update()
    {
        //if my color was different for more than 2 seconds, restore to original
        if (Time.timeSinceLevelLoad - lastColorChangeTime > GLOW_TIME && lastColorChangeTime > 0)
        {
            lastColorChangeTime = 0;
            //change color back
            rend.material = originalMaterial;
        }
    }

    IEnumerator ColorChangeCoroutine(TargetBehavior sender, float time, int distance)
    {
        //after 200ms change color of me and my neighbors
        yield return new WaitForSeconds(PROPAGATE_TIME);
        ChangeColor(distance);
    }
}
