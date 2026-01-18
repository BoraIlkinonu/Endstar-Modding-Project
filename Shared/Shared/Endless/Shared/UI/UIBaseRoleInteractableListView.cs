using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x020001A2 RID: 418
	public class UIBaseRoleInteractableListView<T> : UIBaseListView<T>
	{
		// Token: 0x06000AD8 RID: 2776 RVA: 0x0002F9D8 File Offset: 0x0002DBD8
		protected override void Start()
		{
			base.Start();
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			if (!this.setCellTypesBasedOnRole)
			{
				return;
			}
			base.Model.AddButtonIsInteractableChangedUnityEvent.AddListener(new UnityAction<bool>(this.OnAddButtonIsInteractableChanged));
			this.OnAddButtonIsInteractableChanged(base.Model.AddButtonIsInteractable);
		}

		// Token: 0x06000AD9 RID: 2777 RVA: 0x0002FA3C File Offset: 0x0002DC3C
		public override void Validate()
		{
			base.Validate();
			if (this.IgnoreValidation)
			{
				return;
			}
			DebugUtility.DebugIsNull("compactCellSourceInteractable", this.compactCellSourceInteractable, this);
			DebugUtility.DebugIsNull("cozyCellSourceInteractable", this.cozyCellSourceInteractable, this);
			DebugUtility.DebugIsNull("compactCellSourceNotInteractable", this.compactCellSourceNotInteractable, this);
			DebugUtility.DebugIsNull("cozyCellSourceNotInteractable", this.cozyCellSourceNotInteractable, this);
		}

		// Token: 0x06000ADA RID: 2778 RVA: 0x0002FAA0 File Offset: 0x0002DCA0
		private void OnAddButtonIsInteractableChanged(bool addButtonIsInteractable)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "OnAddButtonIsInteractableChanged", "addButtonIsInteractable", addButtonIsInteractable), this);
			}
			UIBaseListItemView<T> uibaseListItemView = (addButtonIsInteractable ? this.compactCellSourceInteractable : this.compactCellSourceNotInteractable);
			base.SetCompactCellSource(uibaseListItemView);
			UIBaseListItemView<T> uibaseListItemView2 = (addButtonIsInteractable ? this.cozyCellSourceInteractable : this.cozyCellSourceNotInteractable);
			base.SetCozyCellSource(uibaseListItemView2);
		}

		// Token: 0x040006EF RID: 1775
		[Header("UIBaseRoleInteractableListView")]
		[SerializeField]
		private bool setCellTypesBasedOnRole;

		// Token: 0x040006F0 RID: 1776
		[Header("Interactable")]
		[SerializeField]
		private UIBaseListItemView<T> compactCellSourceInteractable;

		// Token: 0x040006F1 RID: 1777
		[SerializeField]
		private UIBaseListItemView<T> cozyCellSourceInteractable;

		// Token: 0x040006F2 RID: 1778
		[Header("Not Interactable")]
		[SerializeField]
		private UIBaseListItemView<T> compactCellSourceNotInteractable;

		// Token: 0x040006F3 RID: 1779
		[SerializeField]
		private UIBaseListItemView<T> cozyCellSourceNotInteractable;
	}
}
