﻿using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {
    public ShipMotor ship;
    public Transform followPoint;
    public Transform lookAt;
    public float thrustSmoothing = 0.1f;
    public float rotationalSmoothing = 0.05f;
    public float rollDamping = 10.0f;

    private Vector3 velocity = Vector3.zero;
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

    void FixedUpdate() {
        transform.position = Vector3.SmoothDamp(transform.position, followPoint.position, ref velocity, thrustSmoothing);

        Vector3 shipRotation = ship.getCurrentRotation();
        Vector3 up = Quaternion.Euler(shipRotation.x, shipRotation.y, shipRotation.z / curRollDamping) * Vector3.up;
        Quaternion target = Quaternion.LookRotation(lookAt.position - transform.position, up);
        float angle = Quaternion.Angle(transform.rotation, target);
        float newAngle = Mathf.SmoothDamp(angle, 0.0f, ref angVel, rotationalSmoothing);
        float lerp = 1.0f;
        if (angle != 0) {
            lerp = 1.0f - newAngle / angle;
        }
        transform.rotation = Quaternion.Slerp(transform.rotation, target, 1.0f - lerp);
    }
}
