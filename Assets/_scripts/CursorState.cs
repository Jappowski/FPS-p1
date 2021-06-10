using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CursorState : MonoBehaviour
{
    [SerializeField] private bool cursor;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cursor = false;
    }

  
    void Update()
    {
        if (Input.GetButton("Cancel") && cursor == false)
        {
            Cursor.lockState = CursorLockMode.None;
            cursor = true;
        }     
        else if (Input.GetButton("Cancel") && Cursor.visible == true)
        {
            Cursor.lockState = CursorLockMode.Locked;
            cursor = false;
        }
    }
    
}
