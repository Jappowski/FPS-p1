using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinLobbyMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField ipAddressInputField = null;
    [SerializeField] private Button joinButton = null;

    [Header("UI")] [SerializeField] private GameObject landingPagePanel = null;
    [SerializeField] private NetworkManagerGame networkManager = null;

    private void OnEnable()
    {
        NetworkManagerGame.OnClientConnected += HandleClientConnected;
        NetworkManagerGame.OnClientDisconnected += HandleClientDisconnected;
    }

    private void OnDisable()
    {
        NetworkManagerGame.OnClientConnected -= HandleClientConnected;
        NetworkManagerGame.OnClientDisconnected -= HandleClientDisconnected;
    }

    public void JoinLobby()
    {
        var ipAddress = ipAddressInputField.text;
        networkManager.networkAddress = ipAddress;
        networkManager.StartClient();
        joinButton.interactable = false;
    }

    private void HandleClientConnected()
    {
        joinButton.interactable = true;
        gameObject.SetActive(false);
        landingPagePanel.SetActive(false);
    }

    private void HandleClientDisconnected()
    {
        joinButton.interactable = true;
    }
}