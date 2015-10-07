using UnityEngine;
using System.Collections;

public class Marker : MonoBehaviour {
    void Start() {
    }

    void OnDrawGizmos() {
        Gizmos.DrawIcon(transform.position, "box.png", true);
    }
}
