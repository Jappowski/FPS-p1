using System;

namespace Mirror.Cloud.Example
{
    public class NetworkManagerListServer : NetworkManager
    {
        public event Action onServerStarted;


        public event Action onServerStopped;


        public event OnPlayerListChanged onPlayerListChanged;

        public delegate void OnPlayerListChanged(int playerCount);


        int connectionCount => NetworkServer.connections.Count;

        public override void OnServerConnect(NetworkConnection conn)
        {
            int count = connectionCount;
            if (count > maxConnections)
            {
                conn.Disconnect();
                return;
            }

            onPlayerListChanged?.Invoke(count);
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            base.OnServerDisconnect(conn);
            onPlayerListChanged?.Invoke(connectionCount);
        }

        public override void OnStartServer()
        {
            onServerStarted?.Invoke();
        }

        public override void OnStopServer()
        {
            onServerStopped?.Invoke();
        }
    }
}