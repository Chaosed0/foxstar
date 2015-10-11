using UnityEngine;
using System.Collections;

public class ShipParticleController : MonoBehaviour {
    public ParticleSystem thrusterTrail;
    public ParticleSystem boostHalo;
    private Rigidbody body;
    private ShipMotor motor;

    public float minSpeed = 5.0f;
    public float maxSpeed = 25.0f;
    public float minSize = 0.5f;
    public float maxSize = 2.0f;

    public Color boostTrailColor;
    private Color initialTrailColor;

    void Start () {
        body = GetComponent<Rigidbody>();
        motor = GetComponent<ShipMotor>();
        motor.OnStartBoost += OnStartBoost;

        initialTrailColor = thrusterTrail.startColor;
    }

    void OnStartBoost() {
        boostHalo.Play();
    }

	void Update () {
        float maxShipSpeed = motor.maxVelocity;
        float lerp = Mathf.Clamp(body.velocity.magnitude / maxShipSpeed, 0.0f, 1.0f);
        thrusterTrail.startSpeed = minSpeed + (maxSpeed - minSpeed) * lerp;
        thrusterTrail.startSize = minSize + (maxSize - minSize) * lerp;
        thrusterTrail.startColor = initialTrailColor;

        if (motor.IsBoosting()) {
            thrusterTrail.startSpeed = maxSpeed*2;
            thrusterTrail.startColor = boostTrailColor;
        }
	}
}
