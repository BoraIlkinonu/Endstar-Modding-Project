using System;
using Endless.Gameplay;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020000DE RID: 222
	public class UIGhostModeController : UIGameObject
	{
		// Token: 0x060003B7 RID: 951 RVA: 0x00018044 File Offset: 0x00016244
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.ghostModeToggleButton.onClick.AddListener(new UnityAction(this.ToggleGhostMode));
		}

		// Token: 0x060003B8 RID: 952 RVA: 0x0001807A File Offset: 0x0001627A
		private void ToggleGhostMode()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ToggleGhostMode", Array.Empty<object>());
			}
			MonoBehaviourSingleton<PlayerManager>.Instance.GetLocalPlayerObject().PlayerNetworkController.ToggleGhostMode();
		}

		// Token: 0x040003DB RID: 987
		[SerializeField]
		private UIButton ghostModeToggleButton;

		// Token: 0x040003DC RID: 988
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
