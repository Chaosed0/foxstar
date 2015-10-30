using UnityEngine;
using System.Collections;

public class FollowPoint : MonoBehaviour {
    public ShipMotor motor;
    public float maxHeight = 4.0f;

    void Start() {
    }

    void FixedUpdate() {
        float lerp = (motor.maxHeight/2.0f - motor.transform.position.y) / motor.maxHeight;
        Vector3 position = transform.localPosition;
        position.y = maxHeight * lerp;
        transform.localPosition = position;
    }
}
