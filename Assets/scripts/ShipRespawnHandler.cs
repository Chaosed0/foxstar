using UnityEngine;
using System.Collections;

public class ShipRespawnHandler : MonoBehaviour {
    public TerrainGenerator terrain;
    private Ship ship;

	void Start () {
        ship = GetComponent<Ship>();
        ship.OnDead += OnDead;
	}

    void OnDead() {
        ship.Reset();
        Vector3 position = new Vector3(Random.Range(-600.0f, 600.0f), 0.0f, Random.Range(-600.0f, 600.0f));
        transform.position = position + new Vector3(0.0f, terrain.GetElevation(position.x, position.z) + 30.0f, 0.0f);
        transform.rotation = Quaternion.AngleAxis(Random.Range(0.0f, 360.0f), Vector3.up);
    }
}
