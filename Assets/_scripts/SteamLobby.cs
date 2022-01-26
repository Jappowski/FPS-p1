using Mirror;
using Steamworks;
using UnityEngine;

public class SteamLobby : MonoBehaviour {
    [SerializeField] private GameObject buttons = null;
    private NetworkManager networkManager;
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;
    private const string HostAddresKey = "HostAddress";

    private void Start() {
        networkManager = GetComponent<NetworkManager>();

        if (!SteamManager.Initialized) {
            return;
        }

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public void HostLobby() {
        buttons.SetActive(false);
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);
    }

    private void OnLobbyCreated(LobbyCreated_t callBack) {
        if (callBack.m_eResult != EResult.k_EResultOK) {
            buttons.SetActive(true);
            return;
        }

        networkManager.StartHost();

        SteamMatchmaking.SetLobbyData(new CSteamID(callBack.m_ulSteamIDLobby), HostAddresKey,
            SteamUser.GetSteamID().ToString());
    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback) {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback) {
        if (NetworkServer.active) {
            return;
        }

        string hostAdress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddresKey);

        networkManager.networkAddress = hostAdress;
        networkManager.StartClient();

        GameEvents.BroadcastOnGameStateChange(GameManager.GameState.InGame);
        buttons.SetActive(false);
        GameManager.instance.hud.StartUi.SetActive(false);
        GameManager.instance.hud.InGameHUD.SetActive(true);
    }
}