using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

namespace Endless.Gameplay.UI;

public class UIEquippedSlotController : OnScreenControl
{
	public enum ItemDropStates
	{
		IsNotInArea,
		IsNotValidType,
		IsValid
	}

	[SerializeField]
	private UIEquippedSlotModel model;

	[SerializeField]
	private UIEquippedSlotView view;

	[Range(0f, 100f)]
	[SerializeField]
	private float dropEventIfOverlapPercentageIsEqualToOrOver = 50f;

	[Header("Mobile")]
	[InputControl(layout = "Button")]
	[SerializeField]
	private string controlPathValue;

	[SerializeField]
	private UIButton button;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	[SerializeField]
	private bool superVerboseLogging;

	private RectTransform rectTransform;

	protected override string controlPathInternal
	{
		get
		{
			return controlPathValue;
		}
		set
		{
			controlPathValue = value;
		}
	}

	private Inventory Inventory => model.Inventory;

	protected override void OnEnable()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEnable");
		}
		base.OnEnable();
		UIItemView.InstanceDraggedAction = (Action<UIItemView>)Delegate.Combine(UIItemView.InstanceDraggedAction, new Action<UIItemView>(OnItemDragged));
	}

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		MonoBehaviourSingleton<UIEquipmentView>.Instance.RegisterEquippedSlotController(this);
		TryGetComponent<RectTransform>(out rectTransform);
		bool isMobile = MobileUtility.IsMobile;
		button.enabled = isMobile;
		if (isMobile)
		{
			button.PointerDownUnityEvent.AddListener(OnButtonDown);
			button.PointerUpUnityEvent.AddListener(OnButtonUp);
		}
	}

	protected override void OnDisable()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDisable");
		}
		base.OnDisable();
		UIItemView.InstanceDraggedAction = (Action<UIItemView>)Delegate.Remove(UIItemView.InstanceDraggedAction, new Action<UIItemView>(OnItemDragged));
	}

	public ItemDropStates GetItemDropState(UIItemView item, bool performActions)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "GetItemDropState", item.DebugSafeName(), performActions);
		}
		if (item.DragHandler.LastPointerEventData == null)
		{
			DebugUtility.LogException(new NullReferenceException("item.DragHandler.LastPointerEventData is null!"), this);
			return ItemDropStates.IsNotInArea;
		}
		bool flag = UnityEngine.RectTransformUtility.RectangleContainsScreenPoint(rectTransform, item.DragHandler.LastPointerEventData.position);
		if (superVerboseLogging)
		{
			DebugUtility.Log(string.Format("{0}: {1}", "pointerIsInInventorySlot", flag), this);
		}
		if (!flag)
		{
			if (verboseLogging)
			{
				DebugUtility.Log("item.DragHandler.LastPointerEventData is not within " + base.gameObject.name + "!", this);
			}
			return ItemDropStates.IsNotInArea;
		}
		bool flag2 = UIDropUtility.WouldBeValidDrop(item.RectTransform, rectTransform, dropEventIfOverlapPercentageIsEqualToOrOver, superVerboseLogging);
		if (superVerboseLogging)
		{
			DebugUtility.Log(string.Format("{0}: {1}", "wouldBeValidDrop", flag2), this);
		}
		if (!flag2)
		{
			return ItemDropStates.IsNotInArea;
		}
		if (item.Model.InventorySlot == model.InventorySlotType)
		{
			if (performActions)
			{
				EquipSlot(item);
			}
			return ItemDropStates.IsValid;
		}
		if (performActions)
		{
			TweenPosition tweenPosition = item.gameObject.AddComponent<TweenPosition>();
			tweenPosition.InSeconds = 0.25f;
			tweenPosition.To = MonoBehaviourSingleton<UIInventoryView>.Instance.GetItemPosition(item.Model);
			tweenPosition.Tween();
		}
		return ItemDropStates.IsNotValidType;
	}

	private void EquipSlot(UIItemView item)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "EquipSlot", item.DebugSafeName());
		}
		int num = -1;
		for (int i = 0; i < Inventory.TotalInventorySlotCount; i++)
		{
			if (!(Inventory.GetSlot(i).Item != item.Model))
			{
				num = i;
				break;
			}
		}
		if (num < 0)
		{
			DebugUtility.LogError("Could not find item in Inventory!", this);
		}
		else if (Inventory.GetEquippedSlotIndex(model.Index) != num)
		{
			Inventory.EquipSlot(num, model.Index);
		}
	}

	private void OnButtonDown()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnButtonDown");
		}
		SendValueToControl(1f);
	}

	private void OnButtonUp()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnButtonUp");
		}
		SendValueToControl(0f);
		OnDisable();
		OnEnable();
	}

	private void OnItemDragged(UIItemView item)
	{
		if (superVerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnItemDragged", item.DebugSafeName());
		}
		bool dropIsValid = GetItemDropState(item, performActions: false) == ItemDropStates.IsValid;
		view.ViewDropFeedback(dropIsValid);
	}
}
