// using Mirror;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;
//
// public class NetworkGamePlayerLobby : NetworkBehaviour
// {
//     [SyncVar(hook = nameof(HandleDisplayNameChanged))]
//     public string DisplayName = "Loading...";
//
//     private bool isLeader;
//
//     [SyncVar(hook = nameof(HandleReadyStatusChanged))]
//     public bool IsReady;
//
//     [Header("UI")] [SerializeField] private readonly GameObject lobbyUI = null;
//
//     [SerializeField] private readonly TMP_Text[] playerNameTexts = new TMP_Text[4];
//     [SerializeField] private readonly TMP_Text[] playerReadyTexts = new TMP_Text[4];
//
//     private NetworkManagerGame room;
//     [SerializeField] private readonly Button startGameButton = null;
//
//     public bool IsLeader
//     {
//         set
//         {
//             isLeader = value;
//             startGameButton.gameObject.SetActive(value);
//         }
//     }
//
//     private NetworkManagerGame Room
//     {
//         get
//         {
//             if (room != null) return room;
//             return room = NetworkManager.singleton as NetworkManagerGame;
//         }
//     }
//
//     public override void OnStartAuthority()
//     {
//         CmdSetDisplayName(PlayerNameInput.DisplayName);
//
//         lobbyUI.SetActive(true);
//     }
//
//     public override void OnStartClient()
//     {
//         Room.RoomPlayers.Add(this);
//
//         UpdateDisplay();
//     }
//
//     public override void OnStopClient()
//     {
//         Room.RoomPlayers.Remove(this);
//
//         UpdateDisplay();
//     }
//
//     public void HandleReadyStatusChanged(bool oldValue, bool newValue)
//     {
//         UpdateDisplay();
//     }
//
//     public void HandleDisplayNameChanged(string oldValue, string newValue)
//     {
//         UpdateDisplay();
//     }
//
//     private void UpdateDisplay()
//     {
//         if (!hasAuthority)
//         {
//             foreach (var player in Room.RoomPlayers)
//                 if (player.hasAuthority)
//                 {
//                     player.UpdateDisplay();
//                     break;
//                 }
//
//             return;
//         }
//
//         for (var i = 0; i < playerNameTexts.Length; i++)
//         {
//             playerNameTexts[i].text = "Waiting For Player...";
//             playerReadyTexts[i].text = string.Empty;
//         }
//
//         for (var i = 0; i < Room.RoomPlayers.Count; i++)
//         {
//             playerNameTexts[i].text = Room.RoomPlayers[i].DisplayName;
//             playerReadyTexts[i].text = Room.RoomPlayers[i].IsReady
//                 ? "<color=green>Ready</color>"
//                 : "<color=red>Not Ready</color>";
//         }
//     }
//
//     public void HandleReadyToStart(bool readyToStart)
//     {
//         if (!isLeader) return;
//
//         startGameButton.interactable = readyToStart;
//     }
//
//     [Command]
//     private void CmdSetDisplayName(string displayName)
//     {
//         DisplayName = displayName;
//     }
//
//     [Command]
//     public void CmdReadyUp()
//     {
//         IsReady = !IsReady;
//
//         Room.NotifyPlayersOfReadyState();
//     }
//
//     [Command]
//     public void CmdStartGame()
//     {
//         if (Room.RoomPlayers[0].connectionToClient != connectionToClient) return;
//     }
// }