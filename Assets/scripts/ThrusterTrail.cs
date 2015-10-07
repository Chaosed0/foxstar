using UnityEngine;
using System.Collections;

public class ThrusterTrail : MonoBehaviour {
    private ParticleSystem system;
    public Rigidbody body;

    public float minSpeed = 5.0f;
    public float maxSpeed = 25.0f;
    public float minSize = 0.5f;
    public float maxSize = 2.0f;

    public float maxShipSpeed = 100.0f;

    void Start () {
        system = GetComponent<ParticleSystem>();
    }

	void Update () {
        float lerp = Mathf.Clamp(body.velocity.magnitude / maxShipSpeed, 0.0f, 1.0f);
        system.startSpeed = minSpeed + (maxSpeed - minSpeed) * lerp;
        system.startSize = minSize + (maxSize - minSize) * lerp;
	}
}
