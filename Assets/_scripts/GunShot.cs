
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class GunShot : MonoBehaviour
{
    public float damage = 25f;
    public float fireRate = 15f;
    public Camera fpsCam;
    public ParticleSystem muzzleFlash;
    public GameObject hitEffect;
    public Animator animator;
    public float impactForce = 100f;

    private float nextShoot = 0f;

    public float currentAmmo;
    private float maxAmmo = 30f;
    public float reloadTime = 3f;
    private bool isReloading = false;
    public Text ammoUI;
    public float maxReloadAmmo = 90f;
    
    void start()
    {
        currentAmmo = maxAmmo;
    }
    // Update is called once per frame
    void Update()
    {
        ammoUI.text = currentAmmo.ToString() + " / " + maxReloadAmmo.ToString();
        if (isReloading)
            return;
        
        if (currentAmmo <= 0 || Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(Reload());
            return;
        }
        if (Input.GetButton("Fire1") && Time.time >= nextShoot)
        {
            nextShoot = Time.time + 1f / fireRate;
            Shoot();
        }
    }
  
    IEnumerator Reload()
    {
        isReloading = true;
        animator.SetBool("Reload", true);
        yield return new WaitForSeconds(reloadTime-.25f);
        animator.SetBool("Reload", false);
        yield return new WaitForSeconds(.25f);
        

        if (maxReloadAmmo >= 30)
        {
            maxReloadAmmo -= (maxAmmo - currentAmmo);
            currentAmmo = maxAmmo;
        }
        else if (maxReloadAmmo == 0)
        {
            currentAmmo = 0;
        }
        else if (maxReloadAmmo < 30 && maxReloadAmmo > 0)
        {
            currentAmmo = maxReloadAmmo;
            maxReloadAmmo = 0;
        }
        
        isReloading = false;
        animator.SetBool("Reload", false);
        
    }

    void Shoot()
    {
        currentAmmo--;
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
