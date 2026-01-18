using System.Collections;
using System.Collections.Generic;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay.PlayerInventory;

public class PlayerEquipmentManager : MonoBehaviour
{
	private const float ANIMATION_SWAP_TIME = 0.65f;

	[SerializeField]
	private string rightHandAttachBone = "Rig.AttachPoint.Hand.R";

	[SerializeField]
	private string leftHandAttachBone = "Rig.AttachPoint.Hand.L";

	[SerializeField]
	private string backAttachBone = "Rig.AttachPoint.Back";

	[SerializeField]
	private string hipsAttachBone = "Rig.AttachPoint.Lumbar";

	[SerializeField]
	private PlayerReferenceManager playerReferences;

	private Dictionary<SerializableGuid, PlayerEquipment> spawnedEquipment = new Dictionary<SerializableGuid, PlayerEquipment>();

	private Dictionary<SerializableGuid, Item> stackableInventoryItems = new Dictionary<SerializableGuid, Item>();

	private Item majorLocallyEquippedItem;

	private Item minorLocallyEquippedItem;

	private Coroutine activeSwapCoroutine;

	public PlayerReferenceManager PlayerReferenceManager => playerReferences;

	public void ShowEquipmentVisuals(Item item, float delay, bool skipAnimation = false)
	{
		if (skipAnimation)
		{
			ShowEquipmentVisuals(item);
			return;
		}
		if (activeSwapCoroutine != null)
		{
			StopCoroutine(activeSwapCoroutine);
		}
		activeSwapCoroutine = StartCoroutine(InvokeShowEquipmentVisuals(item, delay));
	}

	private IEnumerator InvokeShowEquipmentVisuals(Item item, float delay)
	{
		yield return new WaitForSeconds(delay);
		playerReferences.ApperanceController?.AppearanceAnimator.Animator.SetTrigger("Hotswap");
		yield return new WaitForSeconds(0.65f);
		ShowEquipmentVisuals(item);
		activeSwapCoroutine = null;
	}

	public void ShowEquipmentVisuals(Item item)
	{
		item.Equip(playerReferences);
	}

	public void HideEquipmentVisuals(SerializableGuid assetID)
	{
		Item equippedItem = playerReferences.Inventory.GetEquippedItem(assetID);
		if ((bool)equippedItem)
		{
			HideEquipmentVisuals(equippedItem);
		}
	}

	public void HideEquipmentVisuals(Item item, float delay)
	{
		StartCoroutine(InvokeHideEquipmentVisuals(item, delay));
	}

	private IEnumerator InvokeHideEquipmentVisuals(Item item, float delay)
	{
		float num = NetClock.FixedDeltaTime * 13f;
		yield return new WaitForSeconds(delay + num);
		HideEquipmentVisuals(item);
	}

	public void HideEquipmentVisuals(Item item)
	{
		item.UnEquip(playerReferences);
	}

	public Transform GetBoneForEquipment(VisualEquipmentSlot slot)
	{
		string boneName = GetBoneName(slot);
		return playerReferences.PlayerNetworkController.AppearanceController.GetBone(boneName);
	}

	public void TransferEquipment(AppearanceAnimator newAnimator)
	{
		foreach (SerializableGuid key in spawnedEquipment.Keys)
		{
			TransferEquipment(spawnedEquipment[key], newAnimator);
		}
		ReinitialzeEquipmentAppearance(playerReferences.Inventory.GetEquippedItem(0));
		ReinitialzeEquipmentAppearance(playerReferences.Inventory.GetEquippedItem(1));
	}

	private void TransferEquipment(PlayerEquipment equipment, AppearanceAnimator newAnimator)
	{
		string boneName = GetBoneName(equipment.EquipmentVisualSlot);
		equipment.transform.SetParent(newAnimator.GetBone(boneName));
		equipment.transform.localPosition = Vector3.zero;
		equipment.transform.localRotation = Quaternion.identity;
	}

	private void ReinitialzeEquipmentAppearance(Item item)
	{
		if ((bool)item)
		{
			item.UnEquip(playerReferences);
			item.Equip(playerReferences);
		}
	}

	private string GetBoneName(VisualEquipmentSlot slot)
	{
		switch (slot)
		{
		case VisualEquipmentSlot.RightHand:
		case VisualEquipmentSlot.BothHands:
			return rightHandAttachBone;
		case VisualEquipmentSlot.LeftHand:
			return leftHandAttachBone;
		case VisualEquipmentSlot.Hips:
			return hipsAttachBone;
		default:
			return backAttachBone;
		}
	}

	public void SetAppearanceVisibility(InventoryUsableDefinition.EquipmentShowPriority majorItemVisibility, InventoryUsableDefinition.EquipmentShowPriority minorItemVisibility)
	{
		bool flag = false;
		Item equippedItem = playerReferences.Inventory.GetEquippedItem(0);
		Item equippedItem2 = playerReferences.Inventory.GetEquippedItem(1);
		if ((bool)equippedItem2 && minorItemVisibility == InventoryUsableDefinition.EquipmentShowPriority.Major)
		{
			Debug.LogWarning($"Minor items EUS should override visibility priority settings (GetShowPriority): {equippedItem2.InventoryUsableDefinition}", equippedItem2.InventoryUsableDefinition);
		}
		if ((bool)equippedItem && (bool)equippedItem2)
		{
			if (majorItemVisibility > minorItemVisibility && CheckVisualOverlap(equippedItem.EquipmentVisualSlot, equippedItem2.EquipmentVisualSlot))
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
		if ((bool)equippedItem)
		{
			equippedItem.ToggleLocalVisibility(playerReferences, majorItemVisibility != InventoryUsableDefinition.EquipmentShowPriority.NotShown, majorItemVisibility > minorItemVisibility);
		}
		if ((bool)equippedItem2)
		{
			equippedItem2.ToggleLocalVisibility(playerReferences, minorItemVisibility != InventoryUsableDefinition.EquipmentShowPriority.NotShown, minorItemVisibility > majorItemVisibility);
		}
		int activeEquipmentSlot = ((minorItemVisibility == InventoryUsableDefinition.EquipmentShowPriority.MinorInUse) ? 1 : ((majorItemVisibility != InventoryUsableDefinition.EquipmentShowPriority.Major) ? (-1) : 0));
		playerReferences.ApperanceController.AppearanceAnimator.SetActiveEquipmentSlot(activeEquipmentSlot);
	}

	private bool CheckVisualOverlap(VisualEquipmentSlot slot1, VisualEquipmentSlot slot2)
	{
		if (slot1 == slot2)
		{
			return true;
		}
		if ((slot1 == VisualEquipmentSlot.BothHands || slot2 == VisualEquipmentSlot.BothHands) && (slot1 == VisualEquipmentSlot.LeftHand || slot2 == VisualEquipmentSlot.LeftHand || slot1 == VisualEquipmentSlot.RightHand || slot2 == VisualEquipmentSlot.RightHand))
		{
			return true;
		}
		return false;
	}

	public Item GetEquippedItemByEquipmentID(SerializableGuid equipmentGuid)
	{
		Item equippedItem = playerReferences.Inventory.GetEquippedItem(0);
		if ((bool)equippedItem && equippedItem.InventoryUsableDefinition.Guid == equipmentGuid)
		{
			return equippedItem;
		}
		equippedItem = playerReferences.Inventory.GetEquippedItem(1);
		if ((bool)equippedItem && equippedItem.InventoryUsableDefinition.Guid == equipmentGuid)
		{
			return equippedItem;
		}
		return null;
	}
}
