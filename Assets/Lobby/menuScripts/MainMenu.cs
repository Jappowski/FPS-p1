using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [Header("UI")] [SerializeField] private readonly GameObject landingPagePanel = null;
    [SerializeField] private readonly NetworkManagerGame networkManager = null;

    public void HostLobby()
    {
        networkManager.StartHost();
        landingPagePanel.SetActive(false);
    }
}