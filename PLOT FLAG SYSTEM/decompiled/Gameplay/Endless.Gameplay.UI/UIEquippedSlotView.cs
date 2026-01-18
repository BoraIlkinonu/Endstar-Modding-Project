using System;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIEquippedSlotView : UIGameObject
{
	[SerializeField]
	private UIEquippedSlotModel model;

	[SerializeField]
	private UIDisplayAndHideHandler displayAndHideHandler;

	[SerializeField]
	private UIDisplayAndHideHandler occupiedBorderDisplayAndHideHandler;

	[SerializeField]
	private UIItemView item;

	[Header("Drop Feedback")]
	[SerializeField]
	private TweenCollection validDropTweenCollection;

	[SerializeField]
	private TweenCollection invalidDropTweenCollection;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private bool didTweenValidDropTweenCollection;

	private void OnEnable()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEnable");
		}
		UIItemView.DragEndAction = (Action<UIItemView>)Delegate.Combine(UIItemView.DragEndAction, new Action<UIItemView>(OnItemDragEnd));
	}

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		occupiedBorderDisplayAndHideHandler.SetToHideEnd(triggerUnityEvent: true);
		model.OnChanged.AddListener(View);
		View();
	}

	private void OnDisable()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDisable");
		}
		UIItemView.DragEndAction = (Action<UIItemView>)Delegate.Remove(UIItemView.DragEndAction, new Action<UIItemView>(OnItemDragEnd));
	}

	public void Initialize(float delay)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Initialize", delay);
		}
		displayAndHideHandler.SetDisplayDelay(delay);
		displayAndHideHandler.Display();
	}

	public void View()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "View");
		}
		bool flag = model.Item == null;
		item.View(model.Item);
		item.gameObject.SetActive(!flag);
		if (flag)
		{
			occupiedBorderDisplayAndHideHandler.Hide();
		}
		else
		{
			occupiedBorderDisplayAndHideHandler.Display();
		}
	}

	public void ViewDropFeedback(bool dropIsValid)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ViewDropFeedback", dropIsValid);
		}
		if (dropIsValid)
		{
			if (!didTweenValidDropTweenCollection)
			{
				validDropTweenCollection.Tween();
				didTweenValidDropTweenCollection = true;
			}
		}
		else if (didTweenValidDropTweenCollection)
		{
			invalidDropTweenCollection.Tween();
			didTweenValidDropTweenCollection = false;
		}
	}

	private void OnItemDragEnd(UIItemView item)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnItemDragEnd", item.DebugSafeName());
		}
		ViewDropFeedback(dropIsValid: false);
	}
}
