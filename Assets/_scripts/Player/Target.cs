using System.Collections;
using Mirror;
using UnityEngine;

public class Target : NetworkBehaviour
{
    public float health = 100f;
    public Material Material;
    [SerializeField]
    private Behaviour[] disableOnDeath;
    private bool[] wasEnabled;
   [SerializeField] private ParticleSystem blood;

    [SyncVar] 
    private bool _isDead = false;
    public bool isDead
    {
        get => _isDead;
        protected set => _isDead = value;
    }

    public void Setup()
    {
        wasEnabled = new bool[disableOnDeath.Length];
        for (int i = 0; i < wasEnabled.Length; i++)
        {
            wasEnabled[i] = disableOnDeath[i].enabled;
        }

        SetDefaults();
    }
    public void TakeDamage(float dmg)
    {
        if (isDead)
            return;
        
        health -= dmg;
        blood.Play();
        if (health <= 0f)
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
        yield return new WaitForSeconds(3f);
        
        SetDefaults();
        Transform _spawnPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = _spawnPoint.position;
        transform.rotation = _spawnPoint.rotation;
    }
    public void SetDefaults()
    {
        isDead = false;
        health = 100f;

        for (int i = 0; i < disableOnDeath.Length; i++)
        {
            disableOnDeath[i].enabled = wasEnabled[i];
        }

        Collider _col = GetComponent<Collider>();
        if (_col != null)
            _col.enabled = true;
    }
    
}