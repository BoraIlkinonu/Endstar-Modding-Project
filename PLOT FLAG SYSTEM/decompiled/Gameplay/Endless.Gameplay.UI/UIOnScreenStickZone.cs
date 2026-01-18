using Endless.Gameplay.Mobile;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;

namespace Endless.Gameplay.UI;

public class UIOnScreenStickZone : UIGameObject, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IDragHandler
{
	[SerializeField]
	private Transform onScreenStickTransform;

	[SerializeField]
	private UIDisplayAndHideHandler onScreenStickDisplayAndHideHandler;

	[SerializeField]
	private OnScreenStick onScreenStick;

	[SerializeField]
	private bool blockInputIfPinchInputHandlerIsPinching;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private int? activePointerId;

	private Vector2 lastPosition;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		onScreenStickDisplayAndHideHandler.SetToHideEnd(triggerUnityEvent: false);
		if (blockInputIfPinchInputHandlerIsPinching)
		{
			PinchInputHandler.OnFirstPinchStarted += Cancel;
		}
	}

	private void OnDestroy()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDestroy");
		}
		if (blockInputIfPinchInputHandlerIsPinching)
		{
			PinchInputHandler.OnFirstPinchStarted -= Cancel;
		}
	}

	public void OnPointerDown(PointerEventData pointerEventData)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnPointerDown", pointerEventData);
		}
		if ((!blockInputIfPinchInputHandlerIsPinching || !PinchInputHandler.IsAnyInstancePinching) && !activePointerId.HasValue)
		{
			activePointerId = pointerEventData.pointerId;
			lastPosition = pointerEventData.position;
			onScreenStickTransform.position = pointerEventData.position;
			onScreenStickDisplayAndHideHandler.Display();
			onScreenStick.OnPointerDown(pointerEventData);
		}
	}

	public void OnDrag(PointerEventData pointerEventData)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDrag", pointerEventData);
		}
		if ((!blockInputIfPinchInputHandlerIsPinching || !PinchInputHandler.IsAnyInstancePinching) && activePointerId == pointerEventData.pointerId)
		{
			lastPosition = pointerEventData.position;
			onScreenStick.OnDrag(pointerEventData);
		}
	}

	public void OnPointerUp(PointerEventData pointerEventData)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnPointerUp", pointerEventData);
		}
		if ((!blockInputIfPinchInputHandlerIsPinching || !PinchInputHandler.IsAnyInstancePinching) && activePointerId == pointerEventData.pointerId)
		{
			onScreenStick.OnPointerUp(pointerEventData);
			onScreenStickDisplayAndHideHandler.Hide();
			activePointerId = null;
		}
	}

	private void Cancel()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Cancel");
		}
		if (activePointerId.HasValue)
		{
			PointerEventData eventData = new PointerEventData(EventSystem.current)
			{
				pointerId = activePointerId.Value,
				position = lastPosition,
				button = PointerEventData.InputButton.Left
			};
			onScreenStick.OnPointerUp(eventData);
			onScreenStickDisplayAndHideHandler.Hide();
			activePointerId = null;
		}
	}
}
