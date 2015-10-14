using UnityEngine;
using System.Collections;
using TeamUtility.IO;

public class PlayerInput : MonoBehaviour {
    private ShipMotor motor;
    private CannonController cannon;
    private string controllerPrefix = "k";

	private void Start () {
        motor = GetComponent<ShipMotor>();
        cannon = GetComponent<CannonController>();

        lockCursor();
	}

	private void Update () {
        float throttle = InputManager.GetAxis(controllerPrefix + "ThrottleForward");
        float reverse = InputManager.GetAxis(controllerPrefix + "ThrottleBackward");
        float pitch = InputManager.GetAxis(controllerPrefix + "Pitch");
        float roll = InputManager.GetAxis(controllerPrefix + "Roll");
        bool startFire = (bool)InputManager.GetButtonDown(controllerPrefix + "Fire");
        bool stopFire = (bool)InputManager.GetButtonUp(controllerPrefix + "Fire");
        bool cancel = (bool)InputManager.GetButtonDown(controllerPrefix + "Cancel");
        bool boost = (bool)InputManager.GetButtonDown(controllerPrefix + "Boost");

        if (startFire) {
            lockCursor();
        }

        if (cancel) {
            unlockCursor();
        }

        if (boost) {
            motor.Boost();
        }

        throttle = Mathf.Clamp(throttle, -1.0f, 1.0f);
        reverse = Mathf.Clamp(reverse, -1.0f, 1.0f);
        pitch = Mathf.Clamp(pitch, -1.0f, 1.0f);
        roll = Mathf.Clamp(roll, -1.0f, 1.0f);
        motor.SetMovement(throttle - reverse, pitch, roll);

        if (startFire) {
            cannon.setFiring(true);
        } else if (stopFire) {
            cannon.setFiring(false);
        }
	}

    private void lockCursor () {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void unlockCursor () {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void SetPlayerPrefix(string prefix) {
        controllerPrefix = prefix;
    }
}
