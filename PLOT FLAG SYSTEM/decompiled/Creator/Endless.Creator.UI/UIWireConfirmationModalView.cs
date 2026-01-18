using System;
using Endless.Props.Scripting;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using Endless.Shared.Validation;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UIWireConfirmationModalView : UIGameObject, IBackable, IValidatable
{
	public UnityEvent OnDisplay = new UnityEvent();

	public UnityEvent OnHide = new UnityEvent();

	[SerializeField]
	private TextMeshProUGUI emitterNameText;

	[SerializeField]
	private TextMeshProUGUI receiverNameText;

	[SerializeField]
	private GameObject overrideEmitterContextualValueContainer;

	[SerializeField]
	private UIToggle overrideEmitterContextualValueToggle;

	[SerializeField]
	private UIWirePropertyModifierView wiringPropertyModifier;

	[SerializeField]
	private UIWireColorDropdown colorDropdown;

	[SerializeField]
	private TweenCollection displayTweens;

	[SerializeField]
	private TweenCollection hideTweens;

	[SerializeField]
	private UIWiringRerouteView wiringReroute;

	[SerializeField]
	private InterfaceReference<IUIChildLayoutable>[] layoutables = Array.Empty<InterfaceReference<IUIChildLayoutable>>();

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	public bool IsOpen { get; private set; }

	public UIWireView Wire { get; private set; }

	public bool CanOverrideEmitterContextualValue { get; private set; }

	private UIWiringManager WiringManager => MonoBehaviourSingleton<UIWiringManager>.Instance;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		hideTweens.OnAllTweenCompleted.AddListener(ToggleOff);
		hideTweens.SetToEnd();
		base.gameObject.SetActive(value: false);
	}

	public void OnBack()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnBack");
		}
		wiringReroute.HideRerouteSwitch();
		WiringManager.WireCreatorController.Restart();
	}

	[ContextMenu("Validate")]
	public void Validate()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Validate");
		}
		hideTweens.ValidateForNumberOfTweens();
	}

	public void Display(UIWireView wire, EndlessEventInfo receiverEndlessEventInfo)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Display", wire.WireId, receiverEndlessEventInfo.ParamList.Count);
			DebugUtility.Log($"Emitter Param Count: {wire.EmitterNode.NodeEvent.ParamList.Count}, Receiver Param Count: {wire.ReceiverNode.NodeEvent.ParamList.Count}", this);
			foreach (EndlessParameterInfo param in wire.ReceiverNode.NodeEvent.ParamList)
			{
				DebugUtility.Log($"DisplayName: {param.DisplayName}, DataType: {param.DataType}", this);
			}
		}
		Wire = wire;
		IsOpen = true;
		emitterNameText.text = wire.EmitterNode.MemberName;
		receiverNameText.text = wire.ReceiverNode.MemberName;
		wiringPropertyModifier.DisplayDefaultParameters(receiverEndlessEventInfo);
		CanOverrideEmitterContextualValue = UIWireUtility.CanOverrideEmitterContextualValue(wire.EmitterNode.NodeEvent.ParamList, wire.ReceiverNode.NodeEvent.ParamList);
		if (verboseLogging)
		{
			DebugUtility.Log(string.Format("{0}: {1}", "CanOverrideEmitterContextualValue", CanOverrideEmitterContextualValue), this);
		}
		overrideEmitterContextualValueContainer.SetActive(CanOverrideEmitterContextualValue);
		overrideEmitterContextualValueToggle.SetIsOn(state: false, suppressOnChange: true);
		if (CanOverrideEmitterContextualValue)
		{
			SetWiringPropertyModifierVisibility(visible: false);
		}
		displayTweens.Tween();
		base.gameObject.SetActive(value: true);
		OnDisplay.Invoke();
		if (!MonoBehaviourSingleton<BackManager>.Instance.HasContext(this))
		{
			MonoBehaviourSingleton<BackManager>.Instance.ClaimContext(this);
		}
		RequestLayout();
	}

	public void Hide()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Hide");
		}
		IsOpen = false;
		hideTweens.Tween();
		WiringManager.WiresView.ToggleDarkMode(state: false, WiringManager.WireEditorController.WireToEdit);
		OnHide.Invoke();
		MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this);
	}

	public void SetWiringPropertyModifierVisibility(bool visible)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetWiringPropertyModifierVisibility", visible);
		}
		wiringPropertyModifier.gameObject.SetActive(visible);
	}

	private void RequestLayout()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "RequestLayout");
		}
		layoutables.CollectLayoutGroupChildren();
		layoutables.RequestLayout();
	}

	private void ToggleOff()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ToggleOff");
		}
		colorDropdown.Clear();
		base.gameObject.SetActive(value: false);
		wiringPropertyModifier.Clean();
	}
}
