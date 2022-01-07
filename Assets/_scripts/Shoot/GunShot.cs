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

   public int currentAmmo;
    public int maxAmmo = 30; //in mag
    public int maxReloadAmmo = 90;
    public float fireRate = 15;
    public Camera fpsCam;
    public GameObject hitEffect;
    public static bool isReloading;
    public ParticleSystem muzzleFlash;
    public PlayerWeapon weapon;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] shootingClips;
    [SerializeField] private AudioClip reloadSound1;
    [SerializeField] private AudioClip reloadSound2;
    [SerializeField] private AudioClip reloadSound3;
    [SerializeField] private AudioClip emptyGunSound;
    [SerializeField] private LayerMask mask;
    private float nextShot;
    public float reloadTime = 3f;
    
    

    private void Start()
    {
        currentAmmo = maxAmmo;
        ammoUi = GameManager.instance.hud.InGameHUD.GetComponentInChildren<Text>();
    }

    private void Update()
    {
        ammoUi.text = currentAmmo + " / " + maxReloadAmmo;
        if (isReloading)
            return;
        if (currentAmmo == 0 && maxReloadAmmo == 0)
        {
            EmptyGunShot();
            return;
        }
        if (currentAmmo == 0 || Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetButton("Fire1") && Time.time >= nextShot)
        {
            nextShot = Time.time + 1f / fireRate;
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
        yield return new WaitForSeconds(reloadTime - .25f);
        audioSource.clip = reloadSound2;
        audioSource.Play();
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
    }

    private void EmptyGunShot()
    {
        if (currentAmmo == 0 && maxReloadAmmo == 0 && Input.GetButton("Fire1"))
        {
            audioSource.clip = emptyGunSound;
            audioSource.Play();
        }
    }

    [Command]
    void CmdOnShoot()
    {
        RpcDoMuzzleFlash();
    }

    [ClientRpc]
    void RpcDoMuzzleFlash()
    {
        muzzleFlash.Play();
    }
    [Client]
    private void Shoot()
    {
        currentAmmo--;
        int index = Random.Range(0, shootingClips.Length);
        audioSource.clip = shootingClips[index];
        audioSource.Play();
        
        if (!isLocalPlayer)
            return;
        
        CmdOnShoot();
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
        _player.RpcTakeDamage(_damage);
    }
}