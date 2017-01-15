/*
Select button type for menu selection.
*/
using UnityEngine;


namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("SteamVR")]
    [Tooltip("Sends an Event when a Menu button is Pressed.")]
    public class GetMenu : FsmStateAction
    {
        [Tooltip("Controller set to Trigger.")]
        [CheckForComponent(typeof(SteamVR_TrackedObject))]
        public FsmOwnerDefault controller;

        SteamVR_Controller.Device device;
        SteamVR_TrackedObject trackedObj;

        [Tooltip("Select touch or Press for trigger type.")]
        public setMenuType menuType;

        public enum setMenuType
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
                switch (menuType)
                {
                    case setMenuType.getPress:
                        var padDown = device.GetTouchUp(SteamVR_Controller.ButtonMask.ApplicationMenu);
                        if (padDown)
                        {
                            Fsm.Event(sendEvent);
                        }
                        storeResult.Value = padDown;
                        break;
                    case setMenuType.getPressUp:
                        var padDownUp = device.GetPressUp(SteamVR_Controller.ButtonMask.ApplicationMenu);

                        if (padDownUp)
                        {
                            Fsm.Event(sendEvent);
                        }
                        storeResult.Value = padDownUp;
                        break;
                    case setMenuType.getPressDown:
                        var padDownDown = device.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu);

                        if (padDownDown)
                        {
                            Fsm.Event(sendEvent);
                        }
                        storeResult.Value = padDownDown;
                        break;
                    case setMenuType.getTouch:
                        var padTouch = device.GetTouch(SteamVR_Controller.ButtonMask.ApplicationMenu);

                        if (padTouch)
                        {
                            Fsm.Event(sendEvent);
                        }
                        storeResult.Value = padTouch;
                        break;
                    case setMenuType.getTouchUp:
                        var padTouchUp = device.GetTouchUp(SteamVR_Controller.ButtonMask.ApplicationMenu);

                        if (padTouchUp)
                        {
                            Fsm.Event(sendEvent);
                        }
                        storeResult.Value = padTouchUp;
                        break;
                    case setMenuType.getTouchDown:
                        var padTouchDown = device.GetTouchDown(SteamVR_Controller.ButtonMask.ApplicationMenu);

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