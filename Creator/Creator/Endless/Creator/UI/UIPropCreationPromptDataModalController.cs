using System;
using Endless.Shared.Debugging;
using Endless.Shared.SoVariables;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020001CC RID: 460
	public class UIPropCreationPromptDataModalController : UIGameObject
	{
		// Token: 0x060006DD RID: 1757 RVA: 0x00022CD1 File Offset: 0x00020ED1
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.viewSdkButton.onClick.AddListener(new UnityAction(this.ViewSdk));
		}

		// Token: 0x060006DE RID: 1758 RVA: 0x00022D07 File Offset: 0x00020F07
		private void ViewSdk()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewSdk", Array.Empty<object>());
			}
			Application.OpenURL(this.sdkUrl.Value);
		}

		// Token: 0x04000628 RID: 1576
		[SerializeField]
		private UIPropCreationPromptDataModalView view;

		// Token: 0x04000629 RID: 1577
		[SerializeField]
		private UIButton viewSdkButton;

		// Token: 0x0400062A RID: 1578
		[SerializeField]
		private StringVariable sdkUrl;

		// Token: 0x0400062B RID: 1579
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
