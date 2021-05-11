using System;
using Mirror.Cloud.ListServerService;
using UnityEngine;
using UnityEngine.UI;

namespace Mirror.Cloud.Example
{
    /// <summary>
    ///     Displays a server created by ServerListUI
    /// </summary>
    public class ServerListUIItem : MonoBehaviour
    {
        [SerializeField] private readonly Text addressText = null;

        [SerializeField] private readonly Button joinButton = null;
        [SerializeField] private readonly Text namePlayers = null;
        [SerializeField] private readonly Text nameText = null;
        [SerializeField] private readonly string playersFormat = "{0} / {1}";

        private ServerJson server;

        public void Setup(ServerJson server)
        {
            this.server = server;
            nameText.text = server.displayName;
            namePlayers.text = string.Format(playersFormat, server.playerCount, server.maxPlayerCount);
            addressText.text = server.address;

            joinButton.onClick.AddListener(OnJoinClicked);
        }

        private void OnJoinClicked()
        {
            NetworkManager.singleton.StartClient(new Uri(server.address));
        }
    }
}