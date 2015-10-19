using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUD : MonoBehaviour {
    public Ship ship;
    public Slider healthSlider;
    public Slider boostSlider;
    public float margin = 10.0f;

    private ShipMotor motor;

	void Start () {
        motor = ship.GetComponent<ShipMotor>();
        ship.OnHealthChange += OnHealthChange;
        motor.OnBoostChange += OnBoostChange;
	}

    void OnHealthChange(int health, int change) {
        healthSlider.value = health / (float)ship.maxHealth;
    }

    void OnBoostChange(float boost) {
        boostSlider.value = boost / (float)motor.boostTime;
    }

    public void SetOrientation(SplitHelper.Orientation orientation) {
        RectTransform sliderTransform = healthSlider.GetComponent<RectTransform>();
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
                anchor = new Vector2(0.0f, 0.0f);
                position = new Vector2(margin, margin);
                break;
            case SplitHelper.Orientation.BOT_RIGHT:
                anchor = new Vector2(1.0f, 1.0f);
                position = new Vector2(-margin, -margin);
                break;
            case SplitHelper.Orientation.BOT_LEFT:
                anchor = new Vector2(0.0f, 1.0f);
                position = new Vector2(margin, -margin);
                break;
        }
        sliderTransform.anchorMin = anchor;
        sliderTransform.anchorMax = anchor;
        sliderTransform.pivot = anchor;
        sliderTransform.anchoredPosition = position;
    }
}
