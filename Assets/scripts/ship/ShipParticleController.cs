using UnityEngine;
using System.Collections;

public class ShipParticleController : MonoBehaviour {
    public ParticleSystem thrusterTrail;
    public ParticleSystem boostHalo;
    private ShipMotor motor;

    public float minSpeed = 5.0f;
    public float maxSpeed = 25.0f;
    public float minSize = 0.5f;
    public float maxSize = 2.0f;

    public Color boostTrailColor;
    private Color initialTrailColor;

    void Start () {
        motor = GetComponent<ShipMotor>();
        motor.OnStartBoost += OnStartBoost;

        initialTrailColor = thrusterTrail.startColor;
    }

    void OnStartBoost() {
        boostHalo.Play();
    }

	void Update () {
        float lerp = (motor.GetCurrentThrust() + 1.0f) / 2.0f;
        if (motor.IsBoosting()) {
            lerp = 2.0f;
        }
        thrusterTrail.startSpeed = minSpeed + (maxSpeed - minSpeed) * lerp;
        thrusterTrail.startSize = minSize + (maxSize - minSize) * lerp;
        thrusterTrail.startColor = initialTrailColor;

        if (motor.IsBoosting()) {
            thrusterTrail.startColor = boostTrailColor;
        }
	}
}
