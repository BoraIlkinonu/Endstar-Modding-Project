using System;
using Endless.Shared;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Gameplay
{
	// Token: 0x02000129 RID: 297
	public class NetworkStatusIndicator_TempUI : MonoBehaviour
	{
		// Token: 0x060006B5 RID: 1717 RVA: 0x00021220 File Offset: 0x0001F420
		private void OnEnabled()
		{
			this.image.color = this.goodColor;
		}

		// Token: 0x060006B6 RID: 1718 RVA: 0x00021233 File Offset: 0x0001F433
		private void Start()
		{
			if (this.RegisterOnAwake)
			{
				MonoBehaviourSingleton<NetworkStatusIndicator>.Instance.StatusChangedListener.AddListener(new UnityAction<NetworkStatusIndicator.NetworkStatus, NetworkStatusIndicator.NetworkStatus>(this.HandleNetworkStatusChanged));
			}
		}

		// Token: 0x060006B7 RID: 1719 RVA: 0x00021258 File Offset: 0x0001F458
		public void HandleNetworkStatusChanged(NetworkStatusIndicator.NetworkStatus oldStatus, NetworkStatusIndicator.NetworkStatus newNetworkStatus)
		{
			if (newNetworkStatus == NetworkStatusIndicator.NetworkStatus.Good)
			{
				this.image.color = this.goodColor;
				return;
			}
			if (newNetworkStatus == NetworkStatusIndicator.NetworkStatus.Ok)
			{
				this.image.color = this.warningColor;
				return;
			}
			this.image.color = this.badColor;
		}

		// Token: 0x04000569 RID: 1385
		[SerializeField]
		private Color goodColor;

		// Token: 0x0400056A RID: 1386
		[SerializeField]
		private Color warningColor;

		// Token: 0x0400056B RID: 1387
		[SerializeField]
		private Color badColor;

		// Token: 0x0400056C RID: 1388
		[SerializeField]
		private Image image;

		// Token: 0x0400056D RID: 1389
		[SerializeField]
		private bool RegisterOnAwake;
	}
}
