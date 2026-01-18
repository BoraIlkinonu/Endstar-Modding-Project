using System;
using Endless.Gameplay;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000135 RID: 309
	public class UILevelDestinationSelectionListCellController : UIBaseListCellController<LevelDestination>
	{
		// Token: 0x060004DA RID: 1242 RVA: 0x0001B6D0 File Offset: 0x000198D0
		protected override void Start()
		{
			base.Start();
			this.selectButton.onClick.AddListener(new UnityAction(this.OnSelect));
		}

		// Token: 0x060004DB RID: 1243 RVA: 0x0001B6F4 File Offset: 0x000198F4
		protected override void OnAddButton()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAddButton", Array.Empty<object>());
			}
			throw new NotImplementedException();
		}

		// Token: 0x060004DC RID: 1244 RVA: 0x0001B714 File Offset: 0x00019914
		private void OnSelect()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSelect", Array.Empty<object>());
			}
			UILevelDestinationSelectionListView.SelectionTypes selectionType = ((UILevelDestinationSelectionListView)base.ListView).SelectionType;
			if (selectionType == UILevelDestinationSelectionListView.SelectionTypes.ApplyToProperty)
			{
				(MonoBehaviourSingleton<UIModalManager>.Instance.SpawnedModal as UILevelDestinationSelectionModalView).ApplyToProperty(base.Model);
				MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
				return;
			}
			if (selectionType != UILevelDestinationSelectionListView.SelectionTypes.LocalListToggleSelected)
			{
				throw new ArgumentOutOfRangeException();
			}
			base.ToggleSelected();
		}

		// Token: 0x0400047E RID: 1150
		[Header("UILevelDestinationSelectionListCellController")]
		[SerializeField]
		private UIButton selectButton;
	}
}
