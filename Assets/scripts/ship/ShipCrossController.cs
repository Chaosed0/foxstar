using UnityEngine;
using System.Collections;

public class ShipCrossController : MonoBehaviour {
    public Transform crossNear;
    public Transform crossFar;

    private ShipMotor motor;
    private Vector3 initialRotation;

	void Start () {
        motor = GetComponent<ShipMotor>();
        initialRotation = new Vector3(270.0f, 0.0f, 0.0f);
	}

	void Update () {
        Vector3 rotation = initialRotation + motor.getCurrentRotation();
        crossNear.rotation = Quaternion.Euler(rotation.x, rotation.y, 0.0f);
        crossFar.rotation = Quaternion.Euler(rotation.x, rotation.y, 0.0f);
	}
}
