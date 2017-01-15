using UnityEngine;
using System.Collections;

public class testForce : MonoBehaviour {
    public bool on = false;
    public Vector3 linearForce = Vector3.zero;

    private Rigidbody rb;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();
        rb.AddForce(linearForce, ForceMode.VelocityChange);
    }

    // Update is called once per frame
    void Update () {
	
	}
}
