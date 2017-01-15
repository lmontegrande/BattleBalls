using UnityEngine;
using System.Collections;
using Valve.VR;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("SteamVR")]
    [Tooltip("Steam VR Teleporter using controller.")]
    public class SteamTeleporter : FsmStateAction
    {
        [Tooltip("Controller set to Trigger.")]
        [CheckForComponent(typeof(SteamVR_TrackedObject))]
        public FsmOwnerDefault controller;

        SteamVR_Controller.Device device;
        SteamVR_TrackedObject trackedObj;
        SteamVR_TrackedController trackedController;

        public enum TeleportType
        {
            TeleportTypeUseTerrain,
            TeleportTypeUseCollider,
            TeleportTypeUseZeroY
        }
        public TeleportType teleportType = TeleportType.TeleportTypeUseZeroY;


        [Tooltip("Event to send if teleported.")]
        public FsmEvent sendEvent;

        Transform reference
        {
            get
            {
                var top = SteamVR_Render.Top();
                return (top != null) ? top.origin : null;
            }
        }
        public override void Reset()
        {

            sendEvent = null;

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

                    var buttonDownDown = device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger);

                    if (buttonDownDown)
                    {
                            DoClick();
                    }

            }
        }

        //void DoClick(object sender, ClickedEventArgs e)
        void DoClick()
        {
            GameObject go = Fsm.GetOwnerDefaultTarget(controller);
            var i = -1;
            if ((int)trackedObj.index > i++)
            {
                    var t = reference;
                    if (t == null)
                        return;

                    float refY = t.position.y;

                    Plane plane = new Plane(Vector3.up, -refY);
                    Ray ray = new Ray(go.transform.position, go.transform.forward);

                    bool hasGroundTarget = false;
                    float dist = 0f;
                    if (teleportType == TeleportType.TeleportTypeUseTerrain)
                    {
                        RaycastHit hitInfo;
                        TerrainCollider tc = Terrain.activeTerrain.GetComponent<TerrainCollider>();
                        hasGroundTarget = tc.Raycast(ray, out hitInfo, 1000f);
                        dist = hitInfo.distance;

                    }
                    else if (teleportType == TeleportType.TeleportTypeUseCollider)
                    {
                        RaycastHit hitInfo;
                        Physics.Raycast(ray, out hitInfo);
                        dist = hitInfo.distance;

                    }
                    else
                    {
                        hasGroundTarget = plane.Raycast(ray, out dist);

                    }

                    if (hasGroundTarget)
                    {
                        Vector3 headPosOnGround = new Vector3(SteamVR_Render.Top().head.localPosition.x, 0.0f, SteamVR_Render.Top().head.localPosition.z);
                        t.position = ray.origin + ray.direction * dist - new Vector3(t.GetChild(0).localPosition.x, 0f, t.GetChild(0).localPosition.z) - headPosOnGround;

                        Fsm.Event(sendEvent);
                    }
            }
        } 
    }
}
    
