using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental;
using UnityEngine;

public class HealthPickup : MonoBehaviour {

    [SerializeField] private int healthAmount = 50;
    [SerializeField] private float pickupRespawnTime = 15.0f;
    [SerializeField] private GameObject healthPickupObject;
    private Collider healthCollider;

    private void Start() {
        healthCollider = GetComponent<Collider>();
    }

    private IEnumerator OnTriggerEnter(Collider other) {
        var player = other.GetComponent<Player>();
        if (player) {
            player.Heal(healthAmount);
        }
        healthCollider.enabled = false;
        healthPickupObject.SetActive(false);
        yield return new WaitForSeconds(pickupRespawnTime);
        healthCollider.enabled = true;
        healthPickupObject.SetActive(true);
    }
}
