using System;
using Endless.Gameplay.UI;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020001B2 RID: 434
	public class UIGameLibraryAssetDetailModalView : UIEscapableModalView
	{
		// Token: 0x0600066D RID: 1645 RVA: 0x000215AF File Offset: 0x0001F7AF
		public override void OnDespawn()
		{
			base.OnDespawn();
			this.gameAssetDetail.Clear();
		}

		// Token: 0x0600066E RID: 1646 RVA: 0x000215C4 File Offset: 0x0001F7C4
		public override void OnDisplay(params object[] modalData)
		{
			base.OnDisplay(modalData);
			if (!this.initialized)
			{
				this.gameAssetDetail.UserRolesModel.OnLocalClientRoleSet.AddListener(new UnityAction<Roles>(this.HandleEditButtonVisibility));
				this.initialized = true;
			}
			UIGameAsset uigameAsset = (UIGameAsset)modalData[0];
			this.assetContext = (AssetContexts)modalData[1];
			this.modalMatchCloseHandler.enabled = this.assetContext > AssetContexts.MainMenu;
			this.editButton.gameObject.SetActive(false);
			this.duplicateButton.gameObject.SetActive(false);
			this.gameAssetDetail.SetContext(this.assetContext);
			this.gameAssetDetail.View(uigameAsset);
		}

		// Token: 0x0600066F RID: 1647 RVA: 0x00021672 File Offset: 0x0001F872
		public override void Close()
		{
			base.Close();
			this.modalMatchCloseHandler.enabled = false;
		}

		// Token: 0x06000670 RID: 1648 RVA: 0x00021688 File Offset: 0x0001F888
		private void HandleEditButtonVisibility(Roles localClientRole)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleEditButtonVisibility", new object[] { localClientRole });
			}
			bool flag = this.assetContext == AssetContexts.MainMenu && localClientRole.IsGreaterThanOrEqualTo(Roles.Editor);
			this.editButton.gameObject.SetActive(flag);
		}

		// Token: 0x040005C4 RID: 1476
		[SerializeField]
		private UIButton editButton;

		// Token: 0x040005C5 RID: 1477
		[SerializeField]
		private UIButton duplicateButton;

		// Token: 0x040005C6 RID: 1478
		[SerializeField]
		private UIGameAssetDetailView gameAssetDetail;

		// Token: 0x040005C7 RID: 1479
		[SerializeField]
		private UIModalMatchCloseHandler modalMatchCloseHandler;

		// Token: 0x040005C8 RID: 1480
		private AssetContexts assetContext;

		// Token: 0x040005C9 RID: 1481
		private bool initialized;
	}
}
