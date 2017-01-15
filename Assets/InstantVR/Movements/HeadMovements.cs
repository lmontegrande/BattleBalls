/* InstantVR Head Movements
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.5.4
 * date: September 16, 2016
 *
 * - Fixed gaze in VR mode
 */

using UnityEngine;
using UnityEngine.EventSystems;

namespace IVR {

    public class HeadMovements : IVR_Movements {
        public bool useEventSystem;
        public bool showFocusPoint;
        public GameObject focusPointObj;

        public Transform lookTransform;
        public Vector3 focusPoint;

        protected Transform headcam;

        public override void StartMovements(InstantVR ivr) {
            base.StartMovements(ivr);

            Camera headCamera = ivr.GetComponentInChildren<Camera>();
            headcam = headCamera.transform;

            if (useEventSystem) {
                if (headCamera.clearFlags == CameraClearFlags.Nothing) {
                    GameObject esCameraGO = new GameObject("EventSystem Camera");
                    esCameraGO.transform.parent = headCamera.transform;
                    esCameraGO.transform.localPosition = Vector3.zero;
                    esCameraGO.transform.localRotation = Quaternion.identity;

                    Camera newCamera = esCameraGO.AddComponent<Camera>();
                    //newCamera.enabled = false;
                    headcam = newCamera.transform;
                }
                headcam.gameObject.AddComponent<PhysicsRaycaster>();
                HeadInputModule inputModule = headcam.gameObject.AddComponent<HeadInputModule>();
                inputModule.ivr = ivr;
            }

            if (showFocusPoint && focusPointObj == null) {
                focusPointObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                focusPointObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                Collider c = focusPointObj.GetComponent<Collider>();
                Destroy(c);
            }
        }

        protected float lastFocus;

        public override void UpdateMovements() {
            Vector3 lookDirection = DeriveLookDirection(headcam.rotation);

            RaycastHit hit;
            bool raycastHit = Physics.Raycast(headcam.position, lookDirection, out hit);
            UpdateFocusPoint(raycastHit, hit);

            if (showFocusPoint)
                focusPointObj.transform.position = focusPoint;
            base.UpdateMovements();
        }

        public virtual void UpdateFocusPoint(bool raycastHit, RaycastHit hit) {
            if (raycastHit) {
                focusPoint = hit.point;

                if (hit.rigidbody != null) {
                    lookTransform = hit.transform;
                } else {
                    lookTransform = null;
                }
            } else {
                focusPoint = headcam.position + headcam.forward * 10;
                lookTransform = null;
            }
        }

        public virtual Vector3 DeriveLookDirection(Quaternion headRotation) {
            return headRotation * Vector3.forward; // * ivr.characterTransform.forward;
        }
    }

    public class HeadInputModule : BaseInputModule { 
        private PointerEventData pointerData;
        private Vector2 lastHeadPose;
        public InstantVR ivr;
        private Camera headcam;
        private Transform lookTransform;

        private ControllerInput controllerInput;

        /// Time in seconds between the pointer down and up events sent by a magnet click.
        /// Allows time for the UI elements to make their state transitions.
        [HideInInspector]
        public float clickTime = 0.1f;  // Based on default time for a button to animate to Pressed.

        /// The pixel through which to cast rays, in viewport coordinates.  Generally, the center
        /// pixel is best, assuming a monoscopic camera is selected as the `Canvas`' event camera.
        [HideInInspector]
        public Vector2 hotspot = new Vector2(0.5f, 0.5f);

        public override bool ShouldActivateModule() {
            if (!base.ShouldActivateModule()) {
                return false;
            }

            if (headcam == null) { 
                Camera[] headcams = ivr.GetComponentsInChildren<Camera>();
                for (int i = 0; i < headcams.Length; i++) {
                    if (headcams[i].clearFlags != CameraClearFlags.Nothing)
                        headcam = headcams[i];
                }
                controllerInput = Controllers.GetController(0);
            }

            lookTransform = ivr.headTarget.GetComponent<HeadMovements>().lookTransform;

            return true;
        }

        public override void DeactivateModule() {
            base.DeactivateModule();
            if (pointerData != null) {
                HandlePendingClick();
                HandlePointerExitAndEnter(pointerData, null);
                pointerData = null;
            }
            eventSystem.SetSelectedGameObject(null, GetBaseEventData());
        }

        public override bool IsPointerOverGameObject(int pointerId) {
            return pointerData != null && pointerData.pointerEnter != null;
        }

        public override void Process() {
            CastRayFromGaze();
            UpdateCurrentObject();

            if (Time.unscaledTime - pointerData.clickTime < clickTime) {
                // Delay new events until clickTime has passed.
            } else if (!pointerData.eligibleForClick && controllerInput.right.buttons[0]) { 
                // New trigger action.
                HandleTrigger();
            } else
            
            if (!controllerInput.right.buttons[0]) {
                // Check if there is a pending click to handle.
                HandlePendingClick();
            }
        }

        private void CastRayFromGaze() {
            Vector2 headPose = NormalizedCartesianToSpherical(headcam.transform.forward);

            if (pointerData == null) {
                pointerData = new PointerEventData(eventSystem);
                lastHeadPose = headPose;
            }

            pointerData.Reset();
            pointerData.position = new Vector2(hotspot.x * Screen.width, hotspot.y * Screen.height);
            eventSystem.RaycastAll(pointerData, m_RaycastResultCache);
            pointerData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
            m_RaycastResultCache.Clear();
            pointerData.delta = headPose - lastHeadPose;
            lastHeadPose = headPose;
        }

        private void UpdateCurrentObject() {
            // Send enter events and update the highlight.
            //var go = pointerData.pointerCurrentRaycast.gameObject;
            GameObject go = null;
            if (lookTransform != null)
                go = lookTransform.gameObject;

            HandlePointerExitAndEnter(pointerData, go);
            // Update the current selection, or clear if it is no longer the current object.
            var selected = ExecuteEvents.GetEventHandler<ISelectHandler>(go);
            if (selected == eventSystem.currentSelectedGameObject) {
                ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, GetBaseEventData(),
                                      ExecuteEvents.updateSelectedHandler);
            } else {
                eventSystem.SetSelectedGameObject(null, pointerData);
            }
        }

        private void HandlePendingClick() {
            if (!pointerData.eligibleForClick) {
                return;
            }

            //var go = pointerData.pointerCurrentRaycast.gameObject;
            GameObject go = null;
            if (lookTransform != null)
                go = lookTransform.gameObject;

            // Send pointer up and click events.
            ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerClickHandler);

            if (pointerData.pointerDrag != null) {
                ExecuteEvents.ExecuteHierarchy(go, pointerData, ExecuteEvents.dropHandler);
            }

            if (pointerData.pointerDrag != null && pointerData.dragging) {
                ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.endDragHandler);
            }

            // Clear the click state.
            pointerData.pointerPress = null;
            pointerData.rawPointerPress = null;
            pointerData.eligibleForClick = false;
            pointerData.clickCount = 0;
            pointerData.pointerDrag = null;
            pointerData.dragging = false;
        }

        private void HandleTrigger() {
            //var go = pointerData.pointerCurrentRaycast.gameObject;
            GameObject go = null;
            if (lookTransform != null)
                go = lookTransform.gameObject;


            // Send pointer down event.
            pointerData.pressPosition = pointerData.position;
            pointerData.pointerPressRaycast = pointerData.pointerCurrentRaycast;
            pointerData.pointerPress =
              ExecuteEvents.ExecuteHierarchy(go, pointerData, ExecuteEvents.pointerDownHandler)
                ?? ExecuteEvents.GetEventHandler<IPointerClickHandler>(go);

            // Save the pending click state.
            pointerData.rawPointerPress = go;
            pointerData.eligibleForClick = true;
            pointerData.delta = Vector2.zero;
            pointerData.dragging = false;
            pointerData.useDragThreshold = true;
            pointerData.clickCount = 1;
            pointerData.clickTime = Time.unscaledTime;
        }


        private Vector2 NormalizedCartesianToSpherical(Vector3 cartCoords) {
            cartCoords.Normalize();
            if (cartCoords.x == 0)
                cartCoords.x = Mathf.Epsilon;
            float outPolar = Mathf.Atan(cartCoords.z / cartCoords.x);
            if (cartCoords.x < 0)
                outPolar += Mathf.PI;
            float outElevation = Mathf.Asin(cartCoords.y);
            return new Vector2(outPolar, outElevation);
        }
    }

}