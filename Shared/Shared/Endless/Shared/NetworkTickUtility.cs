using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x02000077 RID: 119
	public class NetworkTickUtility : MonoBehaviourSingleton<NetworkTickUtility>
	{
		// Token: 0x060003A1 RID: 929 RVA: 0x00010702 File Offset: 0x0000E902
		private void Start()
		{
			base.StartCoroutine(this.NetworkRestart());
		}

		// Token: 0x060003A2 RID: 930 RVA: 0x00010711 File Offset: 0x0000E911
		private IEnumerator NetworkRestart()
		{
			yield return new WaitUntil(() => NetworkManager.Singleton && NetworkManager.Singleton.NetworkTickSystem != null);
			NetworkManager.Singleton.NetworkTickSystem.Tick += this.HandleNetworkTick;
			yield break;
		}

		// Token: 0x060003A3 RID: 931 RVA: 0x00010720 File Offset: 0x0000E920
		private void HandleNetworkTick()
		{
			this.localTick += 1UL;
		}

		// Token: 0x060003A4 RID: 932 RVA: 0x00010734 File Offset: 0x0000E934
		public async Task WaitForNetworkTicksAsync(int count)
		{
			ulong finshingTick = (MonoBehaviourSingleton<NetworkTickUtility>.Instance ? MonoBehaviourSingleton<NetworkTickUtility>.Instance.localTick : 0UL) + (ulong)((count > 0) ? ((long)count) : 0L);
			while (finshingTick < MonoBehaviourSingleton<NetworkTickUtility>.Instance.localTick)
			{
				await Task.Yield();
			}
		}

		// Token: 0x040001BF RID: 447
		private ulong localTick;

		// Token: 0x02000078 RID: 120
		public class WaitForNetworkTicks : CustomYieldInstruction
		{
			// Token: 0x17000097 RID: 151
			// (get) Token: 0x060003A6 RID: 934 RVA: 0x0001077F File Offset: 0x0000E97F
			public override bool keepWaiting
			{
				get
				{
					if (MonoBehaviourSingleton<NetworkTickUtility>.Instance)
					{
						return this.finshingTick >= MonoBehaviourSingleton<NetworkTickUtility>.Instance.localTick;
					}
					Debug.LogWarning("A coroutine is waiting for network ticks while network is offline. Skipping wait...");
					return false;
				}
			}

			// Token: 0x060003A7 RID: 935 RVA: 0x000107AE File Offset: 0x0000E9AE
			public WaitForNetworkTicks(int count)
			{
				this.finshingTick = (MonoBehaviourSingleton<NetworkTickUtility>.Instance ? MonoBehaviourSingleton<NetworkTickUtility>.Instance.localTick : 0UL) + (ulong)((count > 0) ? ((long)count) : 0L);
			}

			// Token: 0x040001C0 RID: 448
			private ulong finshingTick;
		}
	}
}
