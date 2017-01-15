/*
Slightly little difference between touch and press. Touch is registered faster than
press down. Press up is registered before touch up.
*/
using UnityEngine;


namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("SteamVR")]
    [Tooltip("Sends an Event when a Trigger Button is pressed.")]
    public class GetTrigger : FsmStateAction
    {
        [Tooltip("Controller set to Trigger.")]
        [CheckForComponent(typeof(SteamVR_TrackedObject))]
        public FsmOwnerDefault controller;

        SteamVR_Controller.Device device;
        SteamVR_TrackedObject trackedObj;

        [Tooltip("Select touch or Press for trigger type.")]
        public setTriggerType triggerType;

        public enum setTriggerType
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
                switch (triggerType)
                {
                    case setTriggerType.getPress:
                        var buttonDown = device.GetPress(SteamVR_Controller.ButtonMask.Trigger);

                        if (buttonDown)
                        {
                            Fsm.Event(sendEvent);
                        }
                        storeResult.Value = buttonDown;
                        break;
                    case setTriggerType.getPressUp:
                        var buttonDownUp = device.GetPressUp(SteamVR_Controller.ButtonMask.Trigger);

                        if (buttonDownUp)
                        {
                            Fsm.Event(sendEvent);
                        }
                        storeResult.Value = buttonDownUp;
                        break;
                    case setTriggerType.getPressDown:
                        var buttonDownDown = device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger);

                        if (buttonDownDown)
                        {
                            Fsm.Event(sendEvent);
                        }
                        storeResult.Value = buttonDownDown;
                        break;
                    case setTriggerType.getTouch:
                        var buttonTouch = device.GetTouch(SteamVR_Controller.ButtonMask.Trigger);

                        if (buttonTouch)
                        {
                            Fsm.Event(sendEvent);
                        }
                        storeResult.Value = buttonTouch;
                        break;
                    case setTriggerType.getTouchUp:
                        var buttonTouchUp = device.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger);

                        if (buttonTouchUp)
                        {
                            Fsm.Event(sendEvent);
                        }
                        storeResult.Value = buttonTouchUp;
                        break;
                    case setTriggerType.getTouchDown:
                        var buttonTouchDown = device.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger);

                        if (buttonTouchDown)
                        {
                            Fsm.Event(sendEvent);
                        }
                        storeResult.Value = buttonTouchDown;
                        break;
                }
            }
            
        }
    }
}