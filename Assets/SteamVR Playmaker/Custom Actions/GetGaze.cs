using UnityEngine;

public struct GazeEventArgsPlaymaker
{
    public float distance;
}

public delegate void GazeEventHandlerPlaymaker(object sender, GazeEventArgsPlaymaker e);

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("SteamVR")]
    [Tooltip("Sends an event based on the object being gazed.")]
    public class GetGaze : FsmStateAction
    {
        public FsmOwnerDefault gazeObject;

        SteamVR_TrackedObject trackedObj;

        public FsmBool isInGaze = false;
        public event GazeEventHandlerPlaymaker GazeOn;
        public event GazeEventHandlerPlaymaker GazeOff;
        public FsmFloat gazeInCutoff = 1f;
        public FsmFloat gazeOutCutoff = 1f;

        // Use this for initialization
        public override void OnEnter()
        {
            //trackedObj = headset.Value.GetComponent<SteamVR_TrackedObject>();
        }

        public virtual void OnGazeOn(GazeEventArgsPlaymaker e)
        {
            if (GazeOn != null)
                GazeOn(this, e);
        }

        public virtual void OnGazeOff(GazeEventArgsPlaymaker e)
        {
            if (GazeOff != null)
                GazeOff(this, e);
        }

        // Update is called once per frame
        public override void OnUpdate()
        {
            var vrcam = SteamVR_Render.Top();
            GameObject go = Fsm.GetOwnerDefaultTarget(gazeObject);


                Ray r = new Ray(vrcam.transform.position, vrcam.transform.forward);
                Plane p = new Plane(vrcam.transform.forward, go.transform.position);

                float enter = 0.0f;
                if (p.Raycast(r, out enter))
                {
                    Vector3 intersect = vrcam.transform.position + vrcam.transform.forward * enter;
                    float dist = Vector3.Distance(intersect, go.transform.position);
                    //Debug.Log("Gaze dist = " + dist);
                    if (dist < gazeInCutoff.Value && !isInGaze.Value)
                    {
                        isInGaze.Value = true;
                        GazeEventArgsPlaymaker e;
                        e.distance = dist;
                        OnGazeOn(e);
                    }
                    else if (dist >= gazeOutCutoff.Value && isInGaze.Value)
                    {
                        isInGaze.Value = false;
                        GazeEventArgsPlaymaker e;
                        e.distance = dist;
                        OnGazeOff(e);
                    }

                }

        }
    }
}