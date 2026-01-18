using Endless.Gameplay;
using Endless.Gameplay.UI;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Endless.Creator.UI;

public class UIJumpAndDownSlider : UISlider, IValidatable
{
	[Header("UIJumpAndDownSlider")]
	[SerializeField]
	private UIOnScreenButtonHandler jumpOnScreenButtonHandler;

	[SerializeField]
	private UIOnScreenButtonHandler downOnScreenButtonHandler;

	[SerializeField]
	private UIDisplayAndHideHandler ghostModeSlideRangeDisplayAndHideHandler;

	private PlayerNetworkController playerNetworkController;

	protected override void OnEnable()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("OnEnable", this);
		}
		base.OnEnable();
		if (Application.isPlaying)
		{
			PlayerReferenceManager localPlayerObject = MonoBehaviourSingleton<PlayerManager>.Instance.GetLocalPlayerObject();
			playerNetworkController = localPlayerObject.PlayerNetworkController;
		}
	}

	protected override void Start()
	{
		base.Start();
		if (Application.isPlaying)
		{
			base.onValueChanged.AddListener(OnSlide);
			ghostModeSlideRangeDisplayAndHideHandler.SetToHideEnd(triggerUnityEvent: true);
		}
	}

	protected override void OnDisable()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("OnDisable", this);
		}
		base.OnDisable();
		playerNetworkController = null;
	}

	[ContextMenu("Validate")]
	public void Validate()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Validate");
		}
		float num = 0f;
		float num2 = 1f;
		if (!Mathf.Approximately(base.minValue, num))
		{
			DebugUtility.LogError(string.Format("{0} expects the {1} to be '{2}'!", "UIJumpAndDownSlider", "minValue", num), this);
		}
		if (!Mathf.Approximately(base.maxValue, num2))
		{
			DebugUtility.LogError(string.Format("{0} expects the {1} to be '{2}'!", "UIJumpAndDownSlider", "maxValue", num2), this);
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		base.OnPointerDown(eventData);
		if (playerNetworkController.Ghost)
		{
			ghostModeSlideRangeDisplayAndHideHandler.Display();
		}
		else
		{
			jumpOnScreenButtonHandler.SetButtonState(down: true);
		}
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		base.OnPointerUp(eventData);
		jumpOnScreenButtonHandler.SetButtonState(down: false);
		downOnScreenButtonHandler.SetButtonState(down: false);
		SetValue(0.5f, suppressOnChange: true);
		ghostModeSlideRangeDisplayAndHideHandler.Hide();
	}

	private void OnSlide(float sliderValue)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "OnSlide", "sliderValue", sliderValue), this);
		}
		if (playerNetworkController.Ghost)
		{
			jumpOnScreenButtonHandler.SetButtonState((double)sliderValue >= 0.5);
			downOnScreenButtonHandler.SetButtonState(sliderValue < 0.5f);
		}
		else
		{
			SetValue(0.5f, suppressOnChange: true);
		}
	}
}
