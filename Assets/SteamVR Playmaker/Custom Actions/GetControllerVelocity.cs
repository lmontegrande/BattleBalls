using UnityEngine;


namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("SteamVR")]
    [Tooltip("Get the controller Velocity.")]
    public class GetControllerVelocity : FsmStateAction
    {
        [CheckForComponent(typeof(SteamVR_TrackedObject))]
        public FsmOwnerDefault controller;

        SteamVR_Controller.Device device;
        SteamVR_TrackedObject trackedObj;

        [UIHint(UIHint.Variable)]
        public FsmVector3 vector;

        [UIHint(UIHint.Variable)]
        public FsmFloat x;

        [UIHint(UIHint.Variable)]
        public FsmFloat y;

        [UIHint(UIHint.Variable)]
        public FsmFloat z;

        public Space space;

        public bool everyFrame;

        public override void Reset()
        {
            controller = null;
            vector = null;
            x = null;
            y = null;
            z = null;
            space = Space.World;
            everyFrame = false;
        }

        public override void OnEnter()
        {
           GameObject go = Fsm.GetOwnerDefaultTarget(controller);
           trackedObj = go.GetComponent<SteamVR_TrackedObject>();


            if (!everyFrame)
            {
                Finish();
            }

        }
        public override void OnUpdate()
        {
            var i = -1;
            if ((int)trackedObj.index > i++)
            {
                DoGetVelocity();
            }       
        }
        void DoGetVelocity()
        {
            device = SteamVR_Controller.Input((int)trackedObj.index);
            var go = Fsm.GetOwnerDefaultTarget(controller);
            var rigidbody = go.GetComponent<Rigidbody>();

            var velocity = rigidbody.velocity;
            rigidbody.velocity = device.velocity;
            if (space == Space.Self)
            {
                velocity = go.transform.InverseTransformDirection(velocity);
            }

            vector.Value = velocity;
            x.Value = velocity.x;
            y.Value = velocity.y;
            z.Value = velocity.z;
        }
    }
}