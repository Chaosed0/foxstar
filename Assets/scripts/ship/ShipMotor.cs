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
    public float maxHeight = 800.0f;

    public float pitchDampTime = 0.15f;
    public float rollDampTime = 0.05f;
    public float yawSpeed = 60.0f;

    public float maxPitchAngle = 60.0f;
    public float smallRollAngle = 50.0f;
    public float tightRollAngle = 90.0f;

    public float boostRefillCooldownTime = 1.0f;
    public float boostTime = 2.0f;

    private float boostRefillCooldownTimer = 1.0f;
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

    private bool limitPitch = true;
    private bool limitRoll = true;
    private bool doYaw = true;
    private bool wrapRotation = true;
    private float yawMultiplier = 1.0f;
    private Maneuvers currentManeuver = Maneuvers.NONE;
    private float maneuverDirection = 0.0f;

    /* Immelmann state */
    private bool oob = false;
    private bool apex = false;
    private bool righted = false;
    private bool finished = false;

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

        boostRefillCooldownTimer = boostRefillCooldownTime;
        boostTimer = boostTime;

        dragCoefficient = acceleration/(maxSpeed*maxSpeed);
        rotation = transform.rotation.eulerAngles;
        targetRotation = rotation;
	}

    void OnDisable() {
        body.velocity = Vector3.zero;
    }

    public void Reset() {
        if (IsBoosting() && OnStopBoost != null) {
            OnStopBoost();
        }

        if (IsManeuvering() && OnStopManeuver != null) {
            OnStopManeuver();
        }

        boostRefillCooldownTimer = boostRefillCooldownTime;
        boostTimer = boostTime;
        isBoosting = false;

        if (OnBoostChange != null) {
            OnBoostChange(boostTimer);
        }

        thrust = 0.0f;
        pitch = 0.0f;
        roll = 0.0f;
        tightRoll = 0.0f;

        rotation = transform.rotation.eulerAngles;
        targetRotation = rotation;

        limitPitch = true;
        limitRoll = true;
        doYaw = true;
        wrapRotation = true;

        yawMultiplier = 1.0f;
        speed = 0.0f;

        currentManeuver = Maneuvers.NONE;
        maneuverDirection = 0.0f;
    }

    public bool IsBoosting() {
        return isBoosting;
    }

    public bool IsManeuvering() {
        return currentManeuver != Maneuvers.NONE;
    }

    void FixedUpdate() {
        if (Mathf.Abs(transform.position.x) > Constants.worldSize ||
                Mathf.Abs(transform.position.z) > Constants.worldSize) {
            oob = true;
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

        /* Don't allow pitching up if the player is at maximum height */
        if (!IsManeuvering() && pitch <= -Util.Epsilon &&
                transform.position.y > maxHeight) {
            pitch = 0.0f;
            Vector3 pos = transform.position;
            pos.y = maxHeight;
            transform.position = pos;
        }

        if (Mathf.Abs(pitch) > Util.Epsilon) {
            targetRotation.x = pitch * maxPitchAngle;
        } else {
            targetRotation.x = 0.0f;
        }

        /* Importantly, this is checked before tight roll */
        if (Mathf.Abs(roll) > Util.Epsilon) {
            targetRotation.z = roll * smallRollAngle;
            yawMultiplier = Mathf.Abs(roll);
        } else {
            targetRotation.z = 0.0f;
            yawMultiplier = 0.0f;
        }

        if (Mathf.Abs(tightRoll) > Util.Epsilon) {
            if (Mathf.Sign(tightRoll) != Mathf.Sign(roll)) {
                yawMultiplier = 0.0f;
            }
            targetRotation.z = tightRoll * tightRollAngle;
        }

        Vector3 vel = Vector3.zero;
        if (doYaw) {
            rotation.y -= yawMultiplier * rotation.z / 90.0f * yawSpeed * Time.deltaTime;
            yawMultiplier = 1.0f;
        } else if (oob == true) {
            rotation.y = Mathf.SmoothDamp(rotation.y, targetRotation.y, ref vel.y, pitchDampTime);
        }

        if (limitPitch) {
            rotation.x = Mathf.SmoothDamp(rotation.x, targetRotation.x, ref vel.x, pitchDampTime);
        } else {
            rotation.x += targetRotation.x / (maxPitchAngle * pitchDampTime);
        }

        if (limitRoll) {
            rotation.z = Mathf.SmoothDamp(rotation.z, targetRotation.z, ref vel.z, rollDampTime);
        } else {
            rotation.z += targetRotation.z / (smallRollAngle * rollDampTime);
        }

        if (wrapRotation) {
            rotation.x = rotation.x % 360;
            rotation.y = rotation.y % 360;
            rotation.z = rotation.z % 360;
        }

        float drag = dragCoefficient * speed * speed;
        speed -= drag * Time.deltaTime;
        /* Don't allow backing up */
        speed = Mathf.Max(0.0f, speed);

        body.velocity = transform.forward * speed;
        body.MoveRotation(Quaternion.Euler(rotation));

        if (IsBoosting()) {
            boostTimer -= Time.deltaTime;
            if (OnBoostChange != null) {
                OnBoostChange(boostTimer);
            }

            if (boostTimer <= 0.0f) {
                FinishBoost();
            }

        } else if (boostRefillCooldownTimer >= boostRefillCooldownTime && boostTimer < boostTime) {
            boostTimer = Mathf.Min(boostTimer + Time.deltaTime, boostTime);
            if (OnBoostChange != null) {
                OnBoostChange(boostTimer);
            }
        }

        if (boostRefillCooldownTimer < boostRefillCooldownTime) {
            boostRefillCooldownTimer += Time.deltaTime;
            if (OnBoostAvailable != null) {
                OnBoostAvailable();
            }
        }
    }

    private void FinishBoost() {
        isBoosting = false;
        if (OnStopBoost != null) {
            OnStopBoost();
        }
        boostRefillCooldownTimer = 0.0f;
    }

    public void Boost(bool doBoost) {
        if (doBoost && !IsBoosting()) {
            if (!IsManeuvering() && boostTimer > 0.0f) {
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
            FinishBoost();
        }
    }

    public float GetCurrentThrust() {
        return thrust;
    }

    public bool Somersault(float direction) {
        limitPitch = false;
        wrapRotation = false;
        doYaw = false;
        if (Mathf.Abs(rotation.x) < 360.0f) {
            SetMovement(1.0f, direction/2.0f, 0.0f, 0.0f);
        } else {
            rotation.x = rotation.x % 360.0f;
            limitPitch = true;
            doYaw = true;
            wrapRotation = true;
            return true;
        }
        return false;
    }

    public bool Immelmann(float direction) {
        doYaw = false;
        targetRotation.y = rotation.y;
        if (!apex) {
            /* Pitch until we reach the apex */
            limitPitch = false;
            limitRoll = true;
            SetMovement(1.0f, direction/3.0f, 0.0f, 0.0f);
            if (Mathf.Abs(rotation.x) >= 180.0f) {
                rotation.x = 180.0f;
                apex = true;
            }
        } else if (!righted) {
            /* Roll until we're upright again */
            limitPitch = false;
            limitRoll = false;
            SetMovement(1.0f, 0.0f, direction/3.0f, 0.0f);
            if (Mathf.Abs(rotation.z) >= 180.0f) {
                rotation.z = 180.0f;
                righted = true;
            }
        } else {
            if (!finished) {
                rotation.x = 0.0f;
                rotation.y = (rotation.y + 180.0f) % 360;
                rotation.z = 0.0f;
                finished = true;
            }

            bool returnEarly = false;
            if (transform.position.y > maxHeight) {
                /* If we're about to end an immelmann above max height, pitch
                 * downwards until we reach max height */
                limitPitch = true;
                limitRoll = true;
                SetMovement(1.0f, 1.0f, 0.0f, 0.0f);
                returnEarly = true;
            }

            if (oob) {
                /* If we're about to end an immelmann we started out-of-bounds
                 * back out-of-bounds, cheat - it's better than getting stuck
                 * forever */
                if (transform.position.x > Constants.worldSize) {
                    targetRotation.y = 270.0f;
                    returnEarly = true;
                } else if (transform.position.x < -Constants.worldSize) {
                    targetRotation.y = 90.0f;
                    returnEarly = true;
                } else if (transform.position.z > Constants.worldSize) {
                    targetRotation.y = 180.0f;
                    returnEarly = true;
                } else if (transform.position.z < -Constants.worldSize) {
                    targetRotation.y = 0.0f;
                    returnEarly = true;
                }
            }

            if (returnEarly) {
                return false;
            }

            limitPitch = true;
            limitRoll = true;
            doYaw = true;
            oob = false;
            apex = false;
            righted = false;
            finished = false;
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
