using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {
    public Ship ship;
    public Transform followPoint;
    public Transform lookAt;
    public float thrustSmoothing = 0.1f;
    public float rotationalSmoothing = 0.05f;
    public float rollDamping = 0.1f;
    public float boostFovFactor = 0.8f;
    public float shakeTime = 0.5f;
    public float shakeMagnitude = 2.0f;

    private ShipMotor motor;
    private new Camera camera;
    private Vector3 velocity = Vector3.zero;
    private Vector3 upVelocity = Vector3.zero;
    private float angVel = 0.0f;
    private float curRollDamping;

    private float boostFov;
    private float normalFov;
    private float fovTarget;

    private float shakeTimer = 1.0f;

    void Start() {
        camera = GetComponent<Camera>();
        curRollDamping = rollDamping;

        motor = ship.GetComponent<ShipMotor>();
        motor.OnStartManeuver += OnStartManeuver;
        motor.OnStopManeuver += OnStopManeuver;
        motor.OnStartBoost += OnStartBoost;
        motor.OnStopBoost += OnStopBoost;

        ship.OnRespawn += OnRespawn;
        ship.OnHealthChange += OnHealthChange;

        normalFov = camera.fieldOfView;
        boostFov = normalFov * boostFovFactor;
        fovTarget = normalFov;

        shakeTimer = shakeTime;
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

    void OnRespawn() {
        transform.position = ship.transform.position;
        transform.rotation = ship.transform.rotation;
    }

    void OnHealthChange(int health, int change) {
        if (change < 0) {
            shakeTimer = 0.0f;
        }
    }

    Quaternion smoothDampQuat(Quaternion current, Quaternion target, float smoothTime) {
        float angle = Quaternion.Angle(current, target);
        float newAngle = Mathf.SmoothDamp(angle, 0.0f, ref angVel, smoothTime);
        float lerp = 1.0f;
        if (angle != 0) {
            lerp = 1.0f - newAngle / angle;
        }
        return Quaternion.Slerp(current, target, lerp);
    }

    void FixedUpdate() {
        if (ship.IsDead()) {
            shakeTimer = shakeTime;
            return;
        }

        Vector3 shake = Vector3.zero;
        if (shakeTimer <= shakeTime) {
            float rad = Random.Range(0.0f, 360.0f);
            float magnitude = (1.0f - shakeTimer / shakeTime) * shakeMagnitude;
            shake.x = magnitude * Mathf.Cos(rad);
            shake.y = magnitude * Mathf.Sin(rad);
            shake = ship.transform.rotation * shake;
            shakeTimer += Time.deltaTime;
        }

        transform.position = Vector3.SmoothDamp(transform.position, followPoint.position, ref velocity, thrustSmoothing) + shake;

        Vector3 shipRotation = motor.getCurrentRotation();

        Quaternion target;
        if (motor.IsManeuvering()) {
            Vector3 targetUp = Quaternion.Euler(shipRotation.x, shipRotation.y, shipRotation.z * curRollDamping) * Vector3.up;
            Vector3 up = Vector3.SmoothDamp(transform.up, targetUp, ref upVelocity, 0.01f);
            target = Quaternion.LookRotation(lookAt.position - transform.position, up);
        } else {
            target = Quaternion.Euler(shipRotation.x/3.0f, shipRotation.y, 0.0f);
        }

        transform.rotation = smoothDampQuat(transform.rotation, target, rotationalSmoothing);

        if (Mathf.Abs(camera.fieldOfView - fovTarget) > Util.Epsilon) {
            float vel = 0.0f;
            camera.fieldOfView = Mathf.SmoothDamp(camera.fieldOfView, fovTarget, ref vel, 0.1f);
        }
    }
}
