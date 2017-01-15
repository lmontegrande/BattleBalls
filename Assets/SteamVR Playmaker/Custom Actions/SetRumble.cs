/*
Set Haptic Feedback for Controller.
*/
using UnityEngine;
using System.Collections;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("SteamVR")]
    [Tooltip("Sets the controller to Rumble for a duration of time.")]
    public class SetRumble : FsmStateAction
    {

        [Tooltip("Controller set to Trigger.")]
        [CheckForComponent(typeof(SteamVR_TrackedObject))]
        public FsmOwnerDefault controller;

        SteamVR_Controller.Device device;
        SteamVR_TrackedObject trackedObj;
        public FsmFloat duration = 1f;

        [HasFloatSlider(0, 1)]
        public FsmFloat strength = 1f;

        [Tooltip("Event to send once duration is complete.")]
        public FsmEvent sendEvent;

        public override void Reset()
        {
            duration = null;
            strength = null;
            sendEvent = null;
        }
        public override void OnEnter()
        {
            GameObject go = Fsm.GetOwnerDefaultTarget(controller);
            trackedObj = go.GetComponent<SteamVR_TrackedObject>();
            StartCoroutine(DoStartCoroutine());
        }



        IEnumerator DoStartCoroutine()
        {
            var i = -1;
            if ((int)trackedObj.index > i++)
            {
                device = SteamVR_Controller.Input((int)trackedObj.index);
                strength.Value = Mathf.Clamp01(strength.Value);
                float startTime = FsmTime.RealtimeSinceStartup;

                while (FsmTime.RealtimeSinceStartup - startTime <= duration.Value)
                {
                    int valveStrength = Mathf.RoundToInt(Mathf.Lerp(0, 3999, strength.Value));

                    device.TriggerHapticPulse((ushort)valveStrength);
                    yield return null;
                }
            }
            var curTime = FsmTime.RealtimeSinceStartup - duration.Value;
            if (curTime > duration.Value)
            {
                Fsm.Event(sendEvent);
            }
        }
    }
}