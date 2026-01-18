using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIInventoryController : UIGameObject
{
	[SerializeField]
	private UIInventoryView view;

	[Range(0f, 100f)]
	[SerializeField]
	private float dropEventIfOverlapPercentageIsEqualToOrOver = 50f;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	[SerializeField]
	private bool superVerboseLogging;

	private Inventory Inventory => view.Inventory;

	private IReadOnlyList<UIInventorySlotView> InventorySlots => view.InventorySlots;

	private void OnEnable()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEnable");
		}
		UIItemView.InstanceDraggedAction = (Action<UIItemView>)Delegate.Combine(UIItemView.InstanceDraggedAction, new Action<UIItemView>(OnItemDragged));
		UIItemView.DragEndAction = (Action<UIItemView>)Delegate.Combine(UIItemView.DragEndAction, new Action<UIItemView>(OnItemDragEnded));
	}

	private void OnDisable()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDisable");
		}
		UIItemView.InstanceDraggedAction = (Action<UIItemView>)Delegate.Remove(UIItemView.InstanceDraggedAction, new Action<UIItemView>(OnItemDragged));
		UIItemView.DragEndAction = (Action<UIItemView>)Delegate.Remove(UIItemView.DragEndAction, new Action<UIItemView>(OnItemDragEnded));
	}

	private void OnItemDragged(UIItemView item)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnItemDragged", item.DebugSafeName());
		}
		bool flag = IsEquippedSlotHandledDrop(item, tweenPositionIfNotValid: false);
		if (superVerboseLogging)
		{
			DebugUtility.Log(string.Format("{0}: {1}", "isEquippedSlotHandledDrop", flag), this);
		}
		if (flag)
		{
			return;
		}
		for (int i = 0; i < InventorySlots.Count; i++)
		{
			UIInventorySlotView uIInventorySlotView = InventorySlots[i];
			if (uIInventorySlotView.Model.Item == item.Model)
			{
				uIInventorySlotView.ViewDropFeedback(dropIsValid: false);
				continue;
			}
			if (item.DragHandler.LastPointerEventData == null)
			{
				DebugUtility.LogException(new NullReferenceException("item.DragHandler.LastPointerEventData is null!"), this);
				uIInventorySlotView.ViewDropFeedback(dropIsValid: false);
				continue;
			}
			bool flag2 = UnityEngine.RectTransformUtility.RectangleContainsScreenPoint(uIInventorySlotView.RectTransform, item.DragHandler.LastPointerEventData.position);
			if (superVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "pointerIsInInventorySlot", flag2), this);
			}
			if (!flag2)
			{
				if (verboseLogging)
				{
					DebugUtility.Log(string.Format("{0}.{1}.{2} is not within {3}[{4}]!", "item", "DragHandler", "LastPointerEventData", "inventorySlot", i), this);
				}
				uIInventorySlotView.ViewDropFeedback(dropIsValid: false);
			}
			else
			{
				bool dropIsValid = UIDropUtility.WouldBeValidDrop(item.RectTransform, uIInventorySlotView.RectTransform, dropEventIfOverlapPercentageIsEqualToOrOver, superVerboseLogging);
				uIInventorySlotView.ViewDropFeedback(dropIsValid);
			}
		}
	}

	private void OnItemDragEnded(UIItemView item)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnItemDragEnded", item.DebugSafeName());
		}
		bool flag = IsEquippedSlotHandledDrop(item, tweenPositionIfNotValid: true);
		if (superVerboseLogging)
		{
			DebugUtility.Log(string.Format("{0}: {1}", "isEquippedSlotHandledDrop", flag), this);
		}
		if (flag)
		{
			return;
		}
		int num = -1;
		int num2 = -1;
		for (int i = 0; i < InventorySlots.Count; i++)
		{
			UIInventorySlotView uIInventorySlotView = InventorySlots[i];
			if (uIInventorySlotView.Model.Item == item.Model)
			{
				if (superVerboseLogging)
				{
					DebugUtility.Log(string.Format("{0}: {1}", "oldIndex", num), this);
				}
				num = i;
			}
			if (item.DragHandler.LastPointerEventData == null)
			{
				DebugUtility.LogException(new NullReferenceException("item.DragHandler.LastPointerEventData is null!"), this);
				continue;
			}
			bool flag2 = UnityEngine.RectTransformUtility.RectangleContainsScreenPoint(uIInventorySlotView.RectTransform, item.DragHandler.LastPointerEventData.position);
			if (superVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "pointerIsInInventorySlot", flag2), this);
			}
			if (!flag2)
			{
				if (verboseLogging)
				{
					DebugUtility.Log(string.Format("{0}.{1}.{2} is not within {3}[{4}]!", "item", "DragHandler", "LastPointerEventData", "inventorySlot", i), this);
				}
			}
			else if (UIDropUtility.WouldBeValidDrop(item.RectTransform, uIInventorySlotView.RectTransform, dropEventIfOverlapPercentageIsEqualToOrOver, superVerboseLogging))
			{
				num2 = i;
			}
		}
		if (superVerboseLogging)
		{
			DebugUtility.Log(string.Format("{0}: {1}, {2}: {3}", "oldIndex", num, "newIndex", num2), this);
		}
		if (num == num2)
		{
			return;
		}
		if (num < 0 || num2 < 0)
		{
			if (num >= 0)
			{
				Inventory.DropItemFromSlot(num);
			}
		}
		else
		{
			Inventory.Swap(num, num2);
		}
	}

	private bool IsEquippedSlotHandledDrop(UIItemView item, bool tweenPositionIfNotValid)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "IsEquippedSlotHandledDrop", item.DebugSafeName(), tweenPositionIfNotValid);
		}
		foreach (UIEquippedSlotController equippedSlotController in MonoBehaviourSingleton<UIEquipmentView>.Instance.EquippedSlotControllers)
		{
			UIEquippedSlotController.ItemDropStates itemDropState = equippedSlotController.GetItemDropState(item, tweenPositionIfNotValid);
			if (itemDropState == UIEquippedSlotController.ItemDropStates.IsNotValidType || itemDropState == UIEquippedSlotController.ItemDropStates.IsValid)
			{
				return true;
			}
		}
		return false;
	}
}
