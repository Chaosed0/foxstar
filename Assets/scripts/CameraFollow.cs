using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {
    public ShipMotor ship;
    public Transform followPoint;
    public Transform lookAt;
    public float smoothTime = 0.1f;
    private Vector3 velocity = Vector3.zero;

    void FixedUpdate() {
        transform.position = Vector3.SmoothDamp(transform.position, followPoint.position, ref velocity, smoothTime);

        float angVel = 0.0f;
        Vector3 shipRotation = ship.getCurrentRotation();
        Vector3 up = Quaternion.Euler(shipRotation.x, shipRotation.y, shipRotation.z / 10.0f) * Vector3.up;
        Quaternion target = Quaternion.LookRotation(lookAt.position - transform.position, up);
        float angle = Quaternion.Angle(transform.rotation, target);
        float lerp = Mathf.SmoothDamp(angle, 0.0f, ref angVel, 0.1f);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, 1.0f - lerp / angle);
    }
}
