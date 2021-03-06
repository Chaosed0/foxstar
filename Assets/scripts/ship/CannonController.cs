﻿using UnityEngine;
using System.Collections;

public class CannonController : MonoBehaviour {
    public Laser laserPrefab;
    public float cooldown = 0.2f;
    public Transform[] locations;
    public Transform player;
    public float bulletSpeed = 100.0f;

    private float cooldownTimer = 0.2f;
    private bool firing = false;
    private Collider[] colliders;
    private ShipMotor motor;
    private Ship ship;

    public delegate void Shoot();
    public event Shoot OnShoot;

    public delegate void StartShooting();
    public event StartShooting OnStartShooting;

    public delegate void StopShooting();
    public event StopShooting OnStopShooting;

    void Start() {
        colliders = player.GetComponents<Collider>();
        motor = GetComponent<ShipMotor>();
        ship = GetComponent<Ship>();
    }

    public void setFiring(bool firing) {
        if (!this.firing && firing && OnStartShooting != null) {
            OnStartShooting();
        } else if (this.firing && !firing && OnStopShooting != null) {
            OnStopShooting();
        }
        this.firing = firing;
    }

    public bool isFiring() {
        return this.firing;
    }

    void Update() {
        if (cooldownTimer < cooldown) {
            cooldownTimer += Time.deltaTime;
        } else if (firing) {
            cooldownTimer = 0.0f;
            for (int i = 0; i < locations.Length; i++) {
                Laser laser = Instantiate(laserPrefab, locations[i].position, locations[i].rotation) as Laser;
                laser.owner = ship;

                /* Initialize the laser's speed */
                Rigidbody laserBody = laser.GetComponent<Rigidbody>();
                laserBody.velocity = transform.forward * motor.getCurrentSpeed() + transform.forward.normalized * bulletSpeed;
                
                /* Ignore collisions with the firing ship */
                foreach (Collider collider in colliders) {
                    Physics.IgnoreCollision(laser.transform.GetChild(0).GetComponent<Collider>(), collider);
                }
            }

            if (OnShoot != null) {
                OnShoot();
            }
        }
    }
}
