using Mirror;

namespace Assets.Lobby.menuScripts
{
    public class NetworkGamePlayerLobby : NetworkBehaviour
    {
        [SyncVar]
        private string displayName = "Loading...";

        private NetworkManagerGame room;

        private NetworkManagerGame Room
        {
            get
            {
                if (room != null) return room;
                return room = NetworkManager.singleton as NetworkManagerGame;
            }
        }

        public override void OnStartClient()
        {
            Room.GamePlayers.Add(this);
        }

        public override void OnStopClient()
        {
            Room.GamePlayers.Remove(this);
        }
        [Server]
        public void SetDisplayName(string displayName)
        {
            this.displayName = displayName;
        }
    }
}