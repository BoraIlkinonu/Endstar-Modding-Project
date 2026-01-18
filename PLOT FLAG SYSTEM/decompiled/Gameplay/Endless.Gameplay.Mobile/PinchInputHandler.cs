using System;
using System.Collections.Generic;
using Endless.Data;
using Endless.Shared;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

namespace Endless.Gameplay.Mobile;

public class PinchInputHandler : MonoBehaviour
{
	[SerializeField]
	private float pinchSpeed = 0.00075f;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private bool playerInputActionsCreated;

	private PlayerInputActions playerInputActions;

	private static readonly List<PinchInputHandler> instances = new List<PinchInputHandler>();

	private bool isPinching;

	public static bool IsAnyInstancePinching { get; private set; }

	public UnityEvent<float> OnPinchUnityEvent { get; } = new UnityEvent<float>();

	public bool IsPinching
	{
		get
		{
			return isPinching;
		}
		private set
		{
			if (isPinching != value)
			{
				isPinching = value;
				UpdateStaticPinchingState();
				if (isPinching && PinchCount == 1)
				{
					PinchInputHandler.OnFirstPinchStarted?.Invoke();
				}
				else if (!isPinching && PinchCount == 0)
				{
					PinchInputHandler.OnLastPinchEnded?.Invoke();
				}
			}
		}
	}

	private int PinchCount
	{
		get
		{
			int num = 0;
			foreach (PinchInputHandler instance in instances)
			{
				if (instance.isPinching)
				{
					num++;
				}
			}
			return num;
		}
	}

	public static event Action OnFirstPinchStarted;

	public static event Action OnLastPinchEnded;

	private void Awake()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Awake");
		}
		instances.Add(this);
		if (!MobileUtility.IsMobile)
		{
			base.enabled = false;
		}
		else if (!EnhancedTouchSupport.enabled)
		{
			EnhancedTouchSupport.Enable();
		}
	}

	private void OnEnable()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEnable");
		}
		if (!playerInputActionsCreated)
		{
			playerInputActions = new PlayerInputActions();
			playerInputActionsCreated = true;
		}
		playerInputActions.Player.Pinch.started += OnPinch;
		playerInputActions.Player.Pinch.performed += OnPinch;
		playerInputActions.Player.Pinch.canceled += OnPinch;
		playerInputActions.Player.Pinch.Enable();
	}

	private void OnDisable()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDisable");
		}
		IsPinching = false;
		if (playerInputActionsCreated)
		{
			playerInputActions.Player.Pinch.started -= OnPinch;
			playerInputActions.Player.Pinch.performed -= OnPinch;
			playerInputActions.Player.Pinch.canceled -= OnPinch;
			playerInputActions.Player.Pinch.Disable();
		}
	}

	private void OnDestroy()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDestroy");
		}
		instances.Remove(this);
		UpdateStaticPinchingState();
	}

	private static void UpdateStaticPinchingState()
	{
		bool isAnyInstancePinching = false;
		for (int i = 0; i < instances.Count; i++)
		{
			if (instances[i].isPinching)
			{
				isAnyInstancePinching = true;
				break;
			}
		}
		IsAnyInstancePinching = isAnyInstancePinching;
	}

	private void OnPinch(InputAction.CallbackContext callbackContext)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnPinch", callbackContext);
		}
		if (callbackContext.canceled)
		{
			IsPinching = false;
			return;
		}
		if (Touch.activeTouches.Count < 2)
		{
			IsPinching = false;
			return;
		}
		Touch touch = Touch.activeTouches[0];
		Touch touch2 = Touch.activeTouches[1];
		if (touch.history.Count < 1 || touch2.history.Count < 1)
		{
			IsPinching = false;
			return;
		}
		float num = Vector2.Distance(touch.screenPosition, touch2.screenPosition);
		float num2 = Vector2.Distance(touch.history[0].screenPosition, touch2.history[0].screenPosition);
		float arg = (num - num2) * pinchSpeed;
		OnPinchUnityEvent.Invoke(arg);
		IsPinching = true;
	}
}
