using System.Collections;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
    [SerializeField] private int maxhealth = 100;
    [SyncVar] private int currentHealth;
    [SerializeField]private Behaviour[] disableOnDeathScripts;
    private bool[] wasEnabled;
    [SerializeField] private ParticleSystem blood;
    [SerializeField] private  GameObject[] disableOnDeathGameObjects;
    [SerializeField] private GameObject postproces;
    private Text hp;
    private Text deathMessage;
    private GameObject deathCanvas;
    private GameObject canvas;
    private GunShot _gunShot;
    private PlayerWeapon weapon;
    float currCountdownValue;
    [SyncVar] 
    private bool _isDead = false;
    public bool isDead
    {
        get { return _isDead; }
        protected set { _isDead = value; }
    }
    
    void Update()
    {
        if(isLocalPlayer)
            HpUpdate();
        if (Input.GetKeyDown("k"))
        {
            Debug.Log("DMG");
            RpcTakeDamage(5);
        }
        // if (!isDead)
        //     deathCanvas.SetActive(false);
        // else deathCanvas.SetActive(true);
    }

    private void HpUpdate()
    {
        hp.text = "HP: " + currentHealth;
    }

    public void Setup()
    {
        this.enabled = true;
        _gunShot = GetComponent<GunShot>(); 
        canvas = GameObject.FindGameObjectWithTag("Canvas");
        var textTr = canvas.transform.Find("Health");
        hp = textTr.GetComponent<Text>();
        
        

        wasEnabled = new bool[disableOnDeathScripts.Length];
        for (int i = 0; i < wasEnabled.Length; i++)
        {
            wasEnabled[i] = disableOnDeathScripts[i].enabled;
        }

        SetDefaults();
    }
    [ClientRpc]
    public void RpcTakeDamage(int _dmg)
    {
        if (isDead)
            return;
        if(isLocalPlayer)
            StartCoroutine(ShowVignette());
        currentHealth -= _dmg;
        blood.Play();
        if (currentHealth <= 0f)
            Die();

    }

    private void Die()
    {
        isDead = true;
        for (int i = 0; i < disableOnDeathScripts.Length; i++)
        {
            disableOnDeathScripts[i].enabled = false;
        }
        
        if(!isLocalPlayer)
        foreach (var gameObject in disableOnDeathGameObjects)
        {
            gameObject.SetActive(false);
        }

        // DeadCanvasActive();
        StartCoroutine(Respawn());
        // StartCoroutine(StartCountdown(10));
        // DeadCanvasDeActive();
        
    }

    private IEnumerator ShowVignette()
    {
        postproces.SetActive(true);
        yield return new WaitForSeconds(0.25f);
        postproces.SetActive(false);
    }
    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(5f);
        
        SetDefaults();
        Transform _spawnPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = _spawnPoint.position;
        transform.rotation = _spawnPoint.rotation;
    }
    public void SetDefaults()
    {
        isDead = false;
        currentHealth = maxhealth;

        for (int i = 0; i < disableOnDeathScripts.Length; i++)
        {
            disableOnDeathScripts[i].enabled = wasEnabled[i];
        }

        if (!isLocalPlayer)
        {
            foreach (var gameObject in disableOnDeathGameObjects)
            {
                gameObject.SetActive(true);
            }   
        }
    }

    public void DeadCanvasActive()
    {
        deathCanvas.SetActive(true);
        canvas.SetActive(false);
    }
    public void DeadCanvasDeActive()
    {
        deathCanvas.SetActive(false);
        canvas.SetActive(true);
    }
    public IEnumerator StartCountdown(float countdownValue)
    {
        currCountdownValue = countdownValue;
        while (currCountdownValue > 0)
        {
            deathMessage.text = ("Respawn in... " + currCountdownValue);
            Debug.Log("Countdown: " + currCountdownValue);
            yield return new WaitForSeconds(1.0f);
            currCountdownValue--;
        }
    }
    
}