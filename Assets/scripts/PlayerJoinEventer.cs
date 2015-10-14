using UnityEngine;
using System.Collections;
using TeamUtility.IO;

public class PlayerJoinEventer : MonoBehaviour {
    string[] playerPrefixes = {"k", "j1", "j2", "j3", "j4"};
    bool[] playerPrefixJoined = {false, false, false, false, false};
    int playersJoined = 0;

    public delegate void PlayerJoined(string playerPrefix);
    public event PlayerJoined OnPlayerJoined;

    public delegate void AllPlayersReady();
    public event AllPlayersReady OnAllPlayersReady;
	
	void Update () {
        for (int i = 0; i < playerPrefixes.Length; i++) {
            if (playerPrefixJoined[i]) {
                continue;
            }

            if (InputManager.GetButtonDown(playerPrefixes[i] + "Join")) {
                playerPrefixJoined[i] = true;
                playersJoined++;
                if (OnPlayerJoined != null) {
                    OnPlayerJoined(playerPrefixes[i]);
                }
            }
        }

        int readyCount = 0;
        for (int i = 0; i < playerPrefixes.Length; i++) {
            if (playerPrefixJoined[i] && InputManager.GetButton(playerPrefixes[i] + "Fire")) {
                readyCount++;
            }
        }

        if (playersJoined > 0 && playersJoined == readyCount && OnAllPlayersReady != null) {
            OnAllPlayersReady();
        }
	}
}
