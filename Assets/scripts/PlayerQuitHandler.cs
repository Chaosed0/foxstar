using UnityEngine;
using System.Collections;
using TeamUtility.IO;

public class PlayerQuitHandler : MonoBehaviour {
    public float holdTime = 1.0f;
    private float holdTimer = 0.0f;

    private bool quitting = false;
	
	void Update () {
        bool startQuit = InputManager.GetButtonDown("kCancel");
        bool stopQuit = InputManager.GetButtonUp("kCancel");

        if (startQuit) {
            quitting = true;
            holdTimer = 0.0f;
        } else if (stopQuit) {
            quitting = false;
        }

        if (quitting) {
            holdTimer += Time.deltaTime;
        }

        if (holdTimer >= holdTime) {
            Application.Quit();
        }
	}
}
