using System.Collections;
using Mirror;
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
    private GunShot _gunShot;
    private PlayerWeapon weapon;
    
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
            RpcTakeDamage(30);
        }
    }

    private void HpUpdate()
    {
        hp.text = "HP: " + currentHealth;
    }

    public void Setup()
    {
        this.enabled = true;
        _gunShot = GetComponent<GunShot>();
        var canvas = GameObject.FindGameObjectWithTag("Canvas");
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
        
        StartCoroutine(Respawn());
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
    
}