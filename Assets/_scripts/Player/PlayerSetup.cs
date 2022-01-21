using Mirror;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerSetup : NetworkBehaviour {
    [SerializeField] Behaviour[] componentsToDisable;
    [SerializeField] private GameObject playerBody;
    [SerializeField] private GameObject weapon;
    [SerializeField] private Behaviour TargetScript;
    [SerializeField] private Transform weaponHierarchy;

    [SerializeField] private string remoteLayerName = "RemotePlayer";

     void Start() {
        if (!isLocalPlayer) {
            DisableComponents();
            AssignRemoteLayer();
        }
        else {
            playerBody.SetActive(false);
            TargetScript.enabled = false;
            weapon.layer = 9;
            foreach (Transform child in weaponHierarchy.GetComponentsInChildren<Transform>(true)) {
                child.gameObject.layer = LayerMask.NameToLayer("Weapon");
            }
        }

        GameEvents.onGameStateChange += OnGameStateChangeHandler;
        GetComponent<Player>().Setup();
    }

    private void OnGameStateChangeHandler(GameManager.GameState obj) {
        if (isLocalPlayer)
        {
            if (obj == GameManager.GameState.Stop)
                DisableComponents();
            else
                foreach (var VARIABLE in componentsToDisable)
                {
                    VARIABLE.enabled = true;
                }
        }
    }

    void SetLayer(GameObject Weapon, int layer) {
        Weapon.layer = layer;
    }

    void DisableComponents() {
        foreach (var component in componentsToDisable) {
            component.enabled = false;
        }
    }

    void AssignRemoteLayer() {
        gameObject.layer = LayerMask.NameToLayer(remoteLayerName);
    }

    public override void OnStartClient() {
        base.OnStartClient();
        string _netID = GetComponent<NetworkIdentity>().netId.ToString();
        Player _player = GetComponent<Player>();
        GameManager.RegisterPlayer(_netID, _player);
    }

    void OnDisable() {
        GameManager.UnRegisterPlayer(transform.name);
    }
}