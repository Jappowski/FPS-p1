
using Mirror;
using UnityEngine;
[RequireComponent(typeof(Target))]
public class PlayerSetup : NetworkBehaviour
{
    [SerializeField] Behaviour[] componentsToDisable;
    [SerializeField] private GameObject playerBody;
    [SerializeField] private GameObject weapon;
    [SerializeField] private Transform weaponHierarchy;
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
            weapon.layer = 9;
            foreach (Transform child in weaponHierarchy.GetComponentsInChildren<Transform>(true))  
            {
                child.gameObject.layer = LayerMask.NameToLayer ("Weapon");
            }
        }
        
        GetComponent<Target>().Setup();
    }

    void SetLayer(GameObject Weapon,int layer)
    {
        Weapon.layer = layer;
    }
}

