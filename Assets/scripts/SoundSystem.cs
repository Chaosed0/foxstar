using UnityEngine;
using System.Collections;

public class SoundSystem : MonoBehaviour {
    public AudioSource[] sources;

    private int current = 0;

    public void PlaySound(AudioClip clip) {
        AudioSource source = sources[current];
        source.clip = clip;
        source.loop = false;
        source.Play();
        current = (current+1) % sources.Length;
    }
}
