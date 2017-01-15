/* HandgunController
 * author: Pascal Serrarnes
 * email: unity@serrarens.nl
 * version: 1.2.1
 * date: February 29, 2016
 * 
 * - Added check on INSTANTVR_EDGE
 */
using UnityEngine;
using IVR;

public class HandGunController : MonoBehaviour {

    private bool shooting = false;
    private bool grabbed = false;

    private IVR_HandMovements handMovements;
    
	private Transform handgun;
	private Vector3 nozzleLocation = new Vector3(0f,0.132f,0.09f);

    private Collider gunCollider;

    private Light nozzleFlash;
    private ParticleSystem nozzleSmoke;

    void OnGrabbed(IVR_HandMovements hand) {
        handMovements = hand;

		handgun = transform.GetChild(0);

        gunCollider = GetComponentInChildren<Collider>();
        nozzleFlash = GetComponentInChildren<Light>();
        nozzleSmoke = GetComponentInChildren<ParticleSystem>();

        grabbed = true;
    }

    void OnLetGo() {
        grabbed = false;
    }

    void FixedUpdate() {
        if (grabbed) {
            // is gun trigger pulled?
            if (handMovements.indexCurl > 0.5F) { 
                if (!shooting) {
#if INSTANTVR_EDGE
                    gunCollider.attachedRigidbody.isKinematic = false;
#endif
                    gunCollider.attachedRigidbody.AddForceAtPosition(-handgun.forward * 5, transform.TransformPoint(nozzleLocation), ForceMode.Impulse);

                    RaycastHit hit;
                    if (Physics.Raycast(transform.TransformPoint(nozzleLocation), handgun.forward, out hit)) {
                        if (hit.rigidbody != null) {
                            hit.rigidbody.AddForceAtPosition(handgun.forward * 5, hit.point, ForceMode.Impulse);
                        }
                    }
                    nozzleFlash.enabled = true;
                    nozzleSmoke.Play();
                    shooting = true;
                } else {
                    nozzleFlash.enabled = false;
                    Debug.DrawRay(transform.TransformPoint(nozzleLocation), handgun.forward * 5);
                }
            } else
                shooting = false;
        }
    }
}
