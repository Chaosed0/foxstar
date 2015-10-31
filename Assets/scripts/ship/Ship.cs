using UnityEngine;
using System.Collections;

public class Ship : MonoBehaviour {
    public int maxHealth = 100;
    public int score = 0;

    private ShipMotor motor;
    private int health = 100;

    public delegate void HealthChange(int health, int change);
    public event HealthChange OnHealthChange;

    public delegate void Dead();
    public event Dead OnDead;

    public delegate void Respawn();
    public event Respawn OnRespawn;

    public delegate void ScoreChange(int score, int change);
    public event ScoreChange OnScoreChange;

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
        if (OnHealthChange != null) {
            OnHealthChange(health, 0);
        }
        if (OnRespawn != null) {
            OnRespawn();
        }
    }

    public bool IsDead() {
        return health <= 0;
    }

    public void AddScore() {
        score++;
        if (OnScoreChange != null) {
            OnScoreChange(score, 1);
        }
    }

    public void RemoveScore() {
        if (score > 0) {
            score--;
            if (OnScoreChange != null) {
                OnScoreChange(score, -1);
            }
        }
    }

    void OnCollisionEnter(Collision collision) {
        Collider collider = collision.collider;
        int healthChange = 0;
        Ship killer = null;
        if (collider.gameObject.tag == "Laser") {
            healthChange = -10;
            killer = collider.transform.parent.GetComponent<Laser>().owner;
            /* The laser is destroyed in the Laser script */
        } else {
            /* If we collided with something generic, figure out how fast we
             * hit it and do damage accordingly */
            healthChange = -(int)(collision.relativeVelocity.magnitude / (2*motor.maxSpeed) * maxHealth);

            /* If we got hit by another ship, they killed us; otherwise, we killed ourselves */
            Ship opponent = collider.GetComponent<Ship>();
            if (opponent != null) {
                killer = opponent;
            } else {
                killer = this;
            }
        }

        health += healthChange;

        if (OnHealthChange != null) {
            OnHealthChange(health, healthChange);
        }

        if (IsDead()) {
            if (OnDead != null) {
                OnDead();
            }
            if (killer == this) {
                RemoveScore();
            } else {
                killer.AddScore();
            }
        }
    }
}
