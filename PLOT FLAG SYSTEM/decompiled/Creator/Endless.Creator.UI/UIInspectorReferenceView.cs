using System;
using Endless.Gameplay;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public abstract class UIInspectorReferenceView<TModel, TViewStyle> : UIBaseView<TModel, TViewStyle>, IUIInteractable, IReadAndWritePermissible, IInspectorReferenceViewable where TModel : InspectorReference where TViewStyle : Enum
{
	[SerializeField]
	private TextMeshProUGUI referenceName;

	[SerializeField]
	private UIButton clearButton;

	[SerializeField]
	private UIButton openIEnumerableWindowButton;

	[SerializeField]
	private InterfaceReference<IUILayoutable>[] layoutables = Array.Empty<InterfaceReference<IUILayoutable>>();

	private bool referenceIsEmpty;

	protected Permissions Permission { get; private set; } = Permissions.ReadWrite;

	public event Action OnClear;

	public event Action OnOpenIEnumerableWindow;

	protected virtual void Start()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("Start", this);
		}
		clearButton.onClick.AddListener(OnClearButtonPressed);
		openIEnumerableWindowButton.onClick.AddListener(OnOpenSelectionWindowButtonPressed);
		SetPermission(Permission);
	}

	public override void View(TModel model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "View", "model", model), this);
		}
		referenceName.text = GetReferenceName(model);
		referenceIsEmpty = GetReferenceIsEmpty(model);
		HandleClearButtonVisibility();
		Layout();
	}

	public override void Clear()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("Clear", this);
		}
		Layout();
	}

	public virtual void SetInteractable(bool interactable)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetInteractable", "interactable", interactable), this);
		}
		clearButton.interactable = interactable;
		openIEnumerableWindowButton.interactable = interactable;
	}

	public virtual void SetPermission(Permissions permission)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetPermission", "permission", permission), this);
		}
		Permission = permission;
		HandleClearButtonVisibility();
		openIEnumerableWindowButton.gameObject.SetActive(permission == Permissions.ReadWrite);
		Layout();
	}

	protected abstract string GetReferenceName(TModel model);

	protected virtual bool GetReferenceIsEmpty(TModel model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "GetReferenceIsEmpty", "model", model), this);
		}
		return model.IsReferenceEmpty();
	}

	protected void SetObjectNameAndClearButtonVisibility(string objectName, bool visible)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", "SetObjectNameAndClearButtonVisibility", "objectName", objectName, "visible", visible), this);
		}
		referenceName.text = objectName;
		clearButton.gameObject.SetActive(visible);
		Layout();
	}

	protected void Layout()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("Layout", this);
		}
		layoutables.Layout();
		layoutables.RequestLayout();
	}

	private void OnClearButtonPressed()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("OnClearButtonPressed", this);
		}
		this.OnClear?.Invoke();
	}

	private void OnOpenSelectionWindowButtonPressed()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("OnOpenSelectionWindowButtonPressed", this);
		}
		this.OnOpenIEnumerableWindow?.Invoke();
	}

	private void HandleClearButtonVisibility()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("HandleClearButtonVisibility", this);
		}
		clearButton.gameObject.SetActive(!referenceIsEmpty && Permission == Permissions.ReadWrite);
	}
}
