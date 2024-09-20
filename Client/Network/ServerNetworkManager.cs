using LiteNetLib;
using LiteNetLib.Utils;
using MemoryPack;
using Shared.Network.Packets;
using System.Net;
using System.Net.Sockets;

namespace Client.Network
{
	internal class ServerNetworkManager : Singleton<ServerNetworkManager>, INetEventListener
	{
		private NetPeer? peer = null;
		private NetManager netManager;
		private NetDataWriter writer = new();


		public ServerNetworkManager()
		{
			netManager = new NetManager(this)
			{
				AutoRecycle = false,
				UseNativeSockets = false, // don't use native socket on client
				PacketPoolSize = 1000,
			};
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ip"></param>
		/// <param name="port"></param>
		public void Connect(string ip, int port)
		{
			Log.Write($"Connecting to Server: ip {ip} port {port}");

			netManager.Start();
			netManager.Connect(ip, port, "key");
		}

		/// <summary>
		/// 
		/// </summary>
		public void Update() => netManager.PollEvents();

		/// <summary>
		/// 
		/// </summary>
		public void Close() => netManager.Stop();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="request"></param>
		void INetEventListener.OnConnectionRequest(ConnectionRequest request)
		{
			request.Reject();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="endPoint"></param>
		/// <param name="socketError"></param>
		void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketError)
		{
			Log.Write($"OnNetworkError - EndPoint: {endPoint}, Error: {socketError}");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="peer"></param>
		/// <param name="latency"></param>
		void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="peer"></param>
		/// <param name="reader"></param>
		/// <param name="channelNumber"></param>
		/// <param name="deliveryMethod"></param>
		/// <exception cref="NotImplementedException"></exception>
		void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
		{
            if (reader.AvailableBytes > 1)
            {
                // get the packet id
                ushort tag = reader.GetUShort();
                var bytes = reader.GetRemainingBytesSpan();

                OnReceivePacket(peer, tag, bytes);
            }

            reader.Recycle();
        }
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="remoteEndPoint"></param>
		/// <param name="reader"></param>
		/// <param name="messageType"></param>
		void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="peer"></param>
		void INetEventListener.OnPeerConnected(NetPeer peer)
		{
			Log.Write($"OnPeerConnected - Peer: {peer.Id} connected");

			this.peer = peer;

			// send hello
			SendHello("Hello, this is a test");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="peer"></param>
		/// <param name="disconnectInfo"></param>
		void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
		{
			Log.Write($"OnPeerDisconnected - Peer: {peer.Id}, DisconnectInfo: {disconnectInfo}");

			this.peer = null;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="peer"></param>
        /// <param name="tag"></param>
        /// <param name="bytes"></param>
        void OnReceivePacket(NetPeer peer, ushort tag, ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length <= 0)
            {
                Log.Write($"Peer: {peer.Id} received packet tag {tag} with invalid length: {bytes.Length}");
                return;
            }

            switch (tag)
            {
                case (ushort)eMSG.MSG_Hello_SC:
                    {
                        var packet = MemoryPackSerializer.Deserialize<MSG_Hello_SC>(bytes);

                        Log.Write($"Peer: {peer.Id} received {(eMSG)tag} with ReceivedMessageLength: {packet.ReceivedMessageLength}");
                    }
                    break;

                default: Log.Write($"Peer: {peer.Id} received packet with invalid tag {tag}. Length: {bytes.Length}"); break;
            }

        }

        /// <summary>
		/// 
		/// </summary>
		/// <param name="peer"></param>
		/// <param name="message"></param>
        void SendHello(string message)
        {
            Log.Write($"Send Message MSG_Hello_CS with message length: {message.Length}");

            SendMessage(eMSG.MSG_Hello_CS, new MSG_Hello_CS { Message = message });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tag"></param>
        /// <param name="packet"></param>
        /// <param name="deliveryMethod"></param>
        void SendMessage<T>(eMSG tag, T packet, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
			if (peer == null)
			{
                Log.Write($"Could not send Message {tag} because peer is null");
                return;
			}

            // reset writer
            writer.Reset();

            // write packet to writer
            writer.Put((ushort)tag);

            var bytes = MemoryPackSerializer.Serialize(packet);
            writer.Put(bytes);

            // send
            peer.Send(writer, deliveryMethod);
        }
    }
}
