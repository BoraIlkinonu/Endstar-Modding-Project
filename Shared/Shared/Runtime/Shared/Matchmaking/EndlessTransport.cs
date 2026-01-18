using System;
using System.Threading.Tasks;
using MatchmakingClientSDK;
using Unity.Netcode;
using UnityEngine;

namespace Runtime.Shared.Matchmaking
{
	// Token: 0x02000010 RID: 16
	public class EndlessTransport : NetworkTransport
	{
		// Token: 0x17000013 RID: 19
		// (get) Token: 0x06000081 RID: 129 RVA: 0x00005050 File Offset: 0x00003250
		public override ulong ServerClientId
		{
			get
			{
				return 0UL;
			}
		}

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x06000082 RID: 130 RVA: 0x000043C6 File Offset: 0x000025C6
		public bool IsServer
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x06000083 RID: 131 RVA: 0x00005054 File Offset: 0x00003254
		// (set) Token: 0x06000084 RID: 132 RVA: 0x0000505C File Offset: 0x0000325C
		public string UserId { get; private set; }

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x06000085 RID: 133 RVA: 0x00005065 File Offset: 0x00003265
		// (set) Token: 0x06000086 RID: 134 RVA: 0x0000506D File Offset: 0x0000326D
		public ulong ClientId { get; private set; }

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x06000087 RID: 135 RVA: 0x00005076 File Offset: 0x00003276
		// (set) Token: 0x06000088 RID: 136 RVA: 0x0000507E File Offset: 0x0000327E
		public string Token { get; private set; }

		// Token: 0x06000089 RID: 137 RVA: 0x00005087 File Offset: 0x00003287
		public void Setup(string userId, string token, AllocationData allocationData, bool useRelay)
		{
			this.UserId = userId;
			this.ClientId = ulong.Parse(userId);
			this.Token = token;
			this.allocationData = allocationData;
			this.useRelay = useRelay;
		}

		// Token: 0x0600008A RID: 138 RVA: 0x000050B2 File Offset: 0x000032B2
		public override void Initialize(NetworkManager networkManager = null)
		{
			this.networkManager = networkManager;
		}

		// Token: 0x0600008B RID: 139 RVA: 0x000050BB File Offset: 0x000032BB
		private void Update()
		{
		}

		// Token: 0x0600008C RID: 140 RVA: 0x000050BD File Offset: 0x000032BD
		public override NetworkEvent PollEvent(out ulong clientId, out ArraySegment<byte> payload, out float receiveTime)
		{
			clientId = 0UL;
			receiveTime = Time.realtimeSinceStartup;
			payload = default(ArraySegment<byte>);
			return NetworkEvent.Nothing;
		}

		// Token: 0x0600008D RID: 141 RVA: 0x000050D2 File Offset: 0x000032D2
		public override bool StartClient()
		{
			return true;
		}

		// Token: 0x0600008E RID: 142 RVA: 0x000050D2 File Offset: 0x000032D2
		public override bool StartServer()
		{
			return true;
		}

		// Token: 0x0600008F RID: 143 RVA: 0x000050D5 File Offset: 0x000032D5
		private Task<string> Authenticate(string key)
		{
			return Task.FromResult<string>(key);
		}

		// Token: 0x06000090 RID: 144 RVA: 0x000050E0 File Offset: 0x000032E0
		public override void Send(ulong targetClientId, ArraySegment<byte> payload, NetworkDelivery networkDelivery)
		{
			switch (networkDelivery)
			{
			case NetworkDelivery.Unreliable:
				break;
			case NetworkDelivery.UnreliableSequenced:
				break;
			case NetworkDelivery.Reliable:
				break;
			case NetworkDelivery.ReliableSequenced:
				break;
			case NetworkDelivery.ReliableFragmentedSequenced:
				break;
			default:
				throw new ArgumentOutOfRangeException("networkDelivery", networkDelivery, null);
			}
			bool isServer = this.IsServer;
		}

		// Token: 0x06000091 RID: 145 RVA: 0x000050BB File Offset: 0x000032BB
		public override void DisconnectRemoteClient(ulong clientId)
		{
		}

		// Token: 0x06000092 RID: 146 RVA: 0x000050BB File Offset: 0x000032BB
		public override void DisconnectLocalClient()
		{
		}

		// Token: 0x06000093 RID: 147 RVA: 0x00005050 File Offset: 0x00003250
		public override ulong GetCurrentRtt(ulong clientId)
		{
			return 0UL;
		}

		// Token: 0x06000094 RID: 148 RVA: 0x000050BB File Offset: 0x000032BB
		public override void Shutdown()
		{
		}

		// Token: 0x06000095 RID: 149 RVA: 0x00005136 File Offset: 0x00003336
		private void OnDestroy()
		{
			this.Shutdown();
		}

		// Token: 0x04000025 RID: 37
		private NetworkManager networkManager;

		// Token: 0x04000026 RID: 38
		private AllocationData allocationData;

		// Token: 0x04000027 RID: 39
		private bool useRelay;
	}
}
