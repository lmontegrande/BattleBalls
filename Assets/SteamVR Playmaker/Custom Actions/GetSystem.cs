/*
Select event for system selection.
*/
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("SteamVR")]
    [Tooltip("Sends an Event when a System button is Pressed.")]
    public class GetSystem : FsmStateAction
    {
        [Tooltip("Event to send if the system button is pressed.")]
        public FsmEvent sendEvent;

        [Tooltip("Check is system menu has been enabled.")]
        [UIHint(UIHint.Variable)]
        public FsmBool systemEnabled;

        public override void Reset()
        {
            sendEvent = null;
            systemEnabled = null;
        }
        public override void OnEnter()
        {
            SteamVR_Utils.Event.Listen("input_focus", OnInputFocus);
        }

        private void OnInputFocus(params object[] args)
	    {
		    bool hasFocus = (bool)args[0];
		    if (!hasFocus)
		    {
                Fsm.Event(sendEvent);
                systemEnabled.Value = !hasFocus;
            }
            else
            {
                Fsm.Event(sendEvent);
                systemEnabled.Value = !hasFocus;
            }
        }
    }
}