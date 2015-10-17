using UnityEngine;
using System.Collections;

public class Laser : MonoBehaviour {
    void OnCollisionEnter(Collision collision) {
        Destroy(this.gameObject);
    }
}
