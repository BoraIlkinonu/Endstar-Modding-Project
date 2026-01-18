using System;
using Endless.Creator.DynamicPropCreation;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x0200019E RID: 414
	public class UIAddPropGameAssetToGameLibraryModalController : UIGameObject
	{
		// Token: 0x0600060A RID: 1546 RVA: 0x0001F244 File Offset: 0x0001D444
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.searchButton.onClick.AddListener(new UnityAction(this.Search));
			this.createButton.onClick.AddListener(new UnityAction(this.Create));
		}

		// Token: 0x0600060B RID: 1547 RVA: 0x0001F2A4 File Offset: 0x0001D4A4
		private void Search()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Search", Array.Empty<object>());
			}
			ListCellSizeTypes listCellSizeTypes = ListCellSizeTypes.Cozy;
			UIGameAssetTypes uigameAssetTypes = UIGameAssetTypes.Prop;
			MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.gameAssetSearchModalSource, UIModalManagerStackActions.MaintainStack, new object[] { listCellSizeTypes, uigameAssetTypes });
		}

		// Token: 0x0600060C RID: 1548 RVA: 0x0001F2F6 File Offset: 0x0001D4F6
		private void Create()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Create", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.propCreationMenuModalSource, UIModalManagerStackActions.MaintainStack, new object[] { this.dynamicPropCreationMenuData });
		}

		// Token: 0x0400054D RID: 1357
		[SerializeField]
		private UIButton searchButton;

		// Token: 0x0400054E RID: 1358
		[SerializeField]
		private UIButton createButton;

		// Token: 0x0400054F RID: 1359
		[SerializeField]
		private UIGameLibraryAssetAdditionModalView gameAssetSearchModalSource;

		// Token: 0x04000550 RID: 1360
		[SerializeField]
		private UIPropCreationMenuModalView propCreationMenuModalSource;

		// Token: 0x04000551 RID: 1361
		[SerializeField]
		private PropCreationMenuData dynamicPropCreationMenuData;

		// Token: 0x04000552 RID: 1362
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
