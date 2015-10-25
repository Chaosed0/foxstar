using UnityEngine;
using System.Collections;

public class AIFollowInput : MonoBehaviour {
    public Terrain terrain;
    private ShipMotor motor;
    private CannonController cannon;

	private void Start () {
        motor = GetComponent<ShipMotor>();
        cannon = GetComponent<CannonController>();
	}

	private void Update () {
        /* motor.SetManeuver(ShipMotor.Maneuvers.IMMELMANN); */
        /* motor.Boost(true); */

        float throttle = 1.0f;
        float pitch = 0.0f;

        Vector3 rotation = motor.getCurrentRotation();
        rotation.x = 0.0f;
        bool hit = Physics.Raycast(transform.position, Quaternion.Euler(rotation) * Vector3.forward, 1000.0f);
        
        if (hit) {
            pitch = -1.0f;
        }

        //motor.SetMovement(throttle, pitch, 0.0f, 0.0f);
        cannon.setFiring(true);
	}
}
