/*
Use touch type on the grip to have the bool store result stay true when held.
*/
using UnityEngine;


namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("SteamVR")]
    [Tooltip("Sends an Event when a Grip button is Pressed.")]
    public class GetGrip : FsmStateAction
    {
        [Tooltip("Controller set to Trigger.")]
        [CheckForComponent(typeof(SteamVR_TrackedObject))]
        public FsmOwnerDefault controller;

        SteamVR_Controller.Device device;
        SteamVR_TrackedObject trackedObj;

        [Tooltip("Select touch or Press for trigger type.")]
        public setGripType gripType;

        public enum setGripType
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
                switch (gripType)
                {
                    case setGripType.getPress:
                        var padDown = device.GetTouchUp(SteamVR_Controller.ButtonMask.Grip);
                        if (padDown)
                        {
                            Fsm.Event(sendEvent);
                        }
                        storeResult.Value = padDown;
                        break;
                    case setGripType.getPressUp:
                        var padDownUp = device.GetPressUp(SteamVR_Controller.ButtonMask.Grip);

                        if (padDownUp)
                        {
                            Fsm.Event(sendEvent);
                        }
                        storeResult.Value = padDownUp;
                        break;
                    case setGripType.getPressDown:
                        var padDownDown = device.GetPressDown(SteamVR_Controller.ButtonMask.Grip);

                        if (padDownDown)
                        {
                            Fsm.Event(sendEvent);
                        }
                        storeResult.Value = padDownDown;
                        break;
                    case setGripType.getTouch:
                        var padTouch = device.GetTouch(SteamVR_Controller.ButtonMask.Grip);

                        if (padTouch)
                        {
                            Fsm.Event(sendEvent);
                        }
                        storeResult.Value = padTouch;
                        break;
                    case setGripType.getTouchUp:
                        var padTouchUp = device.GetTouchUp(SteamVR_Controller.ButtonMask.Grip);

                        if (padTouchUp)
                        {
                            Fsm.Event(sendEvent);
                        }
                        storeResult.Value = padTouchUp;
                        break;
                    case setGripType.getTouchDown:
                        var padTouchDown = device.GetTouchDown(SteamVR_Controller.ButtonMask.Grip);

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