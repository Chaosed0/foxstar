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
        float throttle = InputManager.GetAxis(controllerPrefix + "Throttle");
        float pitch = InputManager.GetAxis(controllerPrefix + "Pitch");
        float roll = InputManager.GetAxis(controllerPrefix + "Roll");
        float tightRoll = InputManager.GetAxis(controllerPrefix + "TightRoll");
        bool startFire = (bool)InputManager.GetButtonDown(controllerPrefix + "Fire");
        bool stopFire = (bool)InputManager.GetButtonUp(controllerPrefix + "Fire");
        bool cancel = (bool)InputManager.GetButtonDown(controllerPrefix + "Cancel");
        bool startBoost = (bool)InputManager.GetButtonDown(controllerPrefix + "Boost");
        bool stopBoost = (bool)InputManager.GetButtonUp(controllerPrefix + "Boost");

        bool special = (bool)InputManager.GetButtonDown(controllerPrefix + "Special");

        if (special) {
            motor.SetManeuver(ShipMotor.Maneuvers.IMMELMANN);
        }

        if (startFire) {
            lockCursor();
        }

        if (cancel) {
            unlockCursor();
        }

        if (startBoost) {
            motor.Boost(true);
        } else if (stopBoost) {
            motor.Boost(false);
        }

        throttle = Mathf.Clamp(throttle, -1.0f, 1.0f);
        pitch = Mathf.Clamp(pitch, -1.0f, 1.0f);
        roll = Mathf.Clamp(roll, -1.0f, 1.0f);
        tightRoll = Mathf.Clamp(tightRoll, -1.0f, 1.0f);

        if (Mathf.Abs(roll) > Util.Epsilon &&
                Mathf.Sign(tightRoll) == Mathf.Sign(roll)) {
            roll += tightRoll;
        }

        motor.SetMovement(throttle, pitch, roll);

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
