using UnityEngine;
using System.Collections;

public class Laser : MonoBehaviour {
    void OnCollisionEnter(Collision collision) {
        Debug.Log("Collision");
        Destroy(this.gameObject);
    }
}
