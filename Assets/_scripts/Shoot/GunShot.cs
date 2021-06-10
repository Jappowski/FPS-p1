using System;
using System.Collections;
using Mirror;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GunShot : NetworkBehaviour
{
    private Text ammoUi;
   // public Animator animator;
    private GameObject canvas;
    
    public float currentAmmo;
    public float damage = 25f;
    public float fireRate = 15f;
    public Camera fpsCam;
    public GameObject hitEffect;
    public float impactForce = 100f;
    public static bool isReloading;
    public readonly float maxAmmo = 30f;
    public float maxReloadAmmo = 90f;
    public ParticleSystem muzzleFlash;
    public PlayerWeapon weapon;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] shootingClips;
    [SerializeField] private AudioClip reloadSound1;
    [SerializeField] private AudioClip reloadSound2;
    [SerializeField] private AudioClip reloadSound3;
    [SerializeField] private LayerMask mask;
    private float nextShoot;
    public float reloadTime = 3f;
    

    private void start()
    {
        currentAmmo = maxAmmo;
    }

    private void Awake()
    {
        canvas = GameObject.FindGameObjectWithTag("Canvas");
        var textTr = canvas.transform.Find("AmmoCount");
        ammoUi = textTr.GetComponent<Text>();
    }

    private void Update()
    {
        ammoUi.text = currentAmmo + " / " + maxReloadAmmo;
        if (isReloading)
            return;
        if (currentAmmo == 0 && maxReloadAmmo == 0)
            return;
        if (currentAmmo == 0 || Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetButton("Fire1") && Time.time >= nextShoot)
        {
            nextShoot = Time.time + 1f / fireRate;
            Shoot();
        }
        
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        if (currentAmmo == maxAmmo)
        {
            isReloading = false;
            yield break;
        }

        audioSource.clip = reloadSound1;
        audioSource.Play();
        //   animator.SetBool("Reload", true);
        yield return new WaitForSeconds(reloadTime - .25f);
        audioSource.clip = reloadSound2;
        audioSource.Play();
     //   animator.SetBool("Reload", false);
        yield return new WaitForSeconds(.25f);
        audioSource.clip = reloadSound3;
        audioSource.Play();


        if (maxReloadAmmo >= 30)
        {
            maxReloadAmmo -= maxAmmo - currentAmmo;
            currentAmmo = maxAmmo;
        }
        else if (maxReloadAmmo < 30 && maxReloadAmmo > 0)
        {
            currentAmmo = maxReloadAmmo;
            maxReloadAmmo = 0;
        }


        isReloading = false;
     //   animator.SetBool("Reload", false);
    }
    [Client]
    private void Shoot()
    {
        currentAmmo--;
        muzzleFlash.Play();
        int index = Random.Range(0, shootingClips.Length);
        audioSource.clip = shootingClips[index];
        audioSource.Play();
        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit))
        {


            if (hit.collider.tag == "Player")
            {
                CmdPlayerShot(hit.collider.name, weapon.damage);
            }
            var ImpactGO = Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(ImpactGO, 3f);
        }
    }

    [Command]
    void CmdPlayerShot(string _playerID, int _damage)
    {
        Player _player = GameManager.GetPlayer(_playerID);
        _player.TakeDamge(_damage);
    }
}