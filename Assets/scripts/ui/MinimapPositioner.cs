using UnityEngine;
using System.Collections;

public class MinimapPositioner : MonoBehaviour {
    private int numPlayers = 0;
    private RectTransform minimap;
    private CanvasGroup minimapGroup;
    public PlayerJoinEventer eventer;

	void Start () {
        minimap = GetComponent<RectTransform>();
        minimapGroup = GetComponent<CanvasGroup>();
        eventer.OnPlayerJoined += OnPlayerJoined;
	}

    void OnPlayerJoined(string playerPrefix) {
        /* For single-player, take initial minimap position */
        if (numPlayers == 0) {
            minimapGroup.alpha = 1.0f;
        } else if (numPlayers == 1) {
            minimap.anchorMin = new Vector2(0.5f, 0.0f);
            minimap.anchorMax = new Vector2(0.5f, 0.0f);
            minimap.pivot = new Vector2(0.5f, 0.0f);
            minimap.anchoredPosition = new Vector2(0.0f, minimap.anchoredPosition.y);
        } else if (numPlayers == 2) {
            minimap.anchorMin = new Vector2(0.5f, 0.5f);
            minimap.anchorMax = new Vector2(0.5f, 0.5f);
            minimap.pivot = new Vector2(0.5f, 0.5f);
            minimap.anchoredPosition = new Vector2(0.0f, 0.0f);
        }
        /* For 4 players, just keep it in the center */
        numPlayers++;
    }
}
