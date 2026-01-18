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
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Endless.Creator.LevelEditing.Runtime
{
	// Token: 0x02000395 RID: 917
	public class ToolManager : MonoBehaviourSingleton<ToolManager>
	{
		// Token: 0x1700029C RID: 668
		// (get) Token: 0x060011B4 RID: 4532 RVA: 0x00056F66 File Offset: 0x00055166
		public EndlessTool ActiveTool
		{
			get
			{
				return this.activeTool;
			}
		}

		// Token: 0x1700029D RID: 669
		// (get) Token: 0x060011B5 RID: 4533 RVA: 0x00056F6E File Offset: 0x0005516E
		public bool IsActive
		{
			get
			{
				return this.active;
			}
		}

		// Token: 0x1700029E RID: 670
		// (get) Token: 0x060011B6 RID: 4534 RVA: 0x00056F76 File Offset: 0x00055176
		private EventSystem CurrentEventSystem
		{
			get
			{
				return EventSystem.current;
			}
		}

		// Token: 0x1700029F RID: 671
		// (get) Token: 0x060011B7 RID: 4535 RVA: 0x00056F7D File Offset: 0x0005517D
		private bool CanUseHotKey
		{
			get
			{
				return this.enableHotKeys && InputManager.InputUnrestricted && !MonoBehaviourSingleton<UIModalManager>.Instance.ModalIsDisplaying && !MonoBehaviourSingleton<UIWindowManager>.Instance.IsDisplayingType<UIScriptWindowView>();
			}
		}

		// Token: 0x060011B8 RID: 4536 RVA: 0x00056FAC File Offset: 0x000551AC
		protected override void Awake()
		{
			base.Awake();
			foreach (EndlessTool endlessTool in this.tools)
			{
				this.toolMap.Add(endlessTool.ToolType, endlessTool);
			}
			this.playerInputActions = new PlayerInputActions();
			this.sharedInputActions = new EndlessSharedInputActions();
			this.isMobile = MobileUtility.IsMobile;
		}

		// Token: 0x060011B9 RID: 4537 RVA: 0x0005700C File Offset: 0x0005520C
		private void Start()
		{
			if (this.raycastCamera == null)
			{
				this.raycastCamera = MonoBehaviourSingleton<CameraController>.Instance.GameplayCamera;
			}
			if (this.tools.Length != 0)
			{
				this.SetActiveTool_Internal(this.tools[0]);
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorStarted.AddListener(new UnityAction(this.Activate));
			NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorEnded.AddListener(new UnityAction(this.OnCreatorEnded));
			NetworkBehaviourSingleton<CreatorManager>.Instance.OnLeavingSession.AddListener(new UnityAction(this.OnLeavingSession));
		}

		// Token: 0x060011BA RID: 4538 RVA: 0x000570A0 File Offset: 0x000552A0
		private void Update()
		{
			if (!this.active)
			{
				return;
			}
			if (this.activeTool != null)
			{
				Vector3 vector = EndlessInput.MousePosition();
				if (this.isMobile)
				{
					vector = this.playerInputActions.Player.PrimaryPointerPosition.ReadValue<Vector2>();
				}
				Ray ray = this.raycastCamera.ScreenPointToRay(vector);
				ray.origin += ray.direction * 1f;
				Debug.DrawLine(ray.origin, ray.direction * 10000f, Color.blue);
				Debug.DrawRay(ray.origin, ray.direction, Color.red);
				this.activeTool.ActiveRay = ray;
				if (this.activeTool.PerformsLineCast)
				{
					this.activeTool.PerformAndCacheLineCast();
				}
				bool flag = true;
				bool flag2 = false;
				bool flag3 = false;
				if (this.playerInputActions.Player.AlternativeToolAction.IsPressed() && !this.alternateInputIsDown)
				{
					this.alternateInputIsDown = true;
					flag3 = true;
					this.mousePositionOnFirstAlternatePress = vector;
				}
				else if (!this.playerInputActions.Player.AlternativeToolAction.IsPressed() && this.alternateInputIsDown)
				{
					this.alternateInputIsDown = false;
					flag3 = true;
					if ((vector - this.mousePositionOnFirstAlternatePress).magnitude >= this.alternateActionDeadZone)
					{
						flag = false;
					}
				}
				if ((this.playerInputActions.Player.MainToolAction.IsPressed() && !this.mainInputIsDown) || (!this.playerInputActions.Player.MainToolAction.IsPressed() && this.mainInputIsDown))
				{
					this.mainInputIsDown = !this.mainInputIsDown;
					flag2 = true;
				}
				bool flag4 = false;
				if (this.sharedInputActions.Player.Back.WasPressedThisFrame())
				{
					flag4 = true;
					this.mainInputIsDown = false;
				}
				bool flag5 = this.mainInputIsDown && flag2;
				bool flag6 = !this.mainInputIsDown && flag2;
				bool flag7 = this.alternateInputIsDown;
				bool flag8 = !this.alternateInputIsDown && flag3;
				bool flag9 = this.CurrentEventSystem.IsPointerOverGameObject();
				bool flag10 = false;
				if (flag9 && flag5)
				{
					this.blockToolInput = true;
				}
				if (MonoBehaviourSingleton<UIScreenManager>.Instance.IsDisplaying)
				{
					this.blockToolInput = true;
				}
				if (this.isMobile && PinchInputHandler.IsAnyInstancePinching)
				{
					flag4 = true;
				}
				if (!this.blockToolInput)
				{
					if (!flag4)
					{
						if (flag5 && !flag10)
						{
							this.activeTool.ToolPressed();
						}
						else if (this.mainInputIsDown)
						{
							this.activeTool.ToolHeld();
						}
						if (flag && flag8 && !flag10)
						{
							this.activeTool.ToolSecondaryPressed();
						}
						else if (flag6)
						{
							this.activeTool.ToolReleased();
						}
					}
					else
					{
						this.activeTool.Reset();
					}
				}
				if (this.blockToolInput && flag6 && !MonoBehaviourSingleton<UIScreenManager>.Instance.IsDisplaying)
				{
					this.blockToolInput = false;
				}
				if (!this.blockToolInput && (!this.isMobile || (this.isMobile && this.mainInputIsDown)))
				{
					this.activeTool.UpdateTool();
				}
			}
		}

		// Token: 0x060011BB RID: 4539 RVA: 0x000573AC File Offset: 0x000555AC
		public void Activate()
		{
			this.blockToolInput = false;
			this.active = true;
			if (this.activeTool)
			{
				this.activeTool.HandleSelected();
			}
			if (!this.sharedInputActions.Player.enabled)
			{
				this.sharedInputActions.Enable();
			}
			if (!this.playerInputActions.Player.enabled)
			{
				this.playerInputActions.Player.EmptyTool.performed += this.ActivateEmptyTool;
				this.playerInputActions.Player.PaintingTool.performed += this.ActivatePaintingTool;
				this.playerInputActions.Player.PropBasedTool.performed += this.ActivatePropBasedTool;
				this.playerInputActions.Player.EraseTool.performed += this.ActivateEraseTool;
				this.playerInputActions.Player.WiringTool.performed += this.ActivateWiringTool;
				this.playerInputActions.Player.InspectorTool.performed += this.ActivateInspectorTool;
				this.playerInputActions.Player.CopyTool.performed += this.ActivateCopyTool;
				this.playerInputActions.Player.MoveTool.performed += this.ActivateMoveTool;
				this.playerInputActions.Player.Enable();
			}
			this.OnActiveChange.Invoke(this.active);
		}

		// Token: 0x060011BC RID: 4540 RVA: 0x0005755C File Offset: 0x0005575C
		public void OnCreatorEnded()
		{
			this.active = false;
			EndlessTool[] array = this.tools;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].CreatorExited();
			}
			if (this.playerInputActions.Player.enabled)
			{
				this.playerInputActions.Player.EmptyTool.performed -= this.ActivateEmptyTool;
				this.playerInputActions.Player.PaintingTool.performed -= this.ActivatePaintingTool;
				this.playerInputActions.Player.PropBasedTool.performed -= this.ActivatePropBasedTool;
				this.playerInputActions.Player.EraseTool.performed -= this.ActivateEraseTool;
				this.playerInputActions.Player.WiringTool.performed -= this.ActivateWiringTool;
				this.playerInputActions.Player.InspectorTool.performed -= this.ActivateInspectorTool;
				this.playerInputActions.Player.CopyTool.performed -= this.ActivateCopyTool;
				this.playerInputActions.Player.MoveTool.performed -= this.ActivateMoveTool;
				this.playerInputActions.Disable();
			}
			if (this.sharedInputActions.Player.enabled)
			{
				this.sharedInputActions.Disable();
			}
			this.SetActiveTool(ToolType.Empty);
			this.OnActiveChange.Invoke(this.active);
		}

		// Token: 0x060011BD RID: 4541 RVA: 0x00057708 File Offset: 0x00055908
		public void SetActiveTool(ToolType toolType)
		{
			this.SetActiveTool(this.toolMap[toolType]);
		}

		// Token: 0x060011BE RID: 4542 RVA: 0x0005771C File Offset: 0x0005591C
		public void SetActiveTool(EndlessTool tool)
		{
			if (this.activeTool != tool)
			{
				try
				{
					CustomEvent customEvent = new CustomEvent("toolSelected") { { "toolType", tool.ToolTypeName } };
					AnalyticsService.Instance.RecordEvent(customEvent);
				}
				catch (Exception ex)
				{
					Debug.LogException(new Exception("Analytics exception in ToolManager.SetActiveTool", ex));
				}
				this.SetActiveTool_Internal(tool);
				return;
			}
			this.OnSetActiveToolToSameTool.Invoke(tool);
		}

		// Token: 0x060011BF RID: 4543 RVA: 0x00057798 File Offset: 0x00055998
		private void SetActiveTool_Internal(EndlessTool newActiveTool)
		{
			if (this.activeTool && this.activeTool != newActiveTool)
			{
				this.activeTool.HandleDeselected();
			}
			this.activeTool = newActiveTool;
			if (this.activeTool != null)
			{
				this.activeTool.AutoPlace3DCursor = true;
				this.activeTool.HandleSelected();
				if (this.activeTool.ToolType != ToolType.Wiring)
				{
					MonoBehaviourSingleton<CellMarker>.Instance.SetColor(this.toolTypeColorDictionary[this.activeTool.ToolType]);
				}
			}
			this.OnToolChange.Invoke(this.activeTool);
		}

		// Token: 0x060011C0 RID: 4544 RVA: 0x00057838 File Offset: 0x00055A38
		private void SetActiveToolViaHotKey<T>() where T : EndlessTool
		{
			if (!this.CanUseHotKey)
			{
				return;
			}
			T t = this.RequestToolInstance<T>();
			if (this.activeTool != t)
			{
				try
				{
					CustomEvent customEvent = new CustomEvent("toolSelected") { { "toolType", t.ToolTypeName } };
					AnalyticsService.Instance.RecordEvent(customEvent);
				}
				catch (Exception ex)
				{
					Debug.LogException(new Exception("Analytics exception in ToolManager.SetActiveTool", ex));
				}
				this.SetActiveTool_Internal(t);
				return;
			}
			this.OnSetActiveToolToSameTool.Invoke(t);
		}

		// Token: 0x060011C1 RID: 4545 RVA: 0x000578D8 File Offset: 0x00055AD8
		public T RequestToolInstance<T>()
		{
			for (int i = 0; i < this.tools.Length; i++)
			{
				EndlessTool endlessTool = this.tools[i];
				if (endlessTool is T)
				{
					return endlessTool as T;
				}
			}
			return default(T);
		}

		// Token: 0x060011C2 RID: 4546 RVA: 0x00057920 File Offset: 0x00055B20
		public EndlessTool GetTool(ToolType type)
		{
			return this.toolMap[type];
		}

		// Token: 0x060011C3 RID: 4547 RVA: 0x0005792E File Offset: 0x00055B2E
		public int GetToolHotKey(ToolType type)
		{
			return this.tools.IndexOf(this.toolMap[type]);
		}

		// Token: 0x060011C4 RID: 4548 RVA: 0x00057947 File Offset: 0x00055B47
		private void ActivateEmptyTool(InputAction.CallbackContext context)
		{
			this.SetActiveToolViaHotKey<EmptyTool>();
		}

		// Token: 0x060011C5 RID: 4549 RVA: 0x0005794F File Offset: 0x00055B4F
		private void ActivatePaintingTool(InputAction.CallbackContext context)
		{
			this.SetActiveToolViaHotKey<PaintingTool>();
		}

		// Token: 0x060011C6 RID: 4550 RVA: 0x00057957 File Offset: 0x00055B57
		private void ActivatePropBasedTool(InputAction.CallbackContext context)
		{
			this.SetActiveToolViaHotKey<PropBasedTool>();
		}

		// Token: 0x060011C7 RID: 4551 RVA: 0x0005795F File Offset: 0x00055B5F
		private void ActivateEraseTool(InputAction.CallbackContext context)
		{
			this.SetActiveToolViaHotKey<EraseTool>();
		}

		// Token: 0x060011C8 RID: 4552 RVA: 0x00057967 File Offset: 0x00055B67
		private void ActivateWiringTool(InputAction.CallbackContext context)
		{
			this.SetActiveToolViaHotKey<WiringTool>();
		}

		// Token: 0x060011C9 RID: 4553 RVA: 0x0005796F File Offset: 0x00055B6F
		private void ActivateInspectorTool(InputAction.CallbackContext context)
		{
			this.SetActiveToolViaHotKey<InspectorTool>();
		}

		// Token: 0x060011CA RID: 4554 RVA: 0x00057977 File Offset: 0x00055B77
		private void ActivateCopyTool(InputAction.CallbackContext context)
		{
			this.SetActiveToolViaHotKey<CopyTool>();
		}

		// Token: 0x060011CB RID: 4555 RVA: 0x0005797F File Offset: 0x00055B7F
		private void ActivateMoveTool(InputAction.CallbackContext context)
		{
			this.SetActiveToolViaHotKey<MoveTool>();
		}

		// Token: 0x060011CC RID: 4556 RVA: 0x00057988 File Offset: 0x00055B88
		private void OnLeavingSession()
		{
			this.active = false;
			foreach (EndlessTool endlessTool in this.tools)
			{
				try
				{
					endlessTool.SessionEnded();
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
			try
			{
				this.activeTool.HandleDeselected();
				this.activeTool = this.RequestToolInstance<EmptyTool>();
				this.activeTool.HandleSelected();
				MonoBehaviourSingleton<CellMarker>.Instance.SetColor(this.toolTypeColorDictionary[this.activeTool.ToolType]);
				this.OnToolChange.Invoke(this.activeTool);
				this.OnActiveChange.Invoke(this.active);
			}
			catch (Exception ex2)
			{
				Debug.LogException(ex2);
			}
		}

		// Token: 0x04000E8A RID: 3722
		public UnityEvent<bool> OnActiveChange = new UnityEvent<bool>();

		// Token: 0x04000E8B RID: 3723
		public UnityEvent<EndlessTool> OnToolChange = new UnityEvent<EndlessTool>();

		// Token: 0x04000E8C RID: 3724
		public UnityEvent<EndlessTool> OnSetActiveToolToSameTool = new UnityEvent<EndlessTool>();

		// Token: 0x04000E8D RID: 3725
		[SerializeField]
		private Camera raycastCamera;

		// Token: 0x04000E8E RID: 3726
		[FormerlySerializedAs("secondaryInteractionDeadZone")]
		[SerializeField]
		private float alternateActionDeadZone = 0.01f;

		// Token: 0x04000E8F RID: 3727
		[SerializeField]
		private EndlessTool[] tools;

		// Token: 0x04000E90 RID: 3728
		[SerializeField]
		private bool enableHotKeys = true;

		// Token: 0x04000E91 RID: 3729
		[SerializeField]
		private UIToolTypeColorDictionary toolTypeColorDictionary;

		// Token: 0x04000E92 RID: 3730
		private EndlessTool activeTool;

		// Token: 0x04000E93 RID: 3731
		private bool active;

		// Token: 0x04000E94 RID: 3732
		private bool blockToolInput;

		// Token: 0x04000E95 RID: 3733
		private bool mainInputIsDown;

		// Token: 0x04000E96 RID: 3734
		private bool alternateInputIsDown;

		// Token: 0x04000E97 RID: 3735
		private bool isMobile;

		// Token: 0x04000E98 RID: 3736
		private EndlessSharedInputActions sharedInputActions;

		// Token: 0x04000E99 RID: 3737
		private PlayerInputActions playerInputActions;

		// Token: 0x04000E9A RID: 3738
		private Vector3 mousePositionOnFirstAlternatePress = Vector3.zero;

		// Token: 0x04000E9B RID: 3739
		private Dictionary<ToolType, EndlessTool> toolMap = new Dictionary<ToolType, EndlessTool>();
	}
}
