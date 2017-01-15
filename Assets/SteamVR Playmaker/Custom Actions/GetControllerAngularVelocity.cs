using UnityEngine;


namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("SteamVR")]
    [Tooltip("Receive the Angular Velocity of the controller.")]
    public class GetControllerAngularVelocity : FsmStateAction
    {
        [CheckForComponent(typeof(Rigidbody))]
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
                DoGetangularVelocity();
            }
        }
        void DoGetangularVelocity()
        {
            device = SteamVR_Controller.Input((int)trackedObj.index);
            var go = Fsm.GetOwnerDefaultTarget(controller);
            var rigidbody = go.GetComponent<Rigidbody>();


            var angularVelocity = rigidbody.angularVelocity;
            rigidbody.angularVelocity = device.angularVelocity;
            if (space == Space.Self)
            {
                angularVelocity = go.transform.InverseTransformDirection(angularVelocity);
            }

            vector.Value = angularVelocity;
            x.Value = angularVelocity.x;
            y.Value = angularVelocity.y;
            z.Value = angularVelocity.z;
        }
    }
}