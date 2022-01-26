using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [SerializeField] private int ammoAmount = 60;
    [SerializeField] private float pickupRespawnTime = 20.0f;
    [SerializeField] private GameObject ammoPickupObject;
    private Collider ammoCollider;
    private AudioSource ammoAudioSource;
    [SerializeField] private AudioClip ammoPickupClip;

    private void Start() {
        ammoCollider = GetComponent<Collider>();
        ammoAudioSource = GetComponent<AudioSource>();
    }

    private IEnumerator OnTriggerEnter(Collider other) {
        var gunShot = other.GetComponent<GunShot>();
        if (gunShot.currentReloadAmmo != gunShot.maxReloadAmmo) {
            gunShot.AddAmmo(ammoAmount);
            ammoAudioSource.PlayOneShot(ammoPickupClip);
            ammoCollider.enabled = false;
            ammoPickupObject.SetActive(false);
            yield return new WaitForSeconds(pickupRespawnTime);
            ammoCollider.enabled = true;
            ammoPickupObject.SetActive(true);
        }
    }
}
