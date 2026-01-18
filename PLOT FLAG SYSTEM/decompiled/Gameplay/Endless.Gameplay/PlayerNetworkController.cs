using System;
using Endless.Data;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Endless.Gameplay;

public class PlayerNetworkController : EndlessNetworkBehaviour, IGameEndSubscriber, NetClock.IPreFixedUpdateSubscriber, NetClock.IRollbackSubscriber, NetClock.ISimulateFrameEnvironmentSubscriber, NetClock.ISimulateFrameActorsSubscriber, NetClock.ISimulateFrameLateSubscriber, NetClock.IPostFixedUpdateSubscriber, NetClock.ILoadingFrameSubscriber
{
	public enum ControllerType
	{
		Creator,
		InGame
	}

	public enum InputChangeType
	{
		Changed,
		Released
	}

	private struct InputSwitch
	{
		private enum State
		{
			Up,
			HardDown,
			SoftDown
		}

		private bool latestUpdatedValue;

		private State currentState;

		private double pressTime;

		private bool hasBeenConsumed;

		public void ValueUpdated(bool newValue)
		{
			latestUpdatedValue = newValue;
			if (latestUpdatedValue)
			{
				if (currentState == State.Up)
				{
					pressTime = Time.realtimeSinceStartupAsDouble;
					hasBeenConsumed = false;
				}
				currentState = State.HardDown;
			}
			else if (currentState != State.HardDown)
			{
				currentState = State.Up;
			}
		}

		public void ConsumeValue(out bool value)
		{
			if (hasBeenConsumed)
			{
				State state = currentState;
				value = (state == State.HardDown || state == State.SoftDown) && Time.realtimeSinceStartupAsDouble - pressTime > (double)NetClock.FixedDeltaTime;
				latestUpdatedValue = false;
			}
			else
			{
				State state = currentState;
				value = state == State.HardDown || state == State.SoftDown;
				hasBeenConsumed = true;
			}
			if (latestUpdatedValue)
			{
				currentState = State.SoftDown;
			}
			else
			{
				currentState = State.Up;
			}
		}
	}

	private const uint CLIENT_INPUT_FRAME_BUFFER_SIZE = 4u;

	private NetState resultingState;

	private RingBuffer<NetInput> ClientInputRingBuffer = new RingBuffer<NetInput>(8);

	private RingBuffer<NetState> clientStateRingBuffer = new RingBuffer<NetState>(60);

	private NetInput currentClientInput;

	private uint mostRecentServerStateFrame;

	public RingBuffer<NetInput> ServerInputRingBuffer = new RingBuffer<NetInput>(8);

	private NetInput serverUseInput;

	private PlayerInputActions playerInputActions;

	[SerializeField]
	private PlayerReferenceManager playerReferences;

	[SerializeField]
	private AppearanceController playerAppearanceControllerPrefab;

	[SerializeField]
	private ControllerType controllerType;

	[SerializeField]
	private CharacterController characterController;

	[SerializeField]
	private InputChangeType moveInputChangeType;

	[SerializeField]
	private bool steerPlayerWhenHoldingInput;

	private AppearanceController appearanceController;

	private NetworkVariable<bool> defaultGhostMode = new NetworkVariable<bool>(value: false);

	private NetworkVariable<Endless.Gameplay.LuaEnums.InputSettings> gameplayInputSettingsFlags = new NetworkVariable<Endless.Gameplay.LuaEnums.InputSettings>(Endless.Gameplay.LuaEnums.InputSettings.Walk | Endless.Gameplay.LuaEnums.InputSettings.Run | Endless.Gameplay.LuaEnums.InputSettings.Jump | Endless.Gameplay.LuaEnums.InputSettings.Equipment | Endless.Gameplay.LuaEnums.InputSettings.Interaction);

	private int horizontal;

	private int vertical;

	private bool run;

	private InputSwitch jumpInputSwitch;

	private InputSwitch downInputSwitch;

	private InputSwitch primaryEquipmentInputSwitchP1;

	private InputSwitch primaryEquipmentInputSwitchP2;

	private InputSwitch secondaryEquipmentInputSwitch;

	private ICinemachineCamera holdRotationForCamera;

	private string holdRotationForCameraName;

	private float holdingCameraRotationValue;

	private float lastPlayerCameraRotationValue;

	private int heldVertical;

	private int heldHorizontal;

	private bool holdingInput;

	private bool isMobile;

	private bool aimIK;

	private ProfilerMarker PlayerNetworkControllerSimulateActorsMarker = new ProfilerMarker("PlayerNetworkControllerSimulateActorsMarker");

	private Vector3 initialPosition_client;

	private float initialRotation_client;

	private Transform focusInputPoint;

	public bool HasControl => base.IsOwner;

	public RingBuffer<NetState> ClientStateRingBuffer => clientStateRingBuffer;

	public AppearanceController AppearanceController => appearanceController;

	public bool DefaultGhostMode
	{
		get
		{
			return defaultGhostMode.Value;
		}
		set
		{
			defaultGhostMode.Value = value;
		}
	}

	public bool Ghost { get; private set; }

	public UnityEvent<bool> GhostChangedUnityEvent { get; private set; } = new UnityEvent<bool>();

	public Endless.Gameplay.LuaEnums.InputSettings ActiveInputSettings
	{
		get
		{
			if (!InputManager.InputUnrestricted)
			{
				if (InputManager.CurrentInputState != InputManager.InputState.Cinematic)
				{
					return Endless.Gameplay.LuaEnums.InputSettings.None;
				}
				return NetworkBehaviourSingleton<CutsceneManager>.Instance.CurrentCutsceneInputSettings;
			}
			return gameplayInputSettingsFlags.Value;
		}
	}

	private void Awake()
	{
		playerInputActions = new PlayerInputActions();
		isMobile = MobileUtility.IsMobile;
	}

	private void OnEnable()
	{
		playerInputActions.Player.Ghost.performed += ToggleGhostMode;
		playerInputActions.Player.Enable();
	}

	private new void Start()
	{
		NetClock.Register(this);
		holdRotationForCamera = MonoBehaviourSingleton<CameraController>.Instance.MainPlayerCamera;
		holdRotationForCameraName = holdRotationForCamera.Name;
	}

	public void EndlessGameEnd()
	{
		holdRotationForCamera = MonoBehaviourSingleton<CameraController>.Instance.MainPlayerCamera;
		holdRotationForCameraName = holdRotationForCamera.Name;
	}

	private void OnDisable()
	{
		NetClock.Unregister(this);
		playerInputActions.Player.Ghost.performed -= ToggleGhostMode;
		playerInputActions.Player.Disable();
	}

	public void SetGameplayInputSettingsFlags(Endless.Gameplay.LuaEnums.InputSettings settings)
	{
		if (base.IsServer)
		{
			gameplayInputSettingsFlags.Value = settings;
		}
	}

	public void EnableAimIK(Vector3 worldPosition)
	{
		aimIK = true;
		SetAimIKPosition(worldPosition);
	}

	public void DisableAimIK()
	{
		aimIK = false;
	}

	public void SetAimIKPosition(Vector3 worldPosition)
	{
		if ((bool)focusInputPoint)
		{
			focusInputPoint.position = worldPosition;
		}
	}

	protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer)
	{
		if (serializer.IsWriter)
		{
			initialPosition_client = playerReferences.PlayerController.CurrentState.Position;
			initialRotation_client = playerReferences.PlayerController.CurrentState.CharacterRotation;
		}
		serializer.SerializeValue(ref initialPosition_client);
		serializer.SerializeValue(ref initialRotation_client, default(FastBufferWriter.ForPrimitives));
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		if (base.IsServer)
		{
			initialPosition_client = playerReferences.PlayerController.CurrentState.Position;
			initialRotation_client = playerReferences.PlayerController.CurrentState.CharacterRotation;
		}
		appearanceController = UnityEngine.Object.Instantiate(playerAppearanceControllerPrefab);
		appearanceController.RuntimeInit(playerReferences, base.IsServer ? AppearanceController.AppearancePerspective.Server : ((!HasControl) ? AppearanceController.AppearancePerspective.Interpolate : AppearanceController.AppearancePerspective.Extrapolate), initialPosition_client, initialRotation_client);
		if (HasControl)
		{
			MonoBehaviourSingleton<CameraController>.Instance.InitAppearance(appearanceController, controllerType);
			Ghost = DefaultGhostMode;
			GhostChangedUnityEvent.Invoke(Ghost);
			playerReferences.PlayerController.HandleSpawnedAsOwner(base.transform.rotation.eulerAngles.y);
			focusInputPoint = new GameObject("FOCUS").transform;
			playerInputActions.Player.Reload.performed += OnReloadPressed;
		}
		if (base.IsServer)
		{
			ref NetInput referenceFromBuffer = ref ServerInputRingBuffer.GetReferenceFromBuffer(NetClock.CurrentFrame);
			referenceFromBuffer.NetFrame = NetClock.CurrentFrame;
			referenceFromBuffer.Ghost = DefaultGhostMode;
			ServerInputRingBuffer.UpdateValue(ref referenceFromBuffer);
			if (playerReferences.HealthComponent != null)
			{
				playerReferences.HealthComponent.OnHealthZeroed_Internal.AddListener(OnHealthZeroed);
				playerReferences.HealthComponent.ShouldSaveAndLoad = false;
			}
		}
	}

	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();
		UnityEngine.Object.Destroy(appearanceController.gameObject);
		if ((bool)focusInputPoint)
		{
			UnityEngine.Object.Destroy(focusInputPoint.gameObject);
		}
		playerInputActions.Player.Reload.performed -= OnReloadPressed;
		if (playerReferences.HealthComponent != null)
		{
			playerReferences.HealthComponent.OnHealthZeroed_Internal.RemoveListener(OnHealthZeroed);
		}
	}

	private void Update()
	{
		if (!HasControl)
		{
			return;
		}
		Endless.Gameplay.LuaEnums.InputSettings activeInputSettings = ActiveInputSettings;
		if ((bool)playerReferences.PlayerInteractor)
		{
			playerReferences.PlayerInteractor.enabled = activeInputSettings.CanInteract();
		}
		Vector2 vector = playerInputActions.Player.Move.ReadValue<Vector2>();
		if (activeInputSettings.CanMove())
		{
			run = activeInputSettings.CanRun();
			horizontal = Mathf.RoundToInt(vector.x);
			vertical = Mathf.RoundToInt(vector.y);
			downInputSwitch.ValueUpdated(playerInputActions.Player.Down.IsPressed());
		}
		else
		{
			run = false;
			horizontal = 0;
			vertical = 0;
			downInputSwitch.ValueUpdated(newValue: false);
		}
		if (activeInputSettings.CanJump())
		{
			jumpInputSwitch.ValueUpdated(playerInputActions.Player.Jump.IsPressed() && activeInputSettings.CanJump());
		}
		else
		{
			jumpInputSwitch.ValueUpdated(newValue: false);
		}
		if (activeInputSettings.CanUseEquipment())
		{
			bool flag = !EventSystem.current.IsPointerOverGameObject() || isMobile;
			primaryEquipmentInputSwitchP1.ValueUpdated(flag && !playerInputActions.Player.EnableCursor.IsPressed() && playerInputActions.Player.MajorEquipmentPrimaryAction.IsPressed());
			primaryEquipmentInputSwitchP2.ValueUpdated(flag && !playerInputActions.Player.EnableCursor.IsPressed() && playerInputActions.Player.MajorEquipmentAlternateAction.IsPressed());
			secondaryEquipmentInputSwitch.ValueUpdated(flag && playerInputActions.Player.MinorEquipment.IsPressed());
			if (playerInputActions.Player.Inventory1.triggered)
			{
				TryEquipFromInventory(0);
			}
			else if (playerInputActions.Player.Inventory2.triggered)
			{
				TryEquipFromInventory(1);
			}
			else if (playerInputActions.Player.Inventory3.triggered)
			{
				TryEquipFromInventory(2);
			}
			else if (playerInputActions.Player.Inventory4.triggered)
			{
				TryEquipFromInventory(3);
			}
			else if (playerInputActions.Player.Inventory5.triggered)
			{
				TryEquipFromInventory(4);
			}
			else if (playerInputActions.Player.Inventory6.triggered)
			{
				TryEquipFromInventory(5);
			}
			else if (playerInputActions.Player.Inventory7.triggered)
			{
				TryEquipFromInventory(6);
			}
			else if (playerInputActions.Player.Inventory8.triggered)
			{
				TryEquipFromInventory(7);
			}
			else if (playerInputActions.Player.Inventory9.triggered)
			{
				TryEquipFromInventory(8);
			}
			else if (playerInputActions.Player.Inventory10.triggered)
			{
				TryEquipFromInventory(9);
			}
		}
		else
		{
			primaryEquipmentInputSwitchP1.ValueUpdated(newValue: false);
			primaryEquipmentInputSwitchP2.ValueUpdated(newValue: false);
			secondaryEquipmentInputSwitch.ValueUpdated(newValue: false);
		}
	}

	public void TryEquipFromInventory(int inventoryIndex)
	{
		if ((bool)playerReferences.Inventory)
		{
			playerReferences.Inventory.EquipSlot(inventoryIndex);
		}
	}

	public void SetEquipmentInputSwitch(NetInput.InputEquipmentSlot inputEquipmentSlot)
	{
	}

	public void SetPrimaryEquipmentInputP1(bool p1)
	{
		primaryEquipmentInputSwitchP1.ValueUpdated(p1);
	}

	public void SetPrimaryEquipmentInputP2(bool p2)
	{
		primaryEquipmentInputSwitchP2.ValueUpdated(p2);
	}

	public void SetSecondaryEquipmentInput(bool secondary)
	{
		secondaryEquipmentInputSwitch.ValueUpdated(secondary);
	}

	public void SetHorizontal(int input)
	{
		horizontal = input;
	}

	public void SetVertical(int input)
	{
		vertical = input;
	}

	public void ToggleJump(bool state)
	{
		jumpInputSwitch.ValueUpdated(state);
	}

	public void ToggleDownInput(bool state)
	{
		downInputSwitch.ValueUpdated(state);
	}

	public void ToggleGhostMode(InputAction.CallbackContext callbackContext)
	{
		ToggleGhostMode();
	}

	public void ToggleGhostMode()
	{
		if (InputManager.InputUnrestricted)
		{
			Ghost = !Ghost;
			GhostChangedUnityEvent.Invoke(Ghost);
		}
	}

	public void UpdatedState(uint netFrame)
	{
		clientStateRingBuffer.FrameUpdated(netFrame);
		if (HasControl)
		{
			if (!clientStateRingBuffer.GetAtPosition(netFrame).InputSynced && (bool)NetworkBehaviourSingleton<NetClock>.Instance)
			{
				MonoBehaviourSingleton<NetworkStatusIndicator>.Instance.MissedInput();
			}
		}
		else
		{
			appearanceController.AddState(ref clientStateRingBuffer.GetReferenceFromBuffer(netFrame));
		}
	}

	public void ToggleInput(bool state)
	{
		if (state && !playerInputActions.Player.enabled)
		{
			playerInputActions.Player.Enable();
		}
		else if (!state && playerInputActions.Player.enabled)
		{
			playerInputActions.Player.Disable();
		}
	}

	private void OnReloadPressed(InputAction.CallbackContext obj)
	{
		if (!resultingState.Downed && !resultingState.IsStunned && (InputManager.InputUnrestricted ? gameplayInputSettingsFlags.Value : ((InputManager.CurrentInputState == InputManager.InputState.Cinematic) ? NetworkBehaviourSingleton<CutsceneManager>.Instance.CurrentCutsceneInputSettings : Endless.Gameplay.LuaEnums.InputSettings.None)).CanUseEquipment() && !playerInputActions.Player.EnableCursor.IsPressed() && !resultingState.AnyEquipmentSwapActive && !playerReferences.ApperanceController.AppearanceAnimator.IsInHotswapState && playerReferences.Inventory?.GetEquippedItem(0) is RangedWeaponItem { ReloadStarted: false, ReloadRequested: false } rangedWeaponItem)
		{
			rangedWeaponItem.StartReload();
		}
	}

	private void OnHealthZeroed()
	{
		playerReferences.Inventory.CheckAndCancelReload();
	}

	private void OnDrawGizmosSelected()
	{
		if (!base.IsServer && !base.IsHost)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(clientStateRingBuffer.GetValue(mostRecentServerStateFrame).Position, 0.25f);
		}
	}

	public void PreFixedUpdate(uint frame)
	{
		if (!HasControl)
		{
			return;
		}
		NetInput.PrimaryEquipmentInput primaryEquipment = NetInput.PrimaryEquipmentInput.None;
		primaryEquipmentInputSwitchP1.ConsumeValue(out var value);
		primaryEquipmentInputSwitchP2.ConsumeValue(out var value2);
		secondaryEquipmentInputSwitch.ConsumeValue(out var value3);
		jumpInputSwitch.ConsumeValue(out var value4);
		downInputSwitch.ConsumeValue(out var value5);
		if (value)
		{
			primaryEquipment = ((!value2) ? NetInput.PrimaryEquipmentInput.P1 : NetInput.PrimaryEquipmentInput.Both);
		}
		else if (value2)
		{
			primaryEquipment = NetInput.PrimaryEquipmentInput.P2;
		}
		bool isInCutsceneTransition = MonoBehaviourSingleton<CameraController>.Instance.IsInCutsceneTransition;
		bool characterCameraActive = MonoBehaviourSingleton<CameraController>.Instance.CharacterCameraActive;
		bool flag = isInCutsceneTransition || !characterCameraActive;
		bool flag2 = ((moveInputChangeType != InputChangeType.Changed) ? ((horizontal == 0 && vertical == 0) != (heldHorizontal == 0 && heldVertical == 0)) : (horizontal != heldHorizontal || vertical != heldVertical));
		bool num = MonoBehaviourSingleton<CameraController>.Instance.ActiveCamera != holdRotationForCamera;
		float num2 = (flag ? MonoBehaviourSingleton<CameraController>.Instance.CameraForwardRotation : CameraController.Rotation);
		currentClientInput = new NetInput
		{
			NetFrame = frame,
			Horizontal = horizontal,
			Vertical = vertical,
			Jump = value4,
			Ghost = Ghost,
			Down = value5,
			PrimaryEquipment = primaryEquipment,
			SecondaryEquipment = value3,
			FocusPoint = focusInputPoint.position,
			UseIK = aimIK,
			CharacterRotation = playerReferences.PlayerController.CurrentState.CharacterRotation,
			MotionRotation = num2,
			Run = run
		};
		if (!MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying || (horizontal == 0 && vertical == 0))
		{
			holdingInput = false;
		}
		else if (holdingInput)
		{
			if (flag2)
			{
				holdingInput = false;
			}
		}
		else if (flag)
		{
			holdingInput = true;
		}
		if (num)
		{
			if (!flag && MonoBehaviourSingleton<CameraController>.Instance.IsAPlayerCamera(holdRotationForCameraName))
			{
				holdingInput = false;
			}
			holdRotationForCamera = MonoBehaviourSingleton<CameraController>.Instance.ActiveCamera;
			holdRotationForCameraName = holdRotationForCamera.Name;
		}
		if (holdingInput)
		{
			if (flag2)
			{
				heldHorizontal = currentClientInput.Horizontal;
				heldVertical = currentClientInput.Vertical;
			}
			else if (!flag && steerPlayerWhenHoldingInput)
			{
				float num3 = Mathf.DeltaAngle(lastPlayerCameraRotationValue, num2);
				currentClientInput.MotionRotation = lastPlayerCameraRotationValue + num3;
			}
			else
			{
				currentClientInput.MotionRotation = holdingCameraRotationValue;
			}
		}
		else
		{
			holdingCameraRotationValue = currentClientInput.MotionRotation;
			heldHorizontal = currentClientInput.Horizontal;
			heldVertical = currentClientInput.Vertical;
			if (characterCameraActive)
			{
				lastPlayerCameraRotationValue = holdingCameraRotationValue;
			}
		}
		if (!flag)
		{
			Vector2 vector = (playerReferences.PlayerController.UsingAimFix ? MonoBehaviourSingleton<CameraController>.Instance.GetAimFixADS(playerReferences.PlayerController.CurrentState.CalculatedPostion + Vector3.up) : Vector2.zero);
			currentClientInput.AimPitch = MonoBehaviourSingleton<CameraController>.Instance.AimPitch + vector.x;
			currentClientInput.AimYaw = MonoBehaviourSingleton<CameraController>.Instance.AimYaw + vector.y;
			currentClientInput.MotionRotation = num2;
		}
		else
		{
			currentClientInput.AimPitch = 0f;
			currentClientInput.AimYaw = MonoBehaviourSingleton<CameraController>.Instance.AimYaw;
		}
		ClientInputRingBuffer.UpdateValue(ref currentClientInput);
	}

	public void SimulateFrameEnvironment(uint frame)
	{
		playerReferences.PlayerController.ProcessWorldTriggerCheck_NetFrame(frame);
	}

	public void Rollback(uint frame)
	{
		if (HasControl)
		{
			playerReferences.PlayerController.SetState(clientStateRingBuffer.GetValue(frame));
		}
	}

	public void SimulateFrameActors(uint frame)
	{
		if ((HasControl && base.IsServer) || (HasControl && (clientStateRingBuffer.GetValue(frame).NetFrame != frame || !clientStateRingBuffer.GetValue(frame).serverVerifiedState)))
		{
			NetInput value = ClientInputRingBuffer.GetValue(frame);
			resultingState = playerReferences.PlayerController.SimulateNetFrameWithInput(value);
			resultingState.InputSynced = true;
			resultingState.serverVerifiedState = false;
		}
		else if (base.IsServer)
		{
			serverUseInput = ServerInputRingBuffer.GetValue(frame);
			bool inputSynced = serverUseInput.NetFrame == frame;
			serverUseInput.NetFrame = frame;
			resultingState = playerReferences.PlayerController.SimulateNetFrameWithInput(serverUseInput);
			resultingState.InputSynced = inputSynced;
		}
		else
		{
			resultingState = clientStateRingBuffer.GetValue(frame);
			playerReferences.PlayerController.SetState(resultingState);
		}
	}

	public void SimulateFrameLate(uint frame)
	{
		playerReferences.PhysicsTaker.EndFrame();
		if (base.IsServer || HasControl)
		{
			resultingState = playerReferences.PlayerController.EndFrame();
		}
	}

	public void PostFixedUpdate(uint frame)
	{
		if (base.IsServer)
		{
			NetworkBehaviourSingleton<GameplayMessagingManager>.Instance.SendNetState(playerReferences, resultingState);
			appearanceController.AddState(ref resultingState);
			if (HasControl)
			{
				MonoBehaviourSingleton<CameraController>.Instance.GameStateADS = resultingState.AimState;
			}
		}
		else if (HasControl)
		{
			NetworkBehaviourSingleton<GameplayMessagingManager>.Instance.SendNetInput(currentClientInput);
			appearanceController.AddState(ref resultingState);
			MonoBehaviourSingleton<CameraController>.Instance.GameStateADS = resultingState.AimState;
		}
	}

	public void LoadingFrame(uint frame)
	{
		if (base.IsServer)
		{
			ref NetInput referenceFromBuffer = ref ServerInputRingBuffer.GetReferenceFromBuffer(frame);
			referenceFromBuffer.Clear();
			referenceFromBuffer.NetFrame = frame;
			referenceFromBuffer.CharacterRotation = playerReferences.PlayerController.CurrentState.CharacterRotation;
			ServerInputRingBuffer.FrameUpdated(frame);
			resultingState = playerReferences.PlayerController.HandleLoadingFrame(frame);
		}
		else if (base.IsOwner)
		{
			currentClientInput.Clear();
			currentClientInput.NetFrame = frame;
			currentClientInput.CharacterRotation = playerReferences.PlayerController.CurrentState.CharacterRotation;
			ClientInputRingBuffer.UpdateValue(ref currentClientInput);
			resultingState = playerReferences.PlayerController.HandleLoadingFrame(frame);
		}
	}

	protected override void __initializeVariables()
	{
		if (defaultGhostMode == null)
		{
			throw new Exception("PlayerNetworkController.defaultGhostMode cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		defaultGhostMode.Initialize(this);
		__nameNetworkVariable(defaultGhostMode, "defaultGhostMode");
		NetworkVariableFields.Add(defaultGhostMode);
		if (gameplayInputSettingsFlags == null)
		{
			throw new Exception("PlayerNetworkController.gameplayInputSettingsFlags cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		gameplayInputSettingsFlags.Initialize(this);
		__nameNetworkVariable(gameplayInputSettingsFlags, "gameplayInputSettingsFlags");
		NetworkVariableFields.Add(gameplayInputSettingsFlags);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected internal override string __getTypeName()
	{
		return "PlayerNetworkController";
	}
}
