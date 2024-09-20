using System;
using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using LiteNetLib.Utils;
using MemoryPack;
using Shared.Network.Packets;

namespace Server.Network
{
	internal class ClientNetworkManager : Singleton<ClientNetworkManager>, INetEventListener
	{
		private NetManager netManager;
		private NetDataWriter writer = new();
        private Dictionary<int, NetPeer> peers = new();

		public ClientNetworkManager()
		{
			netManager = new NetManager(this)
			{
				AutoRecycle = false,
				UseNativeSockets = true,
				PacketPoolSize = 1000,
			};
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="port"></param>
		/// <returns></returns>
		public bool Start(int port)
		{
			bool result = netManager.Start(port);

			if (result)
			{
				Log.Write($"Start accepting clients on port {port}");
			}
			else
			{
                Log.Write($"Could not start the NetManager on port {port}");
            }

			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		public void Shutdown()
		{
			// clear peers
			peers.Clear();

			// stop netmanager
            netManager.Stop();
        }

		/// <summary>
		/// 
		/// </summary>
		public void Update() => netManager.PollEvents();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        void INetEventListener.OnConnectionRequest(ConnectionRequest request)
		{
            Log.Write($"OnConnectionRequest - RemoteEndPoint: {request.RemoteEndPoint}");

            request.Accept();
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
            Log.Write($"OnPeerConnected - Peer: {peer.Id} trying to connect ...");

			if(peers.ContainsKey(peer.Id))
			{
                Log.Write($"OnPeerConnected - Peer: {peer.Id} already exist. Force disconnect");

                // disconnect the peer
                peer.Disconnect();

				return;
			}

            Log.Write($"OnPeerConnected - Peer: {peer.Id} connected");

			// Add to dictionary

            peers.Add(peer.Id, peer);
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="peer"></param>
		/// <param name="disconnectInfo"></param>
		void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
		{
            Log.Write($"OnPeerDisconnected - Peer: {peer.Id}, DisconnectInfo: {disconnectInfo}");

			// Remove from dictionary

			peers.Remove(peer.Id);
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
				Log.Write($"Peer: {peer.Id} sent packet tag {tag} with invalid length: {bytes.Length}");
				return;
			}

			switch(tag)
			{
				case (ushort)eMSG.MSG_Hello_CS:
					{ 
						var packet = MemoryPackSerializer.Deserialize<MSG_Hello_CS>(bytes);

                        Log.Write($"Peer: {peer.Id} sent {(eMSG)tag} with message: {packet.Message}");

						// send reply
						SendHelloReply(peer, packet.Message.Length);
                    }
					break;

                default: Log.Write($"Peer: {peer.Id} sent packet with invalid tag {tag}. Length: {bytes.Length}"); break;
			}

        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="peer"></param>
		/// <param name="receivedMessageLength"></param>
		void SendHelloReply(NetPeer peer, int receivedMessageLength)
		{
            SendMessage(peer, eMSG.MSG_Hello_SC, new MSG_Hello_SC { ReceivedMessageLength = receivedMessageLength });
        }

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="peer"></param>
		/// <param name="tag"></param>
		/// <param name="packet"></param>
		/// <param name="deliveryMethod"></param>
		void SendMessage<T>(NetPeer peer, eMSG tag, T packet, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
		{
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
