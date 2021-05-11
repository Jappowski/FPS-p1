using UnityEngine;
using UnityEngine.UI;

namespace Mirror.Cloud.Example
{
    /// <summary>
    ///     Uses the ApiConnector on NetworkManager to update the Server list
    /// </summary>
    public class ServerListManager : MonoBehaviour
    {
        [Header("Auto Refresh")] [SerializeField]
        private readonly bool autoRefreshServerlist = false;

        private ApiConnector connector;

        [Header("UI")] [SerializeField] private readonly ServerListUI listUI = null;

        [Header("Buttons")] [SerializeField] private readonly Button refreshButton = null;

        [SerializeField] private readonly int refreshinterval = 20;
        [SerializeField] private readonly Button startServerButton = null;

        private void Start()
        {
            var manager = NetworkManager.singleton;
            connector = manager.GetComponent<ApiConnector>();

            connector.ListServer.ClientApi.onServerListUpdated += listUI.UpdateList;

            if (autoRefreshServerlist) connector.ListServer.ClientApi.StartGetServerListRepeat(refreshinterval);

            AddButtonHandlers();
        }

        private void AddButtonHandlers()
        {
            refreshButton.onClick.AddListener(RefreshButtonHandler);
            startServerButton.onClick.AddListener(StartServerButtonHandler);
        }

        private void OnDestroy()
        {
            if (connector == null)
                return;

            if (autoRefreshServerlist) connector.ListServer.ClientApi.StopGetServerListRepeat();

            connector.ListServer.ClientApi.onServerListUpdated -= listUI.UpdateList;
        }

        public void RefreshButtonHandler()
        {
            connector.ListServer.ClientApi.GetServerList();
        }

        public void StartServerButtonHandler()
        {
            NetworkManager.singleton.StartServer();
        }
    }
}