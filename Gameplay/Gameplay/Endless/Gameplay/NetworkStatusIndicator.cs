using System;
using Endless.Shared;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x02000127 RID: 295
	public class NetworkStatusIndicator : MonoBehaviourSingleton<NetworkStatusIndicator>
	{
		// Token: 0x1700012F RID: 303
		// (get) Token: 0x060006AA RID: 1706 RVA: 0x000210F6 File Offset: 0x0001F2F6
		// (set) Token: 0x060006AB RID: 1707 RVA: 0x00021100 File Offset: 0x0001F300
		public NetworkStatusIndicator.NetworkStatus CurrentNetworkStatus
		{
			get
			{
				return this._currentNetworkStatus;
			}
			protected set
			{
				if (this._currentNetworkStatus != value)
				{
					NetworkStatusIndicator.NetworkStatus currentNetworkStatus = this._currentNetworkStatus;
					this._currentNetworkStatus = value;
					this.StatusChangedListener.Invoke(currentNetworkStatus, this._currentNetworkStatus);
				}
			}
		}

		// Token: 0x060006AC RID: 1708 RVA: 0x00021136 File Offset: 0x0001F336
		private void OnEnabled()
		{
			this.CurrentNetworkStatus = NetworkStatusIndicator.NetworkStatus.Good;
		}

		// Token: 0x060006AD RID: 1709 RVA: 0x0002113F File Offset: 0x0001F33F
		private void OnDisabled()
		{
			this.CurrentNetworkStatus = NetworkStatusIndicator.NetworkStatus.Offline;
		}

		// Token: 0x060006AE RID: 1710 RVA: 0x00021148 File Offset: 0x0001F348
		public void ServerTimeDesync()
		{
			this.timeDesyncCount++;
			base.Invoke("ClearTimeDesync", 1.25f);
		}

		// Token: 0x060006AF RID: 1711 RVA: 0x00021168 File Offset: 0x0001F368
		public void MissedInput()
		{
			this.missedInputCount++;
			base.Invoke("ClearMissedInput", 1.25f);
		}

		// Token: 0x060006B0 RID: 1712 RVA: 0x00021188 File Offset: 0x0001F388
		public void UpdateServerToClientTime(float value)
		{
			this.serverToClientTime = value;
		}

		// Token: 0x060006B1 RID: 1713 RVA: 0x00021191 File Offset: 0x0001F391
		private void ClearTimeDesync()
		{
			this.timeDesyncCount--;
		}

		// Token: 0x060006B2 RID: 1714 RVA: 0x000211A1 File Offset: 0x0001F3A1
		private void ClearMissedInput()
		{
			this.missedInputCount--;
		}

		// Token: 0x060006B3 RID: 1715 RVA: 0x000211B4 File Offset: 0x0001F3B4
		private void Update()
		{
			if (this.timeDesyncCount > 1 || this.missedInputCount > 0 || this.serverToClientTime > 0.2f)
			{
				this.CurrentNetworkStatus = NetworkStatusIndicator.NetworkStatus.Bad;
				return;
			}
			if (this.timeDesyncCount > 0 || this.serverToClientTime > 0.16f)
			{
				this.CurrentNetworkStatus = NetworkStatusIndicator.NetworkStatus.Ok;
				return;
			}
			this.CurrentNetworkStatus = NetworkStatusIndicator.NetworkStatus.Good;
		}

		// Token: 0x0400055C RID: 1372
		private const float CLEAR_UP_DELAY = 1.25f;

		// Token: 0x0400055D RID: 1373
		private const float WARNING_PONG_THRESHOLD = 0.16f;

		// Token: 0x0400055E RID: 1374
		private const float BAD_PONG_THRESHOLD = 0.2f;

		// Token: 0x0400055F RID: 1375
		private NetworkStatusIndicator.NetworkStatus _currentNetworkStatus;

		// Token: 0x04000560 RID: 1376
		public UnityEvent<NetworkStatusIndicator.NetworkStatus, NetworkStatusIndicator.NetworkStatus> StatusChangedListener = new UnityEvent<NetworkStatusIndicator.NetworkStatus, NetworkStatusIndicator.NetworkStatus>();

		// Token: 0x04000561 RID: 1377
		private int timeDesyncCount;

		// Token: 0x04000562 RID: 1378
		private int missedInputCount;

		// Token: 0x04000563 RID: 1379
		private float serverToClientTime;

		// Token: 0x02000128 RID: 296
		public enum NetworkStatus
		{
			// Token: 0x04000565 RID: 1381
			Offline,
			// Token: 0x04000566 RID: 1382
			Bad,
			// Token: 0x04000567 RID: 1383
			Ok,
			// Token: 0x04000568 RID: 1384
			Good
		}
	}
}
