using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Networking.UDP.Layers;
using Networking.UDP.Utils;

namespace Networking.UDP
{
	// Token: 0x02000025 RID: 37
	public class NetManager : IEnumerable<NetPeer>, IEnumerable
	{
		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000087 RID: 135 RVA: 0x000035DE File Offset: 0x000017DE
		// (set) Token: 0x06000088 RID: 136 RVA: 0x000035E6 File Offset: 0x000017E6
		public bool IsRunning { get; private set; }

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000089 RID: 137 RVA: 0x000035EF File Offset: 0x000017EF
		// (set) Token: 0x0600008A RID: 138 RVA: 0x000035F7 File Offset: 0x000017F7
		public int LocalPort { get; private set; }

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x0600008B RID: 139 RVA: 0x00003600 File Offset: 0x00001800
		public NetPeer FirstPeer
		{
			get
			{
				return this._headPeer;
			}
		}

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x0600008C RID: 140 RVA: 0x0000360A File Offset: 0x0000180A
		// (set) Token: 0x0600008D RID: 141 RVA: 0x00003612 File Offset: 0x00001812
		public byte ChannelsCount
		{
			get
			{
				return this._channelsCount;
			}
			set
			{
				if (value < 1 || value > 64)
				{
					throw new ArgumentException("Channels count must be between 1 and 64");
				}
				this._channelsCount = value;
			}
		}

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x0600008E RID: 142 RVA: 0x0000362F File Offset: 0x0000182F
		public List<NetPeer> ConnectedPeerList
		{
			get
			{
				this.GetPeersNonAlloc(this._connectedPeerListCache, ConnectionState.Connected);
				return this._connectedPeerListCache;
			}
		}

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x0600008F RID: 143 RVA: 0x00003644 File Offset: 0x00001844
		public int ConnectedPeersCount
		{
			get
			{
				return (int)Interlocked.Read(ref this._connectedPeersCount);
			}
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000090 RID: 144 RVA: 0x00003652 File Offset: 0x00001852
		public int ExtraPacketSizeForLayer
		{
			get
			{
				PacketLayerBase extraPacketLayer = this._extraPacketLayer;
				if (extraPacketLayer == null)
				{
					return 0;
				}
				return extraPacketLayer.ExtraPacketSizeForLayer;
			}
		}

		// Token: 0x06000091 RID: 145 RVA: 0x00003668 File Offset: 0x00001868
		public NetManager(INetEventListener listener, PacketLayerBase extraPacketLayer = null)
		{
			this._netEventListener = listener;
			this._deliveryEventListener = listener as IDeliveryEventListener;
			this._ntpEventListener = listener as INtpEventListener;
			this._peerAddressChangedListener = listener as IPeerAddressChangedListener;
			this.NatPunchModule = new NatPunchModule(this);
			this._extraPacketLayer = extraPacketLayer;
		}

		// Token: 0x06000092 RID: 146 RVA: 0x000037AC File Offset: 0x000019AC
		internal void ConnectionLatencyUpdated(NetPeer fromPeer, int latency)
		{
			this.CreateEvent(NetEvent.EType.ConnectionLatencyUpdated, fromPeer, null, SocketError.Success, latency, DisconnectReason.ConnectionFailed, null, DeliveryMethod.Unreliable, 0, null, null);
		}

		// Token: 0x06000093 RID: 147 RVA: 0x000037CC File Offset: 0x000019CC
		internal void MessageDelivered(NetPeer fromPeer, object userData)
		{
			if (this._deliveryEventListener != null)
			{
				this.CreateEvent(NetEvent.EType.MessageDelivered, fromPeer, null, SocketError.Success, 0, DisconnectReason.ConnectionFailed, null, DeliveryMethod.Unreliable, 0, null, userData);
			}
		}

		// Token: 0x06000094 RID: 148 RVA: 0x000037F4 File Offset: 0x000019F4
		internal void DisconnectPeerForce(NetPeer peer, DisconnectReason reason, SocketError socketErrorCode, NetPacket eventData)
		{
			this.DisconnectPeer(peer, reason, socketErrorCode, true, null, 0, 0, eventData);
		}

		// Token: 0x06000095 RID: 149 RVA: 0x00003810 File Offset: 0x00001A10
		private void DisconnectPeer(NetPeer peer, DisconnectReason reason, SocketError socketErrorCode, bool force, byte[] data, int start, int count, NetPacket eventData)
		{
			ShutdownResult shutdownResult = peer.Shutdown(data, start, count, force);
			if (shutdownResult == ShutdownResult.None)
			{
				return;
			}
			if (shutdownResult == ShutdownResult.WasConnected)
			{
				Interlocked.Decrement(ref this._connectedPeersCount);
			}
			this.CreateEvent(NetEvent.EType.Disconnect, peer, null, socketErrorCode, 0, reason, null, DeliveryMethod.Unreliable, 0, eventData, null);
		}

		// Token: 0x06000096 RID: 150 RVA: 0x00003854 File Offset: 0x00001A54
		private void CreateEvent(NetEvent.EType type, NetPeer peer = null, IPEndPoint remoteEndPoint = null, SocketError errorCode = SocketError.Success, int latency = 0, DisconnectReason disconnectReason = DisconnectReason.ConnectionFailed, ConnectionRequest connectionRequest = null, DeliveryMethod deliveryMethod = DeliveryMethod.Unreliable, byte channelNumber = 0, NetPacket readerSource = null, object userData = null)
		{
			bool flag = this.UnsyncedEvents;
			if (type == NetEvent.EType.Connect)
			{
				Interlocked.Increment(ref this._connectedPeersCount);
			}
			else if (type == NetEvent.EType.MessageDelivered)
			{
				flag = this.UnsyncedDeliveryEvent;
			}
			object obj = this._eventLock;
			NetEvent netEvent;
			lock (obj)
			{
				netEvent = this._netEventPoolHead;
				if (netEvent == null)
				{
					netEvent = new NetEvent(this);
				}
				else
				{
					this._netEventPoolHead = netEvent.Next;
				}
			}
			netEvent.Next = null;
			netEvent.Type = type;
			netEvent.DataReader.SetSource(readerSource, (readerSource != null) ? readerSource.GetHeaderSize() : 0);
			netEvent.Peer = peer;
			netEvent.RemoteEndPoint = remoteEndPoint;
			netEvent.Latency = latency;
			netEvent.ErrorCode = errorCode;
			netEvent.DisconnectReason = disconnectReason;
			netEvent.ConnectionRequest = connectionRequest;
			netEvent.DeliveryMethod = deliveryMethod;
			netEvent.ChannelNumber = channelNumber;
			netEvent.UserData = userData;
			if (flag || this._manualMode)
			{
				this.ProcessEvent(netEvent);
				return;
			}
			obj = this._eventLock;
			lock (obj)
			{
				if (this._pendingEventTail == null)
				{
					this._pendingEventHead = netEvent;
				}
				else
				{
					this._pendingEventTail.Next = netEvent;
				}
				this._pendingEventTail = netEvent;
			}
		}

		// Token: 0x06000097 RID: 151 RVA: 0x000039A0 File Offset: 0x00001BA0
		private void ProcessEvent(NetEvent evt)
		{
			bool isNull = evt.DataReader.IsNull;
			switch (evt.Type)
			{
			case NetEvent.EType.Connect:
				this._netEventListener.OnPeerConnected(evt.Peer);
				break;
			case NetEvent.EType.Disconnect:
			{
				DisconnectInfo disconnectInfo = new DisconnectInfo
				{
					Reason = evt.DisconnectReason,
					AdditionalData = evt.DataReader,
					SocketErrorCode = evt.ErrorCode
				};
				this._netEventListener.OnPeerDisconnected(evt.Peer, disconnectInfo);
				break;
			}
			case NetEvent.EType.Receive:
				this._netEventListener.OnNetworkReceive(evt.Peer, evt.DataReader, evt.ChannelNumber, evt.DeliveryMethod);
				break;
			case NetEvent.EType.ReceiveUnconnected:
				this._netEventListener.OnNetworkReceiveUnconnected(evt.RemoteEndPoint, evt.DataReader, UnconnectedMessageType.BasicMessage);
				break;
			case NetEvent.EType.Error:
				this._netEventListener.OnNetworkError(evt.RemoteEndPoint, evt.ErrorCode);
				break;
			case NetEvent.EType.ConnectionLatencyUpdated:
				this._netEventListener.OnNetworkLatencyUpdate(evt.Peer, evt.Latency);
				break;
			case NetEvent.EType.Broadcast:
				this._netEventListener.OnNetworkReceiveUnconnected(evt.RemoteEndPoint, evt.DataReader, UnconnectedMessageType.Broadcast);
				break;
			case NetEvent.EType.ConnectionRequest:
				this._netEventListener.OnConnectionRequest(evt.ConnectionRequest);
				break;
			case NetEvent.EType.MessageDelivered:
				this._deliveryEventListener.OnMessageDelivered(evt.Peer, evt.UserData);
				break;
			case NetEvent.EType.PeerAddressChanged:
			{
				this._peersLock.EnterUpgradeableReadLock();
				IPEndPoint ipendPoint = null;
				if (this.ContainsPeer(evt.Peer))
				{
					this._peersLock.EnterWriteLock();
					this.RemovePeerFromSet(evt.Peer);
					ipendPoint = new IPEndPoint(evt.Peer.Address, evt.Peer.Port);
					evt.Peer.FinishEndPointChange(evt.RemoteEndPoint);
					this.AddPeerToSet(evt.Peer);
					this._peersLock.ExitWriteLock();
				}
				this._peersLock.ExitUpgradeableReadLock();
				if (ipendPoint != null && this._peerAddressChangedListener != null)
				{
					this._peerAddressChangedListener.OnPeerAddressChanged(evt.Peer, ipendPoint);
				}
				break;
			}
			}
			if (isNull)
			{
				this.RecycleEvent(evt);
				return;
			}
			if (this.AutoRecycle)
			{
				evt.DataReader.RecycleInternal();
			}
		}

		// Token: 0x06000098 RID: 152 RVA: 0x00003BDC File Offset: 0x00001DDC
		internal void RecycleEvent(NetEvent evt)
		{
			evt.Peer = null;
			evt.ErrorCode = SocketError.Success;
			evt.RemoteEndPoint = null;
			evt.ConnectionRequest = null;
			object eventLock = this._eventLock;
			lock (eventLock)
			{
				evt.Next = this._netEventPoolHead;
				this._netEventPoolHead = evt;
			}
		}

		// Token: 0x06000099 RID: 153 RVA: 0x00003C48 File Offset: 0x00001E48
		private void UpdateLogic()
		{
			List<NetPeer> list = new List<NetPeer>();
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			while (this.IsRunning)
			{
				try
				{
					float num = (float)((double)stopwatch.ElapsedTicks / (double)Stopwatch.Frequency * 1000.0);
					num = ((num <= 0f) ? 0.001f : num);
					stopwatch.Restart();
					for (NetPeer netPeer = this._headPeer; netPeer != null; netPeer = netPeer.NextPeer)
					{
						if (netPeer.ConnectionState == ConnectionState.Disconnected && netPeer.TimeSinceLastPacket > (float)this.DisconnectTimeout)
						{
							list.Add(netPeer);
						}
						else
						{
							netPeer.Update(num);
						}
					}
					if (list.Count > 0)
					{
						this._peersLock.EnterWriteLock();
						for (int i = 0; i < list.Count; i++)
						{
							this.RemovePeer(list[i], false);
						}
						this._peersLock.ExitWriteLock();
						list.Clear();
					}
					this.ProcessNtpRequests(num);
					int num2 = this.UpdateTime - (int)stopwatch.ElapsedMilliseconds;
					if (num2 > 0)
					{
						this._updateTriggerEvent.WaitOne(num2);
					}
				}
				catch (ThreadAbortException)
				{
					return;
				}
				catch (Exception ex)
				{
					string text = "[NM] LogicThread error: ";
					Exception ex2 = ex;
					NetDebug.WriteError(text + ((ex2 != null) ? ex2.ToString() : null));
				}
			}
			stopwatch.Stop();
		}

		// Token: 0x0600009A RID: 154 RVA: 0x00003DAC File Offset: 0x00001FAC
		[Conditional("DEBUG")]
		private void ProcessDelayedPackets()
		{
			if (!this.SimulateLatency)
			{
				return;
			}
			DateTime utcNow = DateTime.UtcNow;
			List<NetManager.IncomingData> pingSimulationList = this._pingSimulationList;
			lock (pingSimulationList)
			{
				for (int i = 0; i < this._pingSimulationList.Count; i++)
				{
					NetManager.IncomingData incomingData = this._pingSimulationList[i];
					if (incomingData.TimeWhenGet <= utcNow)
					{
						this.HandleMessageReceived(incomingData.Data, incomingData.EndPoint);
						this._pingSimulationList.RemoveAt(i);
						i--;
					}
				}
			}
		}

		// Token: 0x0600009B RID: 155 RVA: 0x00003E4C File Offset: 0x0000204C
		private void ProcessNtpRequests(float elapsedMilliseconds)
		{
			List<IPEndPoint> list = null;
			foreach (KeyValuePair<IPEndPoint, NtpRequest> keyValuePair in this._ntpRequests)
			{
				keyValuePair.Value.Send(this._udpSocketv4, elapsedMilliseconds);
				if (keyValuePair.Value.NeedToKill)
				{
					if (list == null)
					{
						list = new List<IPEndPoint>();
					}
					list.Add(keyValuePair.Key);
				}
			}
			if (list != null)
			{
				foreach (IPEndPoint ipendPoint in list)
				{
					NtpRequest ntpRequest;
					this._ntpRequests.TryRemove(ipendPoint, out ntpRequest);
				}
			}
		}

		// Token: 0x0600009C RID: 156 RVA: 0x00003F18 File Offset: 0x00002118
		public void ManualUpdate(float elapsedMilliseconds)
		{
			if (!this._manualMode)
			{
				return;
			}
			for (NetPeer netPeer = this._headPeer; netPeer != null; netPeer = netPeer.NextPeer)
			{
				if (netPeer.ConnectionState == ConnectionState.Disconnected && netPeer.TimeSinceLastPacket > (float)this.DisconnectTimeout)
				{
					this.RemovePeer(netPeer, false);
				}
				else
				{
					netPeer.Update(elapsedMilliseconds);
				}
			}
			this.ProcessNtpRequests(elapsedMilliseconds);
		}

		// Token: 0x0600009D RID: 157 RVA: 0x00003F78 File Offset: 0x00002178
		internal NetPeer OnConnectionSolved(ConnectionRequest request, byte[] rejectData, int start, int length)
		{
			NetPeer netPeer = null;
			Dictionary<IPEndPoint, ConnectionRequest> dictionary;
			if (request.Result == ConnectionRequestResult.RejectForce)
			{
				if (rejectData != null && length > 0)
				{
					NetPacket netPacket = this.PoolGetWithProperty(PacketProperty.Disconnect, length);
					netPacket.ConnectionNumber = request.InternalPacket.ConnectionNumber;
					FastBitConverter.GetBytes(netPacket.RawData, 1, request.InternalPacket.ConnectionTime);
					if (netPacket.Size >= NetConstants.PossibleMtu[0])
					{
						NetDebug.WriteError("[Peer] Disconnect additional data size more than MTU!");
					}
					else
					{
						Buffer.BlockCopy(rejectData, start, netPacket.RawData, 9, length);
					}
					this.SendRawAndRecycle(netPacket, request.RemoteEndPoint);
				}
				dictionary = this._requestsDict;
				lock (dictionary)
				{
					this._requestsDict.Remove(request.RemoteEndPoint);
					return netPeer;
				}
			}
			dictionary = this._requestsDict;
			lock (dictionary)
			{
				if (!this.TryGetPeer(request.RemoteEndPoint, out netPeer))
				{
					if (request.Result == ConnectionRequestResult.Reject)
					{
						netPeer = new NetPeer(this, request.RemoteEndPoint, this.GetNextPeerId());
						netPeer.Reject(request.InternalPacket, rejectData, start, length);
						this.AddPeer(netPeer);
					}
					else
					{
						netPeer = new NetPeer(this, request, this.GetNextPeerId());
						this.AddPeer(netPeer);
						this.CreateEvent(NetEvent.EType.Connect, netPeer, null, SocketError.Success, 0, DisconnectReason.ConnectionFailed, null, DeliveryMethod.Unreliable, 0, null, null);
					}
				}
				this._requestsDict.Remove(request.RemoteEndPoint);
			}
			return netPeer;
		}

		// Token: 0x0600009E RID: 158 RVA: 0x000040F0 File Offset: 0x000022F0
		private int GetNextPeerId()
		{
			int num;
			if (!this._peerIds.TryDequeue(out num))
			{
				int lastPeerId = this._lastPeerId;
				this._lastPeerId = lastPeerId + 1;
				return lastPeerId;
			}
			return num;
		}

		// Token: 0x0600009F RID: 159 RVA: 0x00004120 File Offset: 0x00002320
		private void ProcessConnectRequest(IPEndPoint remoteEndPoint, NetPeer netPeer, NetConnectRequestPacket connRequest)
		{
			if (netPeer != null)
			{
				ConnectRequestResult connectRequestResult = netPeer.ProcessConnectRequest(connRequest);
				switch (connectRequestResult)
				{
				case ConnectRequestResult.P2PLose:
					this.DisconnectPeerForce(netPeer, DisconnectReason.PeerToPeerConnection, SocketError.Success, null);
					this.RemovePeer(netPeer, true);
					break;
				case ConnectRequestResult.Reconnection:
					this.DisconnectPeerForce(netPeer, DisconnectReason.Reconnect, SocketError.Success, null);
					this.RemovePeer(netPeer, true);
					break;
				case ConnectRequestResult.NewConnection:
					this.RemovePeer(netPeer, true);
					break;
				default:
					return;
				}
				if (connectRequestResult != ConnectRequestResult.P2PLose)
				{
					connRequest.ConnectionNumber = (netPeer.ConnectionNum + 1) % 64;
				}
			}
			Dictionary<IPEndPoint, ConnectionRequest> requestsDict = this._requestsDict;
			ConnectionRequest connectionRequest;
			lock (requestsDict)
			{
				if (this._requestsDict.TryGetValue(remoteEndPoint, out connectionRequest))
				{
					connectionRequest.UpdateRequest(connRequest);
					return;
				}
				connectionRequest = new ConnectionRequest(remoteEndPoint, connRequest, this);
				this._requestsDict.Add(remoteEndPoint, connectionRequest);
			}
			this.CreateEvent(NetEvent.EType.ConnectionRequest, null, null, SocketError.Success, 0, DisconnectReason.ConnectionFailed, connectionRequest, DeliveryMethod.Unreliable, 0, null, null);
		}

		// Token: 0x060000A0 RID: 160 RVA: 0x00004204 File Offset: 0x00002404
		private void OnMessageReceived(NetPacket packet, IPEndPoint remoteEndPoint)
		{
			if (packet.Size == 0)
			{
				this.PoolRecycle(packet);
				return;
			}
			this._dropPacket = false;
			if (this._dropPacket)
			{
				return;
			}
			this.HandleMessageReceived(packet, remoteEndPoint);
		}

		// Token: 0x060000A1 RID: 161 RVA: 0x00004230 File Offset: 0x00002430
		[Conditional("DEBUG")]
		private void HandleSimulateLatency(NetPacket packet, IPEndPoint remoteEndPoint)
		{
			if (!this.SimulateLatency)
			{
				return;
			}
			int num = this._randomGenerator.Next(this.SimulationMinLatency, this.SimulationMaxLatency);
			if (num > 5)
			{
				List<NetManager.IncomingData> pingSimulationList = this._pingSimulationList;
				lock (pingSimulationList)
				{
					this._pingSimulationList.Add(new NetManager.IncomingData
					{
						Data = packet,
						EndPoint = remoteEndPoint,
						TimeWhenGet = DateTime.UtcNow.AddMilliseconds((double)num)
					});
				}
				this._dropPacket = true;
			}
		}

		// Token: 0x060000A2 RID: 162 RVA: 0x000042D4 File Offset: 0x000024D4
		[Conditional("DEBUG")]
		private void HandleSimulatePacketLoss()
		{
			if (this.SimulatePacketLoss && this._randomGenerator.NextDouble() * 100.0 < (double)this.SimulationPacketLossChance)
			{
				this._dropPacket = true;
			}
		}

		// Token: 0x060000A3 RID: 163 RVA: 0x00004304 File Offset: 0x00002504
		private void HandleMessageReceived(NetPacket packet, IPEndPoint remoteEndPoint)
		{
			int size = packet.Size;
			if (this.EnableStatistics)
			{
				this.Statistics.IncrementPacketsReceived();
				this.Statistics.AddBytesReceived((long)size);
			}
			NtpRequest ntpRequest;
			if (this._ntpRequests.Count > 0 && this._ntpRequests.TryGetValue(remoteEndPoint, out ntpRequest))
			{
				if (packet.Size < 48)
				{
					return;
				}
				byte[] array = new byte[packet.Size];
				Buffer.BlockCopy(packet.RawData, 0, array, 0, packet.Size);
				NtpPacket ntpPacket = NtpPacket.FromServerResponse(array, DateTime.UtcNow);
				try
				{
					ntpPacket.ValidateReply();
				}
				catch (InvalidOperationException)
				{
					ntpPacket = null;
				}
				if (ntpPacket != null)
				{
					NtpRequest ntpRequest2;
					this._ntpRequests.TryRemove(remoteEndPoint, out ntpRequest2);
					INtpEventListener ntpEventListener = this._ntpEventListener;
					if (ntpEventListener == null)
					{
						return;
					}
					ntpEventListener.OnNtpResponse(ntpPacket);
				}
				return;
			}
			else
			{
				if (this._extraPacketLayer != null)
				{
					this._extraPacketLayer.ProcessInboundPacket(ref remoteEndPoint, ref packet.RawData, ref packet.Size);
					if (packet.Size == 0)
					{
						return;
					}
				}
				if (!packet.Verify())
				{
					NetDebug.WriteError("[NM] DataReceived: bad!");
					this.PoolRecycle(packet);
					return;
				}
				PacketProperty packetProperty = packet.Property;
				if (packetProperty <= PacketProperty.UnconnectedMessage)
				{
					if (packetProperty != PacketProperty.ConnectRequest)
					{
						if (packetProperty == PacketProperty.UnconnectedMessage)
						{
							if (!this.UnconnectedMessagesEnabled)
							{
								return;
							}
							this.CreateEvent(NetEvent.EType.ReceiveUnconnected, null, remoteEndPoint, SocketError.Success, 0, DisconnectReason.ConnectionFailed, null, DeliveryMethod.Unreliable, 0, packet, null);
							return;
						}
					}
					else if (NetConnectRequestPacket.GetProtocolId(packet) != 13)
					{
						this.SendRawAndRecycle(this.PoolGetWithProperty(PacketProperty.InvalidProtocol), remoteEndPoint);
						return;
					}
				}
				else if (packetProperty != PacketProperty.Broadcast)
				{
					if (packetProperty == PacketProperty.NatMessage)
					{
						if (this.NatPunchEnabled)
						{
							this.NatPunchModule.ProcessMessage(remoteEndPoint, packet);
						}
						return;
					}
				}
				else
				{
					if (!this.BroadcastReceiveEnabled)
					{
						return;
					}
					this.CreateEvent(NetEvent.EType.Broadcast, null, remoteEndPoint, SocketError.Success, 0, DisconnectReason.ConnectionFailed, null, DeliveryMethod.Unreliable, 0, packet, null);
					return;
				}
				NetPeer netPeer = remoteEndPoint as NetPeer;
				bool flag = netPeer != null || this.TryGetPeer(remoteEndPoint, out netPeer);
				if (flag && this.EnableStatistics)
				{
					netPeer.Statistics.IncrementPacketsReceived();
					netPeer.Statistics.AddBytesReceived((long)size);
				}
				packetProperty = packet.Property;
				switch (packetProperty)
				{
				case PacketProperty.ConnectRequest:
				{
					NetConnectRequestPacket netConnectRequestPacket = NetConnectRequestPacket.FromData(packet);
					if (netConnectRequestPacket != null)
					{
						this.ProcessConnectRequest(remoteEndPoint, netPeer, netConnectRequestPacket);
						return;
					}
					break;
				}
				case PacketProperty.ConnectAccept:
				{
					if (!flag)
					{
						return;
					}
					NetConnectAcceptPacket netConnectAcceptPacket = NetConnectAcceptPacket.FromData(packet);
					if (netConnectAcceptPacket != null && netPeer.ProcessConnectAccept(netConnectAcceptPacket))
					{
						this.CreateEvent(NetEvent.EType.Connect, netPeer, null, SocketError.Success, 0, DisconnectReason.ConnectionFailed, null, DeliveryMethod.Unreliable, 0, null, null);
						return;
					}
					break;
				}
				case PacketProperty.Disconnect:
					if (flag)
					{
						DisconnectResult disconnectResult = netPeer.ProcessDisconnect(packet);
						if (disconnectResult == DisconnectResult.None)
						{
							this.PoolRecycle(packet);
							return;
						}
						this.DisconnectPeerForce(netPeer, (disconnectResult == DisconnectResult.Disconnect) ? DisconnectReason.RemoteConnectionClose : DisconnectReason.ConnectionRejected, SocketError.Success, packet);
					}
					else
					{
						this.PoolRecycle(packet);
					}
					this.SendRawAndRecycle(this.PoolGetWithProperty(PacketProperty.ShutdownOk), remoteEndPoint);
					return;
				default:
					if (packetProperty != PacketProperty.PeerNotFound)
					{
						if (packetProperty != PacketProperty.InvalidProtocol)
						{
							if (flag)
							{
								netPeer.ProcessPacket(packet);
								return;
							}
							this.SendRawAndRecycle(this.PoolGetWithProperty(PacketProperty.PeerNotFound), remoteEndPoint);
						}
						else if (flag && netPeer.ConnectionState == ConnectionState.Outgoing)
						{
							this.DisconnectPeerForce(netPeer, DisconnectReason.InvalidProtocol, SocketError.Success, null);
							return;
						}
					}
					else if (flag)
					{
						if (netPeer.ConnectionState != ConnectionState.Connected)
						{
							return;
						}
						if (packet.Size == 1)
						{
							netPeer.ResetMtu();
							this.SendRaw(NetConnectAcceptPacket.MakeNetworkChanged(netPeer), remoteEndPoint);
							return;
						}
						if (packet.Size == 2 && packet.RawData[1] == 1)
						{
							this.DisconnectPeerForce(netPeer, DisconnectReason.PeerNotFound, SocketError.Success, null);
							return;
						}
					}
					else if (packet.Size > 1)
					{
						bool flag2 = false;
						if (this.AllowPeerAddressChange)
						{
							NetConnectAcceptPacket netConnectAcceptPacket2 = NetConnectAcceptPacket.FromData(packet);
							if (netConnectAcceptPacket2 != null && netConnectAcceptPacket2.PeerNetworkChanged && netConnectAcceptPacket2.PeerId < this._peersArray.Length)
							{
								this._peersLock.EnterUpgradeableReadLock();
								NetPeer netPeer2 = this._peersArray[netConnectAcceptPacket2.PeerId];
								this._peersLock.ExitUpgradeableReadLock();
								if (netPeer2 != null && netPeer2.ConnectTime == netConnectAcceptPacket2.ConnectionTime && netPeer2.ConnectionNum == netConnectAcceptPacket2.ConnectionNumber)
								{
									if (netPeer2.ConnectionState == ConnectionState.Connected)
									{
										netPeer2.InitiateEndPointChange();
										this.CreateEvent(NetEvent.EType.PeerAddressChanged, netPeer2, remoteEndPoint, SocketError.Success, 0, DisconnectReason.ConnectionFailed, null, DeliveryMethod.Unreliable, 0, null, null);
									}
									flag2 = true;
								}
							}
						}
						this.PoolRecycle(packet);
						if (!flag2)
						{
							NetPacket netPacket = this.PoolGetWithProperty(PacketProperty.PeerNotFound, 1);
							netPacket.RawData[1] = 1;
							this.SendRawAndRecycle(netPacket, remoteEndPoint);
							return;
						}
					}
					break;
				}
				return;
			}
		}

		// Token: 0x060000A4 RID: 164 RVA: 0x00004710 File Offset: 0x00002910
		internal void CreateReceiveEvent(NetPacket packet, DeliveryMethod method, byte channelNumber, int headerSize, NetPeer fromPeer)
		{
			object obj;
			if (this.UnsyncedEvents || this.UnsyncedReceiveEvent || this._manualMode)
			{
				obj = this._eventLock;
				NetEvent netEvent;
				lock (obj)
				{
					netEvent = this._netEventPoolHead;
					if (netEvent == null)
					{
						netEvent = new NetEvent(this);
					}
					else
					{
						this._netEventPoolHead = netEvent.Next;
					}
				}
				netEvent.Next = null;
				netEvent.Type = NetEvent.EType.Receive;
				netEvent.DataReader.SetSource(packet, headerSize);
				netEvent.Peer = fromPeer;
				netEvent.DeliveryMethod = method;
				netEvent.ChannelNumber = channelNumber;
				this.ProcessEvent(netEvent);
				return;
			}
			obj = this._eventLock;
			lock (obj)
			{
				NetEvent netEvent = this._netEventPoolHead;
				if (netEvent == null)
				{
					netEvent = new NetEvent(this);
				}
				else
				{
					this._netEventPoolHead = netEvent.Next;
				}
				netEvent.Next = null;
				netEvent.Type = NetEvent.EType.Receive;
				netEvent.DataReader.SetSource(packet, headerSize);
				netEvent.Peer = fromPeer;
				netEvent.DeliveryMethod = method;
				netEvent.ChannelNumber = channelNumber;
				if (this._pendingEventTail == null)
				{
					this._pendingEventHead = netEvent;
				}
				else
				{
					this._pendingEventTail.Next = netEvent;
				}
				this._pendingEventTail = netEvent;
			}
		}

		// Token: 0x060000A5 RID: 165 RVA: 0x0000485C File Offset: 0x00002A5C
		public void SendToAll(NetDataWriter writer, DeliveryMethod options)
		{
			this.SendToAll(writer.Data, 0, writer.Length, options);
		}

		// Token: 0x060000A6 RID: 166 RVA: 0x00004872 File Offset: 0x00002A72
		public void SendToAll(byte[] data, DeliveryMethod options)
		{
			this.SendToAll(data, 0, data.Length, options);
		}

		// Token: 0x060000A7 RID: 167 RVA: 0x00004880 File Offset: 0x00002A80
		public void SendToAll(byte[] data, int start, int length, DeliveryMethod options)
		{
			this.SendToAll(data, start, length, 0, options);
		}

		// Token: 0x060000A8 RID: 168 RVA: 0x0000488E File Offset: 0x00002A8E
		public void SendToAll(NetDataWriter writer, byte channelNumber, DeliveryMethod options)
		{
			this.SendToAll(writer.Data, 0, writer.Length, channelNumber, options);
		}

		// Token: 0x060000A9 RID: 169 RVA: 0x000048A5 File Offset: 0x00002AA5
		public void SendToAll(byte[] data, byte channelNumber, DeliveryMethod options)
		{
			this.SendToAll(data, 0, data.Length, channelNumber, options);
		}

		// Token: 0x060000AA RID: 170 RVA: 0x000048B4 File Offset: 0x00002AB4
		public void SendToAll(byte[] data, int start, int length, byte channelNumber, DeliveryMethod options)
		{
			try
			{
				this._peersLock.EnterReadLock();
				for (NetPeer netPeer = this._headPeer; netPeer != null; netPeer = netPeer.NextPeer)
				{
					netPeer.Send(data, start, length, channelNumber, options);
				}
			}
			finally
			{
				this._peersLock.ExitReadLock();
			}
		}

		// Token: 0x060000AB RID: 171 RVA: 0x00004910 File Offset: 0x00002B10
		public void SendToAll(NetDataWriter writer, DeliveryMethod options, NetPeer excludePeer)
		{
			this.SendToAll(writer.Data, 0, writer.Length, 0, options, excludePeer);
		}

		// Token: 0x060000AC RID: 172 RVA: 0x00004928 File Offset: 0x00002B28
		public void SendToAll(byte[] data, DeliveryMethod options, NetPeer excludePeer)
		{
			this.SendToAll(data, 0, data.Length, 0, options, excludePeer);
		}

		// Token: 0x060000AD RID: 173 RVA: 0x00004938 File Offset: 0x00002B38
		public void SendToAll(byte[] data, int start, int length, DeliveryMethod options, NetPeer excludePeer)
		{
			this.SendToAll(data, start, length, 0, options, excludePeer);
		}

		// Token: 0x060000AE RID: 174 RVA: 0x00004948 File Offset: 0x00002B48
		public void SendToAll(NetDataWriter writer, byte channelNumber, DeliveryMethod options, NetPeer excludePeer)
		{
			this.SendToAll(writer.Data, 0, writer.Length, channelNumber, options, excludePeer);
		}

		// Token: 0x060000AF RID: 175 RVA: 0x00004961 File Offset: 0x00002B61
		public void SendToAll(byte[] data, byte channelNumber, DeliveryMethod options, NetPeer excludePeer)
		{
			this.SendToAll(data, 0, data.Length, channelNumber, options, excludePeer);
		}

		// Token: 0x060000B0 RID: 176 RVA: 0x00004974 File Offset: 0x00002B74
		public void SendToAll(byte[] data, int start, int length, byte channelNumber, DeliveryMethod options, NetPeer excludePeer)
		{
			try
			{
				this._peersLock.EnterReadLock();
				for (NetPeer netPeer = this._headPeer; netPeer != null; netPeer = netPeer.NextPeer)
				{
					if (netPeer != excludePeer)
					{
						netPeer.Send(data, start, length, channelNumber, options);
					}
				}
			}
			finally
			{
				this._peersLock.ExitReadLock();
			}
		}

		// Token: 0x060000B1 RID: 177 RVA: 0x000049D4 File Offset: 0x00002BD4
		public bool Start()
		{
			return this.Start(0);
		}

		// Token: 0x060000B2 RID: 178 RVA: 0x000049DD File Offset: 0x00002BDD
		public bool Start(IPAddress addressIPv4, IPAddress addressIPv6, int port)
		{
			return this.Start(addressIPv4, addressIPv6, port, false);
		}

		// Token: 0x060000B3 RID: 179 RVA: 0x000049EC File Offset: 0x00002BEC
		public bool Start(string addressIPv4, string addressIPv6, int port)
		{
			IPAddress ipaddress = NetUtils.ResolveAddress(addressIPv4);
			IPAddress ipaddress2 = NetUtils.ResolveAddress(addressIPv6);
			return this.Start(ipaddress, ipaddress2, port);
		}

		// Token: 0x060000B4 RID: 180 RVA: 0x00004A10 File Offset: 0x00002C10
		public bool Start(int port)
		{
			return this.Start(IPAddress.Any, IPAddress.IPv6Any, port);
		}

		// Token: 0x060000B5 RID: 181 RVA: 0x00004A23 File Offset: 0x00002C23
		public bool StartInManualMode(IPAddress addressIPv4, IPAddress addressIPv6, int port)
		{
			return this.Start(addressIPv4, addressIPv6, port, true);
		}

		// Token: 0x060000B6 RID: 182 RVA: 0x00004A30 File Offset: 0x00002C30
		public bool StartInManualMode(string addressIPv4, string addressIPv6, int port)
		{
			IPAddress ipaddress = NetUtils.ResolveAddress(addressIPv4);
			IPAddress ipaddress2 = NetUtils.ResolveAddress(addressIPv6);
			return this.StartInManualMode(ipaddress, ipaddress2, port);
		}

		// Token: 0x060000B7 RID: 183 RVA: 0x00004A54 File Offset: 0x00002C54
		public bool StartInManualMode(int port)
		{
			return this.StartInManualMode(IPAddress.Any, IPAddress.IPv6Any, port);
		}

		// Token: 0x060000B8 RID: 184 RVA: 0x00004A67 File Offset: 0x00002C67
		public bool SendUnconnectedMessage(byte[] message, IPEndPoint remoteEndPoint)
		{
			return this.SendUnconnectedMessage(message, 0, message.Length, remoteEndPoint);
		}

		// Token: 0x060000B9 RID: 185 RVA: 0x00004A78 File Offset: 0x00002C78
		public bool SendUnconnectedMessage(NetDataWriter writer, string address, int port)
		{
			IPEndPoint ipendPoint = NetUtils.MakeEndPoint(address, port);
			return this.SendUnconnectedMessage(writer.Data, 0, writer.Length, ipendPoint);
		}

		// Token: 0x060000BA RID: 186 RVA: 0x00004AA1 File Offset: 0x00002CA1
		public bool SendUnconnectedMessage(NetDataWriter writer, IPEndPoint remoteEndPoint)
		{
			return this.SendUnconnectedMessage(writer.Data, 0, writer.Length, remoteEndPoint);
		}

		// Token: 0x060000BB RID: 187 RVA: 0x00004AB8 File Offset: 0x00002CB8
		public bool SendUnconnectedMessage(byte[] message, int start, int length, IPEndPoint remoteEndPoint)
		{
			NetPacket netPacket = this.PoolGetWithData(PacketProperty.UnconnectedMessage, message, start, length);
			return this.SendRawAndRecycle(netPacket, remoteEndPoint) > 0;
		}

		// Token: 0x060000BC RID: 188 RVA: 0x00004ADC File Offset: 0x00002CDC
		public void TriggerUpdate()
		{
			this._updateTriggerEvent.Set();
		}

		// Token: 0x060000BD RID: 189 RVA: 0x00004AEC File Offset: 0x00002CEC
		public void PollEvents(int maxProcessedEvents = 0)
		{
			if (this._manualMode)
			{
				if (this._udpSocketv4 != null)
				{
					this.ManualReceive(this._udpSocketv4, this._bufferEndPointv4, maxProcessedEvents);
				}
				if (this._udpSocketv6 != null && this._udpSocketv6 != this._udpSocketv4)
				{
					this.ManualReceive(this._udpSocketv6, this._bufferEndPointv6, maxProcessedEvents);
				}
				return;
			}
			if (this.UnsyncedEvents)
			{
				return;
			}
			object eventLock = this._eventLock;
			NetEvent netEvent;
			lock (eventLock)
			{
				netEvent = this._pendingEventHead;
				this._pendingEventHead = null;
				this._pendingEventTail = null;
			}
			int num = 0;
			while (netEvent != null)
			{
				NetEvent next = netEvent.Next;
				this.ProcessEvent(netEvent);
				netEvent = next;
				num++;
				if (num == maxProcessedEvents)
				{
					break;
				}
			}
		}

		// Token: 0x060000BE RID: 190 RVA: 0x00004BB0 File Offset: 0x00002DB0
		public NetPeer Connect(string address, int port, string key)
		{
			return this.Connect(address, port, NetDataWriter.FromString(key));
		}

		// Token: 0x060000BF RID: 191 RVA: 0x00004BC0 File Offset: 0x00002DC0
		public NetPeer Connect(string address, int port, NetDataWriter connectionData)
		{
			IPEndPoint ipendPoint;
			try
			{
				ipendPoint = NetUtils.MakeEndPoint(address, port);
			}
			catch
			{
				this.CreateEvent(NetEvent.EType.Disconnect, null, null, SocketError.Success, 0, DisconnectReason.UnknownHost, null, DeliveryMethod.Unreliable, 0, null, null);
				return null;
			}
			return this.Connect(ipendPoint, connectionData);
		}

		// Token: 0x060000C0 RID: 192 RVA: 0x00004C08 File Offset: 0x00002E08
		public NetPeer Connect(IPEndPoint target, string key)
		{
			return this.Connect(target, NetDataWriter.FromString(key));
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x00004C18 File Offset: 0x00002E18
		public NetPeer Connect(IPEndPoint target, NetDataWriter connectionData)
		{
			if (!this.IsRunning)
			{
				throw new InvalidOperationException("Client is not running");
			}
			Dictionary<IPEndPoint, ConnectionRequest> requestsDict = this._requestsDict;
			NetPeer netPeer;
			lock (requestsDict)
			{
				if (this._requestsDict.ContainsKey(target))
				{
					netPeer = null;
				}
				else
				{
					byte b = 0;
					NetPeer netPeer2;
					if (this.TryGetPeer(target, out netPeer2))
					{
						ConnectionState connectionState = netPeer2.ConnectionState;
						if (connectionState == ConnectionState.Outgoing || connectionState == ConnectionState.Connected)
						{
							return netPeer2;
						}
						b = (netPeer2.ConnectionNum + 1) % 64;
						this.RemovePeer(netPeer2, true);
					}
					netPeer2 = new NetPeer(this, target, this.GetNextPeerId(), b, connectionData);
					this.AddPeer(netPeer2);
					netPeer = netPeer2;
				}
			}
			return netPeer;
		}

		// Token: 0x060000C2 RID: 194 RVA: 0x00004CD0 File Offset: 0x00002ED0
		public void Stop()
		{
			this.Stop(true);
		}

		// Token: 0x060000C3 RID: 195 RVA: 0x00004CDC File Offset: 0x00002EDC
		public void Stop(bool sendDisconnectMessages)
		{
			if (!this.IsRunning)
			{
				return;
			}
			for (NetPeer netPeer = this._headPeer; netPeer != null; netPeer = netPeer.NextPeer)
			{
				netPeer.Shutdown(null, 0, 0, !sendDisconnectMessages);
			}
			this.CloseSocket();
			this._updateTriggerEvent.Set();
			if (!this._manualMode)
			{
				this._logicThread.Join();
				this._logicThread = null;
			}
			this.ClearPeerSet();
			this._peerIds = new ConcurrentQueue<int>();
			this._lastPeerId = 0;
			this._connectedPeersCount = 0L;
			this._pendingEventHead = null;
			this._pendingEventTail = null;
		}

		// Token: 0x060000C4 RID: 196 RVA: 0x00004D74 File Offset: 0x00002F74
		[Conditional("DEBUG")]
		private void ClearPingSimulationList()
		{
			List<NetManager.IncomingData> pingSimulationList = this._pingSimulationList;
			lock (pingSimulationList)
			{
				this._pingSimulationList.Clear();
			}
		}

		// Token: 0x060000C5 RID: 197 RVA: 0x00004DBC File Offset: 0x00002FBC
		public int GetPeersCount(ConnectionState peerState)
		{
			int num = 0;
			this._peersLock.EnterReadLock();
			for (NetPeer netPeer = this._headPeer; netPeer != null; netPeer = netPeer.NextPeer)
			{
				if ((netPeer.ConnectionState & peerState) != (ConnectionState)0)
				{
					num++;
				}
			}
			this._peersLock.ExitReadLock();
			return num;
		}

		// Token: 0x060000C6 RID: 198 RVA: 0x00004E08 File Offset: 0x00003008
		public void GetPeersNonAlloc(List<NetPeer> peers, ConnectionState peerState)
		{
			peers.Clear();
			this._peersLock.EnterReadLock();
			for (NetPeer netPeer = this._headPeer; netPeer != null; netPeer = netPeer.NextPeer)
			{
				if ((netPeer.ConnectionState & peerState) != (ConnectionState)0)
				{
					peers.Add(netPeer);
				}
			}
			this._peersLock.ExitReadLock();
		}

		// Token: 0x060000C7 RID: 199 RVA: 0x00004E59 File Offset: 0x00003059
		public void DisconnectAll()
		{
			this.DisconnectAll(null, 0, 0);
		}

		// Token: 0x060000C8 RID: 200 RVA: 0x00004E64 File Offset: 0x00003064
		public void DisconnectAll(byte[] data, int start, int count)
		{
			this._peersLock.EnterReadLock();
			for (NetPeer netPeer = this._headPeer; netPeer != null; netPeer = netPeer.NextPeer)
			{
				this.DisconnectPeer(netPeer, DisconnectReason.DisconnectPeerCalled, SocketError.Success, false, data, start, count, null);
			}
			this._peersLock.ExitReadLock();
		}

		// Token: 0x060000C9 RID: 201 RVA: 0x00004EAC File Offset: 0x000030AC
		public void DisconnectPeerForce(NetPeer peer)
		{
			this.DisconnectPeerForce(peer, DisconnectReason.DisconnectPeerCalled, SocketError.Success, null);
		}

		// Token: 0x060000CA RID: 202 RVA: 0x00004EB8 File Offset: 0x000030B8
		public void DisconnectPeer(NetPeer peer)
		{
			this.DisconnectPeer(peer, null, 0, 0);
		}

		// Token: 0x060000CB RID: 203 RVA: 0x00004EC4 File Offset: 0x000030C4
		public void DisconnectPeer(NetPeer peer, byte[] data)
		{
			this.DisconnectPeer(peer, data, 0, data.Length);
		}

		// Token: 0x060000CC RID: 204 RVA: 0x00004ED2 File Offset: 0x000030D2
		public void DisconnectPeer(NetPeer peer, NetDataWriter writer)
		{
			this.DisconnectPeer(peer, writer.Data, 0, writer.Length);
		}

		// Token: 0x060000CD RID: 205 RVA: 0x00004EE8 File Offset: 0x000030E8
		public void DisconnectPeer(NetPeer peer, byte[] data, int start, int count)
		{
			this.DisconnectPeer(peer, DisconnectReason.DisconnectPeerCalled, SocketError.Success, false, data, start, count, null);
		}

		// Token: 0x060000CE RID: 206 RVA: 0x00004F04 File Offset: 0x00003104
		public void CreateNtpRequest(IPEndPoint endPoint)
		{
			this._ntpRequests.TryAdd(endPoint, new NtpRequest(endPoint));
		}

		// Token: 0x060000CF RID: 207 RVA: 0x00004F1C File Offset: 0x0000311C
		public void CreateNtpRequest(string ntpServerAddress, int port)
		{
			IPEndPoint ipendPoint = NetUtils.MakeEndPoint(ntpServerAddress, port);
			this._ntpRequests.TryAdd(ipendPoint, new NtpRequest(ipendPoint));
		}

		// Token: 0x060000D0 RID: 208 RVA: 0x00004F44 File Offset: 0x00003144
		public void CreateNtpRequest(string ntpServerAddress)
		{
			IPEndPoint ipendPoint = NetUtils.MakeEndPoint(ntpServerAddress, 123);
			this._ntpRequests.TryAdd(ipendPoint, new NtpRequest(ipendPoint));
		}

		// Token: 0x060000D1 RID: 209 RVA: 0x00004F6D File Offset: 0x0000316D
		public NetManager.NetPeerEnumerator GetEnumerator()
		{
			return new NetManager.NetPeerEnumerator(this._headPeer);
		}

		// Token: 0x060000D2 RID: 210 RVA: 0x00004F7C File Offset: 0x0000317C
		IEnumerator<NetPeer> IEnumerable<NetPeer>.GetEnumerator()
		{
			return new NetManager.NetPeerEnumerator(this._headPeer);
		}

		// Token: 0x060000D3 RID: 211 RVA: 0x00004F90 File Offset: 0x00003190
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new NetManager.NetPeerEnumerator(this._headPeer);
		}

		// Token: 0x060000D4 RID: 212 RVA: 0x00004FA4 File Offset: 0x000031A4
		private static int HashSetGetPrime(int min)
		{
			foreach (int num in NetManager.Primes)
			{
				if (num >= min)
				{
					return num;
				}
			}
			for (int j = min | 1; j < 2147483647; j += 2)
			{
				if (NetManager.<HashSetGetPrime>g__IsPrime|143_0(j) && (j - 1) % 101 != 0)
				{
					return j;
				}
			}
			return min;
		}

		// Token: 0x060000D5 RID: 213 RVA: 0x00004FF4 File Offset: 0x000031F4
		private void ClearPeerSet()
		{
			this._peersLock.EnterWriteLock();
			this._headPeer = null;
			if (this._lastIndex > 0)
			{
				Array.Clear(this._slots, 0, this._lastIndex);
				Array.Clear(this._buckets, 0, this._buckets.Length);
				this._lastIndex = 0;
				this._count = 0;
				this._freeList = -1;
			}
			this._peersArray = new NetPeer[32];
			this._peersLock.ExitWriteLock();
		}

		// Token: 0x060000D6 RID: 214 RVA: 0x00005074 File Offset: 0x00003274
		private bool ContainsPeer(NetPeer item)
		{
			if (item == null)
			{
				NetDebug.WriteError(string.Format("Contains peer null: {0}", item));
				return false;
			}
			if (this._buckets != null)
			{
				int num = item.GetHashCode() & int.MaxValue;
				for (int i = this._buckets[num % this._buckets.Length] - 1; i >= 0; i = this._slots[i].Next)
				{
					if (this._slots[i].HashCode == num && this._slots[i].Value.Equals(item))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060000D7 RID: 215 RVA: 0x00005107 File Offset: 0x00003307
		public NetPeer GetPeerById(int id)
		{
			if (id < 0 || id >= this._peersArray.Length)
			{
				return null;
			}
			return this._peersArray[id];
		}

		// Token: 0x060000D8 RID: 216 RVA: 0x00005122 File Offset: 0x00003322
		public bool TryGetPeerById(int id, out NetPeer peer)
		{
			peer = this.GetPeerById(id);
			return peer != null;
		}

		// Token: 0x060000D9 RID: 217 RVA: 0x00005134 File Offset: 0x00003334
		private void AddPeer(NetPeer peer)
		{
			if (peer == null)
			{
				NetDebug.WriteError(string.Format("Add peer null: {0}", peer));
				return;
			}
			this._peersLock.EnterWriteLock();
			if (this._headPeer != null)
			{
				peer.NextPeer = this._headPeer;
				this._headPeer.PrevPeer = peer;
			}
			this._headPeer = peer;
			this.AddPeerToSet(peer);
			if (peer.Id >= this._peersArray.Length)
			{
				int num = this._peersArray.Length * 2;
				while (peer.Id >= num)
				{
					num *= 2;
				}
				Array.Resize<NetPeer>(ref this._peersArray, num);
			}
			this._peersArray[peer.Id] = peer;
			this._peersLock.ExitWriteLock();
		}

		// Token: 0x060000DA RID: 218 RVA: 0x000051E8 File Offset: 0x000033E8
		private void RemovePeer(NetPeer peer, bool enableWriteLock)
		{
			if (enableWriteLock)
			{
				this._peersLock.EnterWriteLock();
			}
			if (!this.RemovePeerFromSet(peer))
			{
				if (enableWriteLock)
				{
					this._peersLock.ExitWriteLock();
				}
				return;
			}
			if (peer == this._headPeer)
			{
				this._headPeer = peer.NextPeer;
			}
			if (peer.PrevPeer != null)
			{
				peer.PrevPeer.NextPeer = peer.NextPeer;
			}
			if (peer.NextPeer != null)
			{
				peer.NextPeer.PrevPeer = peer.PrevPeer;
			}
			peer.PrevPeer = null;
			this._peersArray[peer.Id] = null;
			this._peerIds.Enqueue(peer.Id);
			if (enableWriteLock)
			{
				this._peersLock.ExitWriteLock();
			}
		}

		// Token: 0x060000DB RID: 219 RVA: 0x000052A4 File Offset: 0x000034A4
		private bool RemovePeerFromSet(NetPeer peer)
		{
			if (this._buckets == null || peer == null)
			{
				return false;
			}
			int num = peer.GetHashCode() & int.MaxValue;
			int num2 = num % this._buckets.Length;
			int num3 = -1;
			for (int i = this._buckets[num2] - 1; i >= 0; i = this._slots[i].Next)
			{
				if (this._slots[i].HashCode == num && this._slots[i].Value.Equals(peer))
				{
					if (num3 < 0)
					{
						this._buckets[num2] = this._slots[i].Next + 1;
					}
					else
					{
						this._slots[num3].Next = this._slots[i].Next;
					}
					this._slots[i].HashCode = -1;
					this._slots[i].Value = null;
					this._slots[i].Next = this._freeList;
					this._count--;
					if (this._count == 0)
					{
						this._lastIndex = 0;
						this._freeList = -1;
					}
					else
					{
						this._freeList = i;
					}
					return true;
				}
				num3 = i;
			}
			return false;
		}

		// Token: 0x060000DC RID: 220 RVA: 0x000053E4 File Offset: 0x000035E4
		private bool TryGetPeer(IPEndPoint endPoint, out NetPeer actualValue)
		{
			if (this._buckets != null)
			{
				int num = endPoint.GetHashCode() & int.MaxValue;
				this._peersLock.EnterReadLock();
				for (int i = this._buckets[num % this._buckets.Length] - 1; i >= 0; i = this._slots[i].Next)
				{
					if (this._slots[i].HashCode == num && this._slots[i].Value.Equals(endPoint))
					{
						actualValue = this._slots[i].Value;
						this._peersLock.ExitReadLock();
						return true;
					}
				}
				this._peersLock.ExitReadLock();
			}
			actualValue = null;
			return false;
		}

		// Token: 0x060000DD RID: 221 RVA: 0x0000549C File Offset: 0x0000369C
		private bool TryGetPeer(SocketAddress saddr, out NetPeer actualValue)
		{
			if (this._buckets != null)
			{
				int num = saddr.GetHashCode() & int.MaxValue;
				this._peersLock.EnterReadLock();
				for (int i = this._buckets[num % this._buckets.Length] - 1; i >= 0; i = this._slots[i].Next)
				{
					if (this._slots[i].HashCode == num && this._slots[i].Value.Serialize().Equals(saddr))
					{
						actualValue = this._slots[i].Value;
						this._peersLock.ExitReadLock();
						return true;
					}
				}
				this._peersLock.ExitReadLock();
			}
			actualValue = null;
			return false;
		}

		// Token: 0x060000DE RID: 222 RVA: 0x0000555C File Offset: 0x0000375C
		private bool AddPeerToSet(NetPeer value)
		{
			if (this._buckets == null)
			{
				int num = NetManager.HashSetGetPrime(0);
				this._buckets = new int[num];
				this._slots = new NetManager.Slot[num];
			}
			int num2 = value.GetHashCode() & int.MaxValue;
			int num3 = num2 % this._buckets.Length;
			for (int i = this._buckets[num2 % this._buckets.Length] - 1; i >= 0; i = this._slots[i].Next)
			{
				if (this._slots[i].HashCode == num2 && this._slots[i].Value.Equals(value))
				{
					return false;
				}
			}
			int num4;
			if (this._freeList >= 0)
			{
				num4 = this._freeList;
				this._freeList = this._slots[num4].Next;
			}
			else
			{
				if (this._lastIndex == this._slots.Length)
				{
					int num5 = 2 * this._count;
					num5 = ((num5 > 2147483587 && 2147483587 > this._count) ? 2147483587 : NetManager.HashSetGetPrime(num5));
					NetManager.Slot[] array = new NetManager.Slot[num5];
					Array.Copy(this._slots, 0, array, 0, this._lastIndex);
					this._buckets = new int[num5];
					for (int j = 0; j < this._lastIndex; j++)
					{
						int num6 = array[j].HashCode % num5;
						array[j].Next = this._buckets[num6] - 1;
						this._buckets[num6] = j + 1;
					}
					this._slots = array;
					num3 = num2 % this._buckets.Length;
				}
				num4 = this._lastIndex;
				this._lastIndex++;
			}
			this._slots[num4].HashCode = num2;
			this._slots[num4].Value = value;
			this._slots[num4].Next = this._buckets[num3] - 1;
			this._buckets[num3] = num4 + 1;
			this._count++;
			return true;
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x060000DF RID: 223 RVA: 0x00005772 File Offset: 0x00003972
		public int PoolCount
		{
			get
			{
				return this._poolCount;
			}
		}

		// Token: 0x060000E0 RID: 224 RVA: 0x0000577C File Offset: 0x0000397C
		private NetPacket PoolGetWithData(PacketProperty property, byte[] data, int start, int length)
		{
			int headerSize = NetPacket.GetHeaderSize(property);
			NetPacket netPacket = this.PoolGetPacket(length + headerSize);
			netPacket.Property = property;
			Buffer.BlockCopy(data, start, netPacket.RawData, headerSize, length);
			return netPacket;
		}

		// Token: 0x060000E1 RID: 225 RVA: 0x000057B3 File Offset: 0x000039B3
		private NetPacket PoolGetWithProperty(PacketProperty property, int size)
		{
			NetPacket netPacket = this.PoolGetPacket(size + NetPacket.GetHeaderSize(property));
			netPacket.Property = property;
			return netPacket;
		}

		// Token: 0x060000E2 RID: 226 RVA: 0x000057CA File Offset: 0x000039CA
		private NetPacket PoolGetWithProperty(PacketProperty property)
		{
			NetPacket netPacket = this.PoolGetPacket(NetPacket.GetHeaderSize(property));
			netPacket.Property = property;
			return netPacket;
		}

		// Token: 0x060000E3 RID: 227 RVA: 0x000057E0 File Offset: 0x000039E0
		internal NetPacket PoolGetPacket(int size)
		{
			if (size > NetConstants.MaxPacketSize)
			{
				return new NetPacket(size);
			}
			object poolLock = this._poolLock;
			NetPacket poolHead;
			lock (poolLock)
			{
				poolHead = this._poolHead;
				if (poolHead == null)
				{
					return new NetPacket(size);
				}
				this._poolHead = this._poolHead.Next;
				this._poolCount--;
			}
			poolHead.Size = size;
			if (poolHead.RawData.Length < size)
			{
				poolHead.RawData = new byte[size];
			}
			return poolHead;
		}

		// Token: 0x060000E4 RID: 228 RVA: 0x0000587C File Offset: 0x00003A7C
		internal void PoolRecycle(NetPacket packet)
		{
			if (packet.RawData.Length > NetConstants.MaxPacketSize || this._poolCount >= this.PacketPoolSize)
			{
				return;
			}
			packet.RawData[0] = 0;
			object poolLock = this._poolLock;
			lock (poolLock)
			{
				packet.Next = this._poolHead;
				this._poolHead = packet;
				this._poolCount++;
			}
		}

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x060000E5 RID: 229 RVA: 0x00005900 File Offset: 0x00003B00
		// (set) Token: 0x060000E6 RID: 230 RVA: 0x0000590D File Offset: 0x00003B0D
		public short Ttl
		{
			get
			{
				return this._udpSocketv4.Ttl;
			}
			internal set
			{
				this._udpSocketv4.Ttl = value;
			}
		}

		// Token: 0x060000E8 RID: 232 RVA: 0x00005950 File Offset: 0x00003B50
		private bool ProcessError(SocketException ex)
		{
			SocketError socketErrorCode = ex.SocketErrorCode;
			if (socketErrorCode <= SocketError.NotSocket)
			{
				if (socketErrorCode <= SocketError.Interrupted)
				{
					if (socketErrorCode != SocketError.OperationAborted && socketErrorCode != SocketError.Interrupted)
					{
						goto IL_007A;
					}
				}
				else
				{
					if (socketErrorCode == SocketError.WouldBlock)
					{
						return false;
					}
					if (socketErrorCode != SocketError.NotSocket)
					{
						goto IL_007A;
					}
				}
				return true;
			}
			if (socketErrorCode <= SocketError.NetworkReset)
			{
				if (socketErrorCode == SocketError.MessageSize || socketErrorCode == SocketError.NetworkReset)
				{
					return false;
				}
			}
			else
			{
				if (socketErrorCode == SocketError.ConnectionReset)
				{
					return false;
				}
				if (socketErrorCode == SocketError.NotConnected)
				{
					this.NotConnected = true;
					return true;
				}
				if (socketErrorCode == SocketError.TimedOut)
				{
					return false;
				}
			}
			IL_007A:
			NetDebug.WriteError(string.Format("[R]Error code: {0} - {1}", (int)ex.SocketErrorCode, ex));
			this.CreateEvent(NetEvent.EType.Error, null, null, ex.SocketErrorCode, 0, DisconnectReason.ConnectionFailed, null, DeliveryMethod.Unreliable, 0, null, null);
			return false;
		}

		// Token: 0x060000E9 RID: 233 RVA: 0x00005A0C File Offset: 0x00003C0C
		private void ManualReceive(Socket socket, EndPoint bufferEndPoint, int maxReceive)
		{
			try
			{
				int num = 0;
				while (socket.Available > 0)
				{
					this.ReceiveFrom(socket, ref bufferEndPoint);
					num++;
					if (num == maxReceive)
					{
						break;
					}
				}
			}
			catch (SocketException ex)
			{
				this.ProcessError(ex);
			}
			catch (ObjectDisposedException)
			{
			}
			catch (Exception ex2)
			{
				string text = "[NM] SocketReceiveThread error: ";
				Exception ex3 = ex2;
				NetDebug.WriteError(text + ((ex3 != null) ? ex3.ToString() : null));
			}
		}

		// Token: 0x060000EA RID: 234 RVA: 0x00005A90 File Offset: 0x00003C90
		private void NativeReceiveLogic()
		{
			NetManager.<>c__DisplayClass190_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			IntPtr handle = this._udpSocketv4.Handle;
			Socket udpSocketv = this._udpSocketv6;
			IntPtr intPtr = ((udpSocketv != null) ? udpSocketv.Handle : IntPtr.Zero);
			byte[] array = new byte[16];
			byte[] array2 = new byte[28];
			CS$<>8__locals1.tempEndPoint = new IPEndPoint(IPAddress.Any, 0);
			List<Socket> list = new List<Socket>(2);
			Socket udpSocketv2 = this._udpSocketv4;
			Socket udpSocketv3 = this._udpSocketv6;
			CS$<>8__locals1.packet = this.PoolGetPacket(NetConstants.MaxPacketSize);
			while (this.IsRunning)
			{
				try
				{
					if (udpSocketv3 == null)
					{
						if (!this.<NativeReceiveLogic>g__NativeReceiveFrom|190_0(handle, array, ref CS$<>8__locals1))
						{
							break;
						}
					}
					else
					{
						bool flag = false;
						if (udpSocketv2.Available != 0 || list.Contains(udpSocketv2))
						{
							if (!this.<NativeReceiveLogic>g__NativeReceiveFrom|190_0(handle, array, ref CS$<>8__locals1))
							{
								break;
							}
							flag = true;
						}
						if (udpSocketv3.Available != 0 || list.Contains(udpSocketv3))
						{
							if (!this.<NativeReceiveLogic>g__NativeReceiveFrom|190_0(intPtr, array2, ref CS$<>8__locals1))
							{
								break;
							}
							flag = true;
						}
						list.Clear();
						if (!flag)
						{
							list.Add(udpSocketv2);
							list.Add(udpSocketv3);
							Socket.Select(list, null, null, 500000);
						}
					}
				}
				catch (SocketException ex)
				{
					if (this.ProcessError(ex))
					{
						break;
					}
				}
				catch (ObjectDisposedException)
				{
					break;
				}
				catch (ThreadAbortException)
				{
					break;
				}
				catch (Exception ex2)
				{
					string text = "[NM] SocketReceiveThread error: ";
					Exception ex3 = ex2;
					NetDebug.WriteError(text + ((ex3 != null) ? ex3.ToString() : null));
				}
			}
		}

		// Token: 0x060000EB RID: 235 RVA: 0x00005C2C File Offset: 0x00003E2C
		private void ReceiveFrom(Socket s, ref EndPoint bufferEndPoint)
		{
			NetPacket netPacket = this.PoolGetPacket(NetConstants.MaxPacketSize);
			netPacket.Size = s.ReceiveFrom(netPacket.RawData, 0, NetConstants.MaxPacketSize, SocketFlags.None, ref bufferEndPoint);
			this.OnMessageReceived(netPacket, (IPEndPoint)bufferEndPoint);
		}

		// Token: 0x060000EC RID: 236 RVA: 0x00005C70 File Offset: 0x00003E70
		private void ReceiveLogic()
		{
			EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
			EndPoint endPoint2 = new IPEndPoint(IPAddress.IPv6Any, 0);
			List<Socket> list = new List<Socket>(2);
			Socket udpSocketv = this._udpSocketv4;
			Socket udpSocketv2 = this._udpSocketv6;
			while (this.IsRunning)
			{
				try
				{
					if (udpSocketv2 == null)
					{
						if (udpSocketv.Available != 0 || udpSocketv.Poll(500000, SelectMode.SelectRead))
						{
							this.ReceiveFrom(udpSocketv, ref endPoint);
						}
					}
					else
					{
						bool flag = false;
						if (udpSocketv.Available != 0 || list.Contains(udpSocketv))
						{
							this.ReceiveFrom(udpSocketv, ref endPoint);
							flag = true;
						}
						if (udpSocketv2.Available != 0 || list.Contains(udpSocketv2))
						{
							this.ReceiveFrom(udpSocketv2, ref endPoint2);
							flag = true;
						}
						list.Clear();
						if (!flag)
						{
							list.Add(udpSocketv);
							list.Add(udpSocketv2);
							Socket.Select(list, null, null, 500000);
						}
					}
				}
				catch (SocketException ex)
				{
					if (this.ProcessError(ex))
					{
						break;
					}
				}
				catch (ObjectDisposedException)
				{
					break;
				}
				catch (ThreadAbortException)
				{
					break;
				}
				catch (Exception ex2)
				{
					string text = "[NM] SocketReceiveThread error: ";
					Exception ex3 = ex2;
					NetDebug.WriteError(text + ((ex3 != null) ? ex3.ToString() : null));
				}
			}
		}

		// Token: 0x060000ED RID: 237 RVA: 0x00005DBC File Offset: 0x00003FBC
		public bool Start(IPAddress addressIPv4, IPAddress addressIPv6, int port, bool manualMode)
		{
			if (this.IsRunning && !this.NotConnected)
			{
				return false;
			}
			this.NotConnected = false;
			this._manualMode = manualMode;
			this.UseNativeSockets = this.UseNativeSockets && NativeSocket.IsSupported;
			this._udpSocketv4 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			if (!this.BindSocket(this._udpSocketv4, new IPEndPoint(addressIPv4, port)))
			{
				return false;
			}
			this.LocalPort = ((IPEndPoint)this._udpSocketv4.LocalEndPoint).Port;
			this.IsRunning = true;
			if (this._manualMode)
			{
				this._bufferEndPointv4 = new IPEndPoint(IPAddress.Any, 0);
			}
			if (NetManager.IPv6Support && this.IPv6Enabled)
			{
				this._udpSocketv6 = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
				if (this.BindSocket(this._udpSocketv6, new IPEndPoint(addressIPv6, this.LocalPort)))
				{
					if (this._manualMode)
					{
						this._bufferEndPointv6 = new IPEndPoint(IPAddress.IPv6Any, 0);
					}
				}
				else
				{
					this._udpSocketv6 = null;
				}
			}
			if (!manualMode)
			{
				ThreadStart threadStart = new ThreadStart(this.ReceiveLogic);
				if (this.UseNativeSockets)
				{
					threadStart = new ThreadStart(this.NativeReceiveLogic);
				}
				this._receiveThread = new Thread(threadStart)
				{
					Name = string.Format("ReceiveThread({0})", this.LocalPort),
					IsBackground = true
				};
				this._receiveThread.Start();
				if (this._logicThread == null)
				{
					this._logicThread = new Thread(new ThreadStart(this.UpdateLogic))
					{
						Name = "LogicThread",
						IsBackground = true
					};
					this._logicThread.Start();
				}
			}
			return true;
		}

		// Token: 0x060000EE RID: 238 RVA: 0x00005F5C File Offset: 0x0000415C
		private bool BindSocket(Socket socket, IPEndPoint ep)
		{
			socket.ReceiveTimeout = 500;
			socket.SendTimeout = 500;
			socket.ReceiveBufferSize = 1048576;
			socket.SendBufferSize = 1048576;
			socket.Blocking = true;
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				try
				{
					socket.IOControl(-1744830452, new byte[1], null);
				}
				catch
				{
				}
			}
			try
			{
				socket.ExclusiveAddressUse = !this.ReuseAddress;
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, this.ReuseAddress);
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, this.DontRoute);
			}
			catch
			{
			}
			if (ep.AddressFamily == AddressFamily.InterNetwork)
			{
				this.Ttl = 255;
				try
				{
					socket.EnableBroadcast = true;
				}
				catch (SocketException ex)
				{
					NetDebug.WriteError(string.Format("[B]Broadcast error: {0}", ex.SocketErrorCode));
				}
				if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				{
					try
					{
						socket.DontFragment = true;
					}
					catch (SocketException ex2)
					{
						NetDebug.WriteError(string.Format("[B]DontFragment error: {0}", ex2.SocketErrorCode));
					}
				}
			}
			try
			{
				socket.Bind(ep);
				if (ep.AddressFamily == AddressFamily.InterNetworkV6)
				{
					try
					{
						socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership, new IPv6MulticastOption(NetManager.MulticastAddressV6));
					}
					catch (Exception)
					{
					}
				}
			}
			catch (SocketException ex3)
			{
				SocketError socketErrorCode = ex3.SocketErrorCode;
				if (socketErrorCode == SocketError.AddressFamilyNotSupported)
				{
					return true;
				}
				if (socketErrorCode == SocketError.AddressAlreadyInUse && socket.AddressFamily == AddressFamily.InterNetworkV6)
				{
					try
					{
						socket.DualMode = false;
						socket.Bind(ep);
					}
					catch (SocketException ex4)
					{
						NetDebug.WriteError(string.Format("[B]Bind exception: {0}, errorCode: {1}", ex4, ex4.SocketErrorCode));
						return false;
					}
					return true;
				}
				NetDebug.WriteError(string.Format("[B]Bind exception: {0}, errorCode: {1}", ex3, ex3.SocketErrorCode));
				return false;
			}
			return true;
		}

		// Token: 0x060000EF RID: 239 RVA: 0x00006174 File Offset: 0x00004374
		internal int SendRawAndRecycle(NetPacket packet, IPEndPoint remoteEndPoint)
		{
			int num = this.SendRaw(packet.RawData, 0, packet.Size, remoteEndPoint);
			this.PoolRecycle(packet);
			return num;
		}

		// Token: 0x060000F0 RID: 240 RVA: 0x00006191 File Offset: 0x00004391
		internal int SendRaw(NetPacket packet, IPEndPoint remoteEndPoint)
		{
			return this.SendRaw(packet.RawData, 0, packet.Size, remoteEndPoint);
		}

		// Token: 0x060000F1 RID: 241 RVA: 0x000061A8 File Offset: 0x000043A8
		internal unsafe int SendRaw(byte[] message, int start, int length, IPEndPoint remoteEndPoint)
		{
			if (!this.IsRunning)
			{
				return 0;
			}
			NetPacket netPacket = null;
			if (this._extraPacketLayer != null)
			{
				netPacket = this.PoolGetPacket(length + this._extraPacketLayer.ExtraPacketSizeForLayer);
				Buffer.BlockCopy(message, start, netPacket.RawData, 0, length);
				start = 0;
				this._extraPacketLayer.ProcessOutBoundPacket(ref remoteEndPoint, ref netPacket.RawData, ref start, ref length);
				message = netPacket.RawData;
			}
			Socket socket = this._udpSocketv4;
			if (remoteEndPoint.AddressFamily == AddressFamily.InterNetworkV6 && NetManager.IPv6Support)
			{
				socket = this._udpSocketv6;
				if (socket == null)
				{
					return 0;
				}
			}
			int num;
			try
			{
				if (this.UseNativeSockets)
				{
					NetPeer netPeer = remoteEndPoint as NetPeer;
					if (netPeer != null)
					{
						try
						{
							fixed (byte* ptr = &message[start])
							{
								byte* ptr2 = ptr;
								num = NativeSocket.SendTo(socket.Handle, ptr2, length, netPeer.NativeAddress, netPeer.NativeAddress.Length);
							}
						}
						finally
						{
							byte* ptr = null;
						}
						if (num == -1)
						{
							throw NativeSocket.GetSocketException();
						}
						goto IL_00DB;
					}
				}
				num = socket.SendTo(message, start, length, SocketFlags.None, remoteEndPoint);
				IL_00DB:;
			}
			catch (SocketException ex)
			{
				SocketError socketErrorCode = ex.SocketErrorCode;
				if (socketErrorCode <= SocketError.NetworkUnreachable)
				{
					if (socketErrorCode != SocketError.Interrupted)
					{
						if (socketErrorCode == SocketError.MessageSize)
						{
							return 0;
						}
						if (socketErrorCode != SocketError.NetworkUnreachable)
						{
							goto IL_01B5;
						}
						goto IL_0144;
					}
				}
				else if (socketErrorCode != SocketError.NoBufferSpaceAvailable)
				{
					if (socketErrorCode == SocketError.Shutdown)
					{
						this.CreateEvent(NetEvent.EType.Error, null, remoteEndPoint, ex.SocketErrorCode, 0, DisconnectReason.ConnectionFailed, null, DeliveryMethod.Unreliable, 0, null, null);
						return -1;
					}
					if (socketErrorCode != SocketError.HostUnreachable)
					{
						goto IL_01B5;
					}
					goto IL_0144;
				}
				return 0;
				IL_0144:
				if (this.DisconnectOnUnreachable)
				{
					NetPeer netPeer2 = remoteEndPoint as NetPeer;
					if (netPeer2 != null)
					{
						this.DisconnectPeerForce(netPeer2, (ex.SocketErrorCode == SocketError.HostUnreachable) ? DisconnectReason.HostUnreachable : DisconnectReason.NetworkUnreachable, ex.SocketErrorCode, null);
					}
				}
				this.CreateEvent(NetEvent.EType.Error, null, remoteEndPoint, ex.SocketErrorCode, 0, DisconnectReason.ConnectionFailed, null, DeliveryMethod.Unreliable, 0, null, null);
				return -1;
				IL_01B5:
				NetDebug.WriteError(string.Format("[S] {0}", ex));
				return -1;
			}
			catch (Exception ex2)
			{
				NetDebug.WriteError(string.Format("[S] {0}", ex2));
				return 0;
			}
			finally
			{
				if (netPacket != null)
				{
					this.PoolRecycle(netPacket);
				}
			}
			if (num <= 0)
			{
				return 0;
			}
			if (this.EnableStatistics)
			{
				this.Statistics.IncrementPacketsSent();
				this.Statistics.AddBytesSent((long)length);
			}
			return num;
		}

		// Token: 0x060000F2 RID: 242 RVA: 0x00006434 File Offset: 0x00004634
		public bool SendBroadcast(NetDataWriter writer, int port)
		{
			return this.SendBroadcast(writer.Data, 0, writer.Length, port);
		}

		// Token: 0x060000F3 RID: 243 RVA: 0x0000644A File Offset: 0x0000464A
		public bool SendBroadcast(byte[] data, int port)
		{
			return this.SendBroadcast(data, 0, data.Length, port);
		}

		// Token: 0x060000F4 RID: 244 RVA: 0x00006458 File Offset: 0x00004658
		public bool SendBroadcast(byte[] data, int start, int length, int port)
		{
			if (!this.IsRunning)
			{
				return false;
			}
			NetPacket netPacket;
			if (this._extraPacketLayer != null)
			{
				int headerSize = NetPacket.GetHeaderSize(PacketProperty.Broadcast);
				netPacket = this.PoolGetPacket(headerSize + length + this._extraPacketLayer.ExtraPacketSizeForLayer);
				netPacket.Property = PacketProperty.Broadcast;
				Buffer.BlockCopy(data, start, netPacket.RawData, headerSize, length);
				int num = 0;
				int num2 = length + headerSize;
				IPEndPoint ipendPoint = null;
				this._extraPacketLayer.ProcessOutBoundPacket(ref ipendPoint, ref netPacket.RawData, ref num, ref num2);
			}
			else
			{
				netPacket = this.PoolGetWithData(PacketProperty.Broadcast, data, start, length);
			}
			bool flag = false;
			bool flag2 = false;
			try
			{
				flag = this._udpSocketv4.SendTo(netPacket.RawData, 0, netPacket.Size, SocketFlags.None, new IPEndPoint(IPAddress.Broadcast, port)) > 0;
				if (this._udpSocketv6 != null)
				{
					flag2 = this._udpSocketv6.SendTo(netPacket.RawData, 0, netPacket.Size, SocketFlags.None, new IPEndPoint(NetManager.MulticastAddressV6, port)) > 0;
				}
			}
			catch (SocketException ex)
			{
				if (ex.SocketErrorCode == SocketError.HostUnreachable)
				{
					return flag;
				}
				NetDebug.WriteError(string.Format("[S][MCAST] {0}", ex));
				return flag;
			}
			catch (Exception ex2)
			{
				NetDebug.WriteError(string.Format("[S][MCAST] {0}", ex2));
				return flag;
			}
			finally
			{
				this.PoolRecycle(netPacket);
			}
			return flag || flag2;
		}

		// Token: 0x060000F5 RID: 245 RVA: 0x000065B8 File Offset: 0x000047B8
		private void CloseSocket()
		{
			this.IsRunning = false;
			Socket udpSocketv = this._udpSocketv4;
			if (udpSocketv != null)
			{
				udpSocketv.Close();
			}
			Socket udpSocketv2 = this._udpSocketv6;
			if (udpSocketv2 != null)
			{
				udpSocketv2.Close();
			}
			this._udpSocketv4 = null;
			this._udpSocketv6 = null;
			if (this._receiveThread != null && this._receiveThread != Thread.CurrentThread)
			{
				this._receiveThread.Join();
			}
			this._receiveThread = null;
		}

		// Token: 0x060000F6 RID: 246 RVA: 0x00006624 File Offset: 0x00004824
		[CompilerGenerated]
		internal static bool <HashSetGetPrime>g__IsPrime|143_0(int candidate)
		{
			if ((candidate & 1) != 0)
			{
				int num = (int)Math.Sqrt((double)candidate);
				for (int i = 3; i <= num; i += 2)
				{
					if (candidate % i == 0)
					{
						return false;
					}
				}
				return true;
			}
			return candidate == 2;
		}

		// Token: 0x060000F7 RID: 247 RVA: 0x00006658 File Offset: 0x00004858
		[CompilerGenerated]
		private bool <NativeReceiveLogic>g__NativeReceiveFrom|190_0(IntPtr s, byte[] address, ref NetManager.<>c__DisplayClass190_0 A_3)
		{
			int num = address.Length;
			A_3.packet.Size = NativeSocket.RecvFrom(s, A_3.packet.RawData, NetConstants.MaxPacketSize, address, ref num);
			if (A_3.packet.Size == 0)
			{
				return true;
			}
			if (A_3.packet.Size == -1)
			{
				return !this.ProcessError(new SocketException((int)NativeSocket.GetSocketError()));
			}
			short num2 = (short)(((int)address[1] << 8) | (int)address[0]);
			A_3.tempEndPoint.Port = (int)((ushort)(((int)address[2] << 8) | (int)address[3]));
			if ((NativeSocket.UnixMode && num2 == 10) || (!NativeSocket.UnixMode && num2 == 23))
			{
				uint num3 = (uint)(((int)address[27] << 24) + ((int)address[26] << 16) + ((int)address[25] << 8) + (int)address[24]);
				byte[] array = new byte[16];
				Buffer.BlockCopy(address, 8, array, 0, 16);
				A_3.tempEndPoint.Address = new IPAddress(array, (long)((ulong)num3));
			}
			else
			{
				long num4 = (long)((ulong)((int)(address[4] & byte.MaxValue) | (((int)address[5] << 8) & 65280) | (((int)address[6] << 16) & 16711680) | ((int)address[7] << 24)));
				A_3.tempEndPoint.Address = new IPAddress(num4);
			}
			NetPeer netPeer;
			if (this.TryGetPeer(A_3.tempEndPoint, out netPeer))
			{
				this.OnMessageReceived(A_3.packet, netPeer);
			}
			else
			{
				this.OnMessageReceived(A_3.packet, A_3.tempEndPoint);
				A_3.tempEndPoint = new IPEndPoint(IPAddress.Any, 0);
			}
			A_3.packet = this.PoolGetPacket(NetConstants.MaxPacketSize);
			return true;
		}

		// Token: 0x04000081 RID: 129
		private readonly List<NetManager.IncomingData> _pingSimulationList = new List<NetManager.IncomingData>();

		// Token: 0x04000082 RID: 130
		private readonly Random _randomGenerator = new Random();

		// Token: 0x04000083 RID: 131
		private const int MinLatencyThreshold = 5;

		// Token: 0x04000084 RID: 132
		private Thread _logicThread;

		// Token: 0x04000085 RID: 133
		private bool _manualMode;

		// Token: 0x04000086 RID: 134
		private readonly AutoResetEvent _updateTriggerEvent = new AutoResetEvent(true);

		// Token: 0x04000087 RID: 135
		private NetEvent _pendingEventHead;

		// Token: 0x04000088 RID: 136
		private NetEvent _pendingEventTail;

		// Token: 0x04000089 RID: 137
		private NetEvent _netEventPoolHead;

		// Token: 0x0400008A RID: 138
		private readonly INetEventListener _netEventListener;

		// Token: 0x0400008B RID: 139
		private readonly IDeliveryEventListener _deliveryEventListener;

		// Token: 0x0400008C RID: 140
		private readonly INtpEventListener _ntpEventListener;

		// Token: 0x0400008D RID: 141
		private readonly IPeerAddressChangedListener _peerAddressChangedListener;

		// Token: 0x0400008E RID: 142
		private readonly Dictionary<IPEndPoint, ConnectionRequest> _requestsDict = new Dictionary<IPEndPoint, ConnectionRequest>();

		// Token: 0x0400008F RID: 143
		private readonly ConcurrentDictionary<IPEndPoint, NtpRequest> _ntpRequests = new ConcurrentDictionary<IPEndPoint, NtpRequest>();

		// Token: 0x04000090 RID: 144
		private long _connectedPeersCount;

		// Token: 0x04000091 RID: 145
		private readonly List<NetPeer> _connectedPeerListCache = new List<NetPeer>();

		// Token: 0x04000092 RID: 146
		private readonly PacketLayerBase _extraPacketLayer;

		// Token: 0x04000093 RID: 147
		private int _lastPeerId;

		// Token: 0x04000094 RID: 148
		private ConcurrentQueue<int> _peerIds = new ConcurrentQueue<int>();

		// Token: 0x04000095 RID: 149
		private byte _channelsCount = 1;

		// Token: 0x04000096 RID: 150
		private readonly object _eventLock = new object();

		// Token: 0x04000097 RID: 151
		private bool _dropPacket;

		// Token: 0x04000098 RID: 152
		public bool UnconnectedMessagesEnabled;

		// Token: 0x04000099 RID: 153
		public bool NatPunchEnabled;

		// Token: 0x0400009A RID: 154
		public int UpdateTime = 15;

		// Token: 0x0400009B RID: 155
		public int PingInterval = 1000;

		// Token: 0x0400009C RID: 156
		public int DisconnectTimeout = 5000;

		// Token: 0x0400009D RID: 157
		public bool SimulatePacketLoss;

		// Token: 0x0400009E RID: 158
		public bool SimulateLatency;

		// Token: 0x0400009F RID: 159
		public int SimulationPacketLossChance = 10;

		// Token: 0x040000A0 RID: 160
		public int SimulationMinLatency = 30;

		// Token: 0x040000A1 RID: 161
		public int SimulationMaxLatency = 100;

		// Token: 0x040000A2 RID: 162
		public bool UnsyncedEvents;

		// Token: 0x040000A3 RID: 163
		public bool UnsyncedReceiveEvent;

		// Token: 0x040000A4 RID: 164
		public bool UnsyncedDeliveryEvent;

		// Token: 0x040000A5 RID: 165
		public bool BroadcastReceiveEnabled;

		// Token: 0x040000A6 RID: 166
		public int ReconnectDelay = 500;

		// Token: 0x040000A7 RID: 167
		public int MaxConnectAttempts = 10;

		// Token: 0x040000A8 RID: 168
		public bool ReuseAddress;

		// Token: 0x040000A9 RID: 169
		public bool DontRoute;

		// Token: 0x040000AA RID: 170
		public readonly NetStatistics Statistics = new NetStatistics();

		// Token: 0x040000AB RID: 171
		public bool EnableStatistics;

		// Token: 0x040000AC RID: 172
		public readonly NatPunchModule NatPunchModule;

		// Token: 0x040000AF RID: 175
		public bool AutoRecycle;

		// Token: 0x040000B0 RID: 176
		public bool IPv6Enabled = true;

		// Token: 0x040000B1 RID: 177
		public int MtuOverride;

		// Token: 0x040000B2 RID: 178
		public bool MtuDiscovery;

		// Token: 0x040000B3 RID: 179
		public bool UseNativeSockets;

		// Token: 0x040000B4 RID: 180
		public bool DisconnectOnUnreachable;

		// Token: 0x040000B5 RID: 181
		public bool AllowPeerAddressChange;

		// Token: 0x040000B6 RID: 182
		private const int MaxPrimeArrayLength = 2147483587;

		// Token: 0x040000B7 RID: 183
		private const int HashPrime = 101;

		// Token: 0x040000B8 RID: 184
		private const int Lower31BitMask = 2147483647;

		// Token: 0x040000B9 RID: 185
		private static readonly int[] Primes = new int[]
		{
			3, 7, 11, 17, 23, 29, 37, 47, 59, 71,
			89, 107, 131, 163, 197, 239, 293, 353, 431, 521,
			631, 761, 919, 1103, 1327, 1597, 1931, 2333, 2801, 3371,
			4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591, 17519, 21023,
			25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363,
			156437, 187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403,
			968897, 1162687, 1395263, 1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559,
			5999471, 7199369
		};

		// Token: 0x040000BA RID: 186
		private int[] _buckets;

		// Token: 0x040000BB RID: 187
		private NetManager.Slot[] _slots;

		// Token: 0x040000BC RID: 188
		private int _count;

		// Token: 0x040000BD RID: 189
		private int _lastIndex;

		// Token: 0x040000BE RID: 190
		private int _freeList = -1;

		// Token: 0x040000BF RID: 191
		private NetPeer[] _peersArray = new NetPeer[32];

		// Token: 0x040000C0 RID: 192
		private readonly ReaderWriterLockSlim _peersLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

		// Token: 0x040000C1 RID: 193
		private volatile NetPeer _headPeer;

		// Token: 0x040000C2 RID: 194
		private NetPacket _poolHead;

		// Token: 0x040000C3 RID: 195
		private int _poolCount;

		// Token: 0x040000C4 RID: 196
		private readonly object _poolLock = new object();

		// Token: 0x040000C5 RID: 197
		public int PacketPoolSize = 1000;

		// Token: 0x040000C6 RID: 198
		private const int ReceivePollingTime = 500000;

		// Token: 0x040000C7 RID: 199
		private Socket _udpSocketv4;

		// Token: 0x040000C8 RID: 200
		private Socket _udpSocketv6;

		// Token: 0x040000C9 RID: 201
		private Thread _receiveThread;

		// Token: 0x040000CA RID: 202
		private IPEndPoint _bufferEndPointv4;

		// Token: 0x040000CB RID: 203
		private IPEndPoint _bufferEndPointv6;

		// Token: 0x040000CC RID: 204
		private const int SioUdpConnreset = -1744830452;

		// Token: 0x040000CD RID: 205
		private static readonly IPAddress MulticastAddressV6 = IPAddress.Parse("ff02::1");

		// Token: 0x040000CE RID: 206
		public static readonly bool IPv6Support = Socket.OSSupportsIPv6;

		// Token: 0x040000CF RID: 207
		internal bool NotConnected;

		// Token: 0x02000086 RID: 134
		public struct NetPeerEnumerator : IEnumerator<NetPeer>, IEnumerator, IDisposable
		{
			// Token: 0x06000478 RID: 1144 RVA: 0x00012A1F File Offset: 0x00010C1F
			public NetPeerEnumerator(NetPeer p)
			{
				this._initialPeer = p;
				this._p = null;
			}

			// Token: 0x06000479 RID: 1145 RVA: 0x00012A2F File Offset: 0x00010C2F
			public void Dispose()
			{
			}

			// Token: 0x0600047A RID: 1146 RVA: 0x00012A31 File Offset: 0x00010C31
			public bool MoveNext()
			{
				this._p = ((this._p == null) ? this._initialPeer : this._p.NextPeer);
				return this._p != null;
			}

			// Token: 0x0600047B RID: 1147 RVA: 0x00012A5F File Offset: 0x00010C5F
			public void Reset()
			{
				throw new NotSupportedException();
			}

			// Token: 0x1700007E RID: 126
			// (get) Token: 0x0600047C RID: 1148 RVA: 0x00012A66 File Offset: 0x00010C66
			public NetPeer Current
			{
				get
				{
					return this._p;
				}
			}

			// Token: 0x1700007F RID: 127
			// (get) Token: 0x0600047D RID: 1149 RVA: 0x00012A6E File Offset: 0x00010C6E
			object IEnumerator.Current
			{
				get
				{
					return this._p;
				}
			}

			// Token: 0x040002D0 RID: 720
			private readonly NetPeer _initialPeer;

			// Token: 0x040002D1 RID: 721
			private NetPeer _p;
		}

		// Token: 0x02000087 RID: 135
		private struct IncomingData
		{
			// Token: 0x040002D2 RID: 722
			public NetPacket Data;

			// Token: 0x040002D3 RID: 723
			public IPEndPoint EndPoint;

			// Token: 0x040002D4 RID: 724
			public DateTime TimeWhenGet;
		}

		// Token: 0x02000088 RID: 136
		private struct Slot
		{
			// Token: 0x040002D5 RID: 725
			internal int HashCode;

			// Token: 0x040002D6 RID: 726
			internal int Next;

			// Token: 0x040002D7 RID: 727
			internal NetPeer Value;
		}
	}
}
