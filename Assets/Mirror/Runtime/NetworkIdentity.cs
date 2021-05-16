#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Mirror.RemoteCalls;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_2018_3_OR_NEWER
using UnityEditor.Experimental.SceneManagement;

#endif
#endif

namespace Mirror
{
    public enum Visibility
    {
        Default,
        ForceHidden,
        ForceShown
    }

    [DisallowMultipleComponent]
    [AddComponentMenu("Network/NetworkIdentity")]
    [HelpURL("https://mirror-networking.com/docs/Articles/Components/NetworkIdentity.html")]
    public sealed class NetworkIdentity : MonoBehaviour
    {
        public bool isClient { get; internal set; }

        public bool isServer { get; internal set; }

        public bool isLocalPlayer { get; internal set; }

        public bool isServerOnly => isServer && !isClient;

        public bool isClientOnly => isClient && !isServer;

        public bool hasAuthority { get; internal set; }

        public Dictionary<int, NetworkConnection> observers;

        public uint netId { get; internal set; }

        [FormerlySerializedAs("m_SceneId"), HideInInspector]
        public ulong sceneId;

        [FormerlySerializedAs("m_ServerOnly")] [Tooltip("Prevents this object from being spawned / enabled on clients")]
        public bool serverOnly;

        internal bool destroyCalled;

        public NetworkConnection connectionToServer { get; internal set; }

        public NetworkConnectionToClient connectionToClient
        {
            get => _connectionToClient;
            internal set
            {
                _connectionToClient?.RemoveOwnedObject(this);
                _connectionToClient = value;
                _connectionToClient?.AddOwnedObject(this);
            }
        }

        NetworkConnectionToClient _connectionToClient;

        public static readonly Dictionary<uint, NetworkIdentity> spawned =
            new Dictionary<uint, NetworkIdentity>();

        NetworkBehaviour[] _NetworkBehaviours;

        public NetworkBehaviour[] NetworkBehaviours
        {
            get
            {
                if (_NetworkBehaviours == null)
                {
                    _NetworkBehaviours = GetComponents<NetworkBehaviour>();
                    if (_NetworkBehaviours.Length > byte.MaxValue)
                    {
                        Debug.LogError(
                            $"Only {byte.MaxValue} NetworkBehaviour components are allowed for NetworkIdentity: {name} because we send the index as byte.",
                            this);
                        Array.Resize(ref _NetworkBehaviours, byte.MaxValue);
                    }
                }

                return _NetworkBehaviours;
            }
        }

#pragma warning disable 618
        NetworkVisibility visibilityCache;
        [Obsolete(NetworkVisibilityObsoleteMessage.Message)]
        public NetworkVisibility visibility
        {
            get
            {
                if (visibilityCache == null)
                {
                    visibilityCache = GetComponent<NetworkVisibility>();
                }

                return visibilityCache;
            }
        }
#pragma warning restore 618

        [Tooltip(
            "Visibility can overwrite interest management. ForceHidden can be useful to hide monsters while they respawn. ForceShown can be useful for score NetworkIdentities that should always broadcast to everyone in the world.")]
        public Visibility visible = Visibility.Default;

        public Guid assetId
        {
            get
            {
#if UNITY_EDITOR
                if (string.IsNullOrEmpty(m_AssetId))
                    SetupIDs();
#endif
                return string.IsNullOrEmpty(m_AssetId) ? Guid.Empty : new Guid(m_AssetId);
            }
            internal set
            {
                string newAssetIdString = value == Guid.Empty ? string.Empty : value.ToString("N");
                string oldAssetIdSrting = m_AssetId;

                if (oldAssetIdSrting == newAssetIdString)
                {
                    return;
                }

                if (string.IsNullOrEmpty(newAssetIdString))
                {
                    Debug.LogError(
                        $"Can not set AssetId to empty guid on NetworkIdentity '{name}', old assetId '{oldAssetIdSrting}'");
                    return;
                }

                if (!string.IsNullOrEmpty(oldAssetIdSrting))
                {
                    Debug.LogError(
                        $"Can not Set AssetId on NetworkIdentity '{name}' because it already had an assetId, current assetId '{oldAssetIdSrting}', attempted new assetId '{newAssetIdString}'");
                    return;
                }

                m_AssetId = newAssetIdString;
            }
        }

        [SerializeField, HideInInspector] string m_AssetId;

        static readonly Dictionary<ulong, NetworkIdentity> sceneIds =
            new Dictionary<ulong, NetworkIdentity>();

        public static NetworkIdentity GetSceneIdentity(ulong id) => sceneIds[id];

        internal void SetClientOwner(NetworkConnection conn)
        {
            if (connectionToClient != null && conn != connectionToClient)
            {
                Debug.LogError($"Object {this} netId={netId} already has an owner. Use RemoveClientAuthority() first",
                    this);
                return;
            }

            connectionToClient = (NetworkConnectionToClient) conn;
        }

        static uint nextNetworkId = 1;
        internal static uint GetNextNetworkId() => nextNetworkId++;

        public static void ResetNextNetworkId() => nextNetworkId = 1;

        public delegate void ClientAuthorityCallback(NetworkConnection conn, NetworkIdentity identity,
            bool authorityState);

        public static event ClientAuthorityCallback clientAuthorityCallback;

        internal void RemoveObserverInternal(NetworkConnection conn)
        {
            observers?.Remove(conn.connectionId);
        }

        [SerializeField, HideInInspector] bool hasSpawned;
        public bool SpawnedFromInstantiate { get; private set; }

        void Awake()
        {
            if (hasSpawned)
            {
                Debug.LogError(
                    $"{name} has already spawned. Don't call Instantiate for NetworkIdentities that were in the scene since the beginning (aka scene objects).  Otherwise the client won't know which object to use for a SpawnSceneObject message.");
                SpawnedFromInstantiate = true;
                Destroy(gameObject);
            }

            hasSpawned = true;
        }

        void OnValidate()
        {
            hasSpawned = false;

#if UNITY_EDITOR
            SetupIDs();
#endif
        }

#if UNITY_EDITOR
        void AssignAssetID(string path) => m_AssetId = AssetDatabase.AssetPathToGUID(path);
        void AssignAssetID(GameObject prefab) => AssignAssetID(AssetDatabase.GetAssetPath(prefab));

        void AssignSceneID()
        {
            if (Application.isPlaying)
                return;

            bool duplicate = sceneIds.TryGetValue(sceneId, out NetworkIdentity existing) && existing != null &&
                             existing != this;
            if (sceneId == 0 || duplicate)
            {
                sceneId = 0;

                if (BuildPipeline.isBuildingPlayer)
                    throw new InvalidOperationException("Scene " + gameObject.scene.path +
                                                        " needs to be opened and resaved before building, because the scene object " +
                                                        name + " has no valid sceneId yet.");

                Undo.RecordObject(this, "Generated SceneId");

                uint randomId = Utils.GetTrueRandomUInt();

                duplicate = sceneIds.TryGetValue(randomId, out existing) && existing != null && existing != this;
                if (!duplicate)
                {
                    sceneId = randomId;
                }
            }

            sceneIds[sceneId] = this;
        }

        public void SetSceneIdSceneHashPartInternal()
        {
            string scenePath = gameObject.scene.path.ToLower();

            uint pathHash = (uint) scenePath.GetStableHashCode();

            ulong shiftedHash = (ulong) pathHash << 32;

            sceneId = (sceneId & 0xFFFFFFFF) | shiftedHash;
        }

        void SetupIDs()
        {
            if (Utils.IsPrefab(gameObject))
            {
                sceneId = 0;
                AssignAssetID(gameObject);
            }
            else if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                if (PrefabStageUtility.GetPrefabStage(gameObject) != null)
                {
                    sceneId = 0;
#if UNITY_2020_1_OR_NEWER
                    string path = PrefabStageUtility.GetCurrentPrefabStage().assetPath;
#else
                    string path = PrefabStageUtility.GetCurrentPrefabStage().prefabAssetPath;
#endif

                    AssignAssetID(path);
                }
            }
            else if (Utils.IsSceneObjectWithPrefabParent(gameObject, out GameObject prefab))
            {
                AssignSceneID();
                AssignAssetID(prefab);
            }
            else
            {
                AssignSceneID();

                if (!EditorApplication.isPlaying)
                {
                    m_AssetId = "";
                }
            }
        }
#endif

        void OnDestroy()
        {
            if (SpawnedFromInstantiate)
                return;
            if (isServer && !destroyCalled)
            {
                NetworkServer.Destroy(gameObject);
            }

            if (isLocalPlayer)
            {
                if (NetworkClient.localPlayer == this)
                    NetworkClient.localPlayer = null;
            }
        }

        internal void OnStartServer()
        {
            if (isServer)
                return;

            isServer = true;
            if (NetworkClient.localPlayer == this)
            {
                isLocalPlayer = true;
            }

            if (netId != 0)
            {
                return;
            }

            netId = GetNextNetworkId();
            observers = new Dictionary<int, NetworkConnection>();

            spawned[netId] = this;

            if (NetworkClient.active)
            {
                isClient = true;
            }

            foreach (NetworkBehaviour comp in NetworkBehaviours)
            {
                try
                {
                    comp.OnStartServer();
                }
                catch (Exception e)
                {
                    Debug.LogError("Exception in OnStartServer:" + e.Message + " " + e.StackTrace);
                }
            }
        }

        internal void OnStopServer()
        {
            foreach (NetworkBehaviour comp in NetworkBehaviours)
            {
                try
                {
                    comp.OnStopServer();
                }
                catch (Exception e)
                {
                    Debug.LogError("Exception in OnStopServer:" + e.Message + " " + e.StackTrace);
                }
            }
        }

        bool clientStarted;

        internal void OnStartClient()
        {
            if (clientStarted)
                return;
            clientStarted = true;

            isClient = true;

            if (NetworkClient.localPlayer == this)
            {
                isLocalPlayer = true;
            }

            foreach (NetworkBehaviour comp in NetworkBehaviours)
            {
                try
                {
                    comp.OnStartClient();
                }
                catch (Exception e)
                {
                    Debug.LogError("Exception in OnStartClient:" + e.Message + " " + e.StackTrace);
                }
            }
        }

        internal void OnStopClient()
        {
            foreach (NetworkBehaviour comp in NetworkBehaviours)
            {
                try
                {
                    comp.OnStopClient();
                }
                catch (Exception e)
                {
                    Debug.LogError("Exception in OnStopClient:" + e.Message + " " + e.StackTrace);
                }
            }
        }

        static NetworkIdentity previousLocalPlayer = null;

        internal void OnStartLocalPlayer()
        {
            if (previousLocalPlayer == this)
                return;
            previousLocalPlayer = this;

            isLocalPlayer = true;

            foreach (NetworkBehaviour comp in NetworkBehaviours)
            {
                try
                {
                    comp.OnStartLocalPlayer();
                }
                catch (Exception e)
                {
                    Debug.LogError("Exception in OnStartLocalPlayer:" + e.Message + " " + e.StackTrace);
                }
            }
        }

        bool hadAuthority;

        internal void NotifyAuthority()
        {
            if (!hadAuthority && hasAuthority)
                OnStartAuthority();
            if (hadAuthority && !hasAuthority)
                OnStopAuthority();
            hadAuthority = hasAuthority;
        }

        internal void OnStartAuthority()
        {
            foreach (NetworkBehaviour comp in NetworkBehaviours)
            {
                try
                {
                    comp.OnStartAuthority();
                }
                catch (Exception e)
                {
                    Debug.LogError("Exception in OnStartAuthority:" + e.Message + " " + e.StackTrace);
                }
            }
        }

        internal void OnStopAuthority()
        {
            foreach (NetworkBehaviour comp in NetworkBehaviours)
            {
                try
                {
                    comp.OnStopAuthority();
                }
                catch (Exception e)
                {
                    Debug.LogError("Exception in OnStopAuthority:" + e.Message + " " + e.StackTrace);
                }
            }
        }


        [Obsolete("Use NetworkServer.RebuildObservers(identity, initialize) instead.")]
        public void RebuildObservers(bool initialize) => NetworkServer.RebuildObservers(this, initialize);


        internal void OnSetHostVisibility(bool visible)
        {
            foreach (Renderer rend in GetComponentsInChildren<Renderer>())
                rend.enabled = visible;
        }


        bool OnSerializeSafely(NetworkBehaviour comp, NetworkWriter writer, bool initialState)
        {
            int headerPosition = writer.Position;
            writer.WriteInt32(0);
            int contentPosition = writer.Position;


            bool result = false;
            try
            {
                result = comp.OnSerialize(writer, initialState);
            }
            catch (Exception e)
            {
                Debug.LogError("OnSerialize failed for: object=" + name + " component=" + comp.GetType() + " sceneId=" +
                               sceneId.ToString("X") + "\n\n" + e);
            }

            int endPosition = writer.Position;


            writer.Position = headerPosition;
            writer.WriteInt32(endPosition - contentPosition);
            writer.Position = endPosition;


            return result;
        }


        internal void OnSerializeAllSafely(bool initialState, NetworkWriter ownerWriter, out int ownerWritten,
            NetworkWriter observersWriter, out int observersWritten)
        {
            ownerWritten = observersWritten = 0;


            NetworkBehaviour[] components = NetworkBehaviours;
            if (components.Length > byte.MaxValue)
                throw new IndexOutOfRangeException(
                    $"{name} has more than {byte.MaxValue} components. This is not supported.");


            for (int i = 0; i < components.Length; ++i)
            {
                NetworkBehaviour comp = components[i];
                if (initialState || comp.IsDirty())
                {
                    int startPosition = ownerWriter.Position;


                    ownerWriter.WriteByte((byte) i);


                    OnSerializeSafely(comp, ownerWriter, initialState);
                    ++ownerWritten;


                    if (comp.syncMode == SyncMode.Observers)
                    {
                        ArraySegment<byte> segment = ownerWriter.ToArraySegment();
                        int length = ownerWriter.Position - startPosition;
                        observersWriter.WriteBytes(segment.Array, startPosition, length);
                        ++observersWritten;
                    }
                }
            }
        }

        void OnDeserializeSafely(NetworkBehaviour comp, NetworkReader reader, bool initialState)
        {
            int contentSize = reader.ReadInt32();
            int chunkStart = reader.Position;
            int chunkEnd = reader.Position + contentSize;


            try
            {
                comp.OnDeserialize(reader, initialState);
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"OnDeserialize failed Exception={e.GetType()} (see below) object={name} component={comp.GetType()} sceneId={sceneId:X} length={contentSize}. Possible Reasons:\n" +
                    $"  * Do {comp.GetType()}'s OnSerialize and OnDeserialize calls write the same amount of data({contentSize} bytes)? \n" +
                    $"  * Was there an exception in {comp.GetType()}'s OnSerialize/OnDeserialize code?\n" +
                    $"  * Are the server and client the exact same project?\n" +
                    $"  * Maybe this OnDeserialize call was meant for another GameObject? The sceneIds can easily get out of sync if the Hierarchy was modified only in the client OR the server. Try rebuilding both.\n\n" +
                    $"Exception {e}");
            }


            if (reader.Position != chunkEnd)
            {
                int bytesRead = reader.Position - chunkStart;
                Debug.LogWarning("OnDeserialize was expected to read " + contentSize + " instead of " + bytesRead +
                                 " bytes for object:" + name + " component=" + comp.GetType() + " sceneId=" +
                                 sceneId.ToString("X") +
                                 ". Make sure that OnSerialize and OnDeserialize write/read the same amount of data in all cases.");


                reader.Position = chunkEnd;
            }
        }

        internal void OnDeserializeAllSafely(NetworkReader reader, bool initialState)
        {
            NetworkBehaviour[] components = NetworkBehaviours;
            while (reader.Position < reader.Length)
            {
                byte index = reader.ReadByte();
                if (index < components.Length)
                {
                    OnDeserializeSafely(components[index], reader, initialState);
                }
            }
        }


        internal void HandleRemoteCall(int componentIndex, int functionHash, MirrorInvokeType invokeType,
            NetworkReader reader, NetworkConnectionToClient senderConnection = null)
        {
            if (this == null)
            {
                Debug.LogWarning($"{invokeType} [{functionHash}] received for deleted object [netId={netId}]");
                return;
            }


            if (componentIndex < 0 || componentIndex >= NetworkBehaviours.Length)
            {
                Debug.LogWarning($"Component [{componentIndex}] not found for [netId={netId}]");
                return;
            }

            NetworkBehaviour invokeComponent = NetworkBehaviours[componentIndex];
            if (!RemoteCallHelper.InvokeHandlerDelegate(functionHash, invokeType, reader, invokeComponent,
                senderConnection))
            {
                Debug.LogError(
                    $"Found no receiver for incoming {invokeType} [{functionHash}] on {gameObject.name}, the server and client should have the same NetworkBehaviour instances [netId={netId}].");
            }
        }


        internal CommandInfo GetCommandInfo(int componentIndex, int cmdHash)
        {
            if (this == null)
            {
                return default;
            }


            if (0 <= componentIndex && componentIndex < NetworkBehaviours.Length)
            {
                NetworkBehaviour invokeComponent = NetworkBehaviours[componentIndex];
                return RemoteCallHelper.GetCommandInfo(cmdHash, invokeComponent);
            }
            else
            {
                return default;
            }
        }


        internal void ClearObservers()
        {
            if (observers != null)
            {
                foreach (NetworkConnection conn in observers.Values)
                {
                    conn.RemoveFromObserving(this, true);
                }

                observers.Clear();
            }
        }

        internal void AddObserver(NetworkConnection conn)
        {
            if (observers == null)
            {
                Debug.LogError("AddObserver for " + gameObject + " observer list is null");
                return;
            }

            if (observers.ContainsKey(conn.connectionId))
            {
                return;
            }


            observers[conn.connectionId] = conn;
            conn.AddToObserving(this);
        }


        public bool AssignClientAuthority(NetworkConnection conn)
        {
            if (!isServer)
            {
                Debug.LogError("AssignClientAuthority can only be called on the server for spawned objects.");
                return false;
            }

            if (conn == null)
            {
                Debug.LogError("AssignClientAuthority for " + gameObject +
                               " owner cannot be null. Use RemoveClientAuthority() instead.");
                return false;
            }

            if (connectionToClient != null && conn != connectionToClient)
            {
                Debug.LogError("AssignClientAuthority for " + gameObject +
                               " already has an owner. Use RemoveClientAuthority() first.");
                return false;
            }

            SetClientOwner(conn);


            NetworkServer.SendSpawnMessage(this, conn);

            clientAuthorityCallback?.Invoke(conn, this, true);

            return true;
        }


        public void RemoveClientAuthority()
        {
            if (!isServer)
            {
                Debug.LogError("RemoveClientAuthority can only be called on the server for spawned objects.");
                return;
            }

            if (connectionToClient?.identity == this)
            {
                Debug.LogError("RemoveClientAuthority cannot remove authority for a player object");
                return;
            }

            if (connectionToClient != null)
            {
                clientAuthorityCallback?.Invoke(connectionToClient, this, false);

                NetworkConnectionToClient previousOwner = connectionToClient;


                connectionToClient = null;


                NetworkServer.SendSpawnMessage(this, previousOwner);


                connectionToClient = null;
            }
        }


        internal void Reset()
        {
            ResetSyncObjects();

            hasSpawned = false;
            clientStarted = false;
            isClient = false;
            isServer = false;


            netId = 0;
            connectionToServer = null;
            connectionToClient = null;
            _NetworkBehaviours = null;

            ClearObservers();


            if (isLocalPlayer)
            {
                if (NetworkClient.localPlayer == this)
                    NetworkClient.localPlayer = null;
            }

            isLocalPlayer = false;
        }


        internal void ClearAllComponentsDirtyBits()
        {
            foreach (NetworkBehaviour comp in NetworkBehaviours)
            {
                comp.ClearAllDirtyBits();
            }
        }


        internal void ClearDirtyComponentsDirtyBits()
        {
            foreach (NetworkBehaviour comp in NetworkBehaviours)
            {
                if (comp.IsDirty())
                {
                    comp.ClearAllDirtyBits();
                }
            }
        }

        void ResetSyncObjects()
        {
            foreach (NetworkBehaviour comp in NetworkBehaviours)
            {
                comp.ResetSyncObjects();
            }
        }
    }
}