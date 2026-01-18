using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using Networking.UDP.Utils;

namespace Networking.UDP
{
	// Token: 0x0200002C RID: 44
	public class NetPeer : IPEndPoint
	{
		// Token: 0x17000015 RID: 21
		// (get) Token: 0x0600010E RID: 270 RVA: 0x00006A54 File Offset: 0x00004C54
		// (set) Token: 0x0600010F RID: 271 RVA: 0x00006A5C File Offset: 0x00004C5C
		internal byte ConnectionNum
		{
			get
			{
				return this._connectNum;
			}
			private set
			{
				this._connectNum = value;
				this._mergeData.ConnectionNumber = value;
				this._pingPacket.ConnectionNumber = value;
				this._pongPacket.ConnectionNumber = value;
			}
		}

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x06000110 RID: 272 RVA: 0x00006A89 File Offset: 0x00004C89
		public ConnectionState ConnectionState
		{
			get
			{
				return this._connectionState;
			}
		}

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x06000111 RID: 273 RVA: 0x00006A91 File Offset: 0x00004C91
		internal long ConnectTime
		{
			get
			{
				return this._connectTime;
			}
		}

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x06000112 RID: 274 RVA: 0x00006A99 File Offset: 0x00004C99
		// (set) Token: 0x06000113 RID: 275 RVA: 0x00006AA1 File Offset: 0x00004CA1
		public int RemoteId { get; private set; }

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x06000114 RID: 276 RVA: 0x00006AAA File Offset: 0x00004CAA
		public int Ping
		{
			get
			{
				return this._avgRtt / 2;
			}
		}

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x06000115 RID: 277 RVA: 0x00006AB4 File Offset: 0x00004CB4
		public int RoundTripTime
		{
			get
			{
				return this._avgRtt;
			}
		}

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x06000116 RID: 278 RVA: 0x00006ABC File Offset: 0x00004CBC
		public int Mtu
		{
			get
			{
				return this._mtu;
			}
		}

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x06000117 RID: 279 RVA: 0x00006AC4 File Offset: 0x00004CC4
		public long RemoteTimeDelta
		{
			get
			{
				return this._remoteDelta;
			}
		}

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x06000118 RID: 280 RVA: 0x00006ACC File Offset: 0x00004CCC
		public DateTime RemoteUtcTime
		{
			get
			{
				return new DateTime(DateTime.UtcNow.Ticks + this._remoteDelta);
			}
		}

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x06000119 RID: 281 RVA: 0x00006AF2 File Offset: 0x00004CF2
		public float TimeSinceLastPacket
		{
			get
			{
				return this._timeSinceLastPacket;
			}
		}

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x0600011A RID: 282 RVA: 0x00006AFC File Offset: 0x00004CFC
		internal double ResendDelay
		{
			get
			{
				return this._resendDelay;
			}
		}

		// Token: 0x0600011B RID: 283 RVA: 0x00006B04 File Offset: 0x00004D04
		public override SocketAddress Serialize()
		{
			return this._cachedSocketAddr;
		}

		// Token: 0x0600011C RID: 284 RVA: 0x00006B0C File Offset: 0x00004D0C
		public override int GetHashCode()
		{
			return this._cachedHashCode;
		}

		// Token: 0x0600011D RID: 285 RVA: 0x00006B14 File Offset: 0x00004D14
		internal NetPeer(NetManager netManager, IPEndPoint remoteEndPoint, int id)
			: base(remoteEndPoint.Address, remoteEndPoint.Port)
		{
			this.Id = id;
			this.Statistics = new NetStatistics();
			this.NetManager = netManager;
			this._cachedSocketAddr = base.Serialize();
			if (this.NetManager.UseNativeSockets)
			{
				this.NativeAddress = new byte[this._cachedSocketAddr.Size];
				for (int i = 0; i < this._cachedSocketAddr.Size; i++)
				{
					this.NativeAddress[i] = this._cachedSocketAddr[i];
				}
			}
			this._cachedHashCode = base.GetHashCode();
			this.ResetMtu();
			this._connectionState = ConnectionState.Connected;
			this._mergeData = new NetPacket(PacketProperty.Merged, NetConstants.MaxPacketSize);
			this._pongPacket = new NetPacket(PacketProperty.Pong, 0);
			this._pingPacket = new NetPacket(PacketProperty.Ping, 0)
			{
				Sequence = 1
			};
			this._unreliableSecondQueue = new NetPacket[8];
			this._unreliableChannel = new NetPacket[8];
			this._holdedFragments = new Dictionary<ushort, NetPeer.IncomingFragments>();
			this._deliveredFragments = new Dictionary<ushort, ushort>();
			this._channels = new BaseChannel[(int)(netManager.ChannelsCount * 4)];
			this._channelSendQueue = new ConcurrentQueue<BaseChannel>();
		}

		// Token: 0x0600011E RID: 286 RVA: 0x00006C78 File Offset: 0x00004E78
		internal void InitiateEndPointChange()
		{
			this.ResetMtu();
			this._connectionState = ConnectionState.EndPointChange;
		}

		// Token: 0x0600011F RID: 287 RVA: 0x00006C88 File Offset: 0x00004E88
		internal void FinishEndPointChange(IPEndPoint newEndPoint)
		{
			if (this._connectionState != ConnectionState.EndPointChange)
			{
				return;
			}
			this._connectionState = ConnectionState.Connected;
			base.Address = newEndPoint.Address;
			base.Port = newEndPoint.Port;
			if (this.NetManager.UseNativeSockets)
			{
				this.NativeAddress = new byte[this._cachedSocketAddr.Size];
				for (int i = 0; i < this._cachedSocketAddr.Size; i++)
				{
					this.NativeAddress[i] = this._cachedSocketAddr[i];
				}
			}
			this._cachedSocketAddr = base.Serialize();
			this._cachedHashCode = base.GetHashCode();
		}

		// Token: 0x06000120 RID: 288 RVA: 0x00006D24 File Offset: 0x00004F24
		internal void ResetMtu()
		{
			this._finishMtu = !this.NetManager.MtuDiscovery;
			if (this.NetManager.MtuOverride > 0)
			{
				this.OverrideMtu(this.NetManager.MtuOverride);
				return;
			}
			this.SetMtu(0);
		}

		// Token: 0x06000121 RID: 289 RVA: 0x00006D61 File Offset: 0x00004F61
		private void SetMtu(int mtuIdx)
		{
			this._mtuIdx = mtuIdx;
			this._mtu = NetConstants.PossibleMtu[mtuIdx] - this.NetManager.ExtraPacketSizeForLayer;
		}

		// Token: 0x06000122 RID: 290 RVA: 0x00006D83 File Offset: 0x00004F83
		private void OverrideMtu(int mtuValue)
		{
			this._mtu = mtuValue;
			this._finishMtu = true;
		}

		// Token: 0x06000123 RID: 291 RVA: 0x00006D94 File Offset: 0x00004F94
		public int GetPacketsCountInReliableQueue(byte channelNumber, bool ordered)
		{
			int num = (int)(channelNumber * 4 + (ordered ? 2 : 0));
			BaseChannel baseChannel = this._channels[num];
			if (baseChannel == null)
			{
				return 0;
			}
			return ((ReliableChannel)baseChannel).PacketsInQueue;
		}

		// Token: 0x06000124 RID: 292 RVA: 0x00006DC8 File Offset: 0x00004FC8
		public PooledPacket CreatePacketFromPool(DeliveryMethod deliveryMethod, byte channelNumber)
		{
			int mtu = this._mtu;
			NetPacket netPacket = this.NetManager.PoolGetPacket(mtu);
			if (deliveryMethod == DeliveryMethod.Unreliable)
			{
				netPacket.Property = PacketProperty.Unreliable;
				return new PooledPacket(netPacket, mtu, 0);
			}
			netPacket.Property = PacketProperty.Channeled;
			return new PooledPacket(netPacket, mtu, (byte)(channelNumber * 4 + deliveryMethod));
		}

		// Token: 0x06000125 RID: 293 RVA: 0x00006E14 File Offset: 0x00005014
		public void SendPooledPacket(PooledPacket packet, int userDataSize)
		{
			if (this._connectionState != ConnectionState.Connected)
			{
				return;
			}
			packet._packet.Size = packet.UserDataOffset + userDataSize;
			if (packet._packet.Property == PacketProperty.Channeled)
			{
				this.CreateChannel(packet._channelNumber).AddToQueue(packet._packet);
				return;
			}
			this.EnqueueUnreliable(packet._packet);
		}

		// Token: 0x06000126 RID: 294 RVA: 0x00006E70 File Offset: 0x00005070
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void EnqueueUnreliable(NetPacket packet)
		{
			object unreliableChannelLock = this._unreliableChannelLock;
			lock (unreliableChannelLock)
			{
				if (this._unreliablePendingCount == this._unreliableChannel.Length)
				{
					Array.Resize<NetPacket>(ref this._unreliableChannel, this._unreliablePendingCount * 2);
				}
				NetPacket[] unreliableChannel = this._unreliableChannel;
				int unreliablePendingCount = this._unreliablePendingCount;
				this._unreliablePendingCount = unreliablePendingCount + 1;
				unreliableChannel[unreliablePendingCount] = packet;
			}
		}

		// Token: 0x06000127 RID: 295 RVA: 0x00006EE8 File Offset: 0x000050E8
		private BaseChannel CreateChannel(byte idx)
		{
			BaseChannel baseChannel = this._channels[(int)idx];
			if (baseChannel != null)
			{
				return baseChannel;
			}
			switch (idx % 4)
			{
			case 0:
				baseChannel = new ReliableChannel(this, false, idx);
				break;
			case 1:
				baseChannel = new SequencedChannel(this, false, idx);
				break;
			case 2:
				baseChannel = new ReliableChannel(this, true, idx);
				break;
			case 3:
				baseChannel = new SequencedChannel(this, true, idx);
				break;
			}
			BaseChannel baseChannel2 = Interlocked.CompareExchange<BaseChannel>(ref this._channels[(int)idx], baseChannel, null);
			if (baseChannel2 != null)
			{
				return baseChannel2;
			}
			return baseChannel;
		}

		// Token: 0x06000128 RID: 296 RVA: 0x00006F64 File Offset: 0x00005164
		internal NetPeer(NetManager netManager, IPEndPoint remoteEndPoint, int id, byte connectNum, NetDataWriter connectData)
			: this(netManager, remoteEndPoint, id)
		{
			this._connectTime = DateTime.UtcNow.Ticks;
			this._connectionState = ConnectionState.Outgoing;
			this.ConnectionNum = connectNum;
			this._connectRequestPacket = NetConnectRequestPacket.Make(connectData, remoteEndPoint.Serialize(), this._connectTime, id);
			this._connectRequestPacket.ConnectionNumber = connectNum;
			this.NetManager.SendRaw(this._connectRequestPacket, this);
		}

		// Token: 0x06000129 RID: 297 RVA: 0x00006FD8 File Offset: 0x000051D8
		internal NetPeer(NetManager netManager, ConnectionRequest request, int id)
			: this(netManager, request.RemoteEndPoint, id)
		{
			this._connectTime = request.InternalPacket.ConnectionTime;
			this.ConnectionNum = request.InternalPacket.ConnectionNumber;
			this.RemoteId = request.InternalPacket.PeerId;
			this._connectAcceptPacket = NetConnectAcceptPacket.Make(this._connectTime, this.ConnectionNum, id);
			this._connectionState = ConnectionState.Connected;
			this.NetManager.SendRaw(this._connectAcceptPacket, this);
		}

		// Token: 0x0600012A RID: 298 RVA: 0x00007058 File Offset: 0x00005258
		internal void Reject(NetConnectRequestPacket requestData, byte[] data, int start, int length)
		{
			this._connectTime = requestData.ConnectionTime;
			this._connectNum = requestData.ConnectionNumber;
			this.Shutdown(data, start, length, false);
		}

		// Token: 0x0600012B RID: 299 RVA: 0x00007080 File Offset: 0x00005280
		internal bool ProcessConnectAccept(NetConnectAcceptPacket packet)
		{
			if (this._connectionState != ConnectionState.Outgoing)
			{
				return false;
			}
			if (packet.ConnectionTime != this._connectTime)
			{
				return false;
			}
			this.ConnectionNum = packet.ConnectionNumber;
			this.RemoteId = packet.PeerId;
			Interlocked.Exchange(ref this._timeSinceLastPacket, 0f);
			this._connectionState = ConnectionState.Connected;
			return true;
		}

		// Token: 0x0600012C RID: 300 RVA: 0x000070D9 File Offset: 0x000052D9
		public int GetMaxSinglePacketSize(DeliveryMethod options)
		{
			return this._mtu - NetPacket.GetHeaderSize((options == DeliveryMethod.Unreliable) ? PacketProperty.Unreliable : PacketProperty.Channeled);
		}

		// Token: 0x0600012D RID: 301 RVA: 0x000070EF File Offset: 0x000052EF
		public void SendWithDeliveryEvent(byte[] data, byte channelNumber, DeliveryMethod deliveryMethod, object userData)
		{
			if (deliveryMethod != DeliveryMethod.ReliableOrdered && deliveryMethod != DeliveryMethod.ReliableUnordered)
			{
				throw new ArgumentException("Delivery event will work only for ReliableOrdered/Unordered packets");
			}
			this.SendInternal(data, 0, data.Length, channelNumber, deliveryMethod, userData);
		}

		// Token: 0x0600012E RID: 302 RVA: 0x00007112 File Offset: 0x00005312
		public void SendWithDeliveryEvent(byte[] data, int start, int length, byte channelNumber, DeliveryMethod deliveryMethod, object userData)
		{
			if (deliveryMethod != DeliveryMethod.ReliableOrdered && deliveryMethod != DeliveryMethod.ReliableUnordered)
			{
				throw new ArgumentException("Delivery event will work only for ReliableOrdered/Unordered packets");
			}
			this.SendInternal(data, start, length, channelNumber, deliveryMethod, userData);
		}

		// Token: 0x0600012F RID: 303 RVA: 0x00007137 File Offset: 0x00005337
		public void SendWithDeliveryEvent(NetDataWriter dataWriter, byte channelNumber, DeliveryMethod deliveryMethod, object userData)
		{
			if (deliveryMethod != DeliveryMethod.ReliableOrdered && deliveryMethod != DeliveryMethod.ReliableUnordered)
			{
				throw new ArgumentException("Delivery event will work only for ReliableOrdered/Unordered packets");
			}
			this.SendInternal(dataWriter.Data, 0, dataWriter.Length, channelNumber, deliveryMethod, userData);
		}

		// Token: 0x06000130 RID: 304 RVA: 0x00007162 File Offset: 0x00005362
		public void Send(byte[] data, DeliveryMethod deliveryMethod)
		{
			this.SendInternal(data, 0, data.Length, 0, deliveryMethod, null);
		}

		// Token: 0x06000131 RID: 305 RVA: 0x00007172 File Offset: 0x00005372
		public void Send(NetDataWriter dataWriter, DeliveryMethod deliveryMethod)
		{
			this.SendInternal(dataWriter.Data, 0, dataWriter.Length, 0, deliveryMethod, null);
		}

		// Token: 0x06000132 RID: 306 RVA: 0x0000718A File Offset: 0x0000538A
		public void Send(byte[] data, int start, int length, DeliveryMethod options)
		{
			this.SendInternal(data, start, length, 0, options, null);
		}

		// Token: 0x06000133 RID: 307 RVA: 0x00007199 File Offset: 0x00005399
		public void Send(byte[] data, byte channelNumber, DeliveryMethod deliveryMethod)
		{
			this.SendInternal(data, 0, data.Length, channelNumber, deliveryMethod, null);
		}

		// Token: 0x06000134 RID: 308 RVA: 0x000071A9 File Offset: 0x000053A9
		public void Send(NetDataWriter dataWriter, byte channelNumber, DeliveryMethod deliveryMethod)
		{
			this.SendInternal(dataWriter.Data, 0, dataWriter.Length, channelNumber, deliveryMethod, null);
		}

		// Token: 0x06000135 RID: 309 RVA: 0x000071C1 File Offset: 0x000053C1
		public void Send(byte[] data, int start, int length, byte channelNumber, DeliveryMethod deliveryMethod)
		{
			this.SendInternal(data, start, length, channelNumber, deliveryMethod, null);
		}

		// Token: 0x06000136 RID: 310 RVA: 0x000071D4 File Offset: 0x000053D4
		private void SendInternal(byte[] data, int start, int length, byte channelNumber, DeliveryMethod deliveryMethod, object userData)
		{
			if (this._connectionState != ConnectionState.Connected || (int)channelNumber >= this._channels.Length)
			{
				return;
			}
			BaseChannel baseChannel = null;
			PacketProperty packetProperty;
			if (deliveryMethod == DeliveryMethod.Unreliable)
			{
				packetProperty = PacketProperty.Unreliable;
			}
			else
			{
				packetProperty = PacketProperty.Channeled;
				baseChannel = this.CreateChannel((byte)(channelNumber * 4 + deliveryMethod));
			}
			int headerSize = NetPacket.GetHeaderSize(packetProperty);
			int mtu = this._mtu;
			if (length + headerSize > mtu)
			{
				if (deliveryMethod != DeliveryMethod.ReliableOrdered && deliveryMethod != DeliveryMethod.ReliableUnordered)
				{
					throw new TooBigPacketException(string.Format("[{0}] packet size [{1}] exceeded maximum of ", deliveryMethod, length + headerSize) + (mtu - headerSize).ToString() + " bytes, Check allowed size by GetMaxSinglePacketSize()");
				}
				int num = mtu - headerSize - 6;
				int num2 = length / num + ((length % num != 0) ? 1 : 0);
				if (num2 > 65535)
				{
					throw new TooBigPacketException("Data was split in " + num2.ToString() + " fragments, which exceeds " + ushort.MaxValue.ToString());
				}
				ushort num3 = (ushort)Interlocked.Increment(ref this._fragmentId);
				ushort num4 = 0;
				while ((int)num4 < num2)
				{
					int num5 = ((length > num) ? num : length);
					NetPacket netPacket = this.NetManager.PoolGetPacket(headerSize + num5 + 6);
					netPacket.Property = packetProperty;
					netPacket.UserData = userData;
					netPacket.FragmentId = num3;
					netPacket.FragmentPart = num4;
					netPacket.FragmentsTotal = (ushort)num2;
					netPacket.MarkFragmented();
					Buffer.BlockCopy(data, start + (int)num4 * num, netPacket.RawData, 10, num5);
					baseChannel.AddToQueue(netPacket);
					length -= num5;
					num4 += 1;
				}
				return;
			}
			else
			{
				NetPacket netPacket2 = this.NetManager.PoolGetPacket(headerSize + length);
				netPacket2.Property = packetProperty;
				Buffer.BlockCopy(data, start, netPacket2.RawData, headerSize, length);
				netPacket2.UserData = userData;
				if (baseChannel == null)
				{
					this.EnqueueUnreliable(netPacket2);
					return;
				}
				baseChannel.AddToQueue(netPacket2);
				return;
			}
		}

		// Token: 0x06000137 RID: 311 RVA: 0x00007395 File Offset: 0x00005595
		public void Disconnect(byte[] data)
		{
			this.NetManager.DisconnectPeer(this, data);
		}

		// Token: 0x06000138 RID: 312 RVA: 0x000073A4 File Offset: 0x000055A4
		public void Disconnect(NetDataWriter writer)
		{
			this.NetManager.DisconnectPeer(this, writer);
		}

		// Token: 0x06000139 RID: 313 RVA: 0x000073B3 File Offset: 0x000055B3
		public void Disconnect(byte[] data, int start, int count)
		{
			this.NetManager.DisconnectPeer(this, data, start, count);
		}

		// Token: 0x0600013A RID: 314 RVA: 0x000073C4 File Offset: 0x000055C4
		public void Disconnect()
		{
			this.NetManager.DisconnectPeer(this);
		}

		// Token: 0x0600013B RID: 315 RVA: 0x000073D4 File Offset: 0x000055D4
		internal DisconnectResult ProcessDisconnect(NetPacket packet)
		{
			if ((this._connectionState != ConnectionState.Connected && this._connectionState != ConnectionState.Outgoing) || packet.Size < 9 || BitConverter.ToInt64(packet.RawData, 1) != this._connectTime || packet.ConnectionNumber != this._connectNum)
			{
				return DisconnectResult.None;
			}
			if (this._connectionState != ConnectionState.Connected)
			{
				return DisconnectResult.Reject;
			}
			return DisconnectResult.Disconnect;
		}

		// Token: 0x0600013C RID: 316 RVA: 0x0000742D File Offset: 0x0000562D
		internal void AddToReliableChannelSendQueue(BaseChannel channel)
		{
			this._channelSendQueue.Enqueue(channel);
		}

		// Token: 0x0600013D RID: 317 RVA: 0x0000743C File Offset: 0x0000563C
		internal ShutdownResult Shutdown(byte[] data, int start, int length, bool force)
		{
			object shutdownLock = this._shutdownLock;
			ShutdownResult shutdownResult;
			lock (shutdownLock)
			{
				if (this._connectionState == ConnectionState.Disconnected || this._connectionState == ConnectionState.ShutdownRequested)
				{
					shutdownResult = ShutdownResult.None;
				}
				else
				{
					ShutdownResult shutdownResult2 = ((this._connectionState == ConnectionState.Connected) ? ShutdownResult.WasConnected : ShutdownResult.Success);
					if (force)
					{
						this._connectionState = ConnectionState.Disconnected;
						shutdownResult = shutdownResult2;
					}
					else
					{
						Interlocked.Exchange(ref this._timeSinceLastPacket, 0f);
						this._shutdownPacket = new NetPacket(PacketProperty.Disconnect, length)
						{
							ConnectionNumber = this._connectNum
						};
						FastBitConverter.GetBytes(this._shutdownPacket.RawData, 1, this._connectTime);
						if (this._shutdownPacket.Size >= this._mtu)
						{
							NetDebug.WriteError("[Peer] Disconnect additional data size more than MTU - 8!");
						}
						else if (data != null && length > 0)
						{
							Buffer.BlockCopy(data, start, this._shutdownPacket.RawData, 9, length);
						}
						this._connectionState = ConnectionState.ShutdownRequested;
						this.NetManager.SendRaw(this._shutdownPacket, this);
						shutdownResult = shutdownResult2;
					}
				}
			}
			return shutdownResult;
		}

		// Token: 0x0600013E RID: 318 RVA: 0x0000754C File Offset: 0x0000574C
		private void UpdateRoundTripTime(int roundTripTime)
		{
			this._rtt += roundTripTime;
			this._rttCount++;
			this._avgRtt = this._rtt / this._rttCount;
			this._resendDelay = 25.0 + (double)this._avgRtt * 2.1;
		}

		// Token: 0x0600013F RID: 319 RVA: 0x000075AC File Offset: 0x000057AC
		internal void AddReliablePacket(DeliveryMethod method, NetPacket p)
		{
			if (!p.IsFragmented)
			{
				this.NetManager.CreateReceiveEvent(p, method, p.ChannelId / 4, 4, this);
				return;
			}
			ushort fragmentId = p.FragmentId;
			byte channelId = p.ChannelId;
			NetPeer.IncomingFragments incomingFragments;
			if (!this._holdedFragments.TryGetValue(fragmentId, out incomingFragments))
			{
				incomingFragments = new NetPeer.IncomingFragments
				{
					Fragments = new NetPacket[(int)p.FragmentsTotal],
					ChannelId = p.ChannelId
				};
				this._holdedFragments.Add(fragmentId, incomingFragments);
			}
			NetPacket[] fragments = incomingFragments.Fragments;
			if ((int)p.FragmentPart >= fragments.Length || fragments[(int)p.FragmentPart] != null || p.ChannelId != incomingFragments.ChannelId)
			{
				this.NetManager.PoolRecycle(p);
				NetDebug.WriteError("Invalid fragment packet");
				return;
			}
			fragments[(int)p.FragmentPart] = p;
			incomingFragments.ReceivedCount++;
			incomingFragments.TotalSize += p.Size - 10;
			if (incomingFragments.ReceivedCount != fragments.Length)
			{
				return;
			}
			NetPacket netPacket = this.NetManager.PoolGetPacket(incomingFragments.TotalSize);
			int num = 0;
			for (int i = 0; i < incomingFragments.ReceivedCount; i++)
			{
				NetPacket netPacket2 = fragments[i];
				int num2 = netPacket2.Size - 10;
				if (num + num2 > netPacket.RawData.Length)
				{
					this._holdedFragments.Remove(fragmentId);
					NetDebug.WriteError(string.Format("Fragment error pos: {0} >= resultPacketSize: {1} , totalSize: {2}", num + num2, netPacket.RawData.Length, incomingFragments.TotalSize));
					return;
				}
				if (netPacket2.Size > netPacket2.RawData.Length)
				{
					this._holdedFragments.Remove(fragmentId);
					NetDebug.WriteError(string.Format("Fragment error size: {0} > fragment.RawData.Length: {1}", netPacket2.Size, netPacket2.RawData.Length));
					return;
				}
				Buffer.BlockCopy(netPacket2.RawData, 10, netPacket.RawData, num, num2);
				num += num2;
				this.NetManager.PoolRecycle(netPacket2);
				fragments[i] = null;
			}
			this._holdedFragments.Remove(fragmentId);
			this.NetManager.CreateReceiveEvent(netPacket, method, channelId / 4, 0, this);
		}

		// Token: 0x06000140 RID: 320 RVA: 0x000077D4 File Offset: 0x000059D4
		private void ProcessMtuPacket(NetPacket packet)
		{
			if (packet.Size < NetConstants.PossibleMtu[0])
			{
				return;
			}
			int num = BitConverter.ToInt32(packet.RawData, 1);
			int num2 = BitConverter.ToInt32(packet.RawData, packet.Size - 4);
			if (num != packet.Size || num != num2 || num > NetConstants.MaxPacketSize)
			{
				NetDebug.WriteError(string.Format("[MTU] Broken packet. RMTU {0}, EMTU {1}, PSIZE {2}", num, num2, packet.Size));
				return;
			}
			if (packet.Property == PacketProperty.MtuCheck)
			{
				this._mtuCheckAttempts = 0;
				packet.Property = PacketProperty.MtuOk;
				this.NetManager.SendRawAndRecycle(packet, this);
				return;
			}
			if (num > this._mtu && !this._finishMtu)
			{
				if (num != NetConstants.PossibleMtu[this._mtuIdx + 1] - this.NetManager.ExtraPacketSizeForLayer)
				{
					return;
				}
				object mtuMutex = this._mtuMutex;
				lock (mtuMutex)
				{
					this.SetMtu(this._mtuIdx + 1);
				}
				if (this._mtuIdx == NetConstants.PossibleMtu.Length - 1)
				{
					this._finishMtu = true;
				}
				this.NetManager.PoolRecycle(packet);
			}
		}

		// Token: 0x06000141 RID: 321 RVA: 0x00007904 File Offset: 0x00005B04
		private void UpdateMtuLogic(float deltaTime)
		{
			if (this._finishMtu)
			{
				return;
			}
			this._mtuCheckTimer += deltaTime;
			if (this._mtuCheckTimer < 1000f)
			{
				return;
			}
			this._mtuCheckTimer = 0f;
			this._mtuCheckAttempts++;
			if (this._mtuCheckAttempts >= 4)
			{
				this._finishMtu = true;
				return;
			}
			object mtuMutex = this._mtuMutex;
			lock (mtuMutex)
			{
				if (this._mtuIdx < NetConstants.PossibleMtu.Length - 1)
				{
					int num = NetConstants.PossibleMtu[this._mtuIdx + 1] - this.NetManager.ExtraPacketSizeForLayer;
					NetPacket netPacket = this.NetManager.PoolGetPacket(num);
					netPacket.Property = PacketProperty.MtuCheck;
					FastBitConverter.GetBytes(netPacket.RawData, 1, num);
					FastBitConverter.GetBytes(netPacket.RawData, netPacket.Size - 4, num);
					if (this.NetManager.SendRawAndRecycle(netPacket, this) <= 0)
					{
						this._finishMtu = true;
					}
				}
			}
		}

		// Token: 0x06000142 RID: 322 RVA: 0x00007A08 File Offset: 0x00005C08
		internal ConnectRequestResult ProcessConnectRequest(NetConnectRequestPacket connRequest)
		{
			ConnectionState connectionState = this._connectionState;
			if (connectionState <= ConnectionState.Connected)
			{
				if (connectionState != ConnectionState.Outgoing)
				{
					if (connectionState == ConnectionState.Connected)
					{
						if (connRequest.ConnectionTime == this._connectTime)
						{
							this.NetManager.SendRaw(this._connectAcceptPacket, this);
						}
						else if (connRequest.ConnectionTime > this._connectTime)
						{
							return ConnectRequestResult.Reconnection;
						}
					}
				}
				else
				{
					if (connRequest.ConnectionTime < this._connectTime)
					{
						return ConnectRequestResult.P2PLose;
					}
					if (connRequest.ConnectionTime == this._connectTime)
					{
						byte[] targetAddress = connRequest.TargetAddress;
						for (int i = this._cachedSocketAddr.Size - 1; i >= 0; i--)
						{
							byte b = this._cachedSocketAddr[i];
							if (b != targetAddress[i] && b < targetAddress[i])
							{
								return ConnectRequestResult.P2PLose;
							}
						}
					}
				}
			}
			else if (connectionState == ConnectionState.ShutdownRequested || connectionState == ConnectionState.Disconnected)
			{
				if (connRequest.ConnectionTime >= this._connectTime)
				{
					return ConnectRequestResult.NewConnection;
				}
			}
			return ConnectRequestResult.None;
		}

		// Token: 0x06000143 RID: 323 RVA: 0x00007AE0 File Offset: 0x00005CE0
		internal void ProcessPacket(NetPacket packet)
		{
			if (this._connectionState == ConnectionState.Outgoing || this._connectionState == ConnectionState.Disconnected)
			{
				this.NetManager.PoolRecycle(packet);
				return;
			}
			if (packet.Property == PacketProperty.ShutdownOk)
			{
				if (this._connectionState == ConnectionState.ShutdownRequested)
				{
					this._connectionState = ConnectionState.Disconnected;
				}
				this.NetManager.PoolRecycle(packet);
				return;
			}
			if (packet.ConnectionNumber != this._connectNum)
			{
				this.NetManager.PoolRecycle(packet);
				return;
			}
			Interlocked.Exchange(ref this._timeSinceLastPacket, 0f);
			switch (packet.Property)
			{
			case PacketProperty.Unreliable:
				this.NetManager.CreateReceiveEvent(packet, DeliveryMethod.Unreliable, 0, 1, this);
				return;
			case PacketProperty.Channeled:
			case PacketProperty.Ack:
			{
				if ((int)packet.ChannelId >= this._channels.Length)
				{
					this.NetManager.PoolRecycle(packet);
					return;
				}
				BaseChannel baseChannel = this._channels[(int)packet.ChannelId] ?? ((packet.Property == PacketProperty.Ack) ? null : this.CreateChannel(packet.ChannelId));
				if (baseChannel != null && !baseChannel.ProcessPacket(packet))
				{
					this.NetManager.PoolRecycle(packet);
					return;
				}
				return;
			}
			case PacketProperty.Ping:
				if (NetUtils.RelativeSequenceNumber((int)packet.Sequence, (int)this._pongPacket.Sequence) > 0)
				{
					FastBitConverter.GetBytes(this._pongPacket.RawData, 3, DateTime.UtcNow.Ticks);
					this._pongPacket.Sequence = packet.Sequence;
					this.NetManager.SendRaw(this._pongPacket, this);
				}
				this.NetManager.PoolRecycle(packet);
				return;
			case PacketProperty.Pong:
				if (packet.Sequence == this._pingPacket.Sequence)
				{
					this._pingTimer.Stop();
					int num = (int)this._pingTimer.ElapsedMilliseconds;
					this._remoteDelta = BitConverter.ToInt64(packet.RawData, 3) + (long)num * 10000L / 2L - DateTime.UtcNow.Ticks;
					this.UpdateRoundTripTime(num);
					this.NetManager.ConnectionLatencyUpdated(this, num / 2);
				}
				this.NetManager.PoolRecycle(packet);
				return;
			case PacketProperty.MtuCheck:
			case PacketProperty.MtuOk:
				this.ProcessMtuPacket(packet);
				return;
			case PacketProperty.Merged:
			{
				int i = 1;
				while (i < packet.Size)
				{
					ushort num2 = BitConverter.ToUInt16(packet.RawData, i);
					if (num2 == 0)
					{
						break;
					}
					i += 2;
					if (packet.RawData.Length - i < (int)num2)
					{
						break;
					}
					NetPacket netPacket = this.NetManager.PoolGetPacket((int)num2);
					Buffer.BlockCopy(packet.RawData, i, netPacket.RawData, 0, (int)num2);
					netPacket.Size = (int)num2;
					if (!netPacket.Verify())
					{
						break;
					}
					i += (int)num2;
					this.ProcessPacket(netPacket);
				}
				this.NetManager.PoolRecycle(packet);
				return;
			}
			}
			NetDebug.WriteError("Error! Unexpected packet type: " + packet.Property.ToString());
		}

		// Token: 0x06000144 RID: 324 RVA: 0x00007DA8 File Offset: 0x00005FA8
		private void SendMerged()
		{
			if (this._mergeCount == 0)
			{
				return;
			}
			int num;
			if (this._mergeCount > 1)
			{
				num = this.NetManager.SendRaw(this._mergeData.RawData, 0, 1 + this._mergePos, this);
			}
			else
			{
				num = this.NetManager.SendRaw(this._mergeData.RawData, 3, this._mergePos - 2, this);
			}
			if (this.NetManager.EnableStatistics)
			{
				this.Statistics.IncrementPacketsSent();
				this.Statistics.AddBytesSent((long)num);
			}
			this._mergePos = 0;
			this._mergeCount = 0;
		}

		// Token: 0x06000145 RID: 325 RVA: 0x00007E40 File Offset: 0x00006040
		internal void SendUserData(NetPacket packet)
		{
			packet.ConnectionNumber = this._connectNum;
			int num = 1 + packet.Size + 2;
			if (num + 20 >= this._mtu)
			{
				int num2 = this.NetManager.SendRaw(packet, this);
				if (this.NetManager.EnableStatistics)
				{
					this.Statistics.IncrementPacketsSent();
					this.Statistics.AddBytesSent((long)num2);
				}
				return;
			}
			if (this._mergePos + num > this._mtu)
			{
				this.SendMerged();
			}
			FastBitConverter.GetBytes(this._mergeData.RawData, this._mergePos + 1, (ushort)packet.Size);
			Buffer.BlockCopy(packet.RawData, 0, this._mergeData.RawData, this._mergePos + 1 + 2, packet.Size);
			this._mergePos += packet.Size + 2;
			this._mergeCount++;
		}

		// Token: 0x06000146 RID: 326 RVA: 0x00007F24 File Offset: 0x00006124
		internal void Update(float deltaTime)
		{
			this._timeSinceLastPacket += deltaTime;
			ConnectionState connectionState = this._connectionState;
			if (connectionState <= ConnectionState.Connected)
			{
				if (connectionState == ConnectionState.Outgoing)
				{
					this._connectTimer += deltaTime;
					if (this._connectTimer > (float)this.NetManager.ReconnectDelay)
					{
						this._connectTimer = 0f;
						this._connectAttempts++;
						if (this._connectAttempts > this.NetManager.MaxConnectAttempts)
						{
							this.NetManager.DisconnectPeerForce(this, DisconnectReason.ConnectionFailed, SocketError.Success, null);
							return;
						}
						this.NetManager.SendRaw(this._connectRequestPacket, this);
					}
					return;
				}
				if (connectionState == ConnectionState.Connected)
				{
					if (this._timeSinceLastPacket > (float)this.NetManager.DisconnectTimeout)
					{
						this.NetManager.DisconnectPeerForce(this, DisconnectReason.Timeout, SocketError.Success, null);
						return;
					}
				}
			}
			else if (connectionState != ConnectionState.ShutdownRequested)
			{
				if (connectionState == ConnectionState.Disconnected)
				{
					return;
				}
			}
			else
			{
				if (this._timeSinceLastPacket > (float)this.NetManager.DisconnectTimeout)
				{
					this._connectionState = ConnectionState.Disconnected;
					return;
				}
				this._shutdownTimer += deltaTime;
				if (this._shutdownTimer >= 300f)
				{
					this._shutdownTimer = 0f;
					this.NetManager.SendRaw(this._shutdownPacket, this);
				}
				return;
			}
			this._pingSendTimer += deltaTime;
			if (this._pingSendTimer >= (float)this.NetManager.PingInterval)
			{
				this._pingSendTimer = 0f;
				NetPacket pingPacket = this._pingPacket;
				ushort sequence = pingPacket.Sequence;
				pingPacket.Sequence = sequence + 1;
				if (this._pingTimer.IsRunning)
				{
					this.UpdateRoundTripTime((int)this._pingTimer.ElapsedMilliseconds);
				}
				this._pingTimer.Restart();
				this.NetManager.SendRaw(this._pingPacket, this);
			}
			this._rttResetTimer += deltaTime;
			if (this._rttResetTimer >= (float)(this.NetManager.PingInterval * 3))
			{
				this._rttResetTimer = 0f;
				this._rtt = this._avgRtt;
				this._rttCount = 1;
			}
			this.UpdateMtuLogic(deltaTime);
			int count = this._channelSendQueue.Count;
			BaseChannel baseChannel;
			while (count-- > 0 && this._channelSendQueue.TryDequeue(out baseChannel))
			{
				if (baseChannel.SendAndCheckQueue())
				{
					this._channelSendQueue.Enqueue(baseChannel);
				}
			}
			if (this._unreliablePendingCount > 0)
			{
				object unreliableChannelLock = this._unreliableChannelLock;
				int unreliablePendingCount;
				lock (unreliableChannelLock)
				{
					NetPacket[] unreliableSecondQueue = this._unreliableSecondQueue;
					NetPacket[] unreliableChannel = this._unreliableChannel;
					this._unreliableChannel = unreliableSecondQueue;
					this._unreliableSecondQueue = unreliableChannel;
					unreliablePendingCount = this._unreliablePendingCount;
					this._unreliablePendingCount = 0;
				}
				for (int i = 0; i < unreliablePendingCount; i++)
				{
					NetPacket netPacket = this._unreliableSecondQueue[i];
					this.SendUserData(netPacket);
					this.NetManager.PoolRecycle(netPacket);
				}
			}
			this.SendMerged();
		}

		// Token: 0x06000147 RID: 327 RVA: 0x0000820C File Offset: 0x0000640C
		internal void RecycleAndDeliver(NetPacket packet)
		{
			if (packet.UserData != null)
			{
				if (packet.IsFragmented)
				{
					ushort num;
					this._deliveredFragments.TryGetValue(packet.FragmentId, out num);
					num += 1;
					if (num == packet.FragmentsTotal)
					{
						this.NetManager.MessageDelivered(this, packet.UserData);
						this._deliveredFragments.Remove(packet.FragmentId);
					}
					else
					{
						this._deliveredFragments[packet.FragmentId] = num;
					}
				}
				else
				{
					this.NetManager.MessageDelivered(this, packet.UserData);
				}
				packet.UserData = null;
			}
			this.NetManager.PoolRecycle(packet);
		}

		// Token: 0x040000FD RID: 253
		private int _rtt;

		// Token: 0x040000FE RID: 254
		private int _avgRtt;

		// Token: 0x040000FF RID: 255
		private int _rttCount;

		// Token: 0x04000100 RID: 256
		private double _resendDelay = 27.0;

		// Token: 0x04000101 RID: 257
		private float _pingSendTimer;

		// Token: 0x04000102 RID: 258
		private float _rttResetTimer;

		// Token: 0x04000103 RID: 259
		private readonly Stopwatch _pingTimer = new Stopwatch();

		// Token: 0x04000104 RID: 260
		private volatile float _timeSinceLastPacket;

		// Token: 0x04000105 RID: 261
		private long _remoteDelta;

		// Token: 0x04000106 RID: 262
		private readonly object _shutdownLock = new object();

		// Token: 0x04000107 RID: 263
		internal volatile NetPeer NextPeer;

		// Token: 0x04000108 RID: 264
		internal NetPeer PrevPeer;

		// Token: 0x04000109 RID: 265
		private NetPacket[] _unreliableSecondQueue;

		// Token: 0x0400010A RID: 266
		private NetPacket[] _unreliableChannel;

		// Token: 0x0400010B RID: 267
		private int _unreliablePendingCount;

		// Token: 0x0400010C RID: 268
		private readonly object _unreliableChannelLock = new object();

		// Token: 0x0400010D RID: 269
		private readonly ConcurrentQueue<BaseChannel> _channelSendQueue;

		// Token: 0x0400010E RID: 270
		private readonly BaseChannel[] _channels;

		// Token: 0x0400010F RID: 271
		private int _mtu;

		// Token: 0x04000110 RID: 272
		private int _mtuIdx;

		// Token: 0x04000111 RID: 273
		private bool _finishMtu;

		// Token: 0x04000112 RID: 274
		private float _mtuCheckTimer;

		// Token: 0x04000113 RID: 275
		private int _mtuCheckAttempts;

		// Token: 0x04000114 RID: 276
		private const int MtuCheckDelay = 1000;

		// Token: 0x04000115 RID: 277
		private const int MaxMtuCheckAttempts = 4;

		// Token: 0x04000116 RID: 278
		private readonly object _mtuMutex = new object();

		// Token: 0x04000117 RID: 279
		private int _fragmentId;

		// Token: 0x04000118 RID: 280
		private readonly Dictionary<ushort, NetPeer.IncomingFragments> _holdedFragments;

		// Token: 0x04000119 RID: 281
		private readonly Dictionary<ushort, ushort> _deliveredFragments;

		// Token: 0x0400011A RID: 282
		private readonly NetPacket _mergeData;

		// Token: 0x0400011B RID: 283
		private int _mergePos;

		// Token: 0x0400011C RID: 284
		private int _mergeCount;

		// Token: 0x0400011D RID: 285
		private int _connectAttempts;

		// Token: 0x0400011E RID: 286
		private float _connectTimer;

		// Token: 0x0400011F RID: 287
		private long _connectTime;

		// Token: 0x04000120 RID: 288
		private byte _connectNum;

		// Token: 0x04000121 RID: 289
		private ConnectionState _connectionState;

		// Token: 0x04000122 RID: 290
		private NetPacket _shutdownPacket;

		// Token: 0x04000123 RID: 291
		private const int ShutdownDelay = 300;

		// Token: 0x04000124 RID: 292
		private float _shutdownTimer;

		// Token: 0x04000125 RID: 293
		private readonly NetPacket _pingPacket;

		// Token: 0x04000126 RID: 294
		private readonly NetPacket _pongPacket;

		// Token: 0x04000127 RID: 295
		private readonly NetPacket _connectRequestPacket;

		// Token: 0x04000128 RID: 296
		private readonly NetPacket _connectAcceptPacket;

		// Token: 0x04000129 RID: 297
		public readonly NetManager NetManager;

		// Token: 0x0400012A RID: 298
		public readonly int Id;

		// Token: 0x0400012C RID: 300
		public object Tag;

		// Token: 0x0400012D RID: 301
		public readonly NetStatistics Statistics;

		// Token: 0x0400012E RID: 302
		private SocketAddress _cachedSocketAddr;

		// Token: 0x0400012F RID: 303
		private int _cachedHashCode;

		// Token: 0x04000130 RID: 304
		internal byte[] NativeAddress;

		// Token: 0x0200008A RID: 138
		private class IncomingFragments
		{
			// Token: 0x040002DB RID: 731
			public NetPacket[] Fragments;

			// Token: 0x040002DC RID: 732
			public int ReceivedCount;

			// Token: 0x040002DD RID: 733
			public int TotalSize;

			// Token: 0x040002DE RID: 734
			public byte ChannelId;
		}
	}
}
