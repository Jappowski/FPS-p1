
using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking.Types;
using MLAPI;
using UnityEngine.UIElements;

public class PasswordNetworkManager : MonoBehaviour
{
  [SerializeField] TMP_InputField passwordInputField;
  [SerializeField] GameObject passwordEntryUi;
  [SerializeField]  GameObject[] spawnPoint;


  void Start()
  {
    NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
    NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnect;
    NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
  }

  void update()
  {
    if (Input.GetKey(KeyCode.Escape))
    {
      Debug.Log("JD");
      Leave();
    }
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
    NetworkManager.Singleton.StartHost(spawnPoint[0].transform.position, spawnPoint[0].transform.rotation);
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
  }

  private void ApprovalCheck(byte[] connectionData, ulong clientId, NetworkManager.ConnectionApprovedDelegate callback)
  {
    Vector3 spawnPos = Vector3.zero;
    Quaternion spawnRot = Quaternion.identity; 
    string password = Encoding.ASCII.GetString(connectionData);
    bool approvalConnection = password == passwordInputField.text;

    switch (NetworkManager.Singleton.ConnectedClients.Count)
    {
      case 1:
        spawnPos= new Vector3(spawnPoint[1].transform.position.x, spawnPoint[1].transform.position.y, spawnPoint[1].transform.position.z);
        spawnRot =Quaternion.Euler(spawnPoint[1].transform.eulerAngles);
        break;
      case 2: 
        spawnPos= new Vector3(spawnPoint[2].transform.position.x, spawnPoint[2].transform.position.y, spawnPoint[2].transform.position.z);
        spawnRot =Quaternion.Euler(spawnPoint[2].transform.eulerAngles);
        break;
    }
    callback(true, null, approvalConnection, spawnPos, spawnRot);
  }

  void HandleClientConnect(ulong clientId)
  {
    if (clientId == NetworkManager.Singleton.LocalClientId)
    {
      passwordEntryUi.SetActive(false);
    }
  }
  void HandleClientDisconnect(ulong clientId)
  {
    if (clientId == NetworkManager.Singleton.LocalClientId)
    {
      passwordEntryUi.SetActive(true);
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
