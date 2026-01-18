using System;
using Endless.Data;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Endless.Gameplay
{
	// Token: 0x02000278 RID: 632
	public class PlayerNetworkController : EndlessNetworkBehaviour, IGameEndSubscriber, NetClock.IPreFixedUpdateSubscriber, NetClock.IRollbackSubscriber, NetClock.ISimulateFrameEnvironmentSubscriber, NetClock.ISimulateFrameActorsSubscriber, NetClock.ISimulateFrameLateSubscriber, NetClock.IPostFixedUpdateSubscriber, NetClock.ILoadingFrameSubscriber
	{
		// Token: 0x1700026B RID: 619
		// (get) Token: 0x06000D5A RID: 3418 RVA: 0x000487F1 File Offset: 0x000469F1
		public bool HasControl
		{
			get
			{
				return base.IsOwner;
			}
		}

		// Token: 0x1700026C RID: 620
		// (get) Token: 0x06000D5B RID: 3419 RVA: 0x000487F9 File Offset: 0x000469F9
		public RingBuffer<NetState> ClientStateRingBuffer
		{
			get
			{
				return this.clientStateRingBuffer;
			}
		}

		// Token: 0x1700026D RID: 621
		// (get) Token: 0x06000D5C RID: 3420 RVA: 0x00048801 File Offset: 0x00046A01
		public AppearanceController AppearanceController
		{
			get
			{
				return this.appearanceController;
			}
		}

		// Token: 0x1700026E RID: 622
		// (get) Token: 0x06000D5D RID: 3421 RVA: 0x00048809 File Offset: 0x00046A09
		// (set) Token: 0x06000D5E RID: 3422 RVA: 0x00048816 File Offset: 0x00046A16
		public bool DefaultGhostMode
		{
			get
			{
				return this.defaultGhostMode.Value;
			}
			set
			{
				this.defaultGhostMode.Value = value;
			}
		}

		// Token: 0x1700026F RID: 623
		// (get) Token: 0x06000D5F RID: 3423 RVA: 0x00048824 File Offset: 0x00046A24
		// (set) Token: 0x06000D60 RID: 3424 RVA: 0x0004882C File Offset: 0x00046A2C
		public bool Ghost { get; private set; }

		// Token: 0x17000270 RID: 624
		// (get) Token: 0x06000D61 RID: 3425 RVA: 0x00048835 File Offset: 0x00046A35
		// (set) Token: 0x06000D62 RID: 3426 RVA: 0x0004883D File Offset: 0x00046A3D
		public UnityEvent<bool> GhostChangedUnityEvent { get; private set; } = new UnityEvent<bool>();

		// Token: 0x17000271 RID: 625
		// (get) Token: 0x06000D63 RID: 3427 RVA: 0x00048846 File Offset: 0x00046A46
		public Endless.Gameplay.LuaEnums.InputSettings ActiveInputSettings
		{
			get
			{
				if (InputManager.InputUnrestricted)
				{
					return this.gameplayInputSettingsFlags.Value;
				}
				if (InputManager.CurrentInputState != InputManager.InputState.Cinematic)
				{
					return Endless.Gameplay.LuaEnums.InputSettings.None;
				}
				return NetworkBehaviourSingleton<CutsceneManager>.Instance.CurrentCutsceneInputSettings;
			}
		}

		// Token: 0x06000D64 RID: 3428 RVA: 0x0004886F File Offset: 0x00046A6F
		private void Awake()
		{
			this.playerInputActions = new PlayerInputActions();
			this.isMobile = MobileUtility.IsMobile;
		}

		// Token: 0x06000D65 RID: 3429 RVA: 0x00048888 File Offset: 0x00046A88
		private void OnEnable()
		{
			this.playerInputActions.Player.Ghost.performed += this.ToggleGhostMode;
			this.playerInputActions.Player.Enable();
		}

		// Token: 0x06000D66 RID: 3430 RVA: 0x000488CC File Offset: 0x00046ACC
		private new void Start()
		{
			NetClock.Register(this);
			this.holdRotationForCamera = MonoBehaviourSingleton<CameraController>.Instance.MainPlayerCamera;
			this.holdRotationForCameraName = this.holdRotationForCamera.Name;
		}

		// Token: 0x06000D67 RID: 3431 RVA: 0x000488F5 File Offset: 0x00046AF5
		public void EndlessGameEnd()
		{
			this.holdRotationForCamera = MonoBehaviourSingleton<CameraController>.Instance.MainPlayerCamera;
			this.holdRotationForCameraName = this.holdRotationForCamera.Name;
		}

		// Token: 0x06000D68 RID: 3432 RVA: 0x00048918 File Offset: 0x00046B18
		private void OnDisable()
		{
			NetClock.Unregister(this);
			this.playerInputActions.Player.Ghost.performed -= this.ToggleGhostMode;
			this.playerInputActions.Player.Disable();
		}

		// Token: 0x06000D69 RID: 3433 RVA: 0x00048962 File Offset: 0x00046B62
		public void SetGameplayInputSettingsFlags(Endless.Gameplay.LuaEnums.InputSettings settings)
		{
			if (base.IsServer)
			{
				this.gameplayInputSettingsFlags.Value = settings;
			}
		}

		// Token: 0x06000D6A RID: 3434 RVA: 0x00048978 File Offset: 0x00046B78
		public void EnableAimIK(Vector3 worldPosition)
		{
			this.aimIK = true;
			this.SetAimIKPosition(worldPosition);
		}

		// Token: 0x06000D6B RID: 3435 RVA: 0x00048988 File Offset: 0x00046B88
		public void DisableAimIK()
		{
			this.aimIK = false;
		}

		// Token: 0x06000D6C RID: 3436 RVA: 0x00048991 File Offset: 0x00046B91
		public void SetAimIKPosition(Vector3 worldPosition)
		{
			if (this.focusInputPoint)
			{
				this.focusInputPoint.position = worldPosition;
			}
		}

		// Token: 0x06000D6D RID: 3437 RVA: 0x000489AC File Offset: 0x00046BAC
		protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer)
		{
			if (serializer.IsWriter)
			{
				this.initialPosition_client = this.playerReferences.PlayerController.CurrentState.Position;
				this.initialRotation_client = this.playerReferences.PlayerController.CurrentState.CharacterRotation;
			}
			serializer.SerializeValue(ref this.initialPosition_client);
			serializer.SerializeValue<float>(ref this.initialRotation_client, default(FastBufferWriter.ForPrimitives));
		}

		// Token: 0x06000D6E RID: 3438 RVA: 0x00048A18 File Offset: 0x00046C18
		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			if (base.IsServer)
			{
				this.initialPosition_client = this.playerReferences.PlayerController.CurrentState.Position;
				this.initialRotation_client = this.playerReferences.PlayerController.CurrentState.CharacterRotation;
			}
			this.appearanceController = global::UnityEngine.Object.Instantiate<AppearanceController>(this.playerAppearanceControllerPrefab);
			this.appearanceController.RuntimeInit(this.playerReferences, base.IsServer ? AppearanceController.AppearancePerspective.Server : (this.HasControl ? AppearanceController.AppearancePerspective.Extrapolate : AppearanceController.AppearancePerspective.Interpolate), this.initialPosition_client, this.initialRotation_client);
			if (this.HasControl)
			{
				MonoBehaviourSingleton<CameraController>.Instance.InitAppearance(this.appearanceController, this.controllerType);
				this.Ghost = this.DefaultGhostMode;
				this.GhostChangedUnityEvent.Invoke(this.Ghost);
				this.playerReferences.PlayerController.HandleSpawnedAsOwner(base.transform.rotation.eulerAngles.y);
				this.focusInputPoint = new GameObject("FOCUS").transform;
				this.playerInputActions.Player.Reload.performed += this.OnReloadPressed;
			}
			if (base.IsServer)
			{
				ref NetInput referenceFromBuffer = ref this.ServerInputRingBuffer.GetReferenceFromBuffer(NetClock.CurrentFrame);
				referenceFromBuffer.NetFrame = NetClock.CurrentFrame;
				referenceFromBuffer.Ghost = this.DefaultGhostMode;
				this.ServerInputRingBuffer.UpdateValue(ref referenceFromBuffer);
				if (this.playerReferences.HealthComponent != null)
				{
					this.playerReferences.HealthComponent.OnHealthZeroed_Internal.AddListener(new UnityAction(this.OnHealthZeroed));
					this.playerReferences.HealthComponent.ShouldSaveAndLoad = false;
				}
			}
		}

		// Token: 0x06000D6F RID: 3439 RVA: 0x00048BD0 File Offset: 0x00046DD0
		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();
			global::UnityEngine.Object.Destroy(this.appearanceController.gameObject);
			if (this.focusInputPoint)
			{
				global::UnityEngine.Object.Destroy(this.focusInputPoint.gameObject);
			}
			this.playerInputActions.Player.Reload.performed -= this.OnReloadPressed;
			if (this.playerReferences.HealthComponent != null)
			{
				this.playerReferences.HealthComponent.OnHealthZeroed_Internal.RemoveListener(new UnityAction(this.OnHealthZeroed));
			}
		}

		// Token: 0x06000D70 RID: 3440 RVA: 0x00048C68 File Offset: 0x00046E68
		private void Update()
		{
			if (this.HasControl)
			{
				Endless.Gameplay.LuaEnums.InputSettings activeInputSettings = this.ActiveInputSettings;
				if (this.playerReferences.PlayerInteractor)
				{
					this.playerReferences.PlayerInteractor.enabled = activeInputSettings.CanInteract();
				}
				Vector2 vector = this.playerInputActions.Player.Move.ReadValue<Vector2>();
				if (activeInputSettings.CanMove())
				{
					this.run = activeInputSettings.CanRun();
					this.horizontal = Mathf.RoundToInt(vector.x);
					this.vertical = Mathf.RoundToInt(vector.y);
					this.downInputSwitch.ValueUpdated(this.playerInputActions.Player.Down.IsPressed());
				}
				else
				{
					this.run = false;
					this.horizontal = 0;
					this.vertical = 0;
					this.downInputSwitch.ValueUpdated(false);
				}
				if (activeInputSettings.CanJump())
				{
					this.jumpInputSwitch.ValueUpdated(this.playerInputActions.Player.Jump.IsPressed() && activeInputSettings.CanJump());
				}
				else
				{
					this.jumpInputSwitch.ValueUpdated(false);
				}
				if (activeInputSettings.CanUseEquipment())
				{
					bool flag = !EventSystem.current.IsPointerOverGameObject() || this.isMobile;
					this.primaryEquipmentInputSwitchP1.ValueUpdated(flag && !this.playerInputActions.Player.EnableCursor.IsPressed() && this.playerInputActions.Player.MajorEquipmentPrimaryAction.IsPressed());
					this.primaryEquipmentInputSwitchP2.ValueUpdated(flag && !this.playerInputActions.Player.EnableCursor.IsPressed() && this.playerInputActions.Player.MajorEquipmentAlternateAction.IsPressed());
					this.secondaryEquipmentInputSwitch.ValueUpdated(flag && this.playerInputActions.Player.MinorEquipment.IsPressed());
					if (this.playerInputActions.Player.Inventory1.triggered)
					{
						this.TryEquipFromInventory(0);
						return;
					}
					if (this.playerInputActions.Player.Inventory2.triggered)
					{
						this.TryEquipFromInventory(1);
						return;
					}
					if (this.playerInputActions.Player.Inventory3.triggered)
					{
						this.TryEquipFromInventory(2);
						return;
					}
					if (this.playerInputActions.Player.Inventory4.triggered)
					{
						this.TryEquipFromInventory(3);
						return;
					}
					if (this.playerInputActions.Player.Inventory5.triggered)
					{
						this.TryEquipFromInventory(4);
						return;
					}
					if (this.playerInputActions.Player.Inventory6.triggered)
					{
						this.TryEquipFromInventory(5);
						return;
					}
					if (this.playerInputActions.Player.Inventory7.triggered)
					{
						this.TryEquipFromInventory(6);
						return;
					}
					if (this.playerInputActions.Player.Inventory8.triggered)
					{
						this.TryEquipFromInventory(7);
						return;
					}
					if (this.playerInputActions.Player.Inventory9.triggered)
					{
						this.TryEquipFromInventory(8);
						return;
					}
					if (this.playerInputActions.Player.Inventory10.triggered)
					{
						this.TryEquipFromInventory(9);
						return;
					}
				}
				else
				{
					this.primaryEquipmentInputSwitchP1.ValueUpdated(false);
					this.primaryEquipmentInputSwitchP2.ValueUpdated(false);
					this.secondaryEquipmentInputSwitch.ValueUpdated(false);
				}
			}
		}

		// Token: 0x06000D71 RID: 3441 RVA: 0x00048FD1 File Offset: 0x000471D1
		public void TryEquipFromInventory(int inventoryIndex)
		{
			if (this.playerReferences.Inventory)
			{
				this.playerReferences.Inventory.EquipSlot(inventoryIndex);
			}
		}

		// Token: 0x06000D72 RID: 3442 RVA: 0x00002DB0 File Offset: 0x00000FB0
		public void SetEquipmentInputSwitch(NetInput.InputEquipmentSlot inputEquipmentSlot)
		{
		}

		// Token: 0x06000D73 RID: 3443 RVA: 0x00048FF6 File Offset: 0x000471F6
		public void SetPrimaryEquipmentInputP1(bool p1)
		{
			this.primaryEquipmentInputSwitchP1.ValueUpdated(p1);
		}

		// Token: 0x06000D74 RID: 3444 RVA: 0x00049004 File Offset: 0x00047204
		public void SetPrimaryEquipmentInputP2(bool p2)
		{
			this.primaryEquipmentInputSwitchP2.ValueUpdated(p2);
		}

		// Token: 0x06000D75 RID: 3445 RVA: 0x00049012 File Offset: 0x00047212
		public void SetSecondaryEquipmentInput(bool secondary)
		{
			this.secondaryEquipmentInputSwitch.ValueUpdated(secondary);
		}

		// Token: 0x06000D76 RID: 3446 RVA: 0x00049020 File Offset: 0x00047220
		public void SetHorizontal(int input)
		{
			this.horizontal = input;
		}

		// Token: 0x06000D77 RID: 3447 RVA: 0x00049029 File Offset: 0x00047229
		public void SetVertical(int input)
		{
			this.vertical = input;
		}

		// Token: 0x06000D78 RID: 3448 RVA: 0x00049032 File Offset: 0x00047232
		public void ToggleJump(bool state)
		{
			this.jumpInputSwitch.ValueUpdated(state);
		}

		// Token: 0x06000D79 RID: 3449 RVA: 0x00049040 File Offset: 0x00047240
		public void ToggleDownInput(bool state)
		{
			this.downInputSwitch.ValueUpdated(state);
		}

		// Token: 0x06000D7A RID: 3450 RVA: 0x0004904E File Offset: 0x0004724E
		public void ToggleGhostMode(InputAction.CallbackContext callbackContext)
		{
			this.ToggleGhostMode();
		}

		// Token: 0x06000D7B RID: 3451 RVA: 0x00049056 File Offset: 0x00047256
		public void ToggleGhostMode()
		{
			if (!InputManager.InputUnrestricted)
			{
				return;
			}
			this.Ghost = !this.Ghost;
			this.GhostChangedUnityEvent.Invoke(this.Ghost);
		}

		// Token: 0x06000D7C RID: 3452 RVA: 0x00049080 File Offset: 0x00047280
		public void UpdatedState(uint netFrame)
		{
			this.clientStateRingBuffer.FrameUpdated(netFrame);
			if (this.HasControl)
			{
				if (!this.clientStateRingBuffer.GetAtPosition(netFrame).InputSynced && NetworkBehaviourSingleton<NetClock>.Instance)
				{
					MonoBehaviourSingleton<NetworkStatusIndicator>.Instance.MissedInput();
					return;
				}
			}
			else
			{
				this.appearanceController.AddState(this.clientStateRingBuffer.GetReferenceFromBuffer(netFrame));
			}
		}

		// Token: 0x06000D7D RID: 3453 RVA: 0x000490E4 File Offset: 0x000472E4
		public void ToggleInput(bool state)
		{
			if (state && !this.playerInputActions.Player.enabled)
			{
				this.playerInputActions.Player.Enable();
				return;
			}
			if (!state && this.playerInputActions.Player.enabled)
			{
				this.playerInputActions.Player.Disable();
			}
		}

		// Token: 0x06000D7E RID: 3454 RVA: 0x00049148 File Offset: 0x00047348
		private void OnReloadPressed(InputAction.CallbackContext obj)
		{
			if (this.resultingState.Downed || this.resultingState.IsStunned)
			{
				return;
			}
			if (!(InputManager.InputUnrestricted ? this.gameplayInputSettingsFlags.Value : ((InputManager.CurrentInputState == InputManager.InputState.Cinematic) ? NetworkBehaviourSingleton<CutsceneManager>.Instance.CurrentCutsceneInputSettings : Endless.Gameplay.LuaEnums.InputSettings.None)).CanUseEquipment() || this.playerInputActions.Player.EnableCursor.IsPressed())
			{
				return;
			}
			if (this.resultingState.AnyEquipmentSwapActive || this.playerReferences.ApperanceController.AppearanceAnimator.IsInHotswapState)
			{
				return;
			}
			Inventory inventory = this.playerReferences.Inventory;
			RangedWeaponItem rangedWeaponItem = ((inventory != null) ? inventory.GetEquippedItem(0) : null) as RangedWeaponItem;
			if (rangedWeaponItem != null && !rangedWeaponItem.ReloadStarted && !rangedWeaponItem.ReloadRequested)
			{
				rangedWeaponItem.StartReload();
			}
		}

		// Token: 0x06000D7F RID: 3455 RVA: 0x00049217 File Offset: 0x00047417
		private void OnHealthZeroed()
		{
			this.playerReferences.Inventory.CheckAndCancelReload();
		}

		// Token: 0x06000D80 RID: 3456 RVA: 0x00049229 File Offset: 0x00047429
		private void OnDrawGizmosSelected()
		{
			if (!base.IsServer && !base.IsHost)
			{
				Gizmos.color = Color.green;
				Gizmos.DrawWireSphere(this.clientStateRingBuffer.GetValue(this.mostRecentServerStateFrame).Position, 0.25f);
			}
		}

		// Token: 0x06000D81 RID: 3457 RVA: 0x00049268 File Offset: 0x00047468
		public void PreFixedUpdate(uint frame)
		{
			if (this.HasControl)
			{
				NetInput.PrimaryEquipmentInput primaryEquipmentInput = NetInput.PrimaryEquipmentInput.None;
				bool flag;
				this.primaryEquipmentInputSwitchP1.ConsumeValue(out flag);
				bool flag2;
				this.primaryEquipmentInputSwitchP2.ConsumeValue(out flag2);
				bool flag3;
				this.secondaryEquipmentInputSwitch.ConsumeValue(out flag3);
				bool flag4;
				this.jumpInputSwitch.ConsumeValue(out flag4);
				bool flag5;
				this.downInputSwitch.ConsumeValue(out flag5);
				if (flag)
				{
					primaryEquipmentInput = (flag2 ? NetInput.PrimaryEquipmentInput.Both : NetInput.PrimaryEquipmentInput.P1);
				}
				else if (flag2)
				{
					primaryEquipmentInput = NetInput.PrimaryEquipmentInput.P2;
				}
				bool isInCutsceneTransition = MonoBehaviourSingleton<CameraController>.Instance.IsInCutsceneTransition;
				bool characterCameraActive = MonoBehaviourSingleton<CameraController>.Instance.CharacterCameraActive;
				bool flag6 = isInCutsceneTransition || !characterCameraActive;
				bool flag7 = ((this.moveInputChangeType == PlayerNetworkController.InputChangeType.Changed) ? (this.horizontal != this.heldHorizontal || this.vertical != this.heldVertical) : ((this.horizontal == 0 && this.vertical == 0) != (this.heldHorizontal == 0 && this.heldVertical == 0)));
				bool flag8 = MonoBehaviourSingleton<CameraController>.Instance.ActiveCamera != this.holdRotationForCamera;
				float num = (flag6 ? MonoBehaviourSingleton<CameraController>.Instance.CameraForwardRotation : CameraController.Rotation);
				this.currentClientInput = new NetInput
				{
					NetFrame = frame,
					Horizontal = this.horizontal,
					Vertical = this.vertical,
					Jump = flag4,
					Ghost = this.Ghost,
					Down = flag5,
					PrimaryEquipment = primaryEquipmentInput,
					SecondaryEquipment = flag3,
					FocusPoint = this.focusInputPoint.position,
					UseIK = this.aimIK,
					CharacterRotation = this.playerReferences.PlayerController.CurrentState.CharacterRotation,
					MotionRotation = num,
					Run = this.run
				};
				if (!MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying || (this.horizontal == 0 && this.vertical == 0))
				{
					this.holdingInput = false;
				}
				else if (this.holdingInput)
				{
					if (flag7)
					{
						this.holdingInput = false;
					}
				}
				else if (flag6)
				{
					this.holdingInput = true;
				}
				if (flag8)
				{
					if (!flag6 && MonoBehaviourSingleton<CameraController>.Instance.IsAPlayerCamera(this.holdRotationForCameraName))
					{
						this.holdingInput = false;
					}
					this.holdRotationForCamera = MonoBehaviourSingleton<CameraController>.Instance.ActiveCamera;
					this.holdRotationForCameraName = this.holdRotationForCamera.Name;
				}
				if (this.holdingInput)
				{
					if (flag7)
					{
						this.heldHorizontal = this.currentClientInput.Horizontal;
						this.heldVertical = this.currentClientInput.Vertical;
					}
					else if (!flag6 && this.steerPlayerWhenHoldingInput)
					{
						float num2 = Mathf.DeltaAngle(this.lastPlayerCameraRotationValue, num);
						this.currentClientInput.MotionRotation = this.lastPlayerCameraRotationValue + num2;
					}
					else
					{
						this.currentClientInput.MotionRotation = this.holdingCameraRotationValue;
					}
				}
				else
				{
					this.holdingCameraRotationValue = this.currentClientInput.MotionRotation;
					this.heldHorizontal = this.currentClientInput.Horizontal;
					this.heldVertical = this.currentClientInput.Vertical;
					if (characterCameraActive)
					{
						this.lastPlayerCameraRotationValue = this.holdingCameraRotationValue;
					}
				}
				if (!flag6)
				{
					Vector2 vector = (this.playerReferences.PlayerController.UsingAimFix ? MonoBehaviourSingleton<CameraController>.Instance.GetAimFixADS(this.playerReferences.PlayerController.CurrentState.CalculatedPostion + Vector3.up, 0.15f) : Vector2.zero);
					this.currentClientInput.AimPitch = MonoBehaviourSingleton<CameraController>.Instance.AimPitch + vector.x;
					this.currentClientInput.AimYaw = MonoBehaviourSingleton<CameraController>.Instance.AimYaw + vector.y;
					this.currentClientInput.MotionRotation = num;
				}
				else
				{
					this.currentClientInput.AimPitch = 0f;
					this.currentClientInput.AimYaw = MonoBehaviourSingleton<CameraController>.Instance.AimYaw;
				}
				this.ClientInputRingBuffer.UpdateValue(ref this.currentClientInput);
			}
		}

		// Token: 0x06000D82 RID: 3458 RVA: 0x00049642 File Offset: 0x00047842
		public void SimulateFrameEnvironment(uint frame)
		{
			this.playerReferences.PlayerController.ProcessWorldTriggerCheck_NetFrame(frame, false);
		}

		// Token: 0x06000D83 RID: 3459 RVA: 0x00049656 File Offset: 0x00047856
		public void Rollback(uint frame)
		{
			if (this.HasControl)
			{
				this.playerReferences.PlayerController.SetState(this.clientStateRingBuffer.GetValue(frame));
			}
		}

		// Token: 0x06000D84 RID: 3460 RVA: 0x0004967C File Offset: 0x0004787C
		public void SimulateFrameActors(uint frame)
		{
			if ((this.HasControl && base.IsServer) || (this.HasControl && (this.clientStateRingBuffer.GetValue(frame).NetFrame != frame || !this.clientStateRingBuffer.GetValue(frame).serverVerifiedState)))
			{
				NetInput value = this.ClientInputRingBuffer.GetValue(frame);
				this.resultingState = this.playerReferences.PlayerController.SimulateNetFrameWithInput(value);
				this.resultingState.InputSynced = true;
				this.resultingState.serverVerifiedState = false;
				return;
			}
			if (base.IsServer)
			{
				this.serverUseInput = this.ServerInputRingBuffer.GetValue(frame);
				bool flag = this.serverUseInput.NetFrame == frame;
				this.serverUseInput.NetFrame = frame;
				this.resultingState = this.playerReferences.PlayerController.SimulateNetFrameWithInput(this.serverUseInput);
				this.resultingState.InputSynced = flag;
				return;
			}
			this.resultingState = this.clientStateRingBuffer.GetValue(frame);
			this.playerReferences.PlayerController.SetState(this.resultingState);
		}

		// Token: 0x06000D85 RID: 3461 RVA: 0x0004978E File Offset: 0x0004798E
		public void SimulateFrameLate(uint frame)
		{
			this.playerReferences.PhysicsTaker.EndFrame();
			if (base.IsServer || this.HasControl)
			{
				this.resultingState = this.playerReferences.PlayerController.EndFrame();
			}
		}

		// Token: 0x06000D86 RID: 3462 RVA: 0x000497C8 File Offset: 0x000479C8
		public void PostFixedUpdate(uint frame)
		{
			if (base.IsServer)
			{
				NetworkBehaviourSingleton<GameplayMessagingManager>.Instance.SendNetState(this.playerReferences, this.resultingState);
				this.appearanceController.AddState(ref this.resultingState);
				if (this.HasControl)
				{
					MonoBehaviourSingleton<CameraController>.Instance.GameStateADS = this.resultingState.AimState;
					return;
				}
			}
			else if (this.HasControl)
			{
				NetworkBehaviourSingleton<GameplayMessagingManager>.Instance.SendNetInput(this.currentClientInput);
				this.appearanceController.AddState(ref this.resultingState);
				MonoBehaviourSingleton<CameraController>.Instance.GameStateADS = this.resultingState.AimState;
			}
		}

		// Token: 0x06000D87 RID: 3463 RVA: 0x00049860 File Offset: 0x00047A60
		public void LoadingFrame(uint frame)
		{
			if (base.IsServer)
			{
				ref NetInput referenceFromBuffer = ref this.ServerInputRingBuffer.GetReferenceFromBuffer(frame);
				referenceFromBuffer.Clear();
				referenceFromBuffer.NetFrame = frame;
				referenceFromBuffer.CharacterRotation = this.playerReferences.PlayerController.CurrentState.CharacterRotation;
				this.ServerInputRingBuffer.FrameUpdated(frame);
				this.resultingState = this.playerReferences.PlayerController.HandleLoadingFrame(frame);
				return;
			}
			if (base.IsOwner)
			{
				this.currentClientInput.Clear();
				this.currentClientInput.NetFrame = frame;
				this.currentClientInput.CharacterRotation = this.playerReferences.PlayerController.CurrentState.CharacterRotation;
				this.ClientInputRingBuffer.UpdateValue(ref this.currentClientInput);
				this.resultingState = this.playerReferences.PlayerController.HandleLoadingFrame(frame);
			}
		}

		// Token: 0x06000D89 RID: 3465 RVA: 0x000499A4 File Offset: 0x00047BA4
		protected override void __initializeVariables()
		{
			bool flag = this.defaultGhostMode == null;
			if (flag)
			{
				throw new Exception("PlayerNetworkController.defaultGhostMode cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.defaultGhostMode.Initialize(this);
			base.__nameNetworkVariable(this.defaultGhostMode, "defaultGhostMode");
			this.NetworkVariableFields.Add(this.defaultGhostMode);
			flag = this.gameplayInputSettingsFlags == null;
			if (flag)
			{
				throw new Exception("PlayerNetworkController.gameplayInputSettingsFlags cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.gameplayInputSettingsFlags.Initialize(this);
			base.__nameNetworkVariable(this.gameplayInputSettingsFlags, "gameplayInputSettingsFlags");
			this.NetworkVariableFields.Add(this.gameplayInputSettingsFlags);
			base.__initializeVariables();
		}

		// Token: 0x06000D8A RID: 3466 RVA: 0x0001E813 File Offset: 0x0001CA13
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x06000D8B RID: 3467 RVA: 0x00049A54 File Offset: 0x00047C54
		protected internal override string __getTypeName()
		{
			return "PlayerNetworkController";
		}

		// Token: 0x04000C45 RID: 3141
		private const uint CLIENT_INPUT_FRAME_BUFFER_SIZE = 4U;

		// Token: 0x04000C46 RID: 3142
		private NetState resultingState;

		// Token: 0x04000C47 RID: 3143
		private RingBuffer<NetInput> ClientInputRingBuffer = new RingBuffer<NetInput>(8);

		// Token: 0x04000C48 RID: 3144
		private RingBuffer<NetState> clientStateRingBuffer = new RingBuffer<NetState>(60);

		// Token: 0x04000C49 RID: 3145
		private NetInput currentClientInput;

		// Token: 0x04000C4A RID: 3146
		private uint mostRecentServerStateFrame;

		// Token: 0x04000C4B RID: 3147
		public RingBuffer<NetInput> ServerInputRingBuffer = new RingBuffer<NetInput>(8);

		// Token: 0x04000C4C RID: 3148
		private NetInput serverUseInput;

		// Token: 0x04000C4D RID: 3149
		private PlayerInputActions playerInputActions;

		// Token: 0x04000C4E RID: 3150
		[SerializeField]
		private PlayerReferenceManager playerReferences;

		// Token: 0x04000C4F RID: 3151
		[SerializeField]
		private AppearanceController playerAppearanceControllerPrefab;

		// Token: 0x04000C50 RID: 3152
		[SerializeField]
		private PlayerNetworkController.ControllerType controllerType;

		// Token: 0x04000C51 RID: 3153
		[SerializeField]
		private CharacterController characterController;

		// Token: 0x04000C52 RID: 3154
		[SerializeField]
		private PlayerNetworkController.InputChangeType moveInputChangeType;

		// Token: 0x04000C53 RID: 3155
		[SerializeField]
		private bool steerPlayerWhenHoldingInput;

		// Token: 0x04000C54 RID: 3156
		private AppearanceController appearanceController;

		// Token: 0x04000C55 RID: 3157
		private NetworkVariable<bool> defaultGhostMode = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000C56 RID: 3158
		private NetworkVariable<Endless.Gameplay.LuaEnums.InputSettings> gameplayInputSettingsFlags = new NetworkVariable<Endless.Gameplay.LuaEnums.InputSettings>(Endless.Gameplay.LuaEnums.InputSettings.Walk | Endless.Gameplay.LuaEnums.InputSettings.Run | Endless.Gameplay.LuaEnums.InputSettings.Jump | Endless.Gameplay.LuaEnums.InputSettings.Equipment | Endless.Gameplay.LuaEnums.InputSettings.Interaction, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000C57 RID: 3159
		private int horizontal;

		// Token: 0x04000C58 RID: 3160
		private int vertical;

		// Token: 0x04000C59 RID: 3161
		private bool run;

		// Token: 0x04000C5A RID: 3162
		private PlayerNetworkController.InputSwitch jumpInputSwitch;

		// Token: 0x04000C5B RID: 3163
		private PlayerNetworkController.InputSwitch downInputSwitch;

		// Token: 0x04000C5C RID: 3164
		private PlayerNetworkController.InputSwitch primaryEquipmentInputSwitchP1;

		// Token: 0x04000C5D RID: 3165
		private PlayerNetworkController.InputSwitch primaryEquipmentInputSwitchP2;

		// Token: 0x04000C5E RID: 3166
		private PlayerNetworkController.InputSwitch secondaryEquipmentInputSwitch;

		// Token: 0x04000C5F RID: 3167
		private ICinemachineCamera holdRotationForCamera;

		// Token: 0x04000C60 RID: 3168
		private string holdRotationForCameraName;

		// Token: 0x04000C61 RID: 3169
		private float holdingCameraRotationValue;

		// Token: 0x04000C62 RID: 3170
		private float lastPlayerCameraRotationValue;

		// Token: 0x04000C63 RID: 3171
		private int heldVertical;

		// Token: 0x04000C64 RID: 3172
		private int heldHorizontal;

		// Token: 0x04000C65 RID: 3173
		private bool holdingInput;

		// Token: 0x04000C66 RID: 3174
		private bool isMobile;

		// Token: 0x04000C67 RID: 3175
		private bool aimIK;

		// Token: 0x04000C68 RID: 3176
		private ProfilerMarker PlayerNetworkControllerSimulateActorsMarker = new ProfilerMarker("PlayerNetworkControllerSimulateActorsMarker");

		// Token: 0x04000C69 RID: 3177
		private Vector3 initialPosition_client;

		// Token: 0x04000C6A RID: 3178
		private float initialRotation_client;

		// Token: 0x04000C6D RID: 3181
		private Transform focusInputPoint;

		// Token: 0x02000279 RID: 633
		public enum ControllerType
		{
			// Token: 0x04000C6F RID: 3183
			Creator,
			// Token: 0x04000C70 RID: 3184
			InGame
		}

		// Token: 0x0200027A RID: 634
		public enum InputChangeType
		{
			// Token: 0x04000C72 RID: 3186
			Changed,
			// Token: 0x04000C73 RID: 3187
			Released
		}

		// Token: 0x0200027B RID: 635
		private struct InputSwitch
		{
			// Token: 0x06000D8C RID: 3468 RVA: 0x00049A5C File Offset: 0x00047C5C
			public void ValueUpdated(bool newValue)
			{
				this.latestUpdatedValue = newValue;
				if (this.latestUpdatedValue)
				{
					if (this.currentState == PlayerNetworkController.InputSwitch.State.Up)
					{
						this.pressTime = Time.realtimeSinceStartupAsDouble;
						this.hasBeenConsumed = false;
					}
					this.currentState = PlayerNetworkController.InputSwitch.State.HardDown;
					return;
				}
				if (this.currentState != PlayerNetworkController.InputSwitch.State.HardDown)
				{
					this.currentState = PlayerNetworkController.InputSwitch.State.Up;
				}
			}

			// Token: 0x06000D8D RID: 3469 RVA: 0x00049AAC File Offset: 0x00047CAC
			public void ConsumeValue(out bool value)
			{
				if (this.hasBeenConsumed)
				{
					PlayerNetworkController.InputSwitch.State state = this.currentState;
					value = (state == PlayerNetworkController.InputSwitch.State.HardDown || state == PlayerNetworkController.InputSwitch.State.SoftDown) && Time.realtimeSinceStartupAsDouble - this.pressTime > (double)NetClock.FixedDeltaTime;
					this.latestUpdatedValue = false;
				}
				else
				{
					PlayerNetworkController.InputSwitch.State state = this.currentState;
					value = state == PlayerNetworkController.InputSwitch.State.HardDown || state == PlayerNetworkController.InputSwitch.State.SoftDown;
					this.hasBeenConsumed = true;
				}
				if (this.latestUpdatedValue)
				{
					this.currentState = PlayerNetworkController.InputSwitch.State.SoftDown;
					return;
				}
				this.currentState = PlayerNetworkController.InputSwitch.State.Up;
			}

			// Token: 0x04000C74 RID: 3188
			private bool latestUpdatedValue;

			// Token: 0x04000C75 RID: 3189
			private PlayerNetworkController.InputSwitch.State currentState;

			// Token: 0x04000C76 RID: 3190
			private double pressTime;

			// Token: 0x04000C77 RID: 3191
			private bool hasBeenConsumed;

			// Token: 0x0200027C RID: 636
			private enum State
			{
				// Token: 0x04000C79 RID: 3193
				Up,
				// Token: 0x04000C7A RID: 3194
				HardDown,
				// Token: 0x04000C7B RID: 3195
				SoftDown
			}
		}
	}
}
