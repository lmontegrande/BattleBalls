using UnityEngine;
using System.Collections;

public class MoveUpAndDown : MonoBehaviour {

    private Transform titleTransform;

	// Use this for initialization
	void Start ()
    {
        titleTransform = GetComponent<Transform>();
        Debug.Log("My transform is " + titleTransform.position);

    }
	
	// Update is called once per frame
	void Update () {
        transform.position = new Vector3(transform.position.x, Mathf.SmoothStep(-0.8f, -0.7f, Mathf.PingPong(Time.time / 3, 1)), transform.position.z);

    }
}
