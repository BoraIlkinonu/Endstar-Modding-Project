using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000111 RID: 273
	public class UIModalCloseWithoutClearingStackButton : UIGameObject
	{
		// Token: 0x06000694 RID: 1684 RVA: 0x0001C100 File Offset: 0x0001A300
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.button.onClick.AddListener(new UnityAction(this.CloseWithoutClearingStack));
		}

		// Token: 0x06000695 RID: 1685 RVA: 0x0001C136 File Offset: 0x0001A336
		private void CloseWithoutClearingStack()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CloseWithoutClearingStack", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
		}

		// Token: 0x040003D1 RID: 977
		[SerializeField]
		private UIButton button;

		// Token: 0x040003D2 RID: 978
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
