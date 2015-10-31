using UnityEngine;
using System.Collections;

public class SoundSystem : MonoBehaviour {
    public int priorities = 3;
    public int sourcesPerPriority = 20;
    public AudioSource audioSourcePrefab;

    private int[] current;
    private AudioSource[,] sources;

    void Start() {
        sources = new AudioSource[priorities,sourcesPerPriority];
        for (int i = 0; i < priorities; i++) {
            for (int j = 0; j < sourcesPerPriority; j++) {
                AudioSource source = Instantiate(audioSourcePrefab, transform.position, transform.rotation) as AudioSource;
                source.priority = GetPriority(i);
                source.transform.parent = this.transform;
                sources[i,j] = source;
            }
        }

        current = new int[priorities];
        for (int i = 0; i < priorities; i++) {
            current[i] = 0;
        }
    }

    private int GetPriority(int priority) {
        return 128 - priorities + priority;
    }

    private AudioSource GetNextSource(int priority) {
        return GetSource(priority, current[priority]);
    }

    private AudioSource GetSource(int priority, int id) {
        return sources[priority, id%sourcesPerPriority];
    }

    public int PlaySound(AudioClip clip, int priority) {
        AudioSource source = GetNextSource(priority);
        source.clip = clip;
        source.loop = false;
        source.priority = GetPriority(priority);
        source.Play();

        int id = current[priority]++;
        return id;
    }

    public void StopSound(int id, int priority) {
        /* This sound has already been stopped if it was played long enough
         * ago */
        if (current[priority] - id <= sources.Length) {
            GetSource(priority, id).Stop();
        }
    }

    public int MinPriority() {
        return priorities-1;
    }

    public int MaxPriority() {
        return 0;
    }
}
