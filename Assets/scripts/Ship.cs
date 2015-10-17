using UnityEngine;
using System.Collections;

public class Ship : MonoBehaviour {
    public int maxHealth = 100;
    private int health = 100;

    private ShipMotor motor;

    public delegate void HealthChange(int health, int change);
    public event HealthChange OnHealthChange;

    public delegate void Dead();
    public event Dead OnDead;

	void Start () {
        health = maxHealth;
        motor = GetComponent<ShipMotor>();
        if (OnHealthChange != null) {
            OnHealthChange(health, 0);
        }
	}

    public void Reset() {
        motor.Reset();
        health = maxHealth;
        OnHealthChange(health, 0);
    }

    void OnCollisionEnter(Collision collision) {
        Collider collider = collision.collider;
        int healthChange = 0;
        if (collider.gameObject.tag == "Laser") {
            healthChange = -5;
            /* The laser is destroyed in the Laser script */
        } else {
            /* If we collided with something generic, figure out how fast we
             * hit it and do damage accordingly */
            healthChange = -(int)(collision.relativeVelocity.magnitude / (motor.maxSpeed * 0.75f) * maxHealth);
        }

        health += healthChange;

        if (OnHealthChange != null) {
            OnHealthChange(health, healthChange);
        }

        if (health < 0) {
            if (OnDead != null) {
                OnDead();
            }
        }
    }
}
