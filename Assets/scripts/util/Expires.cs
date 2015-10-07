using UnityEngine;
using System.Collections;

public class Expires : MonoBehaviour {
    public float expiryTime = 1.0f;
    
    private float expiryTimer = 0.0f;
	
	void Update () {
        expiryTimer += Time.deltaTime;
        if (expiryTimer >= expiryTime) {
            Destroy(gameObject);
        }
	}
}
