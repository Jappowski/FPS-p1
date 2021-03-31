
using UnityEngine;

public class GunShot : MonoBehaviour
{
    public float damage = 25f;
    public float fireRate = 15f;
    public Camera fpsCam;
    public ParticleSystem muzzleFlash;
    public GameObject hitEffect;
    public float impactForce = 100f;

    private float nextShoot = 0f;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextShoot)
        {
            nextShoot = Time.time + 1f / fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        muzzleFlash.Play();
        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit))
        {
            Target target = hit.transform.GetComponent<Target>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }

            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(-hit.normal * impactForce);
            }
           GameObject ImpactGO =  Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
           Destroy(ImpactGO, 3f);
        }
    }
}
