using System;
using Endless.Gameplay;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIPlayerReferenceView : UIBaseView<PlayerReference, UIPlayerReferenceView.Styles>, IUIInteractable
{
	public enum Styles
	{
		Default
	}

	[SerializeField]
	private UIToggle useContextToggle;

	[SerializeField]
	private GameObject playerNumberContainer;

	[SerializeField]
	private UIInputField playerNumberInputField;

	[field: Header("UIPlayerReferenceView")]
	[field: SerializeField]
	public override Styles Style { get; protected set; }

	public event Action<bool> OnUseContextChanged;

	public event Action<int> OnPlayerNumberChanged;

	private void Start()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		useContextToggle.OnChange.AddListener(InvokeOnUseContextChanged);
		playerNumberInputField.DeselectAndValueChangedUnityEvent.AddListener(InvokeOnPlayerNumberChanged);
	}

	public override void View(PlayerReference model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "View", model);
		}
		bool useContext = PlayerReferenceUtility.GetUseContext(model);
		useContextToggle.SetIsOn(useContext, suppressOnChange: true);
		playerNumberContainer.SetActive(!useContext);
		int playerNumber = PlayerReferenceUtility.GetPlayerNumber(model);
		playerNumberInputField.SetTextWithoutNotify(playerNumber.ToString());
	}

	public override void Clear()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
		playerNumberInputField.Clear(triggerEvent: false);
	}

	public void SetInteractable(bool interactable)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetInteractable", interactable);
		}
		useContextToggle.SetInteractable(interactable, tweenVisuals: false);
		playerNumberInputField.interactable = interactable;
	}

	private void InvokeOnUseContextChanged(bool useContext)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "InvokeOnUseContextChanged", useContext);
		}
		this.OnUseContextChanged?.Invoke(useContext);
	}

	private void InvokeOnPlayerNumberChanged(string playerNumberAsString)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "InvokeOnPlayerNumberChanged", playerNumberAsString);
		}
		if (!int.TryParse(playerNumberAsString, out var result))
		{
			DebugUtility.LogError(this, "InvokeOnPlayerNumberChanged", "Could not convert to int!", playerNumberAsString);
		}
		else
		{
			this.OnPlayerNumberChanged?.Invoke(result);
		}
	}
}
