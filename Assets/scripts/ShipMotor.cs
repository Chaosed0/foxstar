using UnityEngine;
using System.Collections;

public class ShipMotor : MonoBehaviour {
    public enum Maneuvers {
        SOMERSAULT,
        IMMELMANN,
        BARRELROLL,
        DODGE,
        NONE
    };

    private Rigidbody body;

    public float acceleration = 10.0f;
    public float boostAcceleration = 30.0f;
    public float maxSpeed = 100.0f;

    public float rotateTime = 0.1f;
    public float yawSpeed = 60.0f;

    public float maxPitchAngle = 30.0f;
    public float smallRollAngle = 20.0f;
    public float tightRollAngle = 90.0f;

    public float boostCooldownTime = 15.0f;
    public float boostTime = 2.0f;
    public float boostRefillFactor = 2.0f;

    private float boostCooldownTimer = 15.0f;
    private float boostTimer = 2.0f;
    private bool isBoosting = false;

    private float thrust = 0.0f;
    private float pitch = 0.0f;
    private float roll = 0.0f;
    private float tightRoll = 0.0f;

    private Vector3 targetRotation = Vector3.zero;
    private Vector3 rotation = Vector3.zero;

    private float dragCoefficient = 0.2f;
    private float speed = 0.0f;

    private bool limitRotation = true;
    private bool doYaw = true;
    private bool wrapRotation = true;
    private float yawMultiplier = 1.0f;
    private Maneuvers currentManeuver = Maneuvers.NONE;
    public float maneuverDirection = 0.0f;

    public delegate void StartBoost();
    public event StartBoost OnStartBoost;

    public delegate void AttemptedBoost();
    public event AttemptedBoost OnAttemptedBoost;

    public delegate void StopBoost();
    public event StopBoost OnStopBoost;

    public delegate void BoostAvailable();
    public event BoostAvailable OnBoostAvailable;

    public delegate void BoostChange(float boost);
    public event BoostChange OnBoostChange;

    public delegate void StartManeuver();
    public event StartManeuver OnStartManeuver;

    public delegate void StopManeuver();
    public event StopManeuver OnStopManeuver;

	void Start () {
        body = GetComponent<Rigidbody>();

        boostCooldownTimer = boostCooldownTime;
        boostTimer = boostTime;

        dragCoefficient = acceleration/(maxSpeed*maxSpeed);
        rotation = transform.rotation.eulerAngles;
        targetRotation = rotation;
	}

    public void Reset() {
        if (IsBoosting() && OnStopBoost != null) {
            OnStopBoost();
        }

        if (IsManeuvering() && OnStopManeuver != null) {
            OnStopManeuver();
        }

        thrust = 0.0f;
        pitch = 0.0f;
        roll = 0.0f;
        boostTimer = boostTime;
        boostCooldownTimer = boostCooldownTime;
        rotation = transform.rotation.eulerAngles;
        targetRotation = rotation;

        currentManeuver = Maneuvers.NONE;
        maneuverDirection = 0.0f;
        limitRotation = true;
        doYaw = true;
        wrapRotation = true;
        yawMultiplier = 1.0f;
        speed = 0.0f;
    }

    public bool IsBoosting() {
        return isBoosting;
    }

    public bool IsManeuvering() {
        return currentManeuver != Maneuvers.NONE;
    }

    void FixedUpdate() {
        if (Mathf.Abs(transform.position.x) > 2000.0f ||
                Mathf.Abs(transform.position.z) > 2000.0f) {
            SetManeuver(Maneuvers.IMMELMANN, -1.0f);
        }

        if (IsManeuvering()) {
            bool done = false;
            /* Override player's flight controls with our own */
            switch (currentManeuver) {
                case Maneuvers.SOMERSAULT:
                    done = Somersault(maneuverDirection);
                    break;
                case Maneuvers.IMMELMANN:
                    done = Immelmann(maneuverDirection);
                    break;
                case Maneuvers.BARRELROLL:
                    done = BarrelRoll(maneuverDirection);
                    break;
                case Maneuvers.DODGE:
                    done = Dodge(maneuverDirection);
                    break;
            }

            if (done) {
                currentManeuver = Maneuvers.NONE;
                if (OnStopManeuver != null) {
                    OnStopManeuver();
                }
            }
        }

        if (IsBoosting()) {
            speed += boostAcceleration * Time.deltaTime;
        } else if (Mathf.Abs(thrust) > Util.Epsilon) {
            speed += acceleration * thrust * Time.deltaTime;
        }

        if (Mathf.Abs(pitch) > Util.Epsilon) {
            /* In a roll, pitch controls yaw amount */
            float lerp = Mathf.Abs(rotation.z) / 90.0f;
            yawMultiplier = 1.0f - pitch * lerp;
            targetRotation.x = (pitch * (1.0f - lerp)) * maxPitchAngle;
        } else {
            targetRotation.x = 0.0f;
        }

        if (Mathf.Abs(tightRoll) > Util.Epsilon) {
            targetRotation.z = tightRoll * tightRollAngle;
        } else if (Mathf.Abs(roll) > Util.Epsilon) {
            targetRotation.z = roll * smallRollAngle;
        } else {
            targetRotation.z = 0.0f;
        }

        if (doYaw) {
            rotation.y -= yawMultiplier * rotation.z / 90.0f * yawSpeed * Time.deltaTime;
            yawMultiplier = 1.0f;
        }

        float drag = dragCoefficient * speed * speed;
        speed -= drag * Time.deltaTime;

        if (limitRotation) {
            Vector3 vel = Vector3.zero;
            rotation.x = Mathf.SmoothDamp(rotation.x, targetRotation.x, ref vel.x, rotateTime);
            rotation.z = Mathf.SmoothDamp(rotation.z, targetRotation.z, ref vel.z, rotateTime);
        } else {
            rotation.x += targetRotation.x / (maxPitchAngle * rotateTime);
            rotation.z += targetRotation.z / (smallRollAngle * rotateTime);
        }

        if (wrapRotation) {
            rotation.x = rotation.x % 360;
            rotation.y = rotation.y % 360;
            rotation.z = rotation.z % 360;
        }

        /* Don't allow backing up */
        speed = Mathf.Max(0.0f, speed);

        body.velocity = transform.forward * speed;
        body.MoveRotation(Quaternion.Euler(rotation));

        if (IsBoosting()) {
            boostTimer -= Time.deltaTime;
            if (OnBoostChange != null) {
                OnBoostChange(boostTimer);
            }
        } else if (boostTimer < boostTime) {
            boostTimer = Mathf.Min(boostTimer + Time.deltaTime / boostRefillFactor, boostTime);
            if (OnBoostChange != null) {
                OnBoostChange(boostTimer);
            }
        }

        if (boostTimer <= 0.0f) {
            isBoosting = false;
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

    public void Boost(bool doBoost) {
        if (doBoost && !IsManeuvering() && !IsBoosting()) {
            if (boostCooldownTimer >= boostCooldownTime) {
                isBoosting = true;
                if (OnStartBoost != null) {
                    OnStartBoost();
                }
            } else {
                if (OnAttemptedBoost != null) {
                    OnAttemptedBoost();
                }
            }
        } else if (!doBoost && IsBoosting()) {
            isBoosting = false;
            if (OnStopBoost != null) {
                OnStopBoost();
            }
        }
    }

    public float GetCurrentThrust() {
        return thrust;
    }

    public bool Somersault(float direction) {
        limitRotation = false;
        wrapRotation = false;
        doYaw = false;
        if (Mathf.Abs(rotation.x) < 360.0f) {
            SetMovement(1.0f, direction/2.0f, 0.0f, 0.0f);
        } else {
            rotation.x = rotation.x % 360.0f;
            limitRotation = true;
            doYaw = true;
            wrapRotation = true;
            return true;
        }
        return false;
    }

    public bool Immelmann(float direction) {
        limitRotation = false;
        doYaw = false;
        if (Mathf.Abs(rotation.x) < 180.0f) {
            /* Pitch until we reach the apex */
            SetMovement(1.0f, direction/3.0f, 0.0f, 0.0f);
        } else if (Mathf.Abs(rotation.z) < 180.0f) {
            /* Roll until we're upright again */
            SetMovement(1.0f, 0.0f, direction/3.0f, 0.0f);
        } else {
            rotation.x = -rotation.x % 180.0f;
            rotation.y = (rotation.y + 180.0f) % 360;
            rotation.z = -rotation.z % 180.0f;
            limitRotation = true;
            doYaw = true;
            return true;
        }
        return false;
    }


    public bool BarrelRoll(float direction) {
        return true;
    }

    public bool Dodge(float direction) {
        return true;
    }

    public void SetMovement(float thrust, float pitch, float roll, float tightRoll) {
        this.thrust = thrust;
        this.roll = roll;
        this.tightRoll = tightRoll;
        this.pitch = pitch;
    }

    public float getCurrentSpeed() {
        return speed;
    }

    public Vector3 getCurrentRotation() {
        return rotation;
    }

    public void SetManeuver(Maneuvers maneuver, float direction) {
        if (IsManeuvering()) {
            return;
        }

        isBoosting = false;
        currentManeuver = maneuver;
        maneuverDirection = Mathf.Sign(direction);
        if (OnStartManeuver != null) {
            OnStartManeuver();
        }
    }
}
