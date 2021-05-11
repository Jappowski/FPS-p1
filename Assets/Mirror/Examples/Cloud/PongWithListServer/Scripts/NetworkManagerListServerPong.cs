using UnityEngine;

namespace Mirror.Cloud.Example
{
    public sealed class NetworkManagerListServerPong : NetworkManagerListServer
    {
        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            Debug.Assert(startPositions.Count == 2, "Pong Scene should have 2 start Positions");
            // add player at correct spawn position
            var startPos = numPlayers == 0 ? startPositions[0] : startPositions[1];

            var player = startPos != null
                ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                : Instantiate(playerPrefab);

            NetworkServer.AddPlayerForConnection(conn, player);
        }
    }
}