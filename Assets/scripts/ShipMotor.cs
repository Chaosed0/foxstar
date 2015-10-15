﻿using UnityEngine;
using System.Collections;

public class ShipMotor : MonoBehaviour {
    private Rigidbody body;

    public float acceleration = 10.0f;
    public float boostAcceleration = 30.0f;
    public float maxSpeed = 100.0f;

    public float pitchAccel = 10.0f;
    public float rollAccel = 10.0f;
    public float yawAccel = 10.0f;
    public float maxPitchSpeed = 200.0f;
    public float maxRollSpeed = 300.0f;
    public float maxYawSpeed = 50.0f;

    public float boostCooldownTime = 15.0f;
    public float boostTime = 1.0f;
    public float maxPitchAngle = 80.0f;
    public float maxRollAngle = 80.0f;

    private float boostCooldownTimer = 15.0f;
    private float boostTimer = 2.0f;

    private float thrust = 0.0f;
    private float pitch = 0.0f;
    private float roll = 0.0f;

    private Vector3 rotSpeed = Vector3.zero;
    private Vector3 rotation = Vector3.zero;

    private float dragCoefficient = 0.2f;
    private float speed = 0.0f;

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

        dragCoefficient = acceleration/(maxSpeed*maxSpeed);
	}

    public bool IsBoosting() {
        return boostTimer < boostTime;
    }

    void FixedUpdate() {
        if (IsBoosting()) {
            speed += boostAcceleration * Time.deltaTime;
        } else if (Mathf.Abs(thrust) > Util.Epsilon) {
            speed += acceleration * thrust * Time.deltaTime;
        }

        if (Mathf.Abs(pitch) > Util.Epsilon) {
            rotSpeed.x += pitchAccel * pitch * Time.deltaTime;
        }

        if (Mathf.Abs(roll) > Util.Epsilon) {
            rotSpeed.z += rollAccel * (-roll) * Time.deltaTime;
        }

        rotSpeed.y -= rotation.z / maxRollAngle * yawAccel * Time.deltaTime;

        float drag = dragCoefficient * speed * speed;
        speed -= drag * Time.deltaTime;

        Vector3 angularDrag = new Vector3(
                    rotSpeed.x/8.0f,
                    rotSpeed.y/8.0f,
                    rotSpeed.z/8.0f
                );
        rotSpeed -= angularDrag;

        rotation += rotSpeed;
        rotation.x = Mathf.Clamp(rotation.x, -maxPitchAngle, maxPitchAngle);
        rotation.y = rotation.y % 360;
        rotation.z = Mathf.Clamp(rotation.z, -maxRollAngle, maxRollAngle);

        /* Don't allow backing up */
        speed = Mathf.Max(0.0f, speed);

        body.velocity = transform.forward * speed;
        body.MoveRotation(Quaternion.Euler(rotation));

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

    public float getCurrentSpeed() {
        return speed;
    }

    public Vector3 getCurrentRotation() {
        return rotation;
    }
}
