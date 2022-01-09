using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Granade : MonoBehaviour
{
    private const int DELAY = 5;

    private void Update()
    {
        
    }

    IEnumerator TimeForExplode()
    {
        yield return new WaitForSeconds(DELAY);
        Explode();
    }

    private void Explode()
    {
        var colliders = Physics.OverlapSphere(transform.position, 10);

        foreach (var collider in colliders)
        {
            var rb = collider.GetComponent<Rigidbody>();
            var ck = collider.GetComponent<Player>();
            if (rb != null)
            {
                rb.AddExplosionForce(700, transform.position, 10);
            }

            if (ck != null)
            {
                ck.RpcTakeDamage(CalculatePlayerDamage(ck));
            }
        }
    }

    int CalculatePlayerDamage(Player player)
    {
        var distance = Vector3.Distance(player.transform.position, transform.position);
        
        return (int)distance * 7 ;
    }
}
