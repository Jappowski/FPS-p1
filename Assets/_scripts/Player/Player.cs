using System.Collections;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
    [SerializeField] private int maxhealth = 100;
    [SyncVar] private int currentHealth;
    [SerializeField]private Behaviour[] disableOnDeath;
    [SerializeField]private bool[] wasEnabled;
    [SerializeField] private ParticleSystem blood;
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

    void Start()
    {
        // deathCanvas = GameObject.FindGameObjectWithTag("deathCanvas");
        // var deathText = deathCanvas.transform.Find("Respawn");
        // deathMessage = deathText.GetComponent<Text>();
    }
    void Update()
    {
        if(isLocalPlayer)
            HpUpdate();
        if (Input.GetKeyDown("k"))
        {
            Debug.Log("DMG");
            RpcTakeDamage(30);
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
        
        

        wasEnabled = new bool[disableOnDeath.Length];
        for (int i = 0; i < wasEnabled.Length; i++)
        {
            wasEnabled[i] = disableOnDeath[i].enabled;
        }

        SetDefaults();
    }
    [ClientRpc]
    public void RpcTakeDamage(int _dmg)
    {
        if (isDead)
            return;
        
        currentHealth -= _dmg;
        blood.Play();
        if (currentHealth <= 0f)
            Die();

    }

    private void Die()
    {
        isDead = true;
        for (int i = 0; i < disableOnDeath.Length; i++)
        {
            disableOnDeath[i].enabled = false;
        }
        
        Collider _col = GetComponent<Collider>();
        if (_col != null)
            _col.enabled = false;
        
        // DeadCanvasActive();
        // StartCoroutine(Respawn());
        // StartCoroutine(StartCountdown(10));
        // DeadCanvasDeActive();
        
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(10f);
        
        SetDefaults();
        Transform _spawnPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = _spawnPoint.position;
        transform.rotation = _spawnPoint.rotation;
    }
    public void SetDefaults()
    {
        isDead = false;
        currentHealth = maxhealth;

        for (int i = 0; i < disableOnDeath.Length; i++)
        {
            disableOnDeath[i].enabled = wasEnabled[i];
        }
        Collider _col = GetComponent<Collider>();
        if (_col != null)
            _col.enabled = true;
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