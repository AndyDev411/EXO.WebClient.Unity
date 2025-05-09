using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EXO.Networking.Common;
using UnityEngine;

namespace EXO.WebClient
{
    public class ExoNetworkManager : MonoBehaviour
    {

        /// <summary>
        /// The number of ticks per second...
        /// </summary>
        public float ticksPerSecond = 30;

        /// <summary>
        /// Tells wether or not we are host or not.
        /// </summary>
        public bool startAsHost = true;

        /// <summary>
        /// Tells wether or not we are connecting on awake.
        /// </summary>
        public bool connectOnStart = false;

        /// <summary>
        /// Starting example room.
        /// </summary>
        public string startRoomName;

        /// <summary>
        /// The Starting Room Key...
        /// </summary>
        public string startingRoomKey;

        /// <summary>
        /// Name that will be used for connecting to the server.
        /// </summary>
        public string clientName;

        /// <summary>
        /// Static method used for sending things to the relay Server. Can be used by either a Client or a Host.
        /// </summary>
        /// <param name="toSend"> The Packet that we want to send. </param>
        /// <param name="to"> The ClientID of the person we want to send it to. (Only used if NetworkManager is acting as host.) </param>
        public static void Send(Packet toSend, long to = 0)
            => ExoNetworkManager.Instance.SendPacket(toSend, to);

        /// <summary>
        /// Static method use for sending things to the relay Server. Can only be used by a Host.
        /// </summary>
        /// <param name="packet"> Packet we want to send. </param>
        /// <param name="except"> If there is one client we do not want to broadcast to set this to their CliendID. </param>
        public static void Broadcast(Packet packet, long? except = null)
        {
            if (ExoNetworkManager.Instance == null)
            {
                throw new Exception("NetworkManager Instance has not been created, prior to static calls.");
            }

            ExoNetworkManager.Instance.ServerBroadcast(packet, except);
        }

        #region Events

        /// <summary>
        /// Event that is called when we initialize the NetworkManager.
        /// </summary>
        public static event Action OnNetworkManagerInitEvent;

        /// <summary>
        /// Event used execute tick actions.
        /// </summary>
        public static event Action<float> OnClientTickEvent;

        /// <summary>
        /// Event that is called when the Host has connected to the server and was successful.
        /// </summary>
        public event EventHandler<ExoNetworkManager> OnHostStart;

        /// <summary>
        /// Event that is called when the Client joins a room successfully.
        /// </summary>
        public event EventHandler<ExoNetworkManager> OnClientStart;

        /// <summary>
        /// Event that is called when a Client has joined.
        /// </summary>
        public event EventHandler<ExoClient> OnClientJoin;

        /// <summary>
        /// Event that is called when a Client has left.
        /// </summary>
        public event EventHandler<ExoClient> OnClientLeft;

        /// <summary>
        /// Event that is called when we get dissconnected.
        /// </summary>
        public event Action OnDisconnectedEvent;

        #endregion

        /// <summary>
        /// Client WebSocket we will use to communicate to the server.
        /// </summary>
        private IConnection connection;

        /// <summary>
        /// Connection Factory used for managing the connection.
        /// </summary>
        private IConnectionFactory connectionFactory;

        /// <summary>
        /// The local persons ID...
        /// </summary>
        public static long LocalID { get; private set; }

        /// <summary>
        /// Singleton Instance.
        /// </summary>
        public static ExoNetworkManager Instance { get; set; }

        /// <summary>
        /// True if we are the host and false if we are not.
        /// </summary>
        public static bool IsHost { get; private set; }

        /// <summary>
        /// True if we are client and false if not.
        /// </summary>
        public static bool IsClient => !IsHost;

        /// <summary>
        /// The Key to the room we are currently in.
        /// </summary>
        public string RoomKey { get; private set; }

        /// <summary>
        /// The name of the room.
        /// </summary>
        public string RoomName { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        private string roomName;

        private Dictionary<int, MethodInfo> clientHandlers = new();
        private Dictionary<int, MethodInfo> hostHandlers = new();

        private Dictionary<int, MethodInfo> exoSystemClientHandlers = new();
        private Dictionary<int, MethodInfo> exoSystemHostHandlers = new();

        /// <summary>
        /// Clients that are connected to the server.
        /// </summary>
        private Dictionary<long, ExoClient> Clients { get; } = new();

        public void Start()
        {
            this.connectionFactory = this.GetComponent<IConnectionFactory>();


            this.connection = this.connectionFactory.CreateConnection();
            this.connection.OnRecieveEvent += OnMessageHandler;
            this.connection.OnConnectionStartEvent += OnConnectionStartHandler;
            this.connection.OnDisconnectedEvent += OnConnectionDisconnectedHandler;

            if (Instance == null)
            {
                Instance = this;
            }

            InitHandlers();

            if (connectOnStart)
            {
                if (startAsHost)
                {
                    StartHost(startRoomName, clientName);
                }
                else
                {
                    StartClient(startingRoomKey, clientName);
                }
            }

        }

        private void OnConnectionDisconnectedHandler()
        {
            // Stop the ticks...
            StopAllCoroutines();
            OnDisconnectedEvent?.Invoke();
        }

        private void OnConnectionStartHandler()
        {
            if (IsHost)
            {
                try
                {
                    using (var packet = new Packet((byte)PacketType.RequestHostRoom))
                    {
                        packet.Write(clientName);
                        packet.Write(roomName);
                        RoomName = roomName;
                        PSend(packet);


                    }

                }
                catch (Exception exc)
                {
                }
            }
            else
            {

                try
                {
                    using (var packet = new Packet((byte)PacketType.RequestJoinRoom))
                    {
                        packet.Write(clientName);
                        packet.Write(roomKey);
                        RoomKey = roomKey;
                        PSend(packet);
                    }

                }
                catch (Exception exc)
                {

                }
            }
        }



        /// <summary>
        /// Starts the NetworkManager as a Host on the Remote Server.
        /// </summary>
        /// <param name="roomName"> The name of the room you would like to host. </param>
        /// <returns> True if it was successful and false if it was not. </returns>
        public bool StartHost(string roomName, string username)
        {
            this.clientName = username;
            this.roomName = roomName;
            IsHost = true;
            if (!Connect())
            {
                return false;
            }

            return true;

        }
        private string roomKey;

        /// <summary>
        /// Starts the NetworkManager as aHost on the Remote Server.
        /// </summary>
        /// <param name="roomKey"> The Room Key we would like to query for and join. </param>
        /// <returns> True if it sucessfully started as a client, and false if not. </returns>
        public bool StartClient(string roomKey, string username)
        {
            this.clientName = username;
            this.roomKey = roomKey;

            if (!Connect())
            {
                return false;
            }

            return true;


        }

        /// <summary>
        /// Connects to the Server.
        /// </summary>
        /// <returns> True if it sucessfully conects to the server. </returns>
        private bool Connect()
        {
            try
            {
                connection.Connect();
                // Start Connection
                StartCoroutine(HandleServerTick());
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        /// <summary>
        /// Initializes all the handlers.
        /// </summary>
        private void InitHandlers()
        {
            var clientMethods = MethodRetrievalService.GetStaticMethodsWithAttribute<ClientMessageHandlerAttribute>();
            var hostMethods = MethodRetrievalService.GetStaticMethodsWithAttribute<HostMessageHandlerAttribute>();

            var exoSystemClientMethods = MethodRetrievalService.GetStaticMethodsWithAttribute<ExoSystemClientMessageHandlerAttribute>();
            var exoSystemHostMethods = MethodRetrievalService.GetStaticMethodsWithAttribute<ExoSystemHostMessageHandlerAttribute>();

            foreach (var clientMeth in clientMethods)
            {
                var atrib = clientMeth.GetCustomAttribute<ClientMessageHandlerAttribute>();

                if (atrib == null)
                { continue; }

                clientHandlers.Add(atrib.HandlerID, clientMeth);
            }

            foreach (var hostMeth in hostMethods)
            {
                var atrib = hostMeth.GetCustomAttribute<HostMessageHandlerAttribute>();

                if (atrib == null)
                { continue; }

                hostHandlers.Add(atrib.HandlerID, hostMeth);
            }

            foreach (var clientMeth in exoSystemClientMethods)
            {
                var atrib = clientMeth.GetCustomAttribute<ExoSystemClientMessageHandlerAttribute>();

                if (atrib == null)
                { continue; }

                exoSystemClientHandlers.Add(atrib.handlerID, clientMeth);
            }

            foreach (var hostMeth in exoSystemHostMethods)
            {
                var atrib = hostMeth.GetCustomAttribute<ExoSystemHostMessageHandlerAttribute>();

                if (atrib == null)
                { continue; }

                exoSystemHostHandlers.Add(atrib.handlerID, hostMeth);
            }
        }

        /// <summary>
        /// Sends a packet.
        /// </summary>
        /// <param name="packet"> The packet we want to use. </param>
        private void PSend(Packet packet)
            => connection.Send(packet.RawData);

        public void SendPacket(Packet packet, long to = 0)
        {
            if (IsHost)
            {
                ServerSend(to, packet);
            }
            else
            {
                ClientSend(packet);
            }

        }

        public void ClientSend(Packet packet)
        {
            if (IsHost)
            {
                throw new Exception("Cannot use ClientSend as the host!");
            }

            PSend(packet);
        }

        public void ServerSend(long to, Packet packet)
        {
            // Create the Packet that contains the packet...
            using (var newPacket = new Packet(packet.Header))
            {
                newPacket.Write(to);
                newPacket.Write(packet);

                PSend(newPacket);
            }
        }

        public void ServerBroadcast(Packet packet, long? except = null)
        {

            if (IsClient)
            { throw new Exception("Cannot Use ServerBroadcast as a client..."); }

            var clientIDs = Clients.Values.Select(c => c.ID);
            foreach (var clientID in clientIDs)
            {

                // PACKET CONTAINER : [HEADER][TO][PAYLOAD PACKET]
                if (except.HasValue && except.Value == clientID)
                { continue; }

                ServerSend(clientID, packet);

            }
        }

        private void Handle_CustomPacket(Packet packet)
        {
            var handlerID = packet.ReadInt();
            // This is only for customers...
            if (IsHost)
            {
                long from = packet.ReadLong();
                // [Header][HandlerID][ClientID]
                var handler = hostHandlers[handlerID];
                handler.Invoke(null, new object[] { packet, from });
            }
            else
            {

                clientHandlers[handlerID].Invoke(null, new object[] { packet });
            }
        }

        private void OnMessageHandler(byte[] bytes)
        {

            using (var packet = new Packet(bytes))
            {
                var header = (PacketType)packet.Header;

                switch (header)
                {
                    case PacketType.Custom:
                        Handle_CustomPacket(packet);
                        break;
                    case PacketType.ResponseHostRoom:
                        Handle_ResponseHostRoom(packet);
                        break;
                    case PacketType.ResponseJoinRoom:
                        Handle_ResponseJoinedRoom(packet);
                        break;
                    case PacketType.ClientJoinedRoom:
                        Handle_ClientJoinedRoom(packet);
                        break;
                    case PacketType.ClientLeftRoom:
                        Handle_ClientLeftRoom(packet);
                        break;
                    case PacketType.ExoSystem:
                        Handle_ExoSystem(packet);
                        break;
                }
                
            }
        }

        private void Handle_ExoSystem(Packet packet)
        {
            var handlerID = packet.ReadInt();
            // This is only for customers...
            if (IsHost)
            {
                long from = packet.ReadLong();
                // [Header][HandlerID][ClientID]
                var handler = exoSystemHostHandlers[handlerID];
                handler.Invoke(null, new object[] { packet, from });
            }
            else
            {
                exoSystemClientHandlers[handlerID].Invoke(null, new object[] { packet });
            }
        }

        #region Known Handlers

        private void Handle_ResponseHostRoom(Packet packet)
        {
            //[HEADER][ROOM KEY]
            LocalID = packet.ReadLong();
            RoomKey = packet.ReadString();
            OnHostStart?.Invoke(this, this);
        }

        private void Handle_ResponseJoinedRoom(Packet packet)
        {
            //[HEADER][ROOM NAME]
            LocalID = packet.ReadLong();
            RoomName = packet.ReadString();

            // Read all the clients in...
            ReadClients(packet);


            OnClientStart?.Invoke(this, this);
        }

        private void ReadClients(Packet packet)
        {
            int count = packet.ReadInt();

            for (int i = 0; i < count; i++)
            {
                Handle_ClientJoinedRoom(packet);
            }

        }

        private void Handle_ClientLeftRoom(Packet packet)
        {
            // [HEADER][ClientID]
            var id = packet.ReadLong();
            var rec = Clients[id];
            Clients.Remove(id);
            OnClientLeft?.Invoke(this, rec);
        }

        private void Handle_ClientJoinedRoom(Packet packet)
        {
            // [HEADER][ClientID]
            var client = packet.ReadClient();
            Clients.Add(client.ID, client);
            OnClientJoin?.Invoke(this, client);
        }

        #endregion

        public enum NetworkType
        {
            Host,
            Client
        }



        private void Update()
        {
            if (connection != null)
            {
                connection.OnUpdate();
            }
        }

        private IEnumerator HandleServerTick()
        {
            // Calculate the tick rate...
            float tickRate = 1 / ticksPerSecond;

            // Run the OnServerTickEvent!
            while (true)
            {
                if (connection != null)
                {
                    OnClientTickEvent?.Invoke(tickRate);
                }
                yield return new WaitForSeconds(tickRate);
            }
        }


    }
}
