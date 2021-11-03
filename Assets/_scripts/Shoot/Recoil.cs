using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recoil : MonoBehaviour
{
    [SerializeField] private GameObject _camera;
    
    [SerializeField] private Vector3 recoilUp;
    public Vector3 startRotation;
    public Vector3 currentRotation;
    
    void Start()
    {
        
        startRotation = _camera.transform.localEulerAngles;
    }
    void Update()
    {
        if (Input.GetButton("Fire1") && !GunShot.isReloading)
        {
            AddRecoil();                           
        }                                          
                                                   
        if (Input.GetButtonUp("Fire1"))            
        {                                          
            StopRecoil();                          
        }
       
        
    }                                              
                                                   
    public void AddRecoil()                        
    {
        currentRotation += recoilUp;               
        _camera.transform.localEulerAngles += currentRotation;
    }                                              
                                                   
    public void StopRecoil()
    {
        currentRotation = startRotation;
        _camera.transform.localEulerAngles = startRotation;
    }
    
}
