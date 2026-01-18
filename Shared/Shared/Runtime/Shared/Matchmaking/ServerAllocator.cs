using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Endless.Matchmaking;
using UnityEngine;

namespace Runtime.Shared.Matchmaking
{
	// Token: 0x0200001B RID: 27
	public class ServerAllocator : IMatchAllocator
	{
		// Token: 0x14000004 RID: 4
		// (add) Token: 0x060000C4 RID: 196 RVA: 0x00005A94 File Offset: 0x00003C94
		// (remove) Token: 0x060000C5 RID: 197 RVA: 0x00005ACC File Offset: 0x00003CCC
		public event Action OnMatchAllocated;

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x060000C6 RID: 198 RVA: 0x00005B01 File Offset: 0x00003D01
		public object LastAllocation
		{
			get
			{
				IMatchAllocator matchAllocator = this.currentAllocator;
				if (matchAllocator == null)
				{
					return null;
				}
				return matchAllocator.LastAllocation;
			}
		}

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x060000C7 RID: 199 RVA: 0x00005B14 File Offset: 0x00003D14
		public string PublicIp
		{
			get
			{
				IMatchAllocator matchAllocator = this.currentAllocator;
				if (matchAllocator == null)
				{
					return null;
				}
				return matchAllocator.PublicIp;
			}
		}

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x060000C8 RID: 200 RVA: 0x00005B27 File Offset: 0x00003D27
		public string LocalIp
		{
			get
			{
				IMatchAllocator matchAllocator = this.currentAllocator;
				if (matchAllocator == null)
				{
					return null;
				}
				return matchAllocator.LocalIp;
			}
		}

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x060000C9 RID: 201 RVA: 0x00005B3A File Offset: 0x00003D3A
		public string Name
		{
			get
			{
				IMatchAllocator matchAllocator = this.currentAllocator;
				if (matchAllocator == null)
				{
					return null;
				}
				return matchAllocator.Name;
			}
		}

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x060000CA RID: 202 RVA: 0x00005B4D File Offset: 0x00003D4D
		public int Port
		{
			get
			{
				IMatchAllocator matchAllocator = this.currentAllocator;
				if (matchAllocator == null)
				{
					return 0;
				}
				return matchAllocator.Port;
			}
		}

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x060000CB RID: 203 RVA: 0x00005B60 File Offset: 0x00003D60
		public string Key
		{
			get
			{
				IMatchAllocator matchAllocator = this.currentAllocator;
				if (matchAllocator == null)
				{
					return null;
				}
				return matchAllocator.Key;
			}
		}

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x060000CC RID: 204 RVA: 0x00005B73 File Offset: 0x00003D73
		// (set) Token: 0x060000CD RID: 205 RVA: 0x00005B7B File Offset: 0x00003D7B
		public List<string> LastServerWhitelist { get; private set; }

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x060000CE RID: 206 RVA: 0x00005B84 File Offset: 0x00003D84
		// (set) Token: 0x060000CF RID: 207 RVA: 0x00005B8C File Offset: 0x00003D8C
		public string LastServerType { get; private set; }

		// Token: 0x060000D0 RID: 208 RVA: 0x00005B98 File Offset: 0x00003D98
		public ServerAllocator()
		{
			this.endlessAllocator = new EndlessAllocator(MatchmakingClientController.Instance.NetworkEnv.ToString())
			{
				MMCC = MatchmakingClientController.Instance
			};
			this.endlessAllocator.OnMatchAllocated += this.MatchAllocated;
			this.matchAllocator = new MatchAllocator();
			this.matchAllocator.OnMatchAllocated += this.MatchAllocated;
			this.lanAllocator = new LanAllocator();
			this.lanAllocator.OnMatchAllocated += this.MatchAllocated;
			this.currentAllocator = null;
		}

		// Token: 0x060000D1 RID: 209 RVA: 0x00005C3B File Offset: 0x00003E3B
		public void Allocate()
		{
			MatchmakingClientController.Instance.GetServerWhitelist(new Action<List<string>>(this.OnServerWhitelistReceived), new Action<int, string>(this.OnServerWhitelistError));
		}

		// Token: 0x060000D2 RID: 210 RVA: 0x00005C5F File Offset: 0x00003E5F
		private void MatchAllocated()
		{
			if (this.currentAllocator == null)
			{
				return;
			}
			Action onMatchAllocated = this.OnMatchAllocated;
			if (onMatchAllocated == null)
			{
				return;
			}
			onMatchAllocated();
		}

		// Token: 0x060000D3 RID: 211 RVA: 0x00005C7C File Offset: 0x00003E7C
		private void OnServerWhitelistReceived(List<string> serverWhitelist)
		{
			this.LastServerWhitelist = serverWhitelist;
			for (int i = 0; i < serverWhitelist.Count; i++)
			{
				string text = serverWhitelist[i];
				if (text == "UnityRelay")
				{
					this.LastServerType = "UnityRelay";
					this.currentAllocator = this.matchAllocator;
					this.currentAllocator.Allocate();
					return;
				}
				if (text == "EndlessRelay")
				{
					string text2;
					if (EndlessAllocator.relayEndpoints.TryGetValue(this.endlessAllocator.Stage, out text2))
					{
						IPAddress[] hostAddresses = Dns.GetHostAddresses(text2);
						Debug.Log(string.Join(", ", hostAddresses.Select((IPAddress a) => a.ToString())));
						if (hostAddresses.Length == 0 || hostAddresses[0].Equals(IPAddress.Parse("1.1.1.1")))
						{
							Debug.LogError("Endless Relay is not supported in this region. Trying another server type.");
							goto IL_0126;
						}
					}
					this.LastServerType = "EndlessRelay";
					this.currentAllocator = this.endlessAllocator;
					this.currentAllocator.Allocate();
					return;
				}
				if (text == "LAN")
				{
					this.LastServerType = "LAN";
					this.currentAllocator = this.lanAllocator;
					this.currentAllocator.Allocate();
					return;
				}
				IL_0126:;
			}
			Debug.LogError("No valid server type found to allocate relay: " + string.Join(", ", serverWhitelist));
			Action onMatchAllocated = this.OnMatchAllocated;
			if (onMatchAllocated == null)
			{
				return;
			}
			onMatchAllocated();
		}

		// Token: 0x060000D4 RID: 212 RVA: 0x00005DE9 File Offset: 0x00003FE9
		private void OnServerWhitelistError(int errorCode, string error)
		{
			Debug.LogError(string.Format("Error receiving server whitelist: {0} {1}", (HttpStatusCode)errorCode, error));
			this.currentAllocator = null;
			Action onMatchAllocated = this.OnMatchAllocated;
			if (onMatchAllocated == null)
			{
				return;
			}
			onMatchAllocated();
		}

		// Token: 0x060000D5 RID: 213 RVA: 0x00005E18 File Offset: 0x00004018
		public void Reset()
		{
			this.matchAllocator.Reset();
			this.endlessAllocator.Reset();
			this.currentAllocator = null;
		}

		// Token: 0x04000056 RID: 86
		private EndlessAllocator endlessAllocator;

		// Token: 0x04000057 RID: 87
		private MatchAllocator matchAllocator;

		// Token: 0x04000058 RID: 88
		private LanAllocator lanAllocator;

		// Token: 0x04000059 RID: 89
		private IMatchAllocator currentAllocator;
	}
}
