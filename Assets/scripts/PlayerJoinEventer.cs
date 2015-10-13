using UnityEngine;
using System.Collections;
using TeamUtility.IO;

public class PlayerJoinEventer : MonoBehaviour {
    string[] playerPrefixes = {"k", "j1", "j2", "j3", "j4"};
    bool[] playerPrefixJoined = {false, false, false, false, false};

    public delegate void PlayerJoined(string playerPrefix);
    public event PlayerJoined OnPlayerJoined;
	
	void Update () {
        for (int i = 0; i < playerPrefixes.Length; i++) {
            if (playerPrefixJoined[i]) {
                continue;
            }

            if (InputManager.GetButtonDown(playerPrefixes[i] + "Join")) {
                playerPrefixJoined[i] = true;
                if (OnPlayerJoined != null) {
                    OnPlayerJoined(playerPrefixes[i]);
                }
            }
        }
	}
}
