using System;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003DA RID: 986
	[DisallowMultipleComponent]
	public class UIModalMatchCloseHandler : UIGameObject
	{
		// Token: 0x060018ED RID: 6381 RVA: 0x00073C7D File Offset: 0x00071E7D
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			MatchSession.OnMatchSessionClose += this.OnMatchClosed;
		}

		// Token: 0x060018EE RID: 6382 RVA: 0x00073CA8 File Offset: 0x00071EA8
		private void OnDisable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			MatchSession.OnMatchSessionClose -= this.OnMatchClosed;
		}

		// Token: 0x060018EF RID: 6383 RVA: 0x00073CD3 File Offset: 0x00071ED3
		private void OnMatchClosed(MatchSession _)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnMatchClosed", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
		}

		// Token: 0x0400140C RID: 5132
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
