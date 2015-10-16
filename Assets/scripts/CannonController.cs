using UnityEngine;
using System.Collections;

public class CannonController : MonoBehaviour {
    public Transform laserPrefab;
    public float cooldown = 0.2f;
    public Transform[] locations;
    public Transform player;
    public float bulletSpeed = 100.0f;

    private float cooldownTimer = 0.2f;
    private bool firing = false;
    private Collider[] colliders;
    private ShipMotor motor;

    void Start() {
        colliders = player.GetComponents<Collider>();
        motor = GetComponent<ShipMotor>();
    }

    public void setFiring(bool firing) {
        this.firing = firing;
    }

    void Update() {
        if (cooldownTimer < cooldown) {
            cooldownTimer += Time.deltaTime;
        } else if (firing) {
            cooldownTimer = 0.0f;
            for (int i = 0; i < locations.Length; i++) {
                Transform laser = Instantiate(laserPrefab, locations[i].position, locations[i].rotation) as Transform;
                Rigidbody laserBody = laser.GetComponent<Rigidbody>();
                laserBody.velocity = transform.forward * motor.getCurrentSpeed() + transform.forward.normalized * bulletSpeed;
                foreach (Collider collider in colliders) {
                    Physics.IgnoreCollision(laser.GetChild(0).GetComponent<Collider>(), collider);
                }
            }
        }
    }
}
