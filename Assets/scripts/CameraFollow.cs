using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {
    public Transform followPoint;
    public Transform lookAt;
    public float smoothTime = 0.1f;
    private Vector3 velocity = Vector3.zero;

    void FixedUpdate() {
        transform.position = Vector3.SmoothDamp(transform.position, followPoint.position, ref velocity, smoothTime);
        transform.rotation = Quaternion.LookRotation(lookAt.position - transform.position, Vector3.up);
    }
}
