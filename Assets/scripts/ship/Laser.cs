using UnityEngine;
using System.Collections;

public class Laser : MonoBehaviour {
    public Ship owner = null;
    void OnCollisionEnter(Collision collision) {
        Destroy(this.gameObject);
    }
}
