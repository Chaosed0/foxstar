using UnityEngine;
using System.Collections;

public class PlayerSoundController : MonoBehaviour {
    public SoundSystem soundSystem;
    public AudioClip[] shootSounds;

    private CannonController cannon;

	void Start () {
        cannon = GetComponent<CannonController>();
        cannon.OnShoot += OnShoot;

        if (soundSystem == null) {
            this.enabled = false;
        }
	}

    void OnShoot() {
        if (this.enabled) {
            soundSystem.PlaySound(shootSounds[(int)Random.Range(0, shootSounds.Length)]);
        }
    }
}
