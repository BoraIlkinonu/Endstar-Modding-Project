using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x02000066 RID: 102
	public class UILogsModalController : UIGameObject
	{
		// Token: 0x060001DF RID: 479 RVA: 0x0000B0A9 File Offset: 0x000092A9
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.clearButton.onClick.AddListener(new UnityAction(this.ClearLogs));
		}

		// Token: 0x060001E0 RID: 480 RVA: 0x0000B0DF File Offset: 0x000092DF
		private void ClearLogs()
		{
			if (this.verboseLogging)
			{
				Debug.Log("ClearLogs", this);
			}
		}

		// Token: 0x04000155 RID: 341
		[SerializeField]
		private UIButton clearButton;

		// Token: 0x04000156 RID: 342
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
