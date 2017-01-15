using UnityEngine;
using System.Collections;

public class testMove : MonoBehaviour {

	public bool on = false;
	public Vector3 speed = Vector3.zero;
    public Vector3 acceleration = Vector3.zero;
    public Vector3 angularSpeed = Vector3.zero;
    public Vector3 angularAcceleration = Vector3.zero;

	private Rigidbody rb;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (on) {
            if (rb == null) {
                transform.Translate(speed * Time.deltaTime, Space.Self);
                transform.Rotate(angularSpeed * Time.deltaTime, Space.Self);
            } else {
                rb.MovePosition(transform.position + speed * Time.deltaTime);
                rb.MoveRotation(transform.rotation * Quaternion.Euler(angularSpeed * Time.deltaTime));
            }
            speed += acceleration * Time.deltaTime;
            angularSpeed += angularAcceleration * Time.deltaTime;
		}
	}
}
