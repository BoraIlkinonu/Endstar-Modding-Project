using System;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020002DB RID: 731
	public class UIRuntimePropInfoSelectionWindowController : UIWindowController
	{
		// Token: 0x06000C70 RID: 3184 RVA: 0x0003B78F File Offset: 0x0003998F
		protected override void Start()
		{
			base.Start();
			this.confirmButton.onClick.AddListener(new UnityAction(this.Confirm));
		}

		// Token: 0x06000C71 RID: 3185 RVA: 0x0003B7B4 File Offset: 0x000399B4
		private void Confirm()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Confirm", Array.Empty<object>());
			}
			Action<IReadOnlyList<PropLibrary.RuntimePropInfo>> onConfirm = this.view.OnConfirm;
			if (onConfirm != null)
			{
				onConfirm(this.runtimePropInfoListModel.SelectedTypedList);
			}
			this.Close();
		}

		// Token: 0x04000AB5 RID: 2741
		[SerializeField]
		private UIRuntimePropInfoSelectionWindowView view;

		// Token: 0x04000AB6 RID: 2742
		[SerializeField]
		private UIRuntimePropInfoListModel runtimePropInfoListModel;

		// Token: 0x04000AB7 RID: 2743
		[SerializeField]
		private UIButton confirmButton;
	}
}
