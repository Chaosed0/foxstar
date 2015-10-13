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
        float yaw = InputManager.GetAxis(controllerPrefix + "Yaw");
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

        motor.SetMovement(throttle - reverse, yaw, pitch, roll);

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
