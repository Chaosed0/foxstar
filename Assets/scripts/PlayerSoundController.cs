using UnityEngine;
using System.Collections;

public class PlayerSoundController : MonoBehaviour {
    public SoundSystem soundSystem;

    public AudioClip shootSound;
    public AudioClip onBoostSound;
    public AudioClip whileBoostSound;
    public AudioClip stopBoostSound;
    public AudioClip hitSound;
    public AudioClip deadSound;

    int boostSoundId = -1;

    private CannonController cannon;
    private ShipMotor motor;
    private Ship ship;

    private int shootSoundPriority;
    private int boostSoundPriority;

    void Start () {
        if (soundSystem == null) {
            this.enabled = false;
            return;
        }

        cannon = GetComponent<CannonController>();
        motor = GetComponent<ShipMotor>();
        ship = GetComponent<Ship>();

        Bind();

        shootSoundPriority = soundSystem.MinPriority();
        boostSoundPriority = soundSystem.MaxPriority();
    }

    void Bind () {
        ship.OnHealthChange += OnHealthChange;
        ship.OnDead += OnDead;
        cannon.OnShoot += OnShoot;
        motor.OnStartBoost += OnStartBoost;
        motor.OnStopBoost += OnStopBoost;
        motor.OnAttemptedBoost += OnAttemptedBoost;
    }

    void OnShoot() {
        soundSystem.PlaySound(shootSound, shootSoundPriority);
    }

    void OnStartBoost() {
        boostSoundId = soundSystem.PlaySound(whileBoostSound, boostSoundPriority);
        soundSystem.PlaySound(onBoostSound, boostSoundPriority);
    }

    void OnStopBoost() {
        soundSystem.StopSound(boostSoundId, boostSoundPriority);
        soundSystem.PlaySound(stopBoostSound, boostSoundPriority);
    }

    void OnAttemptedBoost() {
        return;
    }

    void OnHealthChange(int health, int change) {
        if (change < 0) {
            soundSystem.PlaySound(hitSound, soundSystem.MaxPriority());
        }
    }

    void OnDead() {
        soundSystem.PlaySound(deadSound, soundSystem.MaxPriority());
    }
}
