using System;
using Endless.Gameplay;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020001EA RID: 490
	public class UIInventorySpawnOptionsPresenter : UIBasePresenter<InventorySpawnOptions>
	{
		// Token: 0x0600079A RID: 1946 RVA: 0x00025A43 File Offset: 0x00023C43
		protected override void Start()
		{
			base.Start();
			(base.View.Interface as UIInventorySpawnOptionsView).OnEditPressed += this.DisplayInventorySpawnOptionsModal;
		}

		// Token: 0x0600079B RID: 1947 RVA: 0x00025A6C File Offset: 0x00023C6C
		private void DisplayInventorySpawnOptionsModal()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayInventorySpawnOptionsModal", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.inventorySpawnOptionsModalSource, UIModalManagerStackActions.ClearStack, new object[] { this });
		}

		// Token: 0x040006CF RID: 1743
		[Header("UIInventorySpawnOptionsPresenter")]
		[SerializeField]
		private UIInventorySpawnOptionsModalView inventorySpawnOptionsModalSource;
	}
}
