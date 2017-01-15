using UnityEngine;

public class RetrieveSign : MonoBehaviour {

    private Transform oldParent;
    private Vector3 oldPosition;
    private Quaternion oldRotation;
    private bool oldKinematic;

    public void Retrieve() {
        oldParent = transform.parent;
        oldPosition = transform.position;
        oldRotation = transform.rotation;

        Transform newParent = FindObjectOfType<Camera>().transform;
        transform.parent = newParent;
        transform.localPosition = new Vector3(0, 0, 0.5F);
        transform.localRotation = Quaternion.identity;

        Rigidbody rigidbody = GetComponent<Rigidbody>();
        oldKinematic = rigidbody.isKinematic;
        rigidbody.isKinematic = true;
    }

    public void PutBack() {
        transform.parent = oldParent;
        transform.position = oldPosition;
        transform.rotation = oldRotation;

        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.isKinematic = oldKinematic;
    }

    public void LookingAt() {
        Debug.Log("LookingAt");
    }

    public void LookingAway() {
        Debug.Log("LookingAway");
    }
}
