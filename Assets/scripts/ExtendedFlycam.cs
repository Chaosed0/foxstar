using UnityEngine;
using System.Collections;
using TeamUtility.IO;
 
public class ExtendedFlycam : MonoBehaviour
{
 
    /*
    EXTENDED FLYCAM
        Desi Quintans (CowfaceGames.com), 17 August 2012.
        Based on FlyThrough.js by Slin (http://wiki.unity3d.com/index.php/FlyThrough), 17 May 2011.
 
    LICENSE
        Free as in speech, and free as in beer.
 
    FEATURES
        WASD/Arrows:    Movement
                  Q:    Climb
                  E:    Drop
                      Shift:    Move faster
                    Control:    Move slower
                        End:    Toggle cursor locking to screen (you can also press Ctrl+P to toggle play mode on and off).
    */
 
    public float cameraSensitivity = 90;
    public float climbSpeed = 4;
    public float normalMoveSpeed = 10;
    public float slowMoveFactor = 0.25f;
    public float fastMoveFactor = 3;
 
    private float rotationX = 0.0f;
    private float rotationY = 0.0f;
 
    void Start ()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
 
    void Update ()
    {
        rotationX += InputManager.GetAxis("LookHorizontal") * cameraSensitivity * Time.deltaTime;
        rotationY += InputManager.GetAxis("LookVertical") * cameraSensitivity * Time.deltaTime;
        rotationY = Mathf.Clamp (rotationY, -90, 90);
 
        transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
        transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);
 
        if (InputManager.GetKey (KeyCode.LeftShift) || InputManager.GetKey (KeyCode.RightShift))
        {
            transform.position += transform.forward * (normalMoveSpeed * fastMoveFactor) * InputManager.GetAxis("Vertical") * Time.deltaTime;
            transform.position += transform.right * (normalMoveSpeed * fastMoveFactor) * InputManager.GetAxis("Horizontal") * Time.deltaTime;
        }
        else if (InputManager.GetKey (KeyCode.LeftControl) || InputManager.GetKey (KeyCode.RightControl))
        {
            transform.position += transform.forward * (normalMoveSpeed * slowMoveFactor) * InputManager.GetAxis("Vertical") * Time.deltaTime;
            transform.position += transform.right * (normalMoveSpeed * slowMoveFactor) * InputManager.GetAxis("Horizontal") * Time.deltaTime;
        }
        else
        {
            transform.position += transform.forward * normalMoveSpeed * InputManager.GetAxis("Vertical") * Time.deltaTime;
            transform.position += transform.right * normalMoveSpeed * InputManager.GetAxis("Horizontal") * Time.deltaTime;
        }
 
 
        if (InputManager.GetKey (KeyCode.Q)) {transform.position += transform.up * climbSpeed * Time.deltaTime;}
        if (InputManager.GetKey (KeyCode.E)) {transform.position -= transform.up * climbSpeed * Time.deltaTime;}
 
        if (InputManager.GetKeyDown (KeyCode.End))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
