using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class GunShot : NetworkBehaviour {
    private const string RELOAD = "reload";
    private const string SHOOT = "shoot";
    private const string ZOOM = "zoom";
    private const string ZOOM_OUT = "zoomout";

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
    [SerializeField] private AudioClip zoomIn;
    [SerializeField] private AudioClip zoomOut;
    [SerializeField] private Animator fpAnimator;
    [SerializeField] private GameObject handAndWeapon;
    private Image crosshair;
    private float nextShot;

    private Image hitmarkerImage;
    private float hitmarkerDuration = 0.5f;

    [SerializeField] private Camera camera;
    private int normalCameraFOV = 60;
    private int zoomCameraFOV = 30;
    private float smooth = 20;

    [SerializeField] private GameObject zoomScope;

    private float rightMouseClickTimer;

    private Recoil recoilScript;
    
    private void Start() {
        currentAmmo = maxAmmo;
        ammoUi = GameManager.instance.hud.InGameHUD.GetComponentInChildren<Text>();
        hitmarkerImage = GameObject.Find("Hitmarker/Image").GetComponent<Image>();
        hitmarkerImage.color = new Color(1, 1, 1, 0);
        recoilScript = transform.Find("Recoil").GetComponent<Recoil>();
        crosshair = GameObject.Find("IngameHUD/Crosshair").GetComponent<Image>();
    }

    private void Update() {
        ammoUi.text = currentAmmo + " / " + maxReloadAmmo;
        if (isReloading)
            return;
        if (currentAmmo == 0 && maxReloadAmmo == 0) {
            EmptyGunShot();
            return;
        }

        if (!fpAnimator.GetCurrentAnimatorStateInfo(0).IsTag(ZOOM)
            && !fpAnimator.GetCurrentAnimatorStateInfo(0).IsTag(ZOOM_OUT)
            && !fpAnimator.GetCurrentAnimatorStateInfo(0).IsTag(SHOOT)) {
            if (currentAmmo == 0 || Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo) {
                StartCoroutine(Reload());
                return;
            }
        }

        if (!isReloading && !fpAnimator.GetCurrentAnimatorStateInfo(0).IsTag(ZOOM_OUT)) {
            if (Input.GetButton("Fire1") && Time.time >= nextShot) {
                nextShot = Time.time + 1f / fireRate;
                Shoot();
            }
        }

        MeasureRightMouseButtonClickDuration();

        PlayZoomInSound();
        
        if (!isReloading) {
            if (Input.GetKey(KeyCode.Mouse1)) {
                ZoomIn();
            }
            else if (Input.GetKeyUp(KeyCode.Mouse1)) {
                ZoomOut();
            }
            else if (currentAmmo == 0 || Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo) {
                ZoomOut();
                StartCoroutine(Reload());
            }
        }
    }

    private IEnumerator Reload() {
        zoomScope.SetActive(false);
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
        if (!fpAnimator.GetCurrentAnimatorStateInfo(0).IsTag(ZOOM) &&
            !fpAnimator.GetCurrentAnimatorStateInfo(0).IsTag(ZOOM_OUT)) {
            fpAnimator.SetTrigger(SHOOT);
            fpAnimator.speed = 6;
            CmdOnShoot();
        }
        
        if (!fpAnimator.GetCurrentAnimatorStateInfo(0).IsTag(ZOOM)) {
            recoilScript.RecoilFire();
        }
        else {
            recoilScript.RecoilFireZoom();
        }

        if (!isLocalPlayer)
            return;

        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit)) {
            if (hit.collider.CompareTag("Player")) {
                CmdPlayerShot(hit.collider.name, weapon.damage);
                HitActive();
                Invoke(nameof(HitDisable), hitmarkerDuration);
            }

            if (!hit.collider.CompareTag("Ceiling")) {
                var ImpactGO = Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(ImpactGO, 3f);
            }
        }
    }

    private void MeasureRightMouseButtonClickDuration() {
        if (!isReloading) {
            if (Input.GetKey(KeyCode.Mouse1)) {
                rightMouseClickTimer += Time.deltaTime;
            }
            else {
                rightMouseClickTimer = 0;
            }
        }
    }

    private void PlayZoomInSound() {
        if (Input.GetKeyDown(KeyCode.Mouse1)) {
            audioSource.PlayOneShot(zoomIn);
        }
    }

    private void ZoomIn() {
        fpAnimator.speed = 2.5f;
        fpAnimator.SetBool(ZOOM, true);
        if (rightMouseClickTimer >= fpAnimator.runtimeAnimatorController.animationClips[4].length - 0.05f) {
            camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, zoomCameraFOV, smooth);
            handAndWeapon.SetActive(false);
            crosshair.color = new Color(1, 1, 1, 0);
            zoomScope.SetActive(true);
        }
    }

    private void ZoomOut() {
        audioSource.PlayOneShot(zoomOut);
        zoomScope.SetActive(false);
        camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, normalCameraFOV, smooth);
        fpAnimator.SetBool(ZOOM, false);
        handAndWeapon.SetActive(true);
        crosshair.color = Color.white;
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