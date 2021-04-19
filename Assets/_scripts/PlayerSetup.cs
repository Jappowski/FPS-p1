using System.Collections;
using System.Collections.Generic;
using MLAPI;
using UnityEngine;

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField] Behaviour[] componentsToDisable;
    [SerializeField] private GameObject playerBody;
    void Start()
    {
        if (!IsLocalPlayer)
        {
            foreach (var component in componentsToDisable)
            {
                component.enabled = false;
            }
        }
        else
        {
            Camera.main.gameObject.SetActive(false);
            playerBody.SetActive(false);
        }
        
    }
}
