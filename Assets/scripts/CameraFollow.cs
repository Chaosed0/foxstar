using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {
    public ShipMotor ship;
    public Transform followPoint;
    public Transform lookAt;
    public float thrustSmoothing = 0.1f;
    public float rotationalSmoothing = 0.05f;
    public float rollDamping = 10.0f;

    private Vector3 velocity = Vector3.zero;
    private Vector3 upVelocity = Vector3.zero;
    private float angVel = 0.0f;
    private float curRollDamping;

    void Start() {
        curRollDamping = rollDamping;
        ship.OnStartManeuver += OnStartManeuver;
        ship.OnStopManeuver += OnStopManeuver;
    }

    void OnStartManeuver() {
        curRollDamping = 1.0f;
    }

    void OnStopManeuver() {
        curRollDamping = rollDamping;
    }

    Quaternion smoothDampQuat(Quaternion current, Quaternion target, float smoothTime) {
        float angle = Quaternion.Angle(transform.rotation, target);
        float newAngle = Mathf.SmoothDamp(angle, 0.0f, ref angVel, smoothTime);
        float lerp = 1.0f;
        if (angle != 0) {
            lerp = newAngle / angle;
        }
        return Quaternion.Slerp(current, target, lerp);
    }

    void FixedUpdate() {
        transform.position = Vector3.SmoothDamp(transform.position, followPoint.position, ref velocity, thrustSmoothing);

        Vector3 shipRotation = ship.getCurrentRotation();
        Vector3 targetUp = Quaternion.Euler(shipRotation.x, shipRotation.y, shipRotation.z / curRollDamping) * Vector3.up;

        Vector3 up = Vector3.SmoothDamp(transform.up, targetUp, ref upVelocity, rotationalSmoothing);
        Quaternion target = Quaternion.LookRotation(lookAt.position - transform.position, up);
        transform.rotation = smoothDampQuat(transform.rotation, target, rotationalSmoothing);
    }
}
