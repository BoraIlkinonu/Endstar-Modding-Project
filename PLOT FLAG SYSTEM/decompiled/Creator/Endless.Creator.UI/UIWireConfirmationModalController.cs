using System;
using Endless.Gameplay;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

[RequireComponent(typeof(UIWireConfirmationModalView))]
public class UIWireConfirmationModalController : UIGameObject
{
	[SerializeField]
	private UIToggle overrideEmitterContextualValueToggle;

	[SerializeField]
	private UIWirePropertyModifierView wiringPropertyModifier;

	[SerializeField]
	private UIWireColorDropdown colorDropdown;

	[SerializeField]
	private UIWiringRerouteView wiringReroute;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private UIWireConfirmationModalView view;

	private UIWiringManager WiringManager => MonoBehaviourSingleton<UIWiringManager>.Instance;

	private UIWireConfirmationModalView View
	{
		get
		{
			if (!view)
			{
				TryGetComponent<UIWireConfirmationModalView>(out view);
			}
			return view;
		}
	}

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		overrideEmitterContextualValueToggle.OnChange.AddListener(View.SetWiringPropertyModifierVisibility);
		colorDropdown.OnColorChanged += SetColor;
	}

	private void OnDestroy()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDestroy");
		}
		colorDropdown.OnColorChanged -= SetColor;
	}

	public void Confirm()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Confirm");
		}
		string[] storedParameterValues = ((!overrideEmitterContextualValueToggle.isActiveAndEnabled || overrideEmitterContextualValueToggle.IsOn) ? wiringPropertyModifier.StoredParameterValues : Array.Empty<string>());
		WiringManager.WireCreatorController.CreateWire(storedParameterValues, colorDropdown.Value[0].WireColor);
	}

	public void Cancel()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Cancel");
		}
		wiringReroute.HideRerouteSwitch();
		WiringManager.WireCreatorController.Restart();
	}

	private void SetColor(WireColor newColor)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetColor", newColor);
		}
		if ((bool)View.Wire)
		{
			View.Wire.SetColor(newColor);
		}
	}
}
