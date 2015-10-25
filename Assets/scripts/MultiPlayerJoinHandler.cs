﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MultiPlayerJoinHandler: MonoBehaviour {
    public Camera initialCamera;
    public TerrainGenerator terrain;
    public CanvasGroup hideOnReady;

    public Transform shipPrefab;
    public Transform cameraPrefab;
    public HUD HUDPrefab;

    public RectTransform minimap;
    public MinimapIcon[] minimapIcons;
    public Material[] shieldMats;

    public bool spawnAI = true;

    private int playerNum = 0;
    private int maxPlayers = 4;
    private PlayerJoinEventer eventer;
    private PlayerInput[] inputs;
    private SplitHelper splitHelper;

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
        splitHelper = new SplitHelper();
	}

    Transform CreateShip() {
        /* Create a ship for this player */
        Vector3 position = initialShipPositions[playerNum];
        Vector3 elevatedPosition = position + new Vector3(0.0f, terrain.GetElevation(position.x, position.z) + 30.0f, 0.0f);
        Quaternion rotation = Quaternion.LookRotation(Vector3.zero - position, Vector3.up);
        Transform shipTransform = Instantiate(shipPrefab, elevatedPosition, rotation) as Transform;

        /* Set the ship's shield material */
        MeshRenderer mesh = shipTransform.Find("ShipModel").GetComponent<MeshRenderer>();
        /* How do we know these numbers? WE JUST DO (look in the inspector) */
        Material[] materials = mesh.materials;
        materials[2] = shieldMats[playerNum];
        materials[4] = shieldMats[playerNum];
        materials[5] = shieldMats[playerNum];
        mesh.materials = materials;

        return shipTransform;
    }

    MinimapIcon CreateIcon(Transform shipTransform) {
        /* Add a minimap icon for this ship */
        MinimapIcon icon = Instantiate(minimapIcons[playerNum], Vector3.zero, Quaternion.identity) as MinimapIcon;
        icon.followTransform = shipTransform;
        icon.worldSize = new Vector2(1200.0f, 1200.0f);
        icon.transform.SetParent(minimap.transform);

        return icon;
    }

    void OnPlayerJoined(string playerPrefix) {
        /* Regardless of who joined, do away with the initial camera */
        initialCamera.enabled = false;

        /* Create this player's ship */
        Transform shipTransform = CreateShip();

        /* Create an icon for the ship */
        CreateIcon(shipTransform);

        /* Create a camera for this player */
        Vector3 cameraPosition = shipTransform.position - shipTransform.position.normalized * 20.0f;
        Transform cameraTransform = Instantiate(cameraPrefab, cameraPosition, shipTransform.rotation) as Transform;

        /* Initialize the camera to follow the player's ship */
        CameraFollow follower = cameraTransform.GetComponent<CameraFollow>();
        ShipMotor motor = shipTransform.GetComponent<ShipMotor>();
        follower.ship = motor;
        follower.lookAt = shipTransform;
        follower.followPoint = shipTransform.Find("CameraTarget");

        /* Add a HUD for this player */
        HUD hud = Instantiate(HUDPrefab, Vector3.zero, Quaternion.identity) as HUD;
        Ship ship = shipTransform.GetComponent<Ship>();
        hud.ship = ship;

        /* Position the camera and canvas correctly on-screen */
        Camera camera = cameraTransform.GetComponent<Camera>();
        splitHelper.addCamera(camera, hud);
        /* Only render the player's own reticles */
        for (int i = 28; i < 32; i++) {
            if (playerNum != i) {
                camera.cullingMask = camera.cullingMask ^ (1 << (28 + playerNum));
            }
        }

        /* Initialize the PlayerInput and disable it until everyone's ready */
        PlayerInput input = shipTransform.GetComponent<PlayerInput>();
        input.SetPlayerPrefix(playerPrefix);
        inputs[playerNum] = input;
        input.enabled = false;

        /* Add respawn handler */
        ShipRespawnHandler respawnHandler = shipTransform.GetComponent<ShipRespawnHandler>();
        respawnHandler.terrain = terrain;

        playerNum++;
    }

    void OnAllPlayersReady() {
        for (int i = 0; i < playerNum; i++) {
            PlayerInput input = inputs[i];
            input.enabled = true;
        }

        if (playerNum == 1 && spawnAI) {
            Transform AIShipTransform = CreateShip();
            CreateIcon(AIShipTransform);
            ShipRespawnHandler respawnHandler = AIShipTransform.GetComponent<ShipRespawnHandler>();
            respawnHandler.terrain = terrain;
            Destroy(AIShipTransform.GetComponent<PlayerInput>());
            AIShipTransform.gameObject.AddComponent<AIFollowInput>();
        }

        splitHelper.Finish();
        hideOnReady.alpha = 0.0f;
    }
}
