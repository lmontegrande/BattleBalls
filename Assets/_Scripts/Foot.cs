using UnityEngine;
using System.Collections;

public class Foot : MonoBehaviour {

    public GameObject controller;
    public GameObject ball;
    public float recallBallHeightOffset = 1f;

    private Rigidbody rigidbody;
    private int controllerIndex;
	
    public void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        controllerIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost);
    }

	// Update is called once per frame
	public void Update () {
        transform.rotation = controller.transform.rotation;
        rigidbody.MovePosition(controller.transform.position);

        if (SteamVR_Controller.Input(controllerIndex).GetHairTriggerDown())
        {
            ball.transform.position = gameObject.transform.position + new Vector3(0, recallBallHeightOffset, 0);
            ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
        if (SteamVR_Controller.Input(controllerIndex).GetHairTrigger())
        {
            ball.transform.position = new Vector3(gameObject.transform.position.x, ball.transform.position.y, gameObject.transform.position.z);
            ball.GetComponent<Rigidbody>().velocity = new Vector3(0, ball.GetComponent<Rigidbody>().velocity.y, 0);
        }
	}
}
