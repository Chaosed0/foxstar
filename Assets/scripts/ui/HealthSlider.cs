using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthSlider : MonoBehaviour {
    public Ship ship;
    private Slider slider;

	void Start () {
        slider = GetComponent<Slider>();
        ship.OnHealthChange += OnHealthChange;
	}

    void OnHealthChange(int health, int change) {
        slider.value = health / (float)ship.maxHealth;
    }
}
