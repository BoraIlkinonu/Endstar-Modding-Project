using System;
using System.Threading.Tasks;
using Runtime.Shared.Matchmaking;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace Endless.Matchmaking
{
	// Token: 0x02000036 RID: 54
	public class MatchAllocator : IMatchAllocator
	{
		// Token: 0x1400000A RID: 10
		// (add) Token: 0x06000170 RID: 368 RVA: 0x00009400 File Offset: 0x00007600
		// (remove) Token: 0x06000171 RID: 369 RVA: 0x00009438 File Offset: 0x00007638
		public event Action OnMatchAllocated;

		// Token: 0x17000042 RID: 66
		// (get) Token: 0x06000172 RID: 370 RVA: 0x0000946D File Offset: 0x0000766D
		public object LastAllocation
		{
			get
			{
				return this.LastRelayAllocation;
			}
		}

		// Token: 0x17000043 RID: 67
		// (get) Token: 0x06000173 RID: 371 RVA: 0x00009475 File Offset: 0x00007675
		// (set) Token: 0x06000174 RID: 372 RVA: 0x0000947D File Offset: 0x0000767D
		public Allocation LastRelayAllocation { get; private set; }

		// Token: 0x17000044 RID: 68
		// (get) Token: 0x06000175 RID: 373 RVA: 0x00009486 File Offset: 0x00007686
		// (set) Token: 0x06000176 RID: 374 RVA: 0x0000948E File Offset: 0x0000768E
		public string LastJoinCode { get; private set; }

		// Token: 0x17000045 RID: 69
		// (get) Token: 0x06000177 RID: 375 RVA: 0x00009497 File Offset: 0x00007697
		public string PublicIp
		{
			get
			{
				return this.LastRelayAllocation.RelayServer.IpV4;
			}
		}

		// Token: 0x17000046 RID: 70
		// (get) Token: 0x06000178 RID: 376 RVA: 0x00009497 File Offset: 0x00007697
		public string LocalIp
		{
			get
			{
				return this.LastRelayAllocation.RelayServer.IpV4;
			}
		}

		// Token: 0x17000047 RID: 71
		// (get) Token: 0x06000179 RID: 377 RVA: 0x000094AC File Offset: 0x000076AC
		public string Name
		{
			get
			{
				return this.LastRelayAllocation.AllocationId.ToString();
			}
		}

		// Token: 0x17000048 RID: 72
		// (get) Token: 0x0600017A RID: 378 RVA: 0x000094D2 File Offset: 0x000076D2
		public int Port
		{
			get
			{
				return this.LastRelayAllocation.RelayServer.Port;
			}
		}

		// Token: 0x17000049 RID: 73
		// (get) Token: 0x0600017B RID: 379 RVA: 0x000094E4 File Offset: 0x000076E4
		public string Key
		{
			get
			{
				return this.LastJoinCode;
			}
		}

		// Token: 0x0600017D RID: 381 RVA: 0x000094FF File Offset: 0x000076FF
		public void Allocate()
		{
			this.Reset();
			this.currentAllocationGuid = Guid.NewGuid();
			this.Allocate(15, null);
		}

		// Token: 0x0600017E RID: 382 RVA: 0x0000951B File Offset: 0x0000771B
		public void Reset()
		{
			this.LastJoinCode = null;
			this.LastRelayAllocation = null;
			this.currentAllocationGuid = Guid.Empty;
		}

		// Token: 0x0600017F RID: 383 RVA: 0x00009538 File Offset: 0x00007738
		private async void Allocate(int maxConnections, string region = null)
		{
			await Task.Delay(2000);
			Guid allocationGuid = this.currentAllocationGuid;
			Allocation allocation = null;
			for (int i = 0; i <= 2; i++)
			{
				try
				{
					allocation = await Relay.Instance.CreateAllocationAsync(maxConnections, region);
					break;
				}
				catch (Exception ex)
				{
					if (allocationGuid != this.currentAllocationGuid)
					{
						return;
					}
					if (i == 2)
					{
						Debug.LogError("Failed to allocate relay after " + 3.ToString() + string.Format(" attempts: {0}", ex));
						Action onMatchAllocated = this.OnMatchAllocated;
						if (onMatchAllocated != null)
						{
							onMatchAllocated();
						}
						return;
					}
				}
			}
			if (allocationGuid != this.currentAllocationGuid)
			{
				return;
			}
			string joinCode = string.Empty;
			for (int i = 0; i <= 2; i++)
			{
				try
				{
					joinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);
					break;
				}
				catch (Exception ex2)
				{
					if (allocationGuid != this.currentAllocationGuid)
					{
						return;
					}
					if (i == 2)
					{
						Debug.LogError("Failed to get join code after " + 3.ToString() + string.Format(" attempts: {0}", ex2));
						Action onMatchAllocated2 = this.OnMatchAllocated;
						if (onMatchAllocated2 != null)
						{
							onMatchAllocated2();
						}
						return;
					}
				}
			}
			if (allocationGuid != this.currentAllocationGuid)
			{
				return;
			}
			this.LastRelayAllocation = allocation;
			this.LastJoinCode = joinCode;
			Action onMatchAllocated3 = this.OnMatchAllocated;
			if (onMatchAllocated3 != null)
			{
				onMatchAllocated3();
			}
		}

		// Token: 0x040000D0 RID: 208
		private const int MAX_CONNECTED_CLIENTS = 15;

		// Token: 0x040000D1 RID: 209
		private const int ALLOCATION_RETRY_COUNT = 2;

		// Token: 0x040000D2 RID: 210
		public MatchmakingClientController MMCC;

		// Token: 0x040000D6 RID: 214
		private Guid currentAllocationGuid = Guid.Empty;
	}
}
