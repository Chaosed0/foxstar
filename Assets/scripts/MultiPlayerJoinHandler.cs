using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class MultiPlayerJoinHandler: MonoBehaviour {
    public Camera initialCamera;
    public TerrainGenerator terrain;
    public CanvasGroup hideOnReady;
    public SoundSystem soundSystem;

    public Transform shipPrefab;
    public Transform cameraPrefab;
    public HUD HUDPrefab;

    public RectTransform minimap;
    public MinimapIcon[] minimapIcons;
    public Material[] shieldMats;

    public bool spawnAI = true;

    private int playerNum = 0;
    private int maxPlayers = 4;
    private float maxHeight = Int32.MinValue;
    private PlayerJoinEventer eventer;
    private PlayerInput[] inputs;
    private SplitHelper splitHelper;

    private Vector3[] initialShipPositions = {
        new Vector3(Constants.worldSize/2.0f, 0.0f, Constants.worldSize/2.0f),
        new Vector3(-Constants.worldSize/2.0f, 0.0f, -Constants.worldSize/2.0f),
        new Vector3(Constants.worldSize/2.0f, 0.0f, -Constants.worldSize/2.0f),
        new Vector3(-Constants.worldSize/2.0f, 0.0f, Constants.worldSize/2.0f),
    };

	void Start() {
        eventer = GetComponent<PlayerJoinEventer>();
        eventer.OnPlayerJoined += OnPlayerJoined;
        eventer.OnAllPlayersReady += OnAllPlayersReady;
        inputs = new PlayerInput[maxPlayers];
        splitHelper = new SplitHelper();
	}

    /* Creates a ship, adding only components that an AI would have */
    Transform CreateShip() {
        if (maxHeight == Int32.MinValue) {
            maxHeight = terrain.GetMaxHeightIn(new Vector2(Constants.worldSize, Constants.worldSize)) +
                Constants.minimumFlyableSpace;
        }

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

        /* Put the crosses into their own layer so they aren't rendered on other cameras */
        Transform cross1 = shipTransform.Find("cross1");
        Transform cross2 = shipTransform.Find("cross2");
        cross1.gameObject.layer = 28 + playerNum;
        cross2.gameObject.layer = 28 + playerNum;

        /* Set the ship's maximum flyable height */
        shipTransform.GetComponent<ShipMotor>().maxHeight = maxHeight;

        return shipTransform;
    }

    MinimapIcon CreateIcon(Transform shipTransform) {
        /* Add a minimap icon for this ship */
        MinimapIcon icon = Instantiate(minimapIcons[playerNum], Vector3.zero, Quaternion.identity) as MinimapIcon;
        icon.followTransform = shipTransform;
        icon.worldSize = new Vector2(Constants.worldSize, Constants.worldSize);
        icon.transform.SetParent(minimap.transform);

        return icon;
    }

    void OnPlayerJoined(string playerPrefix) {
        if (playerNum >= maxPlayers) {
            /* Just ignore */
            return;
        }

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
        Ship ship = shipTransform.GetComponent<Ship>();
        follower.ship = ship;
        follower.lookAt = shipTransform;
        follower.followPoint = shipTransform.Find("CameraTarget");

        /* Add a HUD for this player */
        HUD hud = Instantiate(HUDPrefab, Vector3.zero, Quaternion.identity) as HUD;
        hud.ship = ship;

        /* Position the camera and canvas correctly on-screen */
        Camera camera = cameraTransform.GetComponent<Camera>();
        splitHelper.addCamera(camera, hud);
        /* Only render the player's own reticles */
        camera.cullingMask = camera.cullingMask | (1 << (28 + playerNum));

        /* Initialize the PlayerInput and disable it until everyone's ready */
        PlayerInput input = shipTransform.GetComponent<PlayerInput>();
        input.SetPlayerPrefix(playerPrefix);
        inputs[playerNum] = input;
        input.enabled = false;

        /* Initialize the sound controller */
        PlayerSoundController soundController = shipTransform.GetComponent<PlayerSoundController>();
        soundController.soundSystem = soundSystem;

        /* Initialize respawn handler */
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
