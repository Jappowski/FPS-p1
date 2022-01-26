using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [SerializeField] private int ammoAmount = 60;
    [SerializeField] private float pickupRespawnTime = 20.0f;
    [SerializeField] private GameObject ammoPickupObject;
    private Collider ammoCollider;

    private void Start() {
        ammoCollider = GetComponent<Collider>();
    }

    private IEnumerator OnTriggerEnter(Collider other) {
        var gunShot = other.GetComponent<GunShot>();
        if (gunShot) {
            gunShot.AddAmmo(ammoAmount);
        }
        ammoCollider.enabled = false;
        ammoPickupObject.SetActive(false);
        yield return new WaitForSeconds(pickupRespawnTime);
        ammoCollider.enabled = true;
        ammoPickupObject.SetActive(true);
    }
}
