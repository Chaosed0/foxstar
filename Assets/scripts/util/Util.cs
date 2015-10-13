using UnityEngine;
using System.Collections;

public class Util {
    public static float Epsilon = 0.01f;

    public static GameObject getPlayer() {
        return GameObject.FindGameObjectsWithTag("Player")[0];
    }

    public static float angleTo(Vector3 v1, Vector3 v2, Vector3 vn) {
        return Mathf.Atan2(
                Vector3.Dot(Vector3.Cross(v1, v2), vn),
                Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
    }

    public static float easeInOutQuad(float t) {
        if (t < 0.5) return 2*t*t;
        return -1+(4-2*t)*t;
    }

    public static float easeInQuad(float t) {
        return t*t;
    }

    public static float easeOutQuad(float t) {
        return t*(2-t);
    }

    public static float easeInSine(float t) {
        float r = Mathf.Sin(t * Mathf.PI / 2);
        return r*r;
    }

    public static float easeOutSine(float t) {
        float r = Mathf.Cos(t * Mathf.PI / 2);
        return r*r;
    }
}
