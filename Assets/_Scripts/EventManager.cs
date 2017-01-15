using UnityEngine;
using System.Collections;

public class EventManager : MonoBehaviour {

    public delegate void ChangeColor();
    public static event ChangeColor OnCollided;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
