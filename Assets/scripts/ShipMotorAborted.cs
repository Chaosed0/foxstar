using UnityEngine;
using System.Collections;

public class ShipMotorAborted : MonoBehaviour {
    private Rigidbody body;

    // These defaults should be somewhat reasonable for a mass of 100
    public float thrustForce = 2000.0f;
    public float boostForce = 5000.0f;
    public float yawForce = 2000.0f;
    public float pitchForce = 2000.0f;
    public float rollForce = 2000.0f;
    public float sideDampForce = 100.0f;
    public float stability = 1.0f;
    public float stabilityFactor = 1.0f;
    public float maxVelocity = 100.0f;
    public float boostCooldownTime = 15.0f;
    public float boostTime = 1.0f;
    public float maxRollAngle = 40.0f;

    private float boostCooldownTimer = 15.0f;
    private float boostTimer = 2.0f;

    private float thrust = 0.0f;
    private float pitch = 0.0f;
    private float roll = 0.0f;

    private float dragCoefficient = 0.2f;

    public delegate void StartBoost();
    public event StartBoost OnStartBoost;

    public delegate void AttemptedBoost();
    public event AttemptedBoost OnAttemptedBoost;

    public delegate void StopBoost();
    public event StopBoost OnStopBoost;

    public delegate void BoostAvailable();
    public event BoostAvailable OnBoostAvailable;

	void Start () {
        body = GetComponent<Rigidbody>();
        body.centerOfMass = new Vector3(0,0,0);

        body.maxAngularVelocity = 2.0f;
        boostCooldownTimer = boostCooldownTime;
        boostTimer = boostTime;

        dragCoefficient = thrustForce/(maxVelocity*maxVelocity);
	}

    public bool IsBoosting() {
        return boostTimer < boostTime;
    }

    void FixedUpdate() {
        float speed = body.velocity.magnitude;
        /* Make it harder to turn when at higher velocities */
        float torqueDamper = 1.0f + speed / maxVelocity;

        if (Mathf.Abs(thrust) > Util.Epsilon && !IsBoosting()) {
            body.AddForce(transform.forward * thrust * thrustForce);
        }

        if (Mathf.Abs(pitch) > Util.Epsilon) {
            body.AddTorque(transform.right * pitch * pitchForce / torqueDamper);
        }

        /* Limit the roll angle */
        if (Mathf.Abs(roll) > Util.Epsilon) {
            body.AddTorque(transform.forward * -roll * rollForce / torqueDamper);
        }

        /* Add yaw if we are rolled at all */
        Vector3 pitchPlane = Quaternion.AngleAxis(-transform.eulerAngles.z, transform.forward) * transform.up;
        Vector3 xcomp = Vector3.ProjectOnPlane(transform.up, pitchPlane);
        float yaw = xcomp.magnitude * Mathf.Sign(Util.angleTo(xcomp, pitchPlane, transform.forward));
        body.AddTorque(transform.up * yaw * yawForce / torqueDamper);

        if (IsBoosting()) {
            body.AddForce(transform.forward * boostForce);
        }

        if (Mathf.Abs(speed) > Util.Epsilon) {
            /* Dampen non-forward/backward velocity */
            Vector3 vel = body.velocity;
            Vector3 sideDir = Vector3.ProjectOnPlane(vel, transform.forward).normalized;
            body.AddForce(-sideDir * sideDampForce);

            /* Stabilize up vector */
            Vector3 predictedUp = Quaternion.AngleAxis(
                    body.angularVelocity.magnitude * stability / speed,
                    body.angularVelocity) * transform.up;
            Vector3 torqueVector = Vector3.Cross(predictedUp, Vector3.up);
            torqueVector = Vector3.Project(torqueVector, transform.forward);
            body.AddTorque(torqueVector * speed * speed * stabilityFactor);
        }

        /* Apply a drag force */
        body.AddForce(- dragCoefficient * speed * speed * transform.forward);

        if (boostTimer < boostTime) {
            boostTimer += Time.deltaTime;
        } else {
            if (OnStopBoost != null) {
                OnStopBoost();
            }
        }

        if (boostCooldownTimer < boostCooldownTime) {
            boostCooldownTimer += Time.deltaTime;
            if (OnBoostAvailable != null) {
                OnBoostAvailable();
            }
        }
    }

    public void Boost() {
        if (boostCooldownTimer >= boostCooldownTime) {
            boostTimer = 0.0f;
            boostCooldownTimer = 0.0f;
            if (OnStartBoost != null) {
                OnStartBoost();
            }
        } else {
            if (OnAttemptedBoost != null) {
                OnAttemptedBoost();
            }
        }
    }

    public float GetCurrentThrust() {
        return thrust;
    }

    public void SetMovement(float thrust, float pitch, float roll) {
        this.thrust = thrust;
        this.roll = roll;
        this.pitch = pitch;
    }
}
