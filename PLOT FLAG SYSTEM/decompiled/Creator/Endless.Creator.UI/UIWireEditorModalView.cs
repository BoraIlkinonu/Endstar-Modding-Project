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

public class UIWireEditorModalView : UIGameObject, IBackable, IValidatable
{
	[HideInInspector]
	public UnityEvent OnDisplay = new UnityEvent();

	[HideInInspector]
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
	private TweenCollection displayTweens;

	[SerializeField]
	private TweenCollection hideTweens;

	[SerializeField]
	private InterfaceReference<IUIChildLayoutable>[] layoutables = Array.Empty<InterfaceReference<IUIChildLayoutable>>();

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	public bool IsOpen { get; private set; }

	public UIWireView Wire { get; private set; }

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		hideTweens.OnAllTweenCompleted.AddListener(SetInactive);
		hideTweens.SetToEnd();
		base.gameObject.SetActive(value: false);
		MonoBehaviourSingleton<UIWiringManager>.Instance.OnObjectSelected.AddListener(OnWireObjectSelected);
	}

	public void OnBack()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnBack");
		}
		Hide();
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

	public void InspectWire(UIWireView wire)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "InspectWire", wire.WireId);
			DebugUtility.Log($"Emitter Param Count: {wire.EmitterNode.NodeEvent.ParamList.Count}, Receiver Param Count: {wire.ReceiverNode.NodeEvent.ParamList.Count}", this);
			foreach (EndlessParameterInfo param in wire.ReceiverNode.NodeEvent.ParamList)
			{
				DebugUtility.Log($"DisplayName: {param.DisplayName}, DataType: {param.DataType}", this);
			}
		}
		Wire = wire;
		emitterNameText.text = wire.EmitterNode.NodeEvent.MemberName;
		receiverNameText.text = wire.ReceiverNode.NodeEvent.MemberName;
		bool active = UIWireUtility.CanOverrideEmitterContextualValue(wire.EmitterNode.NodeEvent.ParamList, wire.ReceiverNode.NodeEvent.ParamList);
		overrideEmitterContextualValueContainer.gameObject.SetActive(active);
		bool flag = WiringUtilities.GetWireEntry(wire.EmitterNode.InspectedObjectId, wire.EmitterNode.MemberName, wire.ReceiverNode.InspectedObjectId, wire.ReceiverNode.MemberName).StaticParameters.Length != 0;
		overrideEmitterContextualValueToggle.SetIsOn(flag, suppressOnChange: true);
		wiringPropertyModifier.DisplayExistingWire(wire);
		SetWiringPropertyModifierVisibility(flag);
		Display();
		RequestLayout();
	}

	public void Display()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Display");
		}
		if (hideTweens.IsAnyTweening())
		{
			hideTweens.Cancel();
		}
		IsOpen = true;
		displayTweens.Tween();
		base.gameObject.SetActive(value: true);
		OnDisplay.Invoke();
		if (!MonoBehaviourSingleton<BackManager>.Instance.HasContext(this))
		{
			MonoBehaviourSingleton<BackManager>.Instance.ClaimContext(this);
		}
	}

	public void Hide()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Hide");
		}
		if (displayTweens.IsAnyTweening())
		{
			displayTweens.Cancel();
		}
		IsOpen = false;
		hideTweens.Tween();
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

	private void OnWireObjectSelected()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnWireObjectSelected");
		}
		if (IsOpen)
		{
			Hide();
		}
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

	private void SetInactive()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetInactive");
		}
		base.gameObject.SetActive(value: false);
		wiringPropertyModifier.Clean();
	}
}
