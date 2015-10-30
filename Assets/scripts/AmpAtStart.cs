using UnityEngine;
using System.Collections;

[RequireComponent (typeof(AudioSource))]
public class AmpAtStart : MonoBehaviour {
    public PlayerJoinEventer eventer;
    public float fadeInTime = 1.0f;
    public float target = 0.8f;

    private AudioSource audioSource;
    private float fadeInTimer = 0.0f;
    private float originalVolume; 
    private bool fadeIn = false;

	void Start() {
        audioSource = GetComponent<AudioSource>();
        eventer.OnAllPlayersReady += OnAllPlayersReady;
        originalVolume = audioSource.volume;
	}

    void Update() {
        if (!fadeIn) {
            return;
        }

        fadeInTimer += Time.deltaTime;
        audioSource.volume = originalVolume + (target - originalVolume) * (1.0f - (fadeInTime - fadeInTimer) / fadeInTime);

        if (fadeInTimer > fadeInTime) {
            fadeIn = false;
        }
    }

    void OnAllPlayersReady() {
        fadeIn = true;
        fadeInTimer = 0.0f;
        originalVolume = audioSource.volume;
    }
}
