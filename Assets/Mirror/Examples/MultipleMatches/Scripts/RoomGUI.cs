using UnityEngine;
using UnityEngine.UI;

namespace Mirror.Examples.MultipleMatch
{
    public class RoomGUI : MonoBehaviour
    {
        public GameObject cancelButton;
        public GameObject leaveButton;
        public bool owner;
        public GameObject playerList;
        public GameObject playerPrefab;
        public Button startButton;

        public void RefreshRoomPlayers(PlayerInfo[] playerInfos)
        {
            // Debug.Log($"RefreshRoomPlayers: {playerInfos.Length} playerInfos");

            foreach (Transform child in playerList.transform) Destroy(child.gameObject);

            startButton.interactable = false;
            var everyoneReady = true;

            foreach (var playerInfo in playerInfos)
            {
                var newPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
                newPlayer.transform.SetParent(playerList.transform, false);
                newPlayer.GetComponent<PlayerGUI>().SetPlayerInfo(playerInfo);
                if (!playerInfo.ready) everyoneReady = false;
            }

            startButton.interactable = everyoneReady && owner && playerInfos.Length > 1;
        }

        public void SetOwner(bool owner)
        {
            this.owner = owner;
            cancelButton.SetActive(owner);
            leaveButton.SetActive(!owner);
        }
    }
}