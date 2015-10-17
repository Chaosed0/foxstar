using UnityEngine;
using System.Collections;

/* This assumes that both the world and local are centered around (0,0) */
public class MinimapIcon : MonoBehaviour {
    public Transform followTransform;
    public Vector2 worldSize;

    private Vector2 localSize;
    private RectTransform rectTransform;

	void Start () {
        Rect parentBounds = transform.parent.GetComponent<RectTransform>().rect;
        localSize = new Vector2(parentBounds.width / 2.0f, parentBounds.height / 2.0f);
        rectTransform = GetComponent<RectTransform>();
	}
	
	void Update () {
        Vector2 followLocation = new Vector2(followTransform.position.x, followTransform.position.z);
        followLocation.x = Mathf.Clamp(followLocation.x, -worldSize.x, worldSize.x) / worldSize.x * localSize.x;
        followLocation.y = Mathf.Clamp(followLocation.y, -worldSize.y, worldSize.y) / worldSize.y * localSize.y;
        float angle = followTransform.rotation.eulerAngles.y;

        rectTransform.anchoredPosition = followLocation;
        rectTransform.rotation = Quaternion.AngleAxis(-angle, new Vector3(0.0f,0.0f,1.0f));
        Debug.Log(followLocation);
	}
}
