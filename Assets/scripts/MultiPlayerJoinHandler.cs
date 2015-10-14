using UnityEngine;
using System.Collections;

public class MultiPlayerJoinHandler: MonoBehaviour {
    public Camera initialCamera;

    public Transform shipPrefab;
    public Transform cameraPrefab;
    public CanvasGroup hideOnReady;

    private int playerNum = 0;
    private int maxPlayers = 4;
    private PlayerJoinEventer eventer;
    private Camera topLeftView, topRightView, botLeftView, botRightView;
    private PlayerInput[] inputs;

    private Vector3[] initialShipPositions = {
        new Vector3(1000.0f, 0.0f, 1000.0f),
        new Vector3(-1000.0f, 0.0f, -1000.0f),
        new Vector3(1000.0f, 0.0f, -1000.0f),
        new Vector3(-1000.0f, 0.0f, 1000.0f),
    };

	void Start() {
        eventer = GetComponent<PlayerJoinEventer>();
        eventer.OnPlayerJoined += OnPlayerJoined;
        eventer.OnAllPlayersReady += OnAllPlayersReady;
        inputs = new PlayerInput[maxPlayers];
	}

    void OnPlayerJoined(string playerPrefix) {
        /* Create a camera and a ship for this player */
        Vector3 position = initialShipPositions[playerNum];
        Quaternion rotation = Quaternion.LookRotation(-position, Vector3.up);
        Transform ship = Instantiate(shipPrefab, position, rotation) as Transform;
        Vector3 cameraPosition = position + new Vector3(Mathf.Sign(position.x) * 20.0f, 0.0f, Mathf.Sign(position.z) * 20.0f);
        Transform camera = Instantiate(cameraPrefab, cameraPosition, rotation) as Transform;

        CameraFollow follower = camera.GetComponent<CameraFollow>();
        follower.lookAt = ship;
        follower.followPoint = ship.Find("CameraTarget");

        PlayerInput input = ship.GetComponent<PlayerInput>();
        input.SetPlayerPrefix(playerPrefix);
        inputs[playerNum] = input;
        input.enabled = false;

        Camera cam = camera.GetComponent<Camera>();
        if (playerNum == 0) {
            initialCamera.enabled = false;
            topLeftView = cam;
        } else if (playerNum == 1) {
            topRightView = cam;
            topLeftView.rect = new Rect(0.0f, 0.0f, 0.5f, 1.0f);
            topRightView.rect = new Rect(0.5f, 0.0f, 0.5f, 1.0f);
        } else if (playerNum == 2) {
            botLeftView = cam;
            topLeftView.rect = new Rect(0.0f, 0.5f, 0.5f, 0.5f);
            botLeftView.rect = new Rect(0.0f, 0.0f, 0.5f, 0.5f);
        } else if (playerNum == 3) {
            botRightView = cam;
            topRightView.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
            botRightView.rect = new Rect(0.5f, 0.0f, 0.5f, 0.5f);
        }
        playerNum++;
    }

    void OnAllPlayersReady() {
        for (int i = 0; i < playerNum; i++) {
            PlayerInput input = inputs[i];
            input.enabled = true;
            input.GetComponent<ShipMotor>().Boost();
        }

        hideOnReady.alpha = 0.0f;
        eventer.enabled = false;
    }
}
