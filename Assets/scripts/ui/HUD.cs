using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HUD : MonoBehaviour {
    public Ship ship;
    public Slider healthSlider;
    public Slider boostSlider;
    public float margin = 10.0f;
    public float boostMargin = 16.4f;

    public float minIconDistance = 100.0f;
    public float maxIconDistance = 2400.0f;
    public float minIconSize = 8.0f;
    public float maxIconSize = 24.0f;
    public RectTransform playerIconPrefab;

    private ShipMotor motor;
    private Canvas canvas;
    private RectTransform rectTransform;
    private List<RectTransform> playerIcons = new List<RectTransform>();
    private List<Transform> players = new List<Transform>();

	void Start () {
        motor = ship.GetComponent<ShipMotor>();
        canvas = GetComponent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        ship.OnHealthChange += OnHealthChange;
        motor.OnBoostChange += OnBoostChange;
	}

    public void Finish() {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++) {
            if (players[i].GetComponent<Ship>() != ship) {
                this.players.Add(players[i].transform);

                RectTransform icon = Instantiate(playerIconPrefab, Vector3.zero, Quaternion.identity) as RectTransform;
                icon.SetParent(this.transform);
                icon.transform.localPosition = Vector3.zero;
                icon.transform.localRotation = Quaternion.identity;
                icon.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                playerIcons.Add(icon);
            }
        }
    }

    void LateUpdate() {
        Camera camera = canvas.worldCamera;

        for (int i = 0; i < players.Count; i++) {
            Transform player = players[i].transform;
            Vector3 position = camera.WorldToViewportPoint(player.position);

            if (position.z > 0) {
                Vector2 hudPosition = new Vector2(position.x*rectTransform.sizeDelta.x, position.y*rectTransform.sizeDelta.y);
                float size = (1.0f - Mathf.Clamp((position.z - minIconDistance) / (maxIconDistance - minIconDistance), 0.0f, 1.0f)) *
                    (maxIconSize - minIconSize) + minIconSize;
                playerIcons[i].anchoredPosition = hudPosition;
                playerIcons[i].sizeDelta = new Vector2(size, size);
            } else {
                /* offscreen */
                playerIcons[i].anchoredPosition = new Vector2(-100.0f, -100.0f);
            }
        }
    }

    void OnHealthChange(int health, int change) {
        healthSlider.value = health / (float)ship.maxHealth;
    }

    void OnBoostChange(float boost) {
        boostSlider.value = boost / (float)motor.boostTime;
    }

    public void SetOrientation(SplitHelper.Orientation orientation) {
        RectTransform healthTransform = healthSlider.GetComponent<RectTransform>();
        RectTransform boostTransform = boostSlider.GetComponent<RectTransform>();
        Vector2 anchor = Vector2.zero;
        Vector2 position = Vector2.zero;
        switch (orientation) {
            case SplitHelper.Orientation.CENTER:
            case SplitHelper.Orientation.RIGHT:
            case SplitHelper.Orientation.TOP_RIGHT:
                anchor = new Vector2(1.0f, 0.0f);
                position = new Vector2(-margin, margin);
                break;
            case SplitHelper.Orientation.LEFT:
            case SplitHelper.Orientation.TOP_LEFT:
            case SplitHelper.Orientation.TOP:
                anchor = new Vector2(0.0f, 0.0f);
                position = new Vector2(margin, margin);
                break;
            case SplitHelper.Orientation.BOT_RIGHT:
                anchor = new Vector2(1.0f, 1.0f);
                position = new Vector2(-margin, -margin);
                break;
            case SplitHelper.Orientation.BOT_LEFT:
            case SplitHelper.Orientation.BOTTOM:
                anchor = new Vector2(0.0f, 1.0f);
                position = new Vector2(margin, -margin);
                break;
        }
        healthTransform.anchorMin = anchor;
        healthTransform.anchorMax = anchor;
        healthTransform.pivot = anchor;
        healthTransform.anchoredPosition = position;

        boostTransform.anchorMin = anchor;
        boostTransform.anchorMax = anchor;
        boostTransform.pivot = anchor;
        boostTransform.anchoredPosition = position + new Vector2(0.0f, Mathf.Sign(position.y) * boostMargin);
    }
}
