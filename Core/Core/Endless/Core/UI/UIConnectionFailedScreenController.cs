using System;
using Endless.Matchmaking;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x02000078 RID: 120
	public class UIConnectionFailedScreenController : UIGameObject
	{
		// Token: 0x0600025E RID: 606 RVA: 0x0000D320 File Offset: 0x0000B520
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.quitButton.onClick.AddListener(new UnityAction(this.Quit));
			base.TryGetComponent<UIConnectionFailedScreenView>(out this.view);
		}

		// Token: 0x0600025F RID: 607 RVA: 0x0000D36E File Offset: 0x0000B56E
		private void Quit()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Quit", Array.Empty<object>());
			}
			EndlessCloudService.ClearCachedToken();
			MatchmakingClientController.Instance.Disconnect();
			Application.Quit();
		}

		// Token: 0x040001A9 RID: 425
		[SerializeField]
		private UIButton quitButton;

		// Token: 0x040001AA RID: 426
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040001AB RID: 427
		private UIConnectionFailedScreenView view;
	}
}
