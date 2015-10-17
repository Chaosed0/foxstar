using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MultiPlayerJoinHandler: MonoBehaviour {
    public Camera initialCamera;
    public TerrainGenerator terrain;
    public CanvasGroup hideOnReady;

    public Transform shipPrefab;
    public Transform cameraPrefab;
    public Canvas HUDPrefab;

    public RectTransform minimap;
    public MinimapIcon[] minimapIcons;

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
        Vector3 elevatedPosition = position + new Vector3(0.0f, terrain.GetElevation(position.x, position.z) + 30.0f, 0.0f);
        Quaternion rotation = Quaternion.LookRotation(Vector3.zero - position, Vector3.up);
        Transform ship = Instantiate(shipPrefab, elevatedPosition, rotation) as Transform;
        Vector3 cameraPosition = elevatedPosition - elevatedPosition.normalized * 20.0f;
        Transform camera = Instantiate(cameraPrefab, cameraPosition, rotation) as Transform;

        CameraFollow follower = camera.GetComponent<CameraFollow>();
        follower.ship = ship.GetComponent<ShipMotor>();
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

        /* Only see the player's own reticles */
        cam.cullingMask = cam.cullingMask | (1 << (28 + playerNum));

        /* Add a minimap icon for this player */
        MinimapIcon icon = Instantiate(minimapIcons[playerNum], Vector3.zero, Quaternion.identity) as MinimapIcon;
        icon.followTransform = ship;
        icon.worldSize = new Vector2(1000.0f, 1000.0f);
        icon.transform.SetParent(minimap.transform);

        Canvas HUD = Instantiate(HUDPrefab, Vector3.zero, Quaternion.identity) as Canvas;
        HUD.worldCamera = cam;
        HUD.transform.Find("HealthSlider").GetComponent<HealthSlider>().ship = ship.GetComponent<Ship>();

        playerNum++;
    }

    void OnAllPlayersReady() {
        for (int i = 0; i < playerNum; i++) {
            PlayerInput input = inputs[i];
            input.enabled = true;
            //input.GetComponent<ShipMotor>().Boost();
        }

        hideOnReady.alpha = 0.0f;
        eventer.enabled = false;
    }
}
