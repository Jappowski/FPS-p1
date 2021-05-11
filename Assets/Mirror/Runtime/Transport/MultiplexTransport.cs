using System;
using System.Text;
using UnityEngine;

namespace Mirror
{
    // a transport that can listen to multiple underlying transport at the same time
    [DisallowMultipleComponent]
    public class MultiplexTransport : Transport
    {
        private Transport available;
        public Transport[] transports;

        public void Awake()
        {
            if (transports == null || transports.Length == 0)
                Debug.LogError("Multiplex transport requires at least 1 underlying transport");
        }

        public override void ClientEarlyUpdate()
        {
            foreach (var transport in transports) transport.ClientEarlyUpdate();
        }

        public override void ServerEarlyUpdate()
        {
            foreach (var transport in transports) transport.ServerEarlyUpdate();
        }

        public override void ClientLateUpdate()
        {
            foreach (var transport in transports) transport.ClientLateUpdate();
        }

        public override void ServerLateUpdate()
        {
            foreach (var transport in transports) transport.ServerLateUpdate();
        }

        private void OnEnable()
        {
            foreach (var transport in transports) transport.enabled = true;
        }

        private void OnDisable()
        {
            foreach (var transport in transports) transport.enabled = false;
        }

        public override bool Available()
        {
            // available if any of the transports is available
            foreach (var transport in transports)
                if (transport.Available())
                    return true;
            return false;
        }

        public override int GetMaxPacketSize(int channelId = 0)
        {
            // finding the max packet size in a multiplex environment has to be
            // done very carefully:
            // * servers run multiple transports at the same time
            // * different clients run different transports
            // * there should only ever be ONE true max packet size for everyone,
            //   otherwise a spawn message might be sent to all tcp sockets, but
            //   be too big for some udp sockets. that would be a debugging
            //   nightmare and allow for possible exploits and players on
            //   different platforms seeing a different game state.
            // => the safest solution is to use the smallest max size for all
            //    transports. that will never fail.
            var mininumAllowedSize = int.MaxValue;
            foreach (var transport in transports)
            {
                var size = transport.GetMaxPacketSize(channelId);
                mininumAllowedSize = Mathf.Min(size, mininumAllowedSize);
            }

            return mininumAllowedSize;
        }

        public override void Shutdown()
        {
            foreach (var transport in transports) transport.Shutdown();
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach (var transport in transports) builder.AppendLine(transport.ToString());
            return builder.ToString().Trim();
        }

        #region Client

        public override void ClientConnect(string address)
        {
            foreach (var transport in transports)
                if (transport.Available())
                {
                    available = transport;
                    transport.OnClientConnected = OnClientConnected;
                    transport.OnClientDataReceived = OnClientDataReceived;
                    transport.OnClientError = OnClientError;
                    transport.OnClientDisconnected = OnClientDisconnected;
                    transport.ClientConnect(address);
                    return;
                }

            throw new ArgumentException("No transport suitable for this platform");
        }

        public override void ClientConnect(Uri uri)
        {
            foreach (var transport in transports)
                if (transport.Available())
                    try
                    {
                        available = transport;
                        transport.OnClientConnected = OnClientConnected;
                        transport.OnClientDataReceived = OnClientDataReceived;
                        transport.OnClientError = OnClientError;
                        transport.OnClientDisconnected = OnClientDisconnected;
                        transport.ClientConnect(uri);
                        return;
                    }
                    catch (ArgumentException)
                    {
                        // transport does not support the schema, just move on to the next one
                    }

            throw new ArgumentException("No transport suitable for this platform");
        }

        public override bool ClientConnected()
        {
            return (object) available != null && available.ClientConnected();
        }

        public override void ClientDisconnect()
        {
            if ((object) available != null)
                available.ClientDisconnect();
        }

        public override void ClientSend(int channelId, ArraySegment<byte> segment)
        {
            available.ClientSend(channelId, segment);
        }

        #endregion

        #region Server

        // connection ids get mapped to base transports
        // if we have 3 transports,  then
        // transport 0 will produce connection ids [0, 3, 6, 9, ...]
        // transport 1 will produce connection ids [1, 4, 7, 10, ...]
        // transport 2 will produce connection ids [2, 5, 8, 11, ...]
        private int FromBaseId(int transportId, int connectionId)
        {
            return connectionId * transports.Length + transportId;
        }

        private int ToBaseId(int connectionId)
        {
            return connectionId / transports.Length;
        }

        private int ToTransportId(int connectionId)
        {
            return connectionId % transports.Length;
        }

        private void AddServerCallbacks()
        {
            // wire all the base transports to my events
            for (var i = 0; i < transports.Length; i++)
            {
                // this is required for the handlers,  if I use i directly
                // then all the handlers will use the last i
                var locali = i;
                var transport = transports[i];

                transport.OnServerConnected = baseConnectionId =>
                {
                    OnServerConnected.Invoke(FromBaseId(locali, baseConnectionId));
                };

                transport.OnServerDataReceived = (baseConnectionId, data, channel) =>
                {
                    OnServerDataReceived.Invoke(FromBaseId(locali, baseConnectionId), data, channel);
                };

                transport.OnServerError = (baseConnectionId, error) =>
                {
                    OnServerError.Invoke(FromBaseId(locali, baseConnectionId), error);
                };
                transport.OnServerDisconnected = baseConnectionId =>
                {
                    OnServerDisconnected.Invoke(FromBaseId(locali, baseConnectionId));
                };
            }
        }

        // for now returns the first uri,
        // should we return all available uris?
        public override Uri ServerUri()
        {
            return transports[0].ServerUri();
        }


        public override bool ServerActive()
        {
            // avoid Linq.All allocations
            foreach (var transport in transports)
                if (!transport.ServerActive())
                    return false;
            return true;
        }

        public override string ServerGetClientAddress(int connectionId)
        {
            var baseConnectionId = ToBaseId(connectionId);
            var transportId = ToTransportId(connectionId);
            return transports[transportId].ServerGetClientAddress(baseConnectionId);
        }

        public override bool ServerDisconnect(int connectionId)
        {
            var baseConnectionId = ToBaseId(connectionId);
            var transportId = ToTransportId(connectionId);
            return transports[transportId].ServerDisconnect(baseConnectionId);
        }

        public override void ServerSend(int connectionId, int channelId, ArraySegment<byte> segment)
        {
            var baseConnectionId = ToBaseId(connectionId);
            var transportId = ToTransportId(connectionId);

            for (var i = 0; i < transports.Length; ++i)
                if (i == transportId)
                    transports[i].ServerSend(baseConnectionId, channelId, segment);
        }

        public override void ServerStart()
        {
            foreach (var transport in transports)
            {
                AddServerCallbacks();
                transport.ServerStart();
            }
        }

        public override void ServerStop()
        {
            foreach (var transport in transports) transport.ServerStop();
        }

        #endregion
    }
}