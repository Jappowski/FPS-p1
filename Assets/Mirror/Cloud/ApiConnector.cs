using Mirror.Cloud.ListServerService;
using UnityEngine;

namespace Mirror.Cloud
{
    /// <summary>
    ///     Used to requests and responses from the mirror api
    /// </summary>
    public interface IApiConnector
    {
        ListServer ListServer { get; }
    }

    /// <summary>
    ///     Used to requests and responses from the mirror api
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/CloudServices/ApiConnector")]
    [HelpURL("https://mirror-networking.com/docs/api/Mirror.Cloud.ApiConnector.html")]
    public class ApiConnector : MonoBehaviour, IApiConnector, ICoroutineRunner
    {
        private IRequestCreator requestCreator;

        public ListServer ListServer { get; private set; }

        private void Awake()
        {
            requestCreator = new RequestCreator(ApiAddress, ApiKey, this);

            InitListServer();
        }

        private void InitListServer()
        {
            IListServerServerApi serverApi = new ListServerServerApi(this, requestCreator);
            IListServerClientApi clientApi = new ListServerClientApi(this, requestCreator, _onServerListUpdated);
            ListServer = new ListServer(serverApi, clientApi);
        }

        public void OnDestroy()
        {
            ListServer?.ServerApi.Shutdown();
            ListServer?.ClientApi.Shutdown();
        }

        #region Inspector

        [Header("Settings")] [Tooltip("Base URL of api, including https")] [SerializeField]
        private readonly string ApiAddress = "";

        [Tooltip("Api key required to access api")] [SerializeField]
        private readonly string ApiKey = "";

        [Header("Events")] [Tooltip("Triggered when server list updates")] [SerializeField]
        private readonly ServerListEvent _onServerListUpdated = new ServerListEvent();

        #endregion
    }
}