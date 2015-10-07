using UnityEngine;
using System.Collections;

public class ShipMotor : MonoBehaviour {
    private Rigidbody body;

    public float thrustForce = 1.0f;
    public float yawForce = 1.0f;
    public float pitchForce = 1.0f;
    public float rollForce = 1.0f;
    public float sideDamp = 10.0f;
    public float stability = 1.0f;
    public float stabilityDamper = 1000;
    public float maxVelocity = 100.0f;

	void Start () {
        body = GetComponent<Rigidbody>();

        body.maxAngularVelocity = 2.0f;
	}

    void FixedUpdate() {
        /* Dampen non-forward/backward velocity */
        Vector3 vel = body.velocity;
        Vector3 dampVec = Vector3.ProjectOnPlane(vel, transform.forward);
        vel -= dampVec.normalized * sideDamp * Time.deltaTime;
        body.velocity = vel;

        float speed = body.velocity.magnitude;
        if (Mathf.Abs(speed) > Util.Epsilon) {
            Vector3 predictedUp = Quaternion.AngleAxis(
                    body.angularVelocity.magnitude * stability / speed,
                    body.angularVelocity) * transform.up;
            Vector3 torqueVector = Vector3.Cross(predictedUp, Vector3.up);
            torqueVector = Vector3.Project(torqueVector, transform.forward);
            body.AddTorque(torqueVector * speed * speed / stabilityDamper);
        }
    }

    public void Move(float thrust, float yaw, float pitch, float roll) {
        if (Mathf.Abs(thrust) > Util.Epsilon) {
            body.AddForce(transform.forward * thrust * thrustForce);
        }

        if (body.velocity.magnitude > maxVelocity) {
            body.velocity = Vector3.ClampMagnitude(body.velocity, maxVelocity);
        }

        if (Mathf.Abs(yaw) > Util.Epsilon) {
            body.AddTorque(transform.up * yaw * yawForce * Time.deltaTime);
        }

        if (Mathf.Abs(pitch) > Util.Epsilon) {
            body.AddTorque(transform.right * pitch * pitchForce * Time.deltaTime);
        }

        if (Mathf.Abs(roll) > Util.Epsilon) {
            body.AddTorque(transform.forward * -roll * rollForce * Time.deltaTime);
        }
    }
}
