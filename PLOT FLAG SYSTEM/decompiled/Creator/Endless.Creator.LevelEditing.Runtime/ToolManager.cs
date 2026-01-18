using System;
using System.Collections.Generic;
using Endless.Creator.UI;
using Endless.Data;
using Endless.Gameplay;
using Endless.Gameplay.Mobile;
using Endless.Shared;
using Endless.Shared.UI;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Endless.Creator.LevelEditing.Runtime;

public class ToolManager : MonoBehaviourSingleton<ToolManager>
{
	public UnityEvent<bool> OnActiveChange = new UnityEvent<bool>();

	public UnityEvent<EndlessTool> OnToolChange = new UnityEvent<EndlessTool>();

	public UnityEvent<EndlessTool> OnSetActiveToolToSameTool = new UnityEvent<EndlessTool>();

	[SerializeField]
	private Camera raycastCamera;

	[FormerlySerializedAs("secondaryInteractionDeadZone")]
	[SerializeField]
	private float alternateActionDeadZone = 0.01f;

	[SerializeField]
	private EndlessTool[] tools;

	[SerializeField]
	private bool enableHotKeys = true;

	[SerializeField]
	private UIToolTypeColorDictionary toolTypeColorDictionary;

	private EndlessTool activeTool;

	private bool active;

	private bool blockToolInput;

	private bool mainInputIsDown;

	private bool alternateInputIsDown;

	private bool isMobile;

	private EndlessSharedInputActions sharedInputActions;

	private PlayerInputActions playerInputActions;

	private Vector3 mousePositionOnFirstAlternatePress = Vector3.zero;

	private Dictionary<ToolType, EndlessTool> toolMap = new Dictionary<ToolType, EndlessTool>();

	public EndlessTool ActiveTool => activeTool;

	public bool IsActive => active;

	private EventSystem CurrentEventSystem => EventSystem.current;

	private bool CanUseHotKey
	{
		get
		{
			if (enableHotKeys && InputManager.InputUnrestricted && !MonoBehaviourSingleton<UIModalManager>.Instance.ModalIsDisplaying)
			{
				return !MonoBehaviourSingleton<UIWindowManager>.Instance.IsDisplayingType<UIScriptWindowView>();
			}
			return false;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		EndlessTool[] array = tools;
		foreach (EndlessTool endlessTool in array)
		{
			toolMap.Add(endlessTool.ToolType, endlessTool);
		}
		playerInputActions = new PlayerInputActions();
		sharedInputActions = new EndlessSharedInputActions();
		isMobile = MobileUtility.IsMobile;
	}

	private void Start()
	{
		if (raycastCamera == null)
		{
			raycastCamera = MonoBehaviourSingleton<CameraController>.Instance.GameplayCamera;
		}
		if (tools.Length != 0)
		{
			SetActiveTool_Internal(tools[0]);
		}
		NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorStarted.AddListener(Activate);
		NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorEnded.AddListener(OnCreatorEnded);
		NetworkBehaviourSingleton<CreatorManager>.Instance.OnLeavingSession.AddListener(OnLeavingSession);
	}

	private void Update()
	{
		if (!active || !(activeTool != null))
		{
			return;
		}
		Vector3 vector = EndlessInput.MousePosition();
		if (isMobile)
		{
			vector = playerInputActions.Player.PrimaryPointerPosition.ReadValue<Vector2>();
		}
		Ray activeRay = raycastCamera.ScreenPointToRay(vector);
		activeRay.origin += activeRay.direction * 1f;
		Debug.DrawLine(activeRay.origin, activeRay.direction * 10000f, Color.blue);
		Debug.DrawRay(activeRay.origin, activeRay.direction, Color.red);
		activeTool.ActiveRay = activeRay;
		if (activeTool.PerformsLineCast)
		{
			activeTool.PerformAndCacheLineCast();
		}
		bool flag = true;
		bool flag2 = false;
		bool flag3 = false;
		if (playerInputActions.Player.AlternativeToolAction.IsPressed() && !alternateInputIsDown)
		{
			alternateInputIsDown = true;
			flag3 = true;
			mousePositionOnFirstAlternatePress = vector;
		}
		else if (!playerInputActions.Player.AlternativeToolAction.IsPressed() && alternateInputIsDown)
		{
			alternateInputIsDown = false;
			flag3 = true;
			if ((vector - mousePositionOnFirstAlternatePress).magnitude >= alternateActionDeadZone)
			{
				flag = false;
			}
		}
		if ((playerInputActions.Player.MainToolAction.IsPressed() && !mainInputIsDown) || (!playerInputActions.Player.MainToolAction.IsPressed() && mainInputIsDown))
		{
			mainInputIsDown = !mainInputIsDown;
			flag2 = true;
		}
		bool flag4 = false;
		if (sharedInputActions.Player.Back.WasPressedThisFrame())
		{
			flag4 = true;
			mainInputIsDown = false;
		}
		bool flag5 = mainInputIsDown && flag2;
		bool flag6 = !mainInputIsDown && flag2;
		_ = alternateInputIsDown;
		bool flag7 = !alternateInputIsDown && flag3;
		bool num = CurrentEventSystem.IsPointerOverGameObject();
		bool flag8 = false;
		if (num && flag5)
		{
			blockToolInput = true;
		}
		if (MonoBehaviourSingleton<UIScreenManager>.Instance.IsDisplaying)
		{
			blockToolInput = true;
		}
		if (isMobile && PinchInputHandler.IsAnyInstancePinching)
		{
			flag4 = true;
		}
		if (!blockToolInput)
		{
			if (!flag4)
			{
				if (flag5 && !flag8)
				{
					activeTool.ToolPressed();
				}
				else if (mainInputIsDown)
				{
					activeTool.ToolHeld();
				}
				if (flag && flag7 && !flag8)
				{
					activeTool.ToolSecondaryPressed();
				}
				else if (flag6)
				{
					activeTool.ToolReleased();
				}
			}
			else
			{
				activeTool.Reset();
			}
		}
		if (blockToolInput && flag6 && !MonoBehaviourSingleton<UIScreenManager>.Instance.IsDisplaying)
		{
			blockToolInput = false;
		}
		if (!blockToolInput && (!isMobile || (isMobile && mainInputIsDown)))
		{
			activeTool.UpdateTool();
		}
	}

	public void Activate()
	{
		blockToolInput = false;
		active = true;
		if ((bool)activeTool)
		{
			activeTool.HandleSelected();
		}
		if (!sharedInputActions.Player.enabled)
		{
			sharedInputActions.Enable();
		}
		if (!playerInputActions.Player.enabled)
		{
			playerInputActions.Player.EmptyTool.performed += ActivateEmptyTool;
			playerInputActions.Player.PaintingTool.performed += ActivatePaintingTool;
			playerInputActions.Player.PropBasedTool.performed += ActivatePropBasedTool;
			playerInputActions.Player.EraseTool.performed += ActivateEraseTool;
			playerInputActions.Player.WiringTool.performed += ActivateWiringTool;
			playerInputActions.Player.InspectorTool.performed += ActivateInspectorTool;
			playerInputActions.Player.CopyTool.performed += ActivateCopyTool;
			playerInputActions.Player.MoveTool.performed += ActivateMoveTool;
			playerInputActions.Player.Enable();
		}
		OnActiveChange.Invoke(active);
	}

	public void OnCreatorEnded()
	{
		active = false;
		EndlessTool[] array = tools;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].CreatorExited();
		}
		if (playerInputActions.Player.enabled)
		{
			playerInputActions.Player.EmptyTool.performed -= ActivateEmptyTool;
			playerInputActions.Player.PaintingTool.performed -= ActivatePaintingTool;
			playerInputActions.Player.PropBasedTool.performed -= ActivatePropBasedTool;
			playerInputActions.Player.EraseTool.performed -= ActivateEraseTool;
			playerInputActions.Player.WiringTool.performed -= ActivateWiringTool;
			playerInputActions.Player.InspectorTool.performed -= ActivateInspectorTool;
			playerInputActions.Player.CopyTool.performed -= ActivateCopyTool;
			playerInputActions.Player.MoveTool.performed -= ActivateMoveTool;
			playerInputActions.Disable();
		}
		if (sharedInputActions.Player.enabled)
		{
			sharedInputActions.Disable();
		}
		SetActiveTool(ToolType.Empty);
		OnActiveChange.Invoke(active);
	}

	public void SetActiveTool(ToolType toolType)
	{
		SetActiveTool(toolMap[toolType]);
	}

	public void SetActiveTool(EndlessTool tool)
	{
		if (activeTool != tool)
		{
			try
			{
				CustomEvent e = new CustomEvent("toolSelected") { { "toolType", tool.ToolTypeName } };
				AnalyticsService.Instance.RecordEvent(e);
			}
			catch (Exception innerException)
			{
				Debug.LogException(new Exception("Analytics exception in ToolManager.SetActiveTool", innerException));
			}
			SetActiveTool_Internal(tool);
		}
		else
		{
			OnSetActiveToolToSameTool.Invoke(tool);
		}
	}

	private void SetActiveTool_Internal(EndlessTool newActiveTool)
	{
		if ((bool)activeTool && activeTool != newActiveTool)
		{
			activeTool.HandleDeselected();
		}
		activeTool = newActiveTool;
		if (activeTool != null)
		{
			activeTool.AutoPlace3DCursor = true;
			activeTool.HandleSelected();
			if (activeTool.ToolType != ToolType.Wiring)
			{
				MonoBehaviourSingleton<CellMarker>.Instance.SetColor(toolTypeColorDictionary[activeTool.ToolType]);
			}
		}
		OnToolChange.Invoke(activeTool);
	}

	private void SetActiveToolViaHotKey<T>() where T : EndlessTool
	{
		if (!CanUseHotKey)
		{
			return;
		}
		T val = RequestToolInstance<T>();
		if (activeTool != val)
		{
			try
			{
				CustomEvent e = new CustomEvent("toolSelected") { { "toolType", val.ToolTypeName } };
				AnalyticsService.Instance.RecordEvent(e);
			}
			catch (Exception innerException)
			{
				Debug.LogException(new Exception("Analytics exception in ToolManager.SetActiveTool", innerException));
			}
			SetActiveTool_Internal(val);
		}
		else
		{
			OnSetActiveToolToSameTool.Invoke(val);
		}
	}

	public T RequestToolInstance<T>()
	{
		for (int i = 0; i < tools.Length; i++)
		{
			EndlessTool endlessTool = tools[i];
			if (endlessTool is T)
			{
				return (T)(object)((endlessTool is T) ? endlessTool : null);
			}
		}
		return default(T);
	}

	public EndlessTool GetTool(ToolType type)
	{
		return toolMap[type];
	}

	public int GetToolHotKey(ToolType type)
	{
		return tools.IndexOf(toolMap[type]);
	}

	private void ActivateEmptyTool(InputAction.CallbackContext context)
	{
		SetActiveToolViaHotKey<EmptyTool>();
	}

	private void ActivatePaintingTool(InputAction.CallbackContext context)
	{
		SetActiveToolViaHotKey<PaintingTool>();
	}

	private void ActivatePropBasedTool(InputAction.CallbackContext context)
	{
		SetActiveToolViaHotKey<PropBasedTool>();
	}

	private void ActivateEraseTool(InputAction.CallbackContext context)
	{
		SetActiveToolViaHotKey<EraseTool>();
	}

	private void ActivateWiringTool(InputAction.CallbackContext context)
	{
		SetActiveToolViaHotKey<WiringTool>();
	}

	private void ActivateInspectorTool(InputAction.CallbackContext context)
	{
		SetActiveToolViaHotKey<InspectorTool>();
	}

	private void ActivateCopyTool(InputAction.CallbackContext context)
	{
		SetActiveToolViaHotKey<CopyTool>();
	}

	private void ActivateMoveTool(InputAction.CallbackContext context)
	{
		SetActiveToolViaHotKey<MoveTool>();
	}

	private void OnLeavingSession()
	{
		active = false;
		EndlessTool[] array = tools;
		foreach (EndlessTool endlessTool in array)
		{
			try
			{
				endlessTool.SessionEnded();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		try
		{
			activeTool.HandleDeselected();
			activeTool = RequestToolInstance<EmptyTool>();
			activeTool.HandleSelected();
			MonoBehaviourSingleton<CellMarker>.Instance.SetColor(toolTypeColorDictionary[activeTool.ToolType]);
			OnToolChange.Invoke(activeTool);
			OnActiveChange.Invoke(active);
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
	}
}
