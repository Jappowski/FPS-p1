using UnityEngine;

public class Target : MonoBehaviour
{
    public float health = 100f;
    public Material Material;

    public void TakeDamage(float dmg)
    {
        health -= dmg;
        if (health <= 0f) Die();
    }

    private void Die()
    {
        var my_renderer = GetComponent<MeshRenderer>();
        Destroy(gameObject, 1);
    }
}