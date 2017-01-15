/*
Use touch when touching the actual touchpad to trigger an event, and use press
to trigger the touchpad event as a button.
*/
using UnityEngine;


namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("SteamVR")]
    [Tooltip("Sends an Event when a Touchpad button is Pressed.")]
    public class GetTouchpad : FsmStateAction
    {
        [Tooltip("Controller set to Trigger.")]
        [CheckForComponent(typeof(SteamVR_TrackedObject))]
        public FsmOwnerDefault controller;

        SteamVR_Controller.Device device;
        SteamVR_TrackedObject trackedObj;

        [Tooltip("Select touch or Press for trigger type.")]
        public setTouchpadType touchpadType;

        public enum setTouchpadType
        {
            getPress,
            getPressDown,
            getPressUp,
            getTouch,
            getTouchDown,
            getTouchUp,
        };

        [Tooltip("Event to send if the button is pressed.")]
        public FsmEvent sendEvent;

        [Tooltip("Set to True if the button is pressed.")]
        [UIHint(UIHint.Variable)]
        public FsmBool storeResult;



        public override void Reset()
        {
            sendEvent = null;
            storeResult = null;
        }
        public override void OnEnter()
        {
            GameObject go = Fsm.GetOwnerDefaultTarget(controller);
            trackedObj = go.GetComponent<SteamVR_TrackedObject>();

        }
        public override void OnUpdate()
        {
            var i = -1;
            if ((int)trackedObj.index > i++)
            {
                device = SteamVR_Controller.Input((int)trackedObj.index);
                switch (touchpadType)
                {
                    case setTouchpadType.getPress:
                        var padDown = device.GetPress(SteamVR_Controller.ButtonMask.Touchpad);
                        if (padDown)
                        {
                            Fsm.Event(sendEvent);
                        }
                        storeResult.Value = padDown;
                        break;
                    case setTouchpadType.getPressUp:
                        var padDownUp = device.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad);

                        if (padDownUp)
                        {
                            Fsm.Event(sendEvent);
                        }
                        storeResult.Value = padDownUp;
                        break;
                    case setTouchpadType.getPressDown:
                        var padDownDown = device.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad);

                        if (padDownDown)
                        {
                            Fsm.Event(sendEvent);
                        }
                        storeResult.Value = padDownDown;
                        break;
                    case setTouchpadType.getTouch:
                        var padTouch = device.GetTouch(SteamVR_Controller.ButtonMask.Touchpad);

                        if (padTouch)
                        {
                            Fsm.Event(sendEvent);
                        }
                        storeResult.Value = padTouch;
                        break;
                    case setTouchpadType.getTouchUp:
                        var padTouchUp = device.GetTouchUp(SteamVR_Controller.ButtonMask.Touchpad);

                        if (padTouchUp)
                        {
                            Fsm.Event(sendEvent);
                        }
                        storeResult.Value = padTouchUp;
                        break;
                    case setTouchpadType.getTouchDown:
                        var padTouchDown = device.GetTouchDown(SteamVR_Controller.ButtonMask.Touchpad);

                        if (padTouchDown)
                        {
                            Fsm.Event(sendEvent);
                        }
                        storeResult.Value = padTouchDown;
                        break;
                }
            }
        }
    }
}