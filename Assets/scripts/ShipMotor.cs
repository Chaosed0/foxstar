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

    public float pitchAccel = 20.0f;
    public float smallRollAccel = 15.0f;
    public float tightRollAccel = 40.0f;
    public float yawAccel = 20.0f;
    public float maxPitchSpeed = 200.0f;
    public float maxRollSpeed = 300.0f;
    public float maxYawSpeed = 50.0f;

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

    private Vector3 rotSpeed = Vector3.zero;
    private Vector3 rotation = Vector3.zero;

    private float dragCoefficient = 0.2f;
    private float speed = 0.0f;

    private bool limitRotation = true;
    private bool dampRotation = true;
    private bool doYaw = false;
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

        /* Inherit instantiated rotation */
        rotation = this.transform.eulerAngles;
	}

    public void Reset() {
        thrust = 0.0f;
        pitch = 0.0f;
        roll = 0.0f;
        rotation = Vector3.zero;
        rotSpeed = Vector3.zero;
        boostTimer = boostTime;
        boostCooldownTimer = boostCooldownTime;
        currentManeuver = Maneuvers.NONE;
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
            SetManeuver(Maneuvers.IMMELMANN, 1.0f);
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
            if (Mathf.Abs(tightRoll) > Util.Epsilon) {
                /* In a tight roll, pitch controls yaw amount */
                yawMultiplier = 1.0f + pitch * 1.0f;
            } else {
                rotSpeed.x += pitchAccel * pitch * Time.deltaTime;
            }
        }

        if (Mathf.Abs(tightRoll) > Util.Epsilon) {
            rotSpeed.z += tightRollAccel * (-tightRoll) * Time.deltaTime;
        } else if (Mathf.Abs(roll) > Util.Epsilon) {
            rotSpeed.z += smallRollAccel * (-roll) * Time.deltaTime;
        }

        if (!doYaw) {
            rotSpeed.y -= yawMultiplier * rotation.z / 90.0f * yawAccel * Time.deltaTime;
            yawMultiplier = 1.0f;
        }

        float drag = dragCoefficient * speed * speed;
        speed -= drag * Time.deltaTime;

        if (dampRotation) {
            rotSpeed -= new Vector3(
                    rotSpeed.x/8.0f,
                    rotSpeed.y/8.0f,
                    rotSpeed.z/8.0f
                );
        }

        if (limitRotation) {
            Vector3 vel = Vector3.zero;
            Vector3 damped = Vector3.SmoothDamp(rotation, Vector3.zero, ref vel, 0.1f);
            rotation.x = damped.x;
            rotation.z = damped.z;
        }

        rotation += rotSpeed;
        rotation.x = rotation.x % 360;
        rotation.y = rotation.y % 360;
        rotation.z = rotation.z % 360;

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
        return true;
    }

    public bool Immelmann(float direction) {
        limitRotation = false;
        doYaw = true;
        if (Mathf.Abs(rotation.x) < 180.0f) {
            /* Pitch until we reach the apex */
            SetMovement(1.0f, -direction, 0.0f, 0.0f);
        } else if (Mathf.Abs(rotation.z) < 180.0f) {
            /* Roll until we're upright again */
            SetMovement(1.0f, 0.0f, 0.0f, -0.5f);
        } else {
            rotation.x = -rotation.x % 180.0f;
            rotation.y = (rotation.y + 180.0f) % 360;
            rotation.z = -rotation.z % 180.0f;
            limitRotation = true;
            doYaw = false;
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
