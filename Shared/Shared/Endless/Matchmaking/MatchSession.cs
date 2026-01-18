using System;
using System.Linq;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Matchmaking
{
	// Token: 0x02000043 RID: 67
	public abstract class MatchSession : MonoBehaviour
	{
		// Token: 0x17000058 RID: 88
		// (get) Token: 0x06000253 RID: 595 RVA: 0x0000C53E File Offset: 0x0000A73E
		// (set) Token: 0x06000254 RID: 596 RVA: 0x0000C545 File Offset: 0x0000A745
		public static MatchSession Instance { get; protected set; }

		// Token: 0x14000026 RID: 38
		// (add) Token: 0x06000255 RID: 597 RVA: 0x0000C550 File Offset: 0x0000A750
		// (remove) Token: 0x06000256 RID: 598 RVA: 0x0000C584 File Offset: 0x0000A784
		public static event Action<MatchSession> OnMatchSessionInitialized;

		// Token: 0x14000027 RID: 39
		// (add) Token: 0x06000257 RID: 599 RVA: 0x0000C5B8 File Offset: 0x0000A7B8
		// (remove) Token: 0x06000258 RID: 600 RVA: 0x0000C5EC File Offset: 0x0000A7EC
		public static event Action<MatchSession> OnMatchSessionStart;

		// Token: 0x14000028 RID: 40
		// (add) Token: 0x06000259 RID: 601 RVA: 0x0000C620 File Offset: 0x0000A820
		// (remove) Token: 0x0600025A RID: 602 RVA: 0x0000C654 File Offset: 0x0000A854
		public static event Action<MatchSession> OnMatchSessionStop;

		// Token: 0x14000029 RID: 41
		// (add) Token: 0x0600025B RID: 603 RVA: 0x0000C688 File Offset: 0x0000A888
		// (remove) Token: 0x0600025C RID: 604 RVA: 0x0000C6BC File Offset: 0x0000A8BC
		public static event Action<MatchSession> OnMatchSessionClose;

		// Token: 0x1400002A RID: 42
		// (add) Token: 0x0600025D RID: 605 RVA: 0x0000C6F0 File Offset: 0x0000A8F0
		// (remove) Token: 0x0600025E RID: 606 RVA: 0x0000C724 File Offset: 0x0000A924
		public static event Action<ulong> OnClientConnected;

		// Token: 0x1400002B RID: 43
		// (add) Token: 0x0600025F RID: 607 RVA: 0x0000C758 File Offset: 0x0000A958
		// (remove) Token: 0x06000260 RID: 608 RVA: 0x0000C78C File Offset: 0x0000A98C
		public static event Action<ulong> OnClientUnexpectedDisconnection;

		// Token: 0x1400002C RID: 44
		// (add) Token: 0x06000261 RID: 609 RVA: 0x0000C7C0 File Offset: 0x0000A9C0
		// (remove) Token: 0x06000262 RID: 610 RVA: 0x0000C7F4 File Offset: 0x0000A9F4
		public static event Action<ulong> OnClientKicked;

		// Token: 0x1400002D RID: 45
		// (add) Token: 0x06000263 RID: 611 RVA: 0x0000C828 File Offset: 0x0000AA28
		// (remove) Token: 0x06000264 RID: 612 RVA: 0x0000C85C File Offset: 0x0000AA5C
		public static event Action<ulong> OnClientLeft;

		// Token: 0x17000059 RID: 89
		// (get) Token: 0x06000265 RID: 613 RVA: 0x0000C88F File Offset: 0x0000AA8F
		// (set) Token: 0x06000266 RID: 614 RVA: 0x0000C897 File Offset: 0x0000AA97
		public MatchData MatchData { get; private set; }

		// Token: 0x1700005A RID: 90
		// (get) Token: 0x06000267 RID: 615 RVA: 0x0000C8A0 File Offset: 0x0000AAA0
		// (set) Token: 0x06000268 RID: 616 RVA: 0x0000C8A8 File Offset: 0x0000AAA8
		public SerializableGuid ProjectId { get; private set; }

		// Token: 0x1700005B RID: 91
		// (get) Token: 0x06000269 RID: 617 RVA: 0x0000C8B1 File Offset: 0x0000AAB1
		// (set) Token: 0x0600026A RID: 618 RVA: 0x0000C8B9 File Offset: 0x0000AAB9
		public bool IsServer { get; protected set; }

		// Token: 0x1700005C RID: 92
		// (get) Token: 0x0600026B RID: 619 RVA: 0x0000C8C2 File Offset: 0x0000AAC2
		// (set) Token: 0x0600026C RID: 620 RVA: 0x0000C8CA File Offset: 0x0000AACA
		public bool IsClient { get; protected set; }

		// Token: 0x1700005D RID: 93
		// (get) Token: 0x0600026D RID: 621 RVA: 0x0000C8D3 File Offset: 0x0000AAD3
		public bool IsHost
		{
			get
			{
				return this.IsClient && this.IsServer;
			}
		}

		// Token: 0x0600026E RID: 622 RVA: 0x0000C8E8 File Offset: 0x0000AAE8
		public virtual void Initialize(MatchData matchData, SerializableGuid gameId)
		{
			this.MatchData = matchData;
			this.ProjectId = gameId;
			this.sessionNetworkManager = NetworkManager.Singleton;
			this.sessionNetworkManager.OnServerStarted += this.OnServerStarted;
			this.sessionNetworkManager.OnClientConnectedCallback += this.HandleClientConnected;
			this.sessionNetworkManager.OnClientDisconnectCallback += this.HandleClientDisconnected;
			Action<MatchSession> onMatchSessionInitialized = MatchSession.OnMatchSessionInitialized;
			if (onMatchSessionInitialized == null)
			{
				return;
			}
			onMatchSessionInitialized(this);
		}

		// Token: 0x0600026F RID: 623 RVA: 0x0000C963 File Offset: 0x0000AB63
		protected void OnServerStarted()
		{
			if (this.IsServer)
			{
				Action<MatchSession> onMatchSessionStart = MatchSession.OnMatchSessionStart;
				if (onMatchSessionStart == null)
				{
					return;
				}
				onMatchSessionStart(this);
			}
		}

		// Token: 0x06000270 RID: 624 RVA: 0x0000C980 File Offset: 0x0000AB80
		protected void HandleClientConnected(ulong clientId)
		{
			if (!this.IsServer && this.IsClient && this.sessionNetworkManager.LocalClientId == clientId)
			{
				Action<MatchSession> onMatchSessionStart = MatchSession.OnMatchSessionStart;
				if (onMatchSessionStart != null)
				{
					onMatchSessionStart(this);
				}
			}
			Action<ulong> onClientConnected = MatchSession.OnClientConnected;
			if (onClientConnected == null)
			{
				return;
			}
			onClientConnected(clientId);
		}

		// Token: 0x06000271 RID: 625 RVA: 0x0000C9CC File Offset: 0x0000ABCC
		protected void HandleClientDisconnected(ulong clientId)
		{
			Debug.Log(string.Format("Client {0} was notified of client {1}'s disconnection.", this.sessionNetworkManager.LocalClientId, clientId));
			Action<ulong> onClientUnexpectedDisconnection = MatchSession.OnClientUnexpectedDisconnection;
			if (onClientUnexpectedDisconnection != null)
			{
				onClientUnexpectedDisconnection(clientId);
			}
			Action<ulong> onClientLeft = MatchSession.OnClientLeft;
			if (onClientLeft == null)
			{
				return;
			}
			onClientLeft(clientId);
		}

		// Token: 0x06000272 RID: 626 RVA: 0x0000CA20 File Offset: 0x0000AC20
		public void KickClient(ulong clientId, string message = "")
		{
			if (NetworkManager.Singleton.IsServer && NetworkManager.Singleton.ConnectedClientsIds.Contains(clientId))
			{
				Debug.Log(string.Format("Kicking client {0} with message {1}", clientId, message));
				if (NetworkManager.Singleton.LocalClientId == clientId)
				{
					MonoBehaviourSingleton<UIModalManager>.Instance.DisplayGenericModal("Kicked From Match", null, message, UIModalManagerStackActions.ClearStack, Array.Empty<UIModalGenericViewAction>());
				}
				NetworkManager.Singleton.DisconnectClient(clientId);
				Action<ulong> onClientKicked = MatchSession.OnClientKicked;
				if (onClientKicked != null)
				{
					onClientKicked(clientId);
				}
				Action<ulong> onClientLeft = MatchSession.OnClientLeft;
				if (onClientLeft == null)
				{
					return;
				}
				onClientLeft(clientId);
			}
		}

		// Token: 0x06000273 RID: 627 RVA: 0x0000CAB4 File Offset: 0x0000ACB4
		protected virtual void OnDestroy()
		{
			this.sessionNetworkManager.OnServerStarted -= this.OnServerStarted;
			this.sessionNetworkManager.OnClientConnectedCallback -= this.HandleClientConnected;
			this.sessionNetworkManager.OnClientDisconnectCallback -= this.HandleClientDisconnected;
			try
			{
				Action<MatchSession> onMatchSessionClose = MatchSession.OnMatchSessionClose;
				if (onMatchSessionClose != null)
				{
					onMatchSessionClose(this);
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			this.Shutdown();
		}

		// Token: 0x06000274 RID: 628 RVA: 0x0000CB38 File Offset: 0x0000AD38
		protected void Shutdown()
		{
			if (this.sessionNetworkManager == null || this.sessionNetworkManager.ShutdownInProgress)
			{
				return;
			}
			this.sessionNetworkManager.Shutdown(true);
		}

		// Token: 0x0400013D RID: 317
		protected NetworkManager sessionNetworkManager;
	}
}
