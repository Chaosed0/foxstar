using UnityEngine;
using System.Collections;

public class SinglePlayerJoinHandler: MonoBehaviour {
    private PlayerJoinEventer eventer;
    public PlayerInput input;
    public ShipMotor motor;
    public CanvasGroup group;

	void Start() {
        Time.timeScale = 0.0f;
        eventer = GetComponent<PlayerJoinEventer>();
        eventer.OnPlayerJoined += OnPlayerJoined;
	}

    void OnPlayerJoined(string playerPrefix) {
        Time.timeScale = 1.0f;
        group.alpha = 0.0f;
        input.SetPlayerPrefix(playerPrefix);
        eventer.enabled = false;
    }
}
