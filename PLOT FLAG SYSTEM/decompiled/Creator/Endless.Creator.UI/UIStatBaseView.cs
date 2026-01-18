using System;
using Endless.Gameplay;
using Endless.Gameplay.Stats;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared;
using UnityEngine;

namespace Endless.Creator.UI;

public abstract class UIStatBaseView<TModel> : UIBaseView<TModel, UIStatBaseView<TModel>.Styles>, IUIInteractable where TModel : StatBase
{
	public enum Styles
	{
		Default
	}

	[SerializeField]
	private UIInputField identifierInputField;

	[SerializeField]
	private UILocalizedStringPresenter messageInputField;

	[SerializeField]
	private UIIntPresenter priorityControl;

	[SerializeField]
	private UIInventoryLibraryReferencePresenter inventoryIconControl;

	public override Styles Style { get; protected set; }

	public event Action<string> IdentifierChanged;

	public event Action<LocalizedString> MessageChanged;

	public event Action<int> OrderChanged;

	public event Action<InventoryLibraryReference> InventoryIconChanged;

	protected virtual void Start()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("Start", this);
		}
		identifierInputField.onValueChanged.AddListener(InvokeIdentifierChanged);
		messageInputField.OnModelChanged += InvokeMessageChanged;
		priorityControl.OnModelChanged += InvokePriorityChanged;
		inventoryIconControl.OnModelChanged += InvokeInventoryIconChanged;
	}

	protected virtual void OnDestroy()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("OnDestroy", this);
		}
		identifierInputField.onValueChanged.RemoveListener(InvokeIdentifierChanged);
		messageInputField.OnModelChanged -= InvokeMessageChanged;
		priorityControl.OnModelChanged -= InvokePriorityChanged;
		inventoryIconControl.OnModelChanged -= InvokeInventoryIconChanged;
	}

	public override void View(TModel model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "View", "model", model), this);
		}
		identifierInputField.SetTextWithoutNotify(model.Identifier);
		messageInputField.SetModel(model.Message, triggerOnModelChanged: false);
		priorityControl.SetModel(model.Order, triggerOnModelChanged: false);
		inventoryIconControl.SetModel(model.InventoryIcon, triggerOnModelChanged: false);
	}

	public override void Clear()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("Clear", this);
		}
		identifierInputField.Clear(triggerEvent: false);
		messageInputField.Clear();
		priorityControl.Clear();
		inventoryIconControl.Clear();
	}

	public void SetInteractable(bool interactable)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetInteractable", "interactable", interactable), this);
		}
		identifierInputField.interactable = interactable;
		if (messageInputField.Viewable is IUIInteractable iUIInteractable)
		{
			iUIInteractable.SetInteractable(interactable);
		}
		if (priorityControl.Viewable is IUIInteractable iUIInteractable2)
		{
			iUIInteractable2.SetInteractable(interactable);
		}
		if (inventoryIconControl.Viewable is IUIInteractable iUIInteractable3)
		{
			iUIInteractable3.SetInteractable(interactable);
		}
	}

	private void InvokeIdentifierChanged(string identifier)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("InvokeIdentifierChanged ( identifier: " + identifier + " )", this);
		}
		this.IdentifierChanged?.Invoke(identifier);
	}

	private void InvokeMessageChanged(object message)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "InvokeMessageChanged", "message", message), this);
		}
		this.MessageChanged?.Invoke((LocalizedString)message);
	}

	private void InvokePriorityChanged(object priority)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "InvokePriorityChanged", "priority", priority), this);
		}
		int obj = (int)priority;
		this.OrderChanged?.Invoke(obj);
	}

	private void InvokeInventoryIconChanged(object inventoryIcon)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "InvokeInventoryIconChanged", "inventoryIcon", inventoryIcon), this);
		}
		InventoryLibraryReference obj = inventoryIcon as InventoryLibraryReference;
		this.InventoryIconChanged?.Invoke(obj);
	}
}
