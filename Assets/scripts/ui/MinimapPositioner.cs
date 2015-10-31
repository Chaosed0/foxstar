using UnityEngine;
using System.Collections;

public class MinimapPositioner : MonoBehaviour {
    private int numPlayers = 0;
    private RectTransform minimap;
    private CanvasGroup minimapGroup;
    private RectTransform parentTransform;

    public PlayerJoinEventer eventer;
    public bool verticalSplit = false;

	void Start () {
        minimap = GetComponent<RectTransform>();
        minimapGroup = GetComponent<CanvasGroup>();
        parentTransform = transform.parent.GetComponent<RectTransform>();
        eventer.OnPlayerJoined += OnPlayerJoined;
	}

    void positionAtBottomCenter() {
        minimap.anchorMin = new Vector2(0.5f, 0.0f);
        minimap.anchorMax = new Vector2(0.5f, 0.0f);
        minimap.pivot = new Vector2(0.5f, 0.0f);
        minimap.anchoredPosition = new Vector2(0.0f, minimap.anchoredPosition.y);
    }

    void positionAtCenterRight() {
        minimap.anchorMin = new Vector2(0.0f, 0.5f);
        minimap.anchorMax = new Vector2(0.0f, 0.5f);
        minimap.pivot = new Vector2(1.0f, 0.5f);
        minimap.anchoredPosition = new Vector2(parentTransform.sizeDelta.x - minimap.anchoredPosition.x, 0.0f);
    }

    void positionAtCenter() {
        minimap.anchorMin = new Vector2(0.5f, 0.5f);
        minimap.anchorMax = new Vector2(0.5f, 0.5f);
        minimap.pivot = new Vector2(0.5f, 0.5f);
        minimap.anchoredPosition = new Vector2(0.0f, 0.0f);
    }

    void OnPlayerJoined(string playerPrefix) {
        /* For single-player, take initial minimap position */
        numPlayers++;
        if (numPlayers == 1) {
            minimapGroup.alpha = 1.0f;
        } else if (numPlayers == 2) {
            if (verticalSplit) {
                positionAtBottomCenter();
            } else {
                positionAtCenterRight();
            }
        } else if (numPlayers == 3) {
            positionAtCenter();
        }
        /* For 4 players, just keep it in the center */
    }
}
