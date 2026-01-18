using System;
using Endless.Gameplay;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020000DF RID: 223
	public class UIGhostModeView : UIGameObject
	{
		// Token: 0x060003BA RID: 954 RVA: 0x000180A8 File Offset: 0x000162A8
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			MonoBehaviourSingleton<PlayerManager>.Instance.OnOwnerRegistered.AddListener(new UnityAction<ulong, PlayerReferenceManager>(this.OnOwnerRegistered));
		}

		// Token: 0x060003BB RID: 955 RVA: 0x000180E0 File Offset: 0x000162E0
		private void OnOwnerRegistered(ulong clientId, PlayerReferenceManager playerReferenceManager)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnOwnerRegistered", new object[] { clientId, playerReferenceManager.IsOwner });
			}
			this.playerNetworkController = playerReferenceManager.PlayerNetworkController;
			this.playerNetworkController.GhostChangedUnityEvent.AddListener(new UnityAction<bool>(this.OnGhostModeChanged));
			this.View();
		}

		// Token: 0x060003BC RID: 956 RVA: 0x0001814B File Offset: 0x0001634B
		private void View()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "View", Array.Empty<object>());
			}
			if (this.playerNetworkController.Ghost)
			{
				this.trueTweenCollection.Tween();
				return;
			}
			this.falseTweenCollection.Tween();
		}

		// Token: 0x060003BD RID: 957 RVA: 0x00018189 File Offset: 0x00016389
		private void OnGhostModeChanged(bool state)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnGhostModeChanged", new object[] { state });
			}
			this.View();
		}

		// Token: 0x040003DD RID: 989
		[SerializeField]
		private TweenCollection trueTweenCollection;

		// Token: 0x040003DE RID: 990
		[SerializeField]
		private TweenCollection falseTweenCollection;

		// Token: 0x040003DF RID: 991
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040003E0 RID: 992
		private PlayerNetworkController playerNetworkController;
	}
}
