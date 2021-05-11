using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GunShot : MonoBehaviour
{
    private Text ammoUi;
    public Animator animator;
    private GameObject canvas;

    public float currentAmmo;
    public float damage = 25f;
    public float fireRate = 15f;
    public Camera fpsCam;
    public GameObject hitEffect;
    public float impactForce = 100f;
    private bool isReloading;
    private readonly float maxAmmo = 30f;
    public float maxReloadAmmo = 90f;
    public ParticleSystem muzzleFlash;

    private float nextShoot;
    public float reloadTime = 3f;

    private void start()
    {
        currentAmmo = maxAmmo;
    }

    private void Awake()
    {
        canvas = GameObject.FindGameObjectWithTag("Canvas");
        var textTr = canvas.transform.Find("AmmoCount");
        ammoUi = textTr.GetComponent<Text>();
    }

    private void Update()
    {
        ammoUi.text = currentAmmo + " / " + maxReloadAmmo;
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

    private IEnumerator Reload()
    {
        isReloading = true;
        animator.SetBool("Reload", true);
        yield return new WaitForSeconds(reloadTime - .25f);
        animator.SetBool("Reload", false);
        yield return new WaitForSeconds(.25f);


        if (maxReloadAmmo >= 30)
        {
            maxReloadAmmo -= maxAmmo - currentAmmo;
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

    private void Shoot()
    {
        currentAmmo--;
        muzzleFlash.Play();
        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit))
        {
            var target = hit.transform.GetComponent<Target>();
            if (target != null) target.TakeDamage(damage);

            if (hit.rigidbody != null) hit.rigidbody.AddForce(-hit.normal * impactForce);
            var ImpactGO = Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(ImpactGO, 3f);
        }
    }
}