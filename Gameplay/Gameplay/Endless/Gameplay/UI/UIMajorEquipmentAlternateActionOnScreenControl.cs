using System;
using Endless.Shared.Debugging;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Gameplay.UI
{
	// Token: 0x02000405 RID: 1029
	public class UIMajorEquipmentAlternateActionOnScreenControl : UIBaseEquipmentOnScreenControl
	{
		// Token: 0x17000537 RID: 1335
		// (get) Token: 0x060019B6 RID: 6582 RVA: 0x0001965C File Offset: 0x0001785C
		protected override Item.InventorySlotType InventorySlotType
		{
			get
			{
				return Item.InventorySlotType.Major;
			}
		}

		// Token: 0x060019B7 RID: 6583 RVA: 0x00076393 File Offset: 0x00074593
		protected override void Start()
		{
			base.Start();
			this.onScreenStick.OnPointerDownUnityEvent.AddListener(new UnityAction(this.OnPointerDown));
			this.onScreenStick.OnPointerUpUnityEvent.AddListener(new UnityAction(this.OnPointerUp));
		}

		// Token: 0x060019B8 RID: 6584 RVA: 0x000763D3 File Offset: 0x000745D3
		protected override void Clear()
		{
			base.Clear();
			this.SetSelectableAndOnScreenStickInteractable(false);
		}

		// Token: 0x060019B9 RID: 6585 RVA: 0x000763E4 File Offset: 0x000745E4
		protected override void OnEquippedSlotsChanged(NetworkListEvent<Inventory.EquipmentSlot> changeEvent)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1} {{ {2}: {3}, {4}: {5}, {6}: {7} }} )", new object[] { "OnEquippedSlotsChanged", "changeEvent", "Index", changeEvent.Index, "Value", changeEvent.Value, "PreviousValue", changeEvent.PreviousValue }), this);
			}
			if (!base.HasEquipment)
			{
				this.Clear();
				return;
			}
			if (base.Inventory.GetSlot(base.SlotIndex).Item.InventoryUsableDefinition.MobileUIMajorLayout != InventoryUsableDefinition.MobileUI_MajorLayoutType.Ranged)
			{
				this.Clear();
				return;
			}
			base.Display();
			this.SetSelectableAndOnScreenStickInteractable(true);
		}

		// Token: 0x060019BA RID: 6586 RVA: 0x000764AC File Offset: 0x000746AC
		private void SetSelectableAndOnScreenStickInteractable(bool interactable)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetSelectableAndOnScreenStickInteractable", new object[] { interactable });
			}
			this.onScreenStick.SetInteractable(interactable);
			this.selectable.interactable = interactable;
		}

		// Token: 0x060019BB RID: 6587 RVA: 0x000764E8 File Offset: 0x000746E8
		private void OnPointerDown()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnPointerDown", Array.Empty<object>());
			}
			this.onScreenButtonHandler.SetButtonState(true);
		}

		// Token: 0x060019BC RID: 6588 RVA: 0x0007650E File Offset: 0x0007470E
		private void OnPointerUp()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnPointerUp", Array.Empty<object>());
			}
			this.onScreenButtonHandler.SetButtonState(false);
		}

		// Token: 0x04001466 RID: 5222
		[Header("UIMajorEquipmentAlternateActionOnScreenControl")]
		[SerializeField]
		private Selectable selectable;

		// Token: 0x04001467 RID: 5223
		[SerializeField]
		private UIOnScreenStickHandler onScreenStick;

		// Token: 0x04001468 RID: 5224
		[SerializeField]
		private UIOnScreenButtonHandler onScreenButtonHandler;
	}
}
