using UnityEngine;
using System.Collections;

public class FollowPoint : MonoBehaviour {
    public ShipMotor motor;
    public float maxHeight = 4.0f;

    private float distance = -6.0f;

    void Start() {
        distance = transform.localPosition.z;
    }

    void FixedUpdate() {
        float lerp = (motor.maxHeight/2.0f - motor.transform.position.y) / motor.maxHeight;
        transform.localPosition = new Vector3(0.0f, maxHeight * lerp, distance);
    }
}
