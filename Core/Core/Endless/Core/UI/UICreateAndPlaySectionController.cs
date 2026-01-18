using System;
using Endless.Creator.UI;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x02000089 RID: 137
	[RequireComponent(typeof(UICreateAndPlaySectionView))]
	public class UICreateAndPlaySectionController : UIGameObject
	{
		// Token: 0x060002B4 RID: 692 RVA: 0x0000EDE0 File Offset: 0x0000CFE0
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.tabs.OnValueChangedWithIndex.AddListener(new UnityAction<int>(this.OnTabsChanged));
			this.gameAssetTypeFilterDropdown.OnValueChanged.AddListener(new UnityAction(this.SetGameAssetTypeFilter));
		}

		// Token: 0x060002B5 RID: 693 RVA: 0x0000EE40 File Offset: 0x0000D040
		private void OnDestroy()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			this.tabs.OnValueChangedWithIndex.RemoveListener(new UnityAction<int>(this.OnTabsChanged));
			this.gameAssetTypeFilterDropdown.OnValueChanged.RemoveListener(new UnityAction(this.SetGameAssetTypeFilter));
		}

		// Token: 0x060002B6 RID: 694 RVA: 0x0000EEA0 File Offset: 0x0000D0A0
		private void OnTabsChanged(int index)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnTabsChanged", new object[] { index });
			}
			this.view.SetSectionTitleText(this.tabs.Value.Enum.ToString());
		}

		// Token: 0x060002B7 RID: 695 RVA: 0x0000EEF0 File Offset: 0x0000D0F0
		private void SetGameAssetTypeFilter()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetGameAssetTypeFilter", Array.Empty<object>());
			}
			UIGameAssetTypes uigameAssetTypes = Enum.Parse<UIGameAssetTypes>(this.gameAssetTypeFilterDropdown.Value[0]);
			this.ownedGameAssetCloudPaginatedListModel.SetAssetTypeFilter(uigameAssetTypes, true);
		}

		// Token: 0x04000207 RID: 519
		[SerializeField]
		private UICreateAndPlaySectionView view;

		// Token: 0x04000208 RID: 520
		[SerializeField]
		private UISpriteAndEnumTabGroup tabs;

		// Token: 0x04000209 RID: 521
		[SerializeField]
		private UIDropdownString gameAssetTypeFilterDropdown;

		// Token: 0x0400020A RID: 522
		[SerializeField]
		private UIOwnedGameAssetCloudPaginatedListModel ownedGameAssetCloudPaginatedListModel;

		// Token: 0x0400020B RID: 523
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
