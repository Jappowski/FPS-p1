
using Mirror;
using UnityEngine;
[RequireComponent(typeof(Target))]
public class PlayerSetup : NetworkBehaviour
{
    [SerializeField] Behaviour[] componentsToDisable;
    [SerializeField] private GameObject playerBody;
    void Start()
    {
        if (!isLocalPlayer)
        {
            foreach (var component in componentsToDisable)
            {
                component.enabled = false;
            }
        }
        else
        {
            playerBody.SetActive(false);
        }

        GetComponent<Target>().Setup();
    }
}

