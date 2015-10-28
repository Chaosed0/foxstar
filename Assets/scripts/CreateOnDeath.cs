using UnityEngine;
using System.Collections;

public class CreateOnDeath : MonoBehaviour {
    public Transform prefab;
    private Ship ship;

	void Start () {
        ship = GetComponent<Ship>();
        ship.OnDead += OnDead;
	}

    void OnDead() {
        Instantiate(prefab, transform.position, transform.rotation);
    }
}
