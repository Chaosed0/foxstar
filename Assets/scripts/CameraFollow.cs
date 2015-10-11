using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {
    public Transform followPoint;
    public Transform lookAt;
    public float smoothTime = 0.1f;
    private Vector3 velocity = Vector3.zero;

    void Update() {
        transform.position = Vector3.SmoothDamp(transform.position, followPoint.position, ref velocity, smoothTime);

        /*Vector3 rotationAxis;
        float rotationAngle;
        lookAt.rotation.ToAngleAxis(out rotationAngle, out rotationAxis);
        Vector3 p = Vector3.Project(rotationAxis, lookAt.forward);
        Quaternion roll = Quaternion.AngleAxis(rotationAngle, p);*/
        transform.rotation = Quaternion.LookRotation(lookAt.position - transform.position, Vector3.up);
    }
}
