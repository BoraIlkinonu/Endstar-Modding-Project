using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Networking.UDP.Utils;

namespace Networking.UDP
{
	// Token: 0x0200001B RID: 27
	public sealed class NatPunchModule
	{
		// Token: 0x0600006C RID: 108 RVA: 0x0000302C File Offset: 0x0000122C
		internal NatPunchModule(NetManager socket)
		{
			this._socket = socket;
			this._netPacketProcessor.SubscribeReusable<NatPunchModule.NatIntroduceResponsePacket>(new Action<NatPunchModule.NatIntroduceResponsePacket>(this.OnNatIntroductionResponse));
			this._netPacketProcessor.SubscribeReusable<NatPunchModule.NatIntroduceRequestPacket, IPEndPoint>(new Action<NatPunchModule.NatIntroduceRequestPacket, IPEndPoint>(this.OnNatIntroductionRequest));
			this._netPacketProcessor.SubscribeReusable<NatPunchModule.NatPunchPacket, IPEndPoint>(new Action<NatPunchModule.NatPunchPacket, IPEndPoint>(this.OnNatPunch));
		}

		// Token: 0x0600006D RID: 109 RVA: 0x000030C8 File Offset: 0x000012C8
		internal void ProcessMessage(IPEndPoint senderEndPoint, NetPacket packet)
		{
			NetDataReader cacheReader = this._cacheReader;
			lock (cacheReader)
			{
				this._cacheReader.SetSource(packet.RawData, 1, packet.Size);
				this._netPacketProcessor.ReadAllPackets(this._cacheReader, senderEndPoint);
			}
		}

		// Token: 0x0600006E RID: 110 RVA: 0x0000312C File Offset: 0x0000132C
		public void Init(INatPunchListener listener)
		{
			this._natPunchListener = listener;
		}

		// Token: 0x0600006F RID: 111 RVA: 0x00003138 File Offset: 0x00001338
		private void Send<T>(T packet, IPEndPoint target) where T : class, new()
		{
			this._cacheWriter.Reset();
			this._cacheWriter.Put(16);
			this._netPacketProcessor.Write<T>(this._cacheWriter, packet);
			this._socket.SendRaw(this._cacheWriter.Data, 0, this._cacheWriter.Length, target);
		}

		// Token: 0x06000070 RID: 112 RVA: 0x00003194 File Offset: 0x00001394
		public void NatIntroduce(IPEndPoint hostInternal, IPEndPoint hostExternal, IPEndPoint clientInternal, IPEndPoint clientExternal, string additionalInfo)
		{
			NatPunchModule.NatIntroduceResponsePacket natIntroduceResponsePacket = new NatPunchModule.NatIntroduceResponsePacket
			{
				Token = additionalInfo
			};
			natIntroduceResponsePacket.Internal = hostInternal;
			natIntroduceResponsePacket.External = hostExternal;
			this.Send<NatPunchModule.NatIntroduceResponsePacket>(natIntroduceResponsePacket, clientExternal);
			natIntroduceResponsePacket.Internal = clientInternal;
			natIntroduceResponsePacket.External = clientExternal;
			this.Send<NatPunchModule.NatIntroduceResponsePacket>(natIntroduceResponsePacket, hostExternal);
		}

		// Token: 0x06000071 RID: 113 RVA: 0x000031E0 File Offset: 0x000013E0
		public void PollEvents()
		{
			if (this.UnsyncedEvents)
			{
				return;
			}
			if (this._natPunchListener == null || (this._successEvents.IsEmpty && this._requestEvents.IsEmpty))
			{
				return;
			}
			NatPunchModule.SuccessEventData successEventData;
			while (this._successEvents.TryDequeue(out successEventData))
			{
				this._natPunchListener.OnNatIntroductionSuccess(successEventData.TargetEndPoint, successEventData.Type, successEventData.Token);
			}
			NatPunchModule.RequestEventData requestEventData;
			while (this._requestEvents.TryDequeue(out requestEventData))
			{
				this._natPunchListener.OnNatIntroductionRequest(requestEventData.LocalEndPoint, requestEventData.RemoteEndPoint, requestEventData.Token);
			}
		}

		// Token: 0x06000072 RID: 114 RVA: 0x00003275 File Offset: 0x00001475
		public void SendNatIntroduceRequest(string host, int port, string additionalInfo)
		{
			this.SendNatIntroduceRequest(NetUtils.MakeEndPoint(host, port), additionalInfo);
		}

		// Token: 0x06000073 RID: 115 RVA: 0x00003288 File Offset: 0x00001488
		public void SendNatIntroduceRequest(IPEndPoint masterServerEndPoint, string additionalInfo)
		{
			string text = NetUtils.GetLocalIp(LocalAddrType.IPv4);
			if (string.IsNullOrEmpty(text) || masterServerEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
			{
				text = NetUtils.GetLocalIp(LocalAddrType.IPv6);
			}
			this.Send<NatPunchModule.NatIntroduceRequestPacket>(new NatPunchModule.NatIntroduceRequestPacket
			{
				Internal = NetUtils.MakeEndPoint(text, this._socket.LocalPort),
				Token = additionalInfo
			}, masterServerEndPoint);
		}

		// Token: 0x06000074 RID: 116 RVA: 0x000032E0 File Offset: 0x000014E0
		private void OnNatIntroductionRequest(NatPunchModule.NatIntroduceRequestPacket req, IPEndPoint senderEndPoint)
		{
			if (this.UnsyncedEvents)
			{
				this._natPunchListener.OnNatIntroductionRequest(req.Internal, senderEndPoint, req.Token);
				return;
			}
			this._requestEvents.Enqueue(new NatPunchModule.RequestEventData
			{
				LocalEndPoint = req.Internal,
				RemoteEndPoint = senderEndPoint,
				Token = req.Token
			});
		}

		// Token: 0x06000075 RID: 117 RVA: 0x00003344 File Offset: 0x00001544
		private void OnNatIntroductionResponse(NatPunchModule.NatIntroduceResponsePacket req)
		{
			NatPunchModule.NatPunchPacket natPunchPacket = new NatPunchModule.NatPunchPacket
			{
				Token = req.Token
			};
			this.Send<NatPunchModule.NatPunchPacket>(natPunchPacket, req.Internal);
			this._socket.Ttl = 2;
			this._socket.SendRaw(new byte[] { 17 }, 0, 1, req.External);
			this._socket.Ttl = 255;
			natPunchPacket.IsExternal = true;
			this.Send<NatPunchModule.NatPunchPacket>(natPunchPacket, req.External);
		}

		// Token: 0x06000076 RID: 118 RVA: 0x000033C0 File Offset: 0x000015C0
		private void OnNatPunch(NatPunchModule.NatPunchPacket req, IPEndPoint senderEndPoint)
		{
			if (this.UnsyncedEvents)
			{
				this._natPunchListener.OnNatIntroductionSuccess(senderEndPoint, req.IsExternal ? NatAddressType.External : NatAddressType.Internal, req.Token);
				return;
			}
			this._successEvents.Enqueue(new NatPunchModule.SuccessEventData
			{
				TargetEndPoint = senderEndPoint,
				Type = (req.IsExternal ? NatAddressType.External : NatAddressType.Internal),
				Token = req.Token
			});
		}

		// Token: 0x0400004B RID: 75
		private readonly NetManager _socket;

		// Token: 0x0400004C RID: 76
		private readonly ConcurrentQueue<NatPunchModule.RequestEventData> _requestEvents = new ConcurrentQueue<NatPunchModule.RequestEventData>();

		// Token: 0x0400004D RID: 77
		private readonly ConcurrentQueue<NatPunchModule.SuccessEventData> _successEvents = new ConcurrentQueue<NatPunchModule.SuccessEventData>();

		// Token: 0x0400004E RID: 78
		private readonly NetDataReader _cacheReader = new NetDataReader();

		// Token: 0x0400004F RID: 79
		private readonly NetDataWriter _cacheWriter = new NetDataWriter();

		// Token: 0x04000050 RID: 80
		private readonly NetPacketProcessor _netPacketProcessor = new NetPacketProcessor(256);

		// Token: 0x04000051 RID: 81
		private INatPunchListener _natPunchListener;

		// Token: 0x04000052 RID: 82
		public const int MaxTokenLength = 256;

		// Token: 0x04000053 RID: 83
		public bool UnsyncedEvents;

		// Token: 0x02000080 RID: 128
		private struct RequestEventData
		{
			// Token: 0x040002B8 RID: 696
			public IPEndPoint LocalEndPoint;

			// Token: 0x040002B9 RID: 697
			public IPEndPoint RemoteEndPoint;

			// Token: 0x040002BA RID: 698
			public string Token;
		}

		// Token: 0x02000081 RID: 129
		private struct SuccessEventData
		{
			// Token: 0x040002BB RID: 699
			public IPEndPoint TargetEndPoint;

			// Token: 0x040002BC RID: 700
			public NatAddressType Type;

			// Token: 0x040002BD RID: 701
			public string Token;
		}

		// Token: 0x02000082 RID: 130
		private class NatIntroduceRequestPacket
		{
			// Token: 0x17000077 RID: 119
			// (get) Token: 0x06000467 RID: 1127 RVA: 0x00012990 File Offset: 0x00010B90
			// (set) Token: 0x06000468 RID: 1128 RVA: 0x00012998 File Offset: 0x00010B98
			public IPEndPoint Internal
			{
				[Preserve]
				get;
				[Preserve]
				set;
			}

			// Token: 0x17000078 RID: 120
			// (get) Token: 0x06000469 RID: 1129 RVA: 0x000129A1 File Offset: 0x00010BA1
			// (set) Token: 0x0600046A RID: 1130 RVA: 0x000129A9 File Offset: 0x00010BA9
			public string Token
			{
				[Preserve]
				get;
				[Preserve]
				set;
			}
		}

		// Token: 0x02000083 RID: 131
		private class NatIntroduceResponsePacket
		{
			// Token: 0x17000079 RID: 121
			// (get) Token: 0x0600046C RID: 1132 RVA: 0x000129BA File Offset: 0x00010BBA
			// (set) Token: 0x0600046D RID: 1133 RVA: 0x000129C2 File Offset: 0x00010BC2
			public IPEndPoint Internal
			{
				[Preserve]
				get;
				[Preserve]
				set;
			}

			// Token: 0x1700007A RID: 122
			// (get) Token: 0x0600046E RID: 1134 RVA: 0x000129CB File Offset: 0x00010BCB
			// (set) Token: 0x0600046F RID: 1135 RVA: 0x000129D3 File Offset: 0x00010BD3
			public IPEndPoint External
			{
				[Preserve]
				get;
				[Preserve]
				set;
			}

			// Token: 0x1700007B RID: 123
			// (get) Token: 0x06000470 RID: 1136 RVA: 0x000129DC File Offset: 0x00010BDC
			// (set) Token: 0x06000471 RID: 1137 RVA: 0x000129E4 File Offset: 0x00010BE4
			public string Token
			{
				[Preserve]
				get;
				[Preserve]
				set;
			}
		}

		// Token: 0x02000084 RID: 132
		private class NatPunchPacket
		{
			// Token: 0x1700007C RID: 124
			// (get) Token: 0x06000473 RID: 1139 RVA: 0x000129F5 File Offset: 0x00010BF5
			// (set) Token: 0x06000474 RID: 1140 RVA: 0x000129FD File Offset: 0x00010BFD
			public string Token
			{
				[Preserve]
				get;
				[Preserve]
				set;
			}

			// Token: 0x1700007D RID: 125
			// (get) Token: 0x06000475 RID: 1141 RVA: 0x00012A06 File Offset: 0x00010C06
			// (set) Token: 0x06000476 RID: 1142 RVA: 0x00012A0E File Offset: 0x00010C0E
			public bool IsExternal
			{
				[Preserve]
				get;
				[Preserve]
				set;
			}
		}
	}
}
