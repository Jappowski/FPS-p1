using System;
using System.Collections;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Player : NetworkBehaviour {
    [SerializeField] private int maxhealth = 100;
    [SyncVar] private int currentHealth;
    [SerializeField] private Behaviour[] disableOnDeathScripts;
    private bool[] wasEnabled;
    [SerializeField] private ParticleSystem blood;
    [SerializeField] private GameObject[] disableOnDeathGameObjects;
    private CharacterController controller;
    [SerializeField] private GameObject postproces;
    private Text hp;
    [SerializeField] private TextMeshProUGUI deathMessage;
    [SerializeField] private GameObject deathCanvas;
    private GameObject canvas;
    private GunShot _gunShot;
    private PlayerWeapon weapon;
    float currCountdownValue;
    [SyncVar] private bool _isDead = false;

    public bool isDead {
        get { return _isDead; }
        protected set { _isDead = value; }
    }

    [SerializeField] private AudioSource deathRespawnAudioSource;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip respawnSound;

    [SerializeField] private AudioSource gettingHitAudioSource;
    [SerializeField] private AudioClip[] gettingHitSounds;

    private void Start() {
        controller = GetComponent<CharacterController>();
    }

    void Update() {
        if (isLocalPlayer)
            HpUpdate();
#if UNITY_EDITOR
        if (Input.GetKeyDown("k")) {
            Debug.Log("DMG");
            RpcTakeDamage(20);
        }
#endif
        // if (!isDead)
        //     deathCanvas.SetActive(false);
        // else deathCanvas.SetActive(true);
    }

    private void HpUpdate() {
        GameManager.instance.hud.HP.text = "HP: " + currentHealth;
    }

    public void Setup() {
        this.enabled = true;
        _gunShot = GetComponent<GunShot>();
        canvas = GameObject.FindGameObjectWithTag("Canvas");
        hp = GameManager.instance.hud.HP;
        wasEnabled = new bool[disableOnDeathScripts.Length];
        for (int i = 0; i < wasEnabled.Length; i++) {
            wasEnabled[i] = disableOnDeathScripts[i].enabled;
        }

        SetDefaults();
    }

    [ClientRpc]
    public void RpcTakeDamage(int _dmg) {
        if (isDead)
            return;
        if (isLocalPlayer)
            StartCoroutine(ShowVignette());
        currentHealth -= _dmg;
        blood.Play();
        var index = Random.Range(0, gettingHitSounds.Length);
        gettingHitAudioSource.clip = gettingHitSounds[index];
        gettingHitAudioSource.Play();
        if (currentHealth <= 0f)
            Die();
    }

    private void Die() {
        isDead = true;
        for (int i = 0; i < disableOnDeathScripts.Length; i++) {
            disableOnDeathScripts[i].enabled = false;
        }

        controller.enabled = false;

        deathRespawnAudioSource.clip = deathSound;
        deathRespawnAudioSource.Play();

        if (!isLocalPlayer)
            foreach (var gameObject in disableOnDeathGameObjects) {
                gameObject.SetActive(false);
            }

        if (isLocalPlayer)
            DeadCanvasActive();
        StartCoroutine(Respawn());
        StartCoroutine(StartCountdown(5));
    }

    private IEnumerator ShowVignette() {
        postproces.SetActive(true);
        yield return new WaitForSeconds(0.25f);
        postproces.SetActive(false);
    }

    private IEnumerator Respawn() {
        yield return new WaitForSeconds(5f);
        controller.enabled = true;
        DeadCanvasDeActive();
        SetDefaults();
        Transform _spawnPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = _spawnPoint.position;
        transform.rotation = _spawnPoint.rotation;
        deathRespawnAudioSource.clip = respawnSound;
        deathRespawnAudioSource.Play();
    }

    public void SetDefaults() {
        isDead = false;
        currentHealth = maxhealth;

        for (int i = 0; i < disableOnDeathScripts.Length; i++) {
            disableOnDeathScripts[i].enabled = wasEnabled[i];
        }

        if (!isLocalPlayer) {
            foreach (var gameObject in disableOnDeathGameObjects) {
                gameObject.SetActive(true);
            }
        }
    }

    public void DeadCanvasActive() {
        deathCanvas.SetActive(true);
        GameManager.instance.hud.InGameHUD.SetActive(false);
    }

    public void DeadCanvasDeActive() {
        deathCanvas.SetActive(false);
        GameManager.instance.hud.InGameHUD.SetActive(true);
    }

    public IEnumerator StartCountdown(float countdownValue) {
        currCountdownValue = countdownValue;
        while (currCountdownValue > 0) {
            deathMessage.text = ("Respawn in... " + currCountdownValue);
            Debug.Log("Countdown: " + currCountdownValue);
            yield return new WaitForSeconds(1.0f);
            currCountdownValue--;
        }
    }
}