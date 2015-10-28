using UnityEngine;
using System.Collections;

public class ShipRespawnHandler : MonoBehaviour {
    public TerrainGenerator terrain;
    public float respawnTime = 3.0f;

    private Ship ship;
    private ShipMotor motor;
    private CannonController cannon;
    private float respawnTimer = 3.0f;
    private bool respawning = false;

	void Start () {
        cannon = GetComponent<CannonController>();
        motor = GetComponent<ShipMotor>();
        ship = GetComponent<Ship>();
        ship.OnDead += OnDead;
	}

    void Update() {
        if (respawning) {
            respawnTimer += Time.deltaTime;
            if (respawnTimer >= respawnTime) {
                Vector3 position = new Vector3(Random.Range(-600.0f, 600.0f), 0.0f, Random.Range(-600.0f, 600.0f));
                transform.position = position + new Vector3(0.0f, terrain.GetElevation(position.x, position.z) + 30.0f, 0.0f);
                transform.rotation = Quaternion.AngleAxis(Random.Range(0.0f, 360.0f), Vector3.up);
                ship.Reset();

                enableShip();
                respawning = false;
            }
        }
    }

    void OnDead() {
        respawning = true;
        respawnTimer = 0.0f;
        disableShip();
    }

    void enableShip() {
        enableDisableShip(true);
    }

    void disableShip() {
        enableDisableShip(false);
    }

    void enableDisableShip(bool yes) {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++) {
            renderers[i].enabled = yes;
        }

        Collider[] colliders = GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++) {
            colliders[i].enabled = yes;
        }

        motor.enabled = yes;
        cannon.enabled = yes;
    }
}
