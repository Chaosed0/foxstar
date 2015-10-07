using UnityEngine;
using System.Collections;

public class Velocity : MonoBehaviour {
    public float speed = 100.0f;
    private bool waitOneFrame = true;

	void Start () {
	}

	void Update () {
        if (waitOneFrame) {
            waitOneFrame = false;
            return;
        }
        transform.position += transform.forward.normalized * speed * Time.deltaTime;
	}
}
