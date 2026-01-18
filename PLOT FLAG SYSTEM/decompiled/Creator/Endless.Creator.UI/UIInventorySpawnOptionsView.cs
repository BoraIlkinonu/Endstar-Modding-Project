using System;
using Endless.Gameplay;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIInventorySpawnOptionsView : UIBaseView<InventorySpawnOptions, UIInventorySpawnOptionsView.Styles>, IUIInteractable
{
	public enum Styles
	{
		Default
	}

	[SerializeField]
	private UIButton editButton;

	[field: Header("UIInventorySpawnOptionsView")]
	[field: SerializeField]
	public override Styles Style { get; protected set; }

	public event Action OnEditPressed;

	private void Start()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		editButton.onClick.AddListener(OnEditButtonPressed);
	}

	public override void View(InventorySpawnOptions model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "View", model);
		}
	}

	public override void Clear()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
	}

	public void SetInteractable(bool interactable)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetInteractable", interactable);
		}
		editButton.interactable = interactable;
	}

	private void OnEditButtonPressed()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEditButtonPressed");
		}
		this.OnEditPressed?.Invoke();
	}
}
