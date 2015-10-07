using UnityEngine;
using System.Collections;

public class PlayerInput : MonoBehaviour {
    private ShipMotor motor;
    private CannonController cannon;

	private void Start () {
        motor = GetComponent<ShipMotor>();
        cannon = GetComponent<CannonController>();

        lockCursor();
	}

	private void Update () {
        float hmove = Input.GetAxis("Horizontal");
        float vmove = Input.GetAxis("Vertical");
        float hlook = Input.GetAxis("Mouse X");
        float vlook = Input.GetAxis("Mouse Y");
        bool startFire = (bool)Input.GetButtonDown("Fire1");
        bool stopFire = (bool)Input.GetButtonUp("Fire1");
        bool cancel = (bool)Input.GetButtonDown("Cancel");

        if (startFire) {
            lockCursor();
        }

        if (cancel) {
            unlockCursor();
        }

        motor.Move(vmove, hlook, vlook, hmove);

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
}
