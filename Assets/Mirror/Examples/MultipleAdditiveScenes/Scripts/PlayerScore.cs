using UnityEngine;

namespace Mirror.Examples.MultipleAdditiveScenes
{
    public class PlayerScore : NetworkBehaviour
    {
        public int clientMatchIndex = -1;

        [SyncVar] public int matchIndex;

        [SyncVar] public int playerNumber;

        [SyncVar] public uint score;

        [SyncVar] public int scoreIndex;

        private void OnGUI()
        {
            if (!isServerOnly && !isLocalPlayer && clientMatchIndex < 0)
                clientMatchIndex = NetworkClient.connection.identity.GetComponent<PlayerScore>().matchIndex;

            if (isLocalPlayer || matchIndex == clientMatchIndex)
                GUI.Box(new Rect(10f + scoreIndex * 110, 10f, 100f, 25f), $"P{playerNumber}: {score}");
        }
    }
}