using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x02000068 RID: 104
	[RequireComponent(typeof(UIScreenConfirmationModalView))]
	public class UIScreenConfirmationModalController : UIGameObject
	{
		// Token: 0x060001E3 RID: 483 RVA: 0x0000B0FC File Offset: 0x000092FC
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			base.TryGetComponent<UIScreenConfirmationModalView>(out this.view);
			this.applyButton.onClick.AddListener(new UnityAction(MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack));
			this.revertButton.onClick.AddListener(new UnityAction(this.view.Revert));
		}

		// Token: 0x04000157 RID: 343
		[SerializeField]
		private UIButton applyButton;

		// Token: 0x04000158 RID: 344
		[SerializeField]
		private UIButton revertButton;

		// Token: 0x04000159 RID: 345
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x0400015A RID: 346
		private UIScreenConfirmationModalView view;
	}
}
