using System;
using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GunShot : NetworkBehaviour {
    private const string RELOAD = "reload";
    private const string SHOOT = "shoot";
    
    private Text ammoUi;    
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
    [SerializeField] private Animator fpAnimator;
    [SerializeField] private LayerMask mask;
    private float nextShot;

    private Image hitmarkerImage;
    private float hitmarkerDuration = 0.5f;

    private Recoil recoilScript;


    private void Start() {
        currentAmmo = maxAmmo;
        ammoUi = GameManager.instance.hud.InGameHUD.GetComponentInChildren<Text>();
        hitmarkerImage = GameObject.Find("Hitmarker/Image").GetComponent<Image>();
        hitmarkerImage.color = new Color(1, 1, 1, 0);
        recoilScript = transform.Find("Recoil").GetComponent<Recoil>();
    }

    private void Update() {
        ammoUi.text = currentAmmo + " / " + maxReloadAmmo;
        if (isReloading)
            return;
        if (currentAmmo == 0 && maxReloadAmmo == 0) {
            EmptyGunShot();
            return;
        }

        if (!fpAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Shoot")) {
            if (currentAmmo == 0 || Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo) {
                StartCoroutine(Reload());
                return;
            }
        }

        if (!fpAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Reload")) {
            if (Input.GetButton("Fire1") && Time.time >= nextShot) {
                nextShot = Time.time + 1f / fireRate;
                Shoot();
            }
        }
    }

    private IEnumerator Reload() {
        isReloading = true;
        if (maxReloadAmmo != 0) {
            fpAnimator.speed = 2;
            fpAnimator.SetTrigger(RELOAD);
            yield return new WaitForSeconds(fpAnimator.runtimeAnimatorController.animationClips[0].length);
        }

        if (currentAmmo == maxAmmo) {
            isReloading = false;
            yield break;
        }
        
        if (maxReloadAmmo >= 30) { 
            maxReloadAmmo -= maxAmmo - currentAmmo;
            currentAmmo = maxAmmo;
        }
        else if (maxReloadAmmo < 30 && maxReloadAmmo > 0) {
            if (currentAmmo + maxReloadAmmo > 30) {
                maxReloadAmmo -= maxAmmo - currentAmmo;
                currentAmmo = maxAmmo;
            }
            else {
                currentAmmo += maxReloadAmmo;
                maxReloadAmmo = 0;
            }
        }

        isReloading = false;
    }

    public void ReloadSoundPlay(int music) {
        switch (music) {
            case 1:
                audioSource.clip = reloadSound1;
                audioSource.Play();
                break;
            case 2:
                audioSource.clip = reloadSound2;
                audioSource.Play();
                break;
            case 3:
                audioSource.clip = reloadSound3;
                audioSource.Play();
                break;
        }
        
    }
    private void EmptyGunShot() {
        if (currentAmmo == 0 && maxReloadAmmo == 0 && Input.GetButton("Fire1")) {
            if (!audioSource.isPlaying) {
                audioSource.clip = emptyGunSound;
                audioSource.Play();
            }
        }
    }

    [Command]
    void CmdOnShoot() {
        RpcDoMuzzleFlash();
    }

    [ClientRpc]
    void RpcDoMuzzleFlash() {
        muzzleFlash.Play();
    }

    [Client]
    private void Shoot() {
        currentAmmo--;
        var index = Random.Range(0, shootingClips.Length);
        audioSource.clip = shootingClips[index];
        audioSource.Play();
        
        fpAnimator.SetTrigger(SHOOT);
        fpAnimator.speed = 6;

        recoilScript.RecoilFire();

        if (!isLocalPlayer)
            return;
        
        CmdOnShoot();
        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit)) {
            if (hit.collider.CompareTag("Player")) {
                CmdPlayerShot(hit.collider.name, weapon.damage);
                HitActive();
                Invoke(nameof(HitDisable), hitmarkerDuration);
            }

            var ImpactGO = Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(ImpactGO, 3f);
        }
    }

    private void HitActive() {
        hitmarkerImage.color = Color.white;
    }

    private void HitDisable() {
        hitmarkerImage.color = new Color(1, 1, 1, 0);
    }

    [Command]
    void CmdPlayerShot(string _playerID, int _damage) {
        Player _player = GameManager.GetPlayer(_playerID);
        _player.RpcTakeDamage(_damage);
    }
}