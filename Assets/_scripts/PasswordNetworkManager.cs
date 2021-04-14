
using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking.Types;
using MLAPI;
public class PasswordNetworkManager : MonoBehaviour
{
  [SerializeField] TMP_InputField passwordInputField;
  [SerializeField] GameObject passwordEntryUi;
  [SerializeField] GameObject leaveButton;



  void Start()
  {
    NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
    NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnect;
    NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
  }

  void OnDestroy()
  {
    if (NetworkManager.Singleton == null)
    {
      return;
    }
    NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
    NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnect;
    NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
    
  }
  
  
  public void Host()
  {
    NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
    NetworkManager.Singleton.StartHost();
  }
  
  public void Client()
  {
    NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(passwordInputField.text);  
    NetworkManager.Singleton.StartClient();
  }

  public void Leave()
  {
    if (NetworkManager.Singleton.IsHost)
    {
      NetworkManager.Singleton.StopClient();
      NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
    }

    if (NetworkManager.Singleton.IsClient)
    {
      NetworkManager.Singleton.StopClient();
    }
    passwordEntryUi.SetActive(true);
    leaveButton.SetActive(false);
  }
  private void ApprovalCheck(byte[] connectionData, ulong clientId, NetworkManager.ConnectionApprovedDelegate callback)
  {
    string password = Encoding.ASCII.GetString(connectionData);
    bool approvalConnection = password == passwordInputField.text;
    callback(true, null, approvalConnection, null, null);
  }

  void HandleClientConnect(ulong clientId)
  {
    if (clientId == NetworkManager.Singleton.LocalClientId)
    {
      passwordEntryUi.SetActive(false);
      leaveButton.SetActive(true);
    }
  }
  void HandleClientDisconnect(ulong clientId)
  {
    if (clientId == NetworkManager.Singleton.LocalClientId)
    {
      passwordEntryUi.SetActive(true);
      leaveButton.SetActive(false);
    }
  }

  void HandleServerStarted()
  {
    if (NetworkManager.Singleton.IsHost)
    {
      HandleClientConnect(NetworkManager.Singleton.LocalClientId);
    }
  }
}
