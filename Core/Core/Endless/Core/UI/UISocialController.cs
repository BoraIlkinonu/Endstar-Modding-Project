using System;
using Endless.Data;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x0200009C RID: 156
	public class UISocialController : UIGameObject
	{
		// Token: 0x06000347 RID: 839 RVA: 0x00011754 File Offset: 0x0000F954
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.displayButton.onClick.AddListener(new UnityAction(this.view.Display));
			this.hideButton.onClick.AddListener(new UnityAction(this.view.Hide));
		}

		// Token: 0x06000348 RID: 840 RVA: 0x000117BC File Offset: 0x0000F9BC
		private void OnDestroy()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			this.displayButton.onClick.RemoveListener(new UnityAction(this.view.Display));
			this.hideButton.onClick.RemoveListener(new UnityAction(this.view.Hide));
		}

		// Token: 0x04000266 RID: 614
		[SerializeField]
		private UISocialView view;

		// Token: 0x04000267 RID: 615
		[SerializeField]
		private UIButton displayButton;

		// Token: 0x04000268 RID: 616
		[SerializeField]
		private UIButton hideButton;

		// Token: 0x04000269 RID: 617
		[SerializeField]
		private EndlessStudiosUserId endlessStudiosUserId;

		// Token: 0x0400026A RID: 618
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
