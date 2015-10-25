using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {
    public ShipMotor ship;
    public Transform followPoint;
    public Transform lookAt;
    public float thrustSmoothing = 0.1f;
    public float rotationalSmoothing = 0.05f;
    public float rollDamping = 0.1f;
    public float boostFovFactor = 0.8f;

    private new Camera camera;
    private Vector3 velocity = Vector3.zero;
    private Vector3 upVelocity = Vector3.zero;
    private float angVel = 0.0f;
    private float curRollDamping;

    private float boostFov;
    private float normalFov;
    private float fovTarget;

    void Start() {
        camera = GetComponent<Camera>();
        curRollDamping = rollDamping;
        ship.OnStartManeuver += OnStartManeuver;
        ship.OnStopManeuver += OnStopManeuver;
        ship.OnStartBoost += OnStartBoost;
        ship.OnStopBoost += OnStopBoost;

        normalFov = camera.fieldOfView;
        boostFov = normalFov * boostFovFactor;
        fovTarget = normalFov;
    }

    void OnStartManeuver() {
        curRollDamping = 1.0f;
    }

    void OnStopManeuver() {
        curRollDamping = rollDamping;
    }

    void OnStartBoost() {
        fovTarget = boostFov;
    }

    void OnStopBoost() {
        fovTarget = normalFov;
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
        Vector3 targetUp = Quaternion.Euler(shipRotation.x, shipRotation.y, shipRotation.z * curRollDamping) * Vector3.up;

        Vector3 up = Vector3.SmoothDamp(transform.up, targetUp, ref upVelocity, rotationalSmoothing);
        Quaternion target = Quaternion.LookRotation(lookAt.position - transform.position, up);
        transform.rotation = smoothDampQuat(transform.rotation, target, rotationalSmoothing);

        if (Mathf.Abs(camera.fieldOfView - fovTarget) > Util.Epsilon) {
            float vel = 0.0f;
            camera.fieldOfView = Mathf.SmoothDamp(camera.fieldOfView, fovTarget, ref vel, 0.1f);
        }
    }
}
