using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003AE RID: 942
	public class UIInventoryController : UIGameObject
	{
		// Token: 0x170004F1 RID: 1265
		// (get) Token: 0x06001805 RID: 6149 RVA: 0x0006F8A8 File Offset: 0x0006DAA8
		private Inventory Inventory
		{
			get
			{
				return this.view.Inventory;
			}
		}

		// Token: 0x170004F2 RID: 1266
		// (get) Token: 0x06001806 RID: 6150 RVA: 0x0006F8B5 File Offset: 0x0006DAB5
		private IReadOnlyList<UIInventorySlotView> InventorySlots
		{
			get
			{
				return this.view.InventorySlots;
			}
		}

		// Token: 0x06001807 RID: 6151 RVA: 0x0006F8C4 File Offset: 0x0006DAC4
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			UIItemView.InstanceDraggedAction = (Action<UIItemView>)Delegate.Combine(UIItemView.InstanceDraggedAction, new Action<UIItemView>(this.OnItemDragged));
			UIItemView.DragEndAction = (Action<UIItemView>)Delegate.Combine(UIItemView.DragEndAction, new Action<UIItemView>(this.OnItemDragEnded));
		}

		// Token: 0x06001808 RID: 6152 RVA: 0x0006F92C File Offset: 0x0006DB2C
		private void OnDisable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			UIItemView.InstanceDraggedAction = (Action<UIItemView>)Delegate.Remove(UIItemView.InstanceDraggedAction, new Action<UIItemView>(this.OnItemDragged));
			UIItemView.DragEndAction = (Action<UIItemView>)Delegate.Remove(UIItemView.DragEndAction, new Action<UIItemView>(this.OnItemDragEnded));
		}

		// Token: 0x06001809 RID: 6153 RVA: 0x0006F994 File Offset: 0x0006DB94
		private void OnItemDragged(UIItemView item)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnItemDragged", new object[] { item.DebugSafeName(true) });
			}
			bool flag = this.IsEquippedSlotHandledDrop(item, false);
			if (this.superVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "isEquippedSlotHandledDrop", flag), this);
			}
			if (flag)
			{
				return;
			}
			for (int i = 0; i < this.InventorySlots.Count; i++)
			{
				UIInventorySlotView uiinventorySlotView = this.InventorySlots[i];
				if (uiinventorySlotView.Model.Item == item.Model)
				{
					uiinventorySlotView.ViewDropFeedback(false);
				}
				else if (item.DragHandler.LastPointerEventData == null)
				{
					DebugUtility.LogException(new NullReferenceException("item.DragHandler.LastPointerEventData is null!"), this);
					uiinventorySlotView.ViewDropFeedback(false);
				}
				else
				{
					bool flag2 = global::UnityEngine.RectTransformUtility.RectangleContainsScreenPoint(uiinventorySlotView.RectTransform, item.DragHandler.LastPointerEventData.position);
					if (this.superVerboseLogging)
					{
						DebugUtility.Log(string.Format("{0}: {1}", "pointerIsInInventorySlot", flag2), this);
					}
					if (!flag2)
					{
						if (this.verboseLogging)
						{
							DebugUtility.Log(string.Format("{0}.{1}.{2} is not within {3}[{4}]!", new object[] { "item", "DragHandler", "LastPointerEventData", "inventorySlot", i }), this);
						}
						uiinventorySlotView.ViewDropFeedback(false);
					}
					else
					{
						bool flag3 = UIDropUtility.WouldBeValidDrop(item.RectTransform, uiinventorySlotView.RectTransform, this.dropEventIfOverlapPercentageIsEqualToOrOver, this.superVerboseLogging);
						uiinventorySlotView.ViewDropFeedback(flag3);
					}
				}
			}
		}

		// Token: 0x0600180A RID: 6154 RVA: 0x0006FB28 File Offset: 0x0006DD28
		private void OnItemDragEnded(UIItemView item)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnItemDragEnded", new object[] { item.DebugSafeName(true) });
			}
			bool flag = this.IsEquippedSlotHandledDrop(item, true);
			if (this.superVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "isEquippedSlotHandledDrop", flag), this);
			}
			if (flag)
			{
				return;
			}
			int num = -1;
			int num2 = -1;
			for (int i = 0; i < this.InventorySlots.Count; i++)
			{
				UIInventorySlotView uiinventorySlotView = this.InventorySlots[i];
				if (uiinventorySlotView.Model.Item == item.Model)
				{
					if (this.superVerboseLogging)
					{
						DebugUtility.Log(string.Format("{0}: {1}", "oldIndex", num), this);
					}
					num = i;
				}
				if (item.DragHandler.LastPointerEventData == null)
				{
					DebugUtility.LogException(new NullReferenceException("item.DragHandler.LastPointerEventData is null!"), this);
				}
				else
				{
					bool flag2 = global::UnityEngine.RectTransformUtility.RectangleContainsScreenPoint(uiinventorySlotView.RectTransform, item.DragHandler.LastPointerEventData.position);
					if (this.superVerboseLogging)
					{
						DebugUtility.Log(string.Format("{0}: {1}", "pointerIsInInventorySlot", flag2), this);
					}
					if (!flag2)
					{
						if (this.verboseLogging)
						{
							DebugUtility.Log(string.Format("{0}.{1}.{2} is not within {3}[{4}]!", new object[] { "item", "DragHandler", "LastPointerEventData", "inventorySlot", i }), this);
						}
					}
					else if (UIDropUtility.WouldBeValidDrop(item.RectTransform, uiinventorySlotView.RectTransform, this.dropEventIfOverlapPercentageIsEqualToOrOver, this.superVerboseLogging))
					{
						num2 = i;
					}
				}
			}
			if (this.superVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}, {2}: {3}", new object[] { "oldIndex", num, "newIndex", num2 }), this);
			}
			if (num == num2)
			{
				return;
			}
			if (num < 0 || num2 < 0)
			{
				if (num >= 0)
				{
					this.Inventory.DropItemFromSlot(num);
				}
				return;
			}
			this.Inventory.Swap(num, num2);
		}

		// Token: 0x0600180B RID: 6155 RVA: 0x0006FD38 File Offset: 0x0006DF38
		private bool IsEquippedSlotHandledDrop(UIItemView item, bool tweenPositionIfNotValid)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "IsEquippedSlotHandledDrop", new object[]
				{
					item.DebugSafeName(true),
					tweenPositionIfNotValid
				});
			}
			foreach (UIEquippedSlotController uiequippedSlotController in MonoBehaviourSingleton<UIEquipmentView>.Instance.EquippedSlotControllers)
			{
				UIEquippedSlotController.ItemDropStates itemDropState = uiequippedSlotController.GetItemDropState(item, tweenPositionIfNotValid);
				if (itemDropState == UIEquippedSlotController.ItemDropStates.IsNotValidType || itemDropState == UIEquippedSlotController.ItemDropStates.IsValid)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x04001348 RID: 4936
		[SerializeField]
		private UIInventoryView view;

		// Token: 0x04001349 RID: 4937
		[Range(0f, 100f)]
		[SerializeField]
		private float dropEventIfOverlapPercentageIsEqualToOrOver = 50f;

		// Token: 0x0400134A RID: 4938
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x0400134B RID: 4939
		[SerializeField]
		private bool superVerboseLogging;
	}
}
