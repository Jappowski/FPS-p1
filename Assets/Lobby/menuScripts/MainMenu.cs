using Mirror;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [Header("UI")] [SerializeField] private GameObject landingPagePanel = null;
    [SerializeField] private NetworkManagerGame networkManager = null;

    public void HostLobby()
    {
        networkManager.StartHost();
        landingPagePanel.SetActive(false);
    }
}