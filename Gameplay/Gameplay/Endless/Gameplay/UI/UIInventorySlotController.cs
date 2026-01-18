using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003B0 RID: 944
	public class UIInventorySlotController : UIGameObject
	{
		// Token: 0x170004F5 RID: 1269
		// (get) Token: 0x0600181A RID: 6170 RVA: 0x000703BE File Offset: 0x0006E5BE
		private InventorySlot Model
		{
			get
			{
				return this.view.Model;
			}
		}

		// Token: 0x170004F6 RID: 1270
		// (get) Token: 0x0600181B RID: 6171 RVA: 0x000703CB File Offset: 0x0006E5CB
		private int Index
		{
			get
			{
				return this.view.Index;
			}
		}

		// Token: 0x0600181C RID: 6172 RVA: 0x000703D8 File Offset: 0x0006E5D8
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			if (MobileUtility.IsMobile)
			{
				this.itemController.OnSelectUnityEvent.AddListener(new UnityAction(this.Equip));
			}
		}

		// Token: 0x0600181D RID: 6173 RVA: 0x00070418 File Offset: 0x0006E618
		private void Equip()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Equip", Array.Empty<object>());
			}
			PlayerReferenceManager localPlayerObject = MonoBehaviourSingleton<PlayerManager>.Instance.GetLocalPlayerObject();
			int index = this.Index;
			int inventorySlot = (int)this.Model.Item.InventorySlot;
			localPlayerObject.Inventory.EquipSlot(index, inventorySlot);
		}

		// Token: 0x04001351 RID: 4945
		[SerializeField]
		private UIInventorySlotView view;

		// Token: 0x04001352 RID: 4946
		[SerializeField]
		private UIItemController itemController;

		// Token: 0x04001353 RID: 4947
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
