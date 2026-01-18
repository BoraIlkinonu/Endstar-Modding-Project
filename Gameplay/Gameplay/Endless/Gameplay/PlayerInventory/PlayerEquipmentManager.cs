using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay.PlayerInventory
{
	// Token: 0x0200059F RID: 1439
	public class PlayerEquipmentManager : MonoBehaviour
	{
		// Token: 0x17000682 RID: 1666
		// (get) Token: 0x060022A9 RID: 8873 RVA: 0x0009ED5F File Offset: 0x0009CF5F
		public PlayerReferenceManager PlayerReferenceManager
		{
			get
			{
				return this.playerReferences;
			}
		}

		// Token: 0x060022AA RID: 8874 RVA: 0x0009ED67 File Offset: 0x0009CF67
		public void ShowEquipmentVisuals(Item item, float delay, bool skipAnimation = false)
		{
			if (skipAnimation)
			{
				this.ShowEquipmentVisuals(item);
				return;
			}
			if (this.activeSwapCoroutine != null)
			{
				base.StopCoroutine(this.activeSwapCoroutine);
			}
			this.activeSwapCoroutine = base.StartCoroutine(this.InvokeShowEquipmentVisuals(item, delay));
		}

		// Token: 0x060022AB RID: 8875 RVA: 0x0009ED9C File Offset: 0x0009CF9C
		private IEnumerator InvokeShowEquipmentVisuals(Item item, float delay)
		{
			yield return new WaitForSeconds(delay);
			AppearanceController apperanceController = this.playerReferences.ApperanceController;
			if (apperanceController != null)
			{
				apperanceController.AppearanceAnimator.Animator.SetTrigger("Hotswap");
			}
			yield return new WaitForSeconds(0.65f);
			this.ShowEquipmentVisuals(item);
			this.activeSwapCoroutine = null;
			yield break;
		}

		// Token: 0x060022AC RID: 8876 RVA: 0x0009EDB9 File Offset: 0x0009CFB9
		public void ShowEquipmentVisuals(Item item)
		{
			item.Equip(this.playerReferences);
		}

		// Token: 0x060022AD RID: 8877 RVA: 0x0009EDC8 File Offset: 0x0009CFC8
		public void HideEquipmentVisuals(SerializableGuid assetID)
		{
			Item equippedItem = this.playerReferences.Inventory.GetEquippedItem(assetID);
			if (equippedItem)
			{
				this.HideEquipmentVisuals(equippedItem);
			}
		}

		// Token: 0x060022AE RID: 8878 RVA: 0x0009EDF6 File Offset: 0x0009CFF6
		public void HideEquipmentVisuals(Item item, float delay)
		{
			base.StartCoroutine(this.InvokeHideEquipmentVisuals(item, delay));
		}

		// Token: 0x060022AF RID: 8879 RVA: 0x0009EE07 File Offset: 0x0009D007
		private IEnumerator InvokeHideEquipmentVisuals(Item item, float delay)
		{
			float num = NetClock.FixedDeltaTime * 13f;
			yield return new WaitForSeconds(delay + num);
			this.HideEquipmentVisuals(item);
			yield break;
		}

		// Token: 0x060022B0 RID: 8880 RVA: 0x0009EE24 File Offset: 0x0009D024
		public void HideEquipmentVisuals(Item item)
		{
			item.UnEquip(this.playerReferences);
		}

		// Token: 0x060022B1 RID: 8881 RVA: 0x0009EE34 File Offset: 0x0009D034
		public Transform GetBoneForEquipment(VisualEquipmentSlot slot)
		{
			string boneName = this.GetBoneName(slot);
			return this.playerReferences.PlayerNetworkController.AppearanceController.GetBone(boneName);
		}

		// Token: 0x060022B2 RID: 8882 RVA: 0x0009EE60 File Offset: 0x0009D060
		public void TransferEquipment(AppearanceAnimator newAnimator)
		{
			foreach (SerializableGuid serializableGuid in this.spawnedEquipment.Keys)
			{
				this.TransferEquipment(this.spawnedEquipment[serializableGuid], newAnimator);
			}
			this.ReinitialzeEquipmentAppearance(this.playerReferences.Inventory.GetEquippedItem(0));
			this.ReinitialzeEquipmentAppearance(this.playerReferences.Inventory.GetEquippedItem(1));
		}

		// Token: 0x060022B3 RID: 8883 RVA: 0x0009EEF4 File Offset: 0x0009D0F4
		private void TransferEquipment(PlayerEquipment equipment, AppearanceAnimator newAnimator)
		{
			string boneName = this.GetBoneName(equipment.EquipmentVisualSlot);
			equipment.transform.SetParent(newAnimator.GetBone(boneName));
			equipment.transform.localPosition = Vector3.zero;
			equipment.transform.localRotation = Quaternion.identity;
		}

		// Token: 0x060022B4 RID: 8884 RVA: 0x0009EF40 File Offset: 0x0009D140
		private void ReinitialzeEquipmentAppearance(Item item)
		{
			if (item)
			{
				item.UnEquip(this.playerReferences);
				item.Equip(this.playerReferences);
			}
		}

		// Token: 0x060022B5 RID: 8885 RVA: 0x0009EF64 File Offset: 0x0009D164
		private string GetBoneName(VisualEquipmentSlot slot)
		{
			switch (slot)
			{
			case VisualEquipmentSlot.LeftHand:
				return this.leftHandAttachBone;
			case VisualEquipmentSlot.RightHand:
			case VisualEquipmentSlot.BothHands:
				return this.rightHandAttachBone;
			case VisualEquipmentSlot.Hips:
				return this.hipsAttachBone;
			}
			return this.backAttachBone;
		}

		// Token: 0x060022B6 RID: 8886 RVA: 0x0009EFB0 File Offset: 0x0009D1B0
		public void SetAppearanceVisibility(InventoryUsableDefinition.EquipmentShowPriority majorItemVisibility, InventoryUsableDefinition.EquipmentShowPriority minorItemVisibility)
		{
			bool flag = false;
			Item equippedItem = this.playerReferences.Inventory.GetEquippedItem(0);
			Item equippedItem2 = this.playerReferences.Inventory.GetEquippedItem(1);
			if (equippedItem2 && minorItemVisibility == InventoryUsableDefinition.EquipmentShowPriority.Major)
			{
				Debug.LogWarning(string.Format("Minor items EUS should override visibility priority settings (GetShowPriority): {0}", equippedItem2.InventoryUsableDefinition), equippedItem2.InventoryUsableDefinition);
			}
			if (equippedItem && equippedItem2)
			{
				if (majorItemVisibility > minorItemVisibility && this.CheckVisualOverlap(equippedItem.EquipmentVisualSlot, equippedItem2.EquipmentVisualSlot))
				{
					flag = true;
				}
				else if (minorItemVisibility > majorItemVisibility && equippedItem.EquipmentVisualSlot == equippedItem2.EquipmentVisualSlot)
				{
					flag = true;
				}
			}
			if (equippedItem == null)
			{
				majorItemVisibility = InventoryUsableDefinition.EquipmentShowPriority.NotShown;
			}
			if (equippedItem2 == null)
			{
				minorItemVisibility = InventoryUsableDefinition.EquipmentShowPriority.NotShown;
			}
			if (flag)
			{
				if (majorItemVisibility > minorItemVisibility)
				{
					minorItemVisibility = InventoryUsableDefinition.EquipmentShowPriority.NotShown;
				}
				else
				{
					majorItemVisibility = InventoryUsableDefinition.EquipmentShowPriority.NotShown;
				}
			}
			if (equippedItem)
			{
				equippedItem.ToggleLocalVisibility(this.playerReferences, majorItemVisibility > InventoryUsableDefinition.EquipmentShowPriority.NotShown, majorItemVisibility > minorItemVisibility);
			}
			if (equippedItem2)
			{
				equippedItem2.ToggleLocalVisibility(this.playerReferences, minorItemVisibility > InventoryUsableDefinition.EquipmentShowPriority.NotShown, minorItemVisibility > majorItemVisibility);
			}
			int num = ((minorItemVisibility == InventoryUsableDefinition.EquipmentShowPriority.MinorInUse) ? 1 : ((majorItemVisibility == InventoryUsableDefinition.EquipmentShowPriority.Major) ? 0 : (-1)));
			this.playerReferences.ApperanceController.AppearanceAnimator.SetActiveEquipmentSlot(num);
		}

		// Token: 0x060022B7 RID: 8887 RVA: 0x0009F0CF File Offset: 0x0009D2CF
		private bool CheckVisualOverlap(VisualEquipmentSlot slot1, VisualEquipmentSlot slot2)
		{
			return slot1 == slot2 || ((slot1 == VisualEquipmentSlot.BothHands || slot2 == VisualEquipmentSlot.BothHands) && (slot1 == VisualEquipmentSlot.LeftHand || slot2 == VisualEquipmentSlot.LeftHand || slot1 == VisualEquipmentSlot.RightHand || slot2 == VisualEquipmentSlot.RightHand));
		}

		// Token: 0x060022B8 RID: 8888 RVA: 0x0009F0F0 File Offset: 0x0009D2F0
		public Item GetEquippedItemByEquipmentID(SerializableGuid equipmentGuid)
		{
			Item item = this.playerReferences.Inventory.GetEquippedItem(0);
			if (item && item.InventoryUsableDefinition.Guid == equipmentGuid)
			{
				return item;
			}
			item = this.playerReferences.Inventory.GetEquippedItem(1);
			if (item && item.InventoryUsableDefinition.Guid == equipmentGuid)
			{
				return item;
			}
			return null;
		}

		// Token: 0x04001B8D RID: 7053
		private const float ANIMATION_SWAP_TIME = 0.65f;

		// Token: 0x04001B8E RID: 7054
		[SerializeField]
		private string rightHandAttachBone = "Rig.AttachPoint.Hand.R";

		// Token: 0x04001B8F RID: 7055
		[SerializeField]
		private string leftHandAttachBone = "Rig.AttachPoint.Hand.L";

		// Token: 0x04001B90 RID: 7056
		[SerializeField]
		private string backAttachBone = "Rig.AttachPoint.Back";

		// Token: 0x04001B91 RID: 7057
		[SerializeField]
		private string hipsAttachBone = "Rig.AttachPoint.Lumbar";

		// Token: 0x04001B92 RID: 7058
		[SerializeField]
		private PlayerReferenceManager playerReferences;

		// Token: 0x04001B93 RID: 7059
		private Dictionary<SerializableGuid, PlayerEquipment> spawnedEquipment = new Dictionary<SerializableGuid, PlayerEquipment>();

		// Token: 0x04001B94 RID: 7060
		private Dictionary<SerializableGuid, Item> stackableInventoryItems = new Dictionary<SerializableGuid, Item>();

		// Token: 0x04001B95 RID: 7061
		private Item majorLocallyEquippedItem;

		// Token: 0x04001B96 RID: 7062
		private Item minorLocallyEquippedItem;

		// Token: 0x04001B97 RID: 7063
		private Coroutine activeSwapCoroutine;
	}
}
