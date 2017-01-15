/*
Get the Vector points on the axis of the controllers touchpad.
*/
using UnityEngine;


namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("SteamVR")]
    [Tooltip("Get Touchpad vector axis.")]
    public class GetTouchpadAxis : FsmStateAction
    {
        [Tooltip("Controller touchpad to receive axis information.")]
        [CheckForComponent(typeof(SteamVR_TrackedObject))]
        public FsmOwnerDefault controller;

        SteamVR_Controller.Device device;
        SteamVR_TrackedObject trackedObj;

        [Tooltip("Multiplier to adjust axis range.")]
        public FsmFloat multiplier;

        [Tooltip("Set to True if the button is pressed.")]
        [UIHint(UIHint.Variable)]
        public FsmVector2 storeVector;

        public override void Reset()
        {
            multiplier = null;
            storeVector = null;
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

                var axisValue = device.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad);

                if (multiplier.Value > 0.0f)
                {
                    axisValue *= multiplier.Value;
                }

                storeVector.Value = axisValue;
            }
                   
            
        }
    }
}