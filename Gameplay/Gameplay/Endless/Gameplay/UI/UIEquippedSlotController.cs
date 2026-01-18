using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

namespace Endless.Gameplay.UI
{
	// Token: 0x0200039E RID: 926
	public class UIEquippedSlotController : OnScreenControl
	{
		// Token: 0x170004DB RID: 1243
		// (get) Token: 0x0600178D RID: 6029 RVA: 0x0006D854 File Offset: 0x0006BA54
		// (set) Token: 0x0600178E RID: 6030 RVA: 0x0006D85C File Offset: 0x0006BA5C
		protected override string controlPathInternal
		{
			get
			{
				return this.controlPathValue;
			}
			set
			{
				this.controlPathValue = value;
			}
		}

		// Token: 0x170004DC RID: 1244
		// (get) Token: 0x0600178F RID: 6031 RVA: 0x0006D865 File Offset: 0x0006BA65
		private Inventory Inventory
		{
			get
			{
				return this.model.Inventory;
			}
		}

		// Token: 0x06001790 RID: 6032 RVA: 0x0006D872 File Offset: 0x0006BA72
		protected override void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			base.OnEnable();
			UIItemView.InstanceDraggedAction = (Action<UIItemView>)Delegate.Combine(UIItemView.InstanceDraggedAction, new Action<UIItemView>(this.OnItemDragged));
		}

		// Token: 0x06001791 RID: 6033 RVA: 0x0006D8B4 File Offset: 0x0006BAB4
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIEquipmentView>.Instance.RegisterEquippedSlotController(this);
			base.TryGetComponent<RectTransform>(out this.rectTransform);
			bool isMobile = MobileUtility.IsMobile;
			this.button.enabled = isMobile;
			if (!isMobile)
			{
				return;
			}
			this.button.PointerDownUnityEvent.AddListener(new UnityAction(this.OnButtonDown));
			this.button.PointerUpUnityEvent.AddListener(new UnityAction(this.OnButtonUp));
		}

		// Token: 0x06001792 RID: 6034 RVA: 0x0006D93F File Offset: 0x0006BB3F
		protected override void OnDisable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			base.OnDisable();
			UIItemView.InstanceDraggedAction = (Action<UIItemView>)Delegate.Remove(UIItemView.InstanceDraggedAction, new Action<UIItemView>(this.OnItemDragged));
		}

		// Token: 0x06001793 RID: 6035 RVA: 0x0006D980 File Offset: 0x0006BB80
		public UIEquippedSlotController.ItemDropStates GetItemDropState(UIItemView item, bool performActions)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "GetItemDropState", new object[]
				{
					item.DebugSafeName(true),
					performActions
				});
			}
			if (item.DragHandler.LastPointerEventData == null)
			{
				DebugUtility.LogException(new NullReferenceException("item.DragHandler.LastPointerEventData is null!"), this);
				return UIEquippedSlotController.ItemDropStates.IsNotInArea;
			}
			bool flag = global::UnityEngine.RectTransformUtility.RectangleContainsScreenPoint(this.rectTransform, item.DragHandler.LastPointerEventData.position);
			if (this.superVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "pointerIsInInventorySlot", flag), this);
			}
			if (!flag)
			{
				if (this.verboseLogging)
				{
					DebugUtility.Log("item.DragHandler.LastPointerEventData is not within " + base.gameObject.name + "!", this);
				}
				return UIEquippedSlotController.ItemDropStates.IsNotInArea;
			}
			bool flag2 = UIDropUtility.WouldBeValidDrop(item.RectTransform, this.rectTransform, this.dropEventIfOverlapPercentageIsEqualToOrOver, this.superVerboseLogging);
			if (this.superVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "wouldBeValidDrop", flag2), this);
			}
			if (!flag2)
			{
				return UIEquippedSlotController.ItemDropStates.IsNotInArea;
			}
			if (item.Model.InventorySlot == this.model.InventorySlotType)
			{
				if (performActions)
				{
					this.EquipSlot(item);
				}
				return UIEquippedSlotController.ItemDropStates.IsValid;
			}
			if (performActions)
			{
				TweenPosition tweenPosition = item.gameObject.AddComponent<TweenPosition>();
				tweenPosition.InSeconds = 0.25f;
				tweenPosition.To = MonoBehaviourSingleton<UIInventoryView>.Instance.GetItemPosition(item.Model);
				tweenPosition.Tween();
			}
			return UIEquippedSlotController.ItemDropStates.IsNotValidType;
		}

		// Token: 0x06001794 RID: 6036 RVA: 0x0006DAE4 File Offset: 0x0006BCE4
		private void EquipSlot(UIItemView item)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "EquipSlot", new object[] { item.DebugSafeName(true) });
			}
			int num = -1;
			for (int i = 0; i < this.Inventory.TotalInventorySlotCount; i++)
			{
				if (!(this.Inventory.GetSlot(i).Item != item.Model))
				{
					num = i;
					break;
				}
			}
			if (num < 0)
			{
				DebugUtility.LogError("Could not find item in Inventory!", this);
				return;
			}
			if (this.Inventory.GetEquippedSlotIndex(this.model.Index) == num)
			{
				return;
			}
			this.Inventory.EquipSlot(num, this.model.Index);
		}

		// Token: 0x06001795 RID: 6037 RVA: 0x0006DB92 File Offset: 0x0006BD92
		private void OnButtonDown()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnButtonDown", Array.Empty<object>());
			}
			base.SendValueToControl<float>(1f);
		}

		// Token: 0x06001796 RID: 6038 RVA: 0x0006DBB7 File Offset: 0x0006BDB7
		private void OnButtonUp()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnButtonUp", Array.Empty<object>());
			}
			base.SendValueToControl<float>(0f);
			this.OnDisable();
			this.OnEnable();
		}

		// Token: 0x06001797 RID: 6039 RVA: 0x0006DBE8 File Offset: 0x0006BDE8
		private void OnItemDragged(UIItemView item)
		{
			if (this.superVerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnItemDragged", new object[] { item.DebugSafeName(true) });
			}
			bool flag = this.GetItemDropState(item, false) == UIEquippedSlotController.ItemDropStates.IsValid;
			this.view.ViewDropFeedback(flag);
		}

		// Token: 0x040012F6 RID: 4854
		[SerializeField]
		private UIEquippedSlotModel model;

		// Token: 0x040012F7 RID: 4855
		[SerializeField]
		private UIEquippedSlotView view;

		// Token: 0x040012F8 RID: 4856
		[Range(0f, 100f)]
		[SerializeField]
		private float dropEventIfOverlapPercentageIsEqualToOrOver = 50f;

		// Token: 0x040012F9 RID: 4857
		[Header("Mobile")]
		[InputControl(layout = "Button")]
		[SerializeField]
		private string controlPathValue;

		// Token: 0x040012FA RID: 4858
		[SerializeField]
		private UIButton button;

		// Token: 0x040012FB RID: 4859
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040012FC RID: 4860
		[SerializeField]
		private bool superVerboseLogging;

		// Token: 0x040012FD RID: 4861
		private RectTransform rectTransform;

		// Token: 0x0200039F RID: 927
		public enum ItemDropStates
		{
			// Token: 0x040012FF RID: 4863
			IsNotInArea,
			// Token: 0x04001300 RID: 4864
			IsNotValidType,
			// Token: 0x04001301 RID: 4865
			IsValid
		}
	}
}
