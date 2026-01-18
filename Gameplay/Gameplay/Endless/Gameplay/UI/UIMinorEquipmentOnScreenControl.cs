using System;
using Endless.Shared.Debugging;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Gameplay.UI
{
	// Token: 0x02000407 RID: 1031
	public class UIMinorEquipmentOnScreenControl : UIBaseEquipmentOnScreenControl
	{
		// Token: 0x17000539 RID: 1337
		// (get) Token: 0x060019C2 RID: 6594 RVA: 0x00017586 File Offset: 0x00015786
		protected override Item.InventorySlotType InventorySlotType
		{
			get
			{
				return Item.InventorySlotType.Minor;
			}
		}

		// Token: 0x060019C3 RID: 6595 RVA: 0x0007659E File Offset: 0x0007479E
		protected override void Start()
		{
			base.Start();
			this.onScreenStick.OnPointerDownUnityEvent.AddListener(new UnityAction(this.OnPointerDown));
			this.onScreenStick.OnPointerUpUnityEvent.AddListener(new UnityAction(this.OnPointerUp));
		}

		// Token: 0x060019C4 RID: 6596 RVA: 0x000765E0 File Offset: 0x000747E0
		protected override void OnEquippedSlotsChanged(NetworkListEvent<Inventory.EquipmentSlot> changeEvent)
		{
			base.OnEquippedSlotsChanged(changeEvent);
			this.SetInteractable(base.HasEquipment);
			if (!base.HasEquipment)
			{
				return;
			}
			Item item = base.Inventory.GetSlot(base.SlotIndex).Item;
			this.itemView.View(item);
			this.layoutType = item.InventoryUsableDefinition.MobileUIMinorLayout;
			float num = (float)((this.layoutType == InventoryUsableDefinition.MobileUI_MinorLayoutType.ButtonWithJoystick) ? 50 : 0);
			this.onScreenStick.SetMovementRange(num);
		}

		// Token: 0x060019C5 RID: 6597 RVA: 0x0007665C File Offset: 0x0007485C
		protected override void Clear()
		{
			base.Clear();
			this.itemView.Clear();
			this.SetInteractable(false);
		}

		// Token: 0x060019C6 RID: 6598 RVA: 0x00076676 File Offset: 0x00074876
		private void OnPointerDown()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnPointerDown", Array.Empty<object>());
			}
			this.onScreenButtonHandler.SetButtonState(true);
		}

		// Token: 0x060019C7 RID: 6599 RVA: 0x0007669C File Offset: 0x0007489C
		private void OnPointerUp()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnPointerUp", Array.Empty<object>());
			}
			this.onScreenButtonHandler.SetButtonState(false);
		}

		// Token: 0x060019C8 RID: 6600 RVA: 0x000766C2 File Offset: 0x000748C2
		private void SetInteractable(bool interactable)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetInteractable", new object[] { interactable });
			}
			this.selectable.interactable = interactable;
			this.onScreenStick.SetInteractable(interactable);
		}

		// Token: 0x0400146B RID: 5227
		[Header("UIMinorEquipmentOnScreenControl")]
		[SerializeField]
		private Selectable selectable;

		// Token: 0x0400146C RID: 5228
		[SerializeField]
		private UIOnScreenStickHandler onScreenStick;

		// Token: 0x0400146D RID: 5229
		[SerializeField]
		private UIOnScreenButtonHandler onScreenButtonHandler;

		// Token: 0x0400146E RID: 5230
		[SerializeField]
		private UIItemView itemView;

		// Token: 0x0400146F RID: 5231
		private InventoryUsableDefinition.MobileUI_MinorLayoutType layoutType;
	}
}
