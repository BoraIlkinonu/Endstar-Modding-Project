using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

namespace Endless.Gameplay.UI;

public class UIOnScreenStickHandler : UIGameObject, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IDragHandler
{
	[SerializeField]
	private Graphic graphic;

	[SerializeField]
	private Selectable selectable;

	[SerializeField]
	private OnScreenStick onScreenStick;

	[Header("Sensitivity")]
	[SerializeField]
	private float sensitivity = 1f;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	public UnityEvent OnPointerDownUnityEvent { get; } = new UnityEvent();

	public UnityEvent OnPointerUpUnityEvent { get; } = new UnityEvent();

	public void SetMovementRange(float movementRange)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetMovementRange", movementRange);
		}
		onScreenStick.movementRange = movementRange;
	}

	public void OnPointerDown(PointerEventData pointerEventData)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnPointerDown", pointerEventData);
		}
		ScalePointerEventData(pointerEventData);
		onScreenStick.OnPointerDown(pointerEventData);
		OnPointerDownUnityEvent.Invoke();
	}

	public void OnPointerUp(PointerEventData pointerEventData)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnPointerUp", pointerEventData);
		}
		onScreenStick.OnPointerUp(pointerEventData);
		OnPointerUpUnityEvent.Invoke();
	}

	public void OnDrag(PointerEventData pointerEventData)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDrag", pointerEventData);
		}
		ScalePointerEventData(pointerEventData);
		onScreenStick.OnDrag(pointerEventData);
	}

	public void SetInteractable(bool interactable)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetInteractable", interactable);
		}
		graphic.raycastTarget = interactable;
		selectable.interactable = interactable;
	}

	private void ScalePointerEventData(PointerEventData pointerEventData)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ScalePointerEventData", pointerEventData);
		}
		pointerEventData.position *= sensitivity;
	}
}
