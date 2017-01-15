using UnityEngine;
using IVR;

// Example implementation to handle input.
// Change this to your own needs!

public class InputHandler : MonoBehaviour {
    public enum WalkTypes {
        SmoothWalking,
        Teleport
    }
    public bool walking = true;
    public WalkTypes walkingType = WalkTypes.SmoothWalking;
    public bool sidestepping = true;
    public bool rotation = false;

    private InstantVR character;
    private ControllerInput controller0;

#if INSTANTVR_ADVANCED
    private IVR_HandMovements leftHandMovements;
    private IVR_HandMovements rightHandMovements;

    public bool fingerMovements = false;
#endif

    private float avatarNeckHeight;

    void Start () {
        character = GetComponent<InstantVR>();

        Animator avatar = character.characterTransform.GetComponent<Animator>();
        Transform avatarNeck = avatar.GetBoneTransform(HumanBodyBones.Neck);
        avatarNeckHeight = avatarNeck.position.y - character.transform.position.y;

#if INSTANTVR_ADVANCED
        leftHandMovements = character.leftHandTarget.GetComponent<IVR_HandMovements>();
        rightHandMovements = character.rightHandTarget.GetComponent<IVR_HandMovements>();
#endif

        // get the first player's controller
        controller0 = Controllers.GetController(0);

        // register for button down events
        controller0.right.OnButtonDownEvent += OnButtonDown;
	}

    // this function is called when a button has been pressed
    void OnButtonDown(int buttonID) {
        if (buttonID == ControllerInput.ButtonOne && walkingType == WalkTypes.Teleport) {
            // When button One (A on Xbox controller) is pressed we teleport in the looking direction;
            Teleport(character.headTarget.transform.forward * 50 * Time.deltaTime);
        }
    }

    void Update() {
        if (walkingType == WalkTypes.SmoothWalking) {
            // move the character using the left analog stick
            float horizontal = 0;
            float vertical = 0;

            // forward/backward walking using the left analog stick up/down
            if (walking)
                vertical = controller0.left.stickVertical;
            
            // left/right sidestepping using the left analog stick left/right
            if (sidestepping)
                horizontal = controller0.left.stickHorizontal;

            // now move the character
            character.Move(horizontal, 0, vertical);

            if (rotation) {
                // rotate the character using the right analog stick left/right
                horizontal = controller0.left.stickHorizontal * 5;
                character.Rotate(horizontal);
            }
        }
        // calibrate tracking when both left & right option buttons are pressed
        if ((controller0.left.option && controller0.right.option) || Input.GetKeyDown(KeyCode.Tab))
            character.Calibrate();

#if INSTANTVR_ADVANCED
        if (fingerMovements) {
            if (leftHandMovements) {
                leftHandMovements.thumbCurl = Mathf.Max(controller0.left.trigger, controller0.left.bumper);
                leftHandMovements.thumbCurl = controller0.left.stickTouch ? leftHandMovements.thumbCurl : -0.5F;
                leftHandMovements.indexCurl = controller0.left.bumper;
                leftHandMovements.middleCurl = Mathf.Max(controller0.left.trigger, controller0.left.bumper);
                leftHandMovements.ringCurl = Mathf.Max(controller0.left.trigger, controller0.left.bumper);
                leftHandMovements.littleCurl = Mathf.Max(controller0.left.trigger, controller0.left.bumper);
            }

            if (rightHandMovements) {
                rightHandMovements.thumbCurl = Mathf.Max(controller0.right.trigger, controller0.right.bumper);
                rightHandMovements.thumbCurl = controller0.right.stickTouch ? rightHandMovements.thumbCurl : -0.5F;
                rightHandMovements.indexCurl = controller0.right.bumper;
                rightHandMovements.middleCurl = Mathf.Max(controller0.right.trigger, controller0.right.bumper);
                rightHandMovements.ringCurl = Mathf.Max(controller0.right.trigger, controller0.right.bumper);
                rightHandMovements.littleCurl = Mathf.Max(controller0.right.trigger, controller0.right.bumper);
            }
        }
#endif        
    }
    
    private void Teleport(Vector3 translation) {
        // raycast ensures (a little) that we do not teleport into objects
        RaycastHit hit;
        if (Physics.Raycast(character.transform.position + translation + Vector3.up, Vector3.down, out hit, 1.5F)) {
            translation += Vector3.up * ((hit.point.y - character.transform.position.y) + 0.05f);
        }
        character.transform.position += translation;
    }

    protected void CheckQuit() {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    void OnNewNeckMeasurement(float neckHeight) {
        Debug.Log("Neck Height:" + neckHeight + " Avatar Height:" + avatarNeckHeight);
        float deltaY = avatarNeckHeight - neckHeight;
        IVR_UnityVRHead unityVRhead = character.headTarget.GetComponent<IVR_UnityVRHead>();
        unityVRhead.cameraRoot.transform.Translate(0, deltaY, 0);
        //character.Calibrate();
    }
}
