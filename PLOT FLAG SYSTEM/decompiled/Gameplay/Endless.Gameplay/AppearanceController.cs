using System.Collections;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using Endless.Gameplay.VisualManagement;
using Endless.Shared;
using Endless.Shared.DataTypes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Endless.Gameplay;

public class AppearanceController : MonoBehaviour
{
	public enum AppearancePerspective
	{
		Extrapolate,
		Server,
		Interpolate
	}

	private class EquipmentEventSwitch
	{
		private AppearanceController targetAppearanceController;

		private InventoryUsableDefinition.EventData previousSlotData;

		private InventoryUsableDefinition.EventData currentSlotData;

		private SerializableGuid slotGUID = SerializableGuid.Empty;

		private SerializableGuid slotAssedId = SerializableGuid.Empty;

		private bool slotChanged;

		private UsableDefinition.UseState previousSlotEus;

		private UsableDefinition.UseState slotEUS;

		public EquipmentEventSwitch(AppearanceController target)
		{
			targetAppearanceController = target;
		}

		public void FrameSetup(SerializableGuid assetID)
		{
			slotAssedId = assetID;
			SerializableGuid usableDefinitionIDFromAssetID = RuntimeDatabase.GetUsableDefinitionIDFromAssetID(assetID);
			slotChanged = !slotGUID.Equals(usableDefinitionIDFromAssetID);
			slotGUID = usableDefinitionIDFromAssetID;
			previousSlotEus = slotEUS;
			slotEUS = null;
		}

		public void CheckEUS(UsableDefinition.UseState eus)
		{
			if (slotGUID.Equals(eus.EquipmentGuid))
			{
				slotEUS = eus;
			}
		}

		public void ProcessEvents(NetState state, double appearanceTime, out InventoryUsableDefinition.EquipmentShowPriority equipmentShowPriority)
		{
			if (!slotGUID.Equals(SerializableGuid.Empty))
			{
				InventoryUsableDefinition usableDefinition = RuntimeDatabase.GetUsableDefinition<InventoryUsableDefinition>(slotGUID);
				usableDefinition.GetEventData(state, slotEUS, appearanceTime, ref currentSlotData);
				equipmentShowPriority = usableDefinition.GetShowPriority(slotEUS);
			}
			else
			{
				equipmentShowPriority = InventoryUsableDefinition.EquipmentShowPriority.NotShown;
			}
			if (slotChanged)
			{
				targetAppearanceController.OnEquipmentUseStateChanged.Invoke(slotGUID, slotEUS);
				targetAppearanceController.OnEquipmentAvailableChanged.Invoke(slotGUID, currentSlotData.Available);
				targetAppearanceController.OnEquipmentInUseChanged.Invoke(slotGUID, currentSlotData.InUse);
				targetAppearanceController.OnEquipmentCooldownChanged.Invoke(slotGUID, currentSlotData.CooldownSecondsLeft, currentSlotData.CooldownSecondsTotal);
				targetAppearanceController.OnEquipmentResourceChanged.Invoke(slotGUID, currentSlotData.ResourcePercent);
			}
			else
			{
				if (slotEUS != null || slotEUS != previousSlotEus)
				{
					targetAppearanceController.OnEquipmentUseStateChanged.Invoke(slotGUID, slotEUS);
				}
				if (previousSlotData.Available != currentSlotData.Available)
				{
					targetAppearanceController.OnEquipmentAvailableChanged.Invoke(slotGUID, currentSlotData.Available);
				}
				if (previousSlotData.InUse != currentSlotData.InUse)
				{
					targetAppearanceController.OnEquipmentInUseChanged.Invoke(slotGUID, currentSlotData.InUse);
				}
				if (!Mathf.Approximately(previousSlotData.CooldownSecondsLeft, currentSlotData.CooldownSecondsLeft))
				{
					targetAppearanceController.OnEquipmentCooldownChanged.Invoke(slotGUID, currentSlotData.CooldownSecondsLeft, currentSlotData.CooldownSecondsTotal);
				}
				if (!Mathf.Approximately(previousSlotData.ResourcePercent, currentSlotData.ResourcePercent))
				{
					targetAppearanceController.OnEquipmentResourceChanged.Invoke(slotGUID, currentSlotData.ResourcePercent);
				}
			}
			currentSlotData.CopyTo(ref previousSlotData);
			currentSlotData.Reset();
		}
	}

	private const float POSITION_SMOOTHING_TIME = 0.05f;

	public UnityEvent<SerializableGuid, bool> OnEquipmentAvailableChanged = new UnityEvent<SerializableGuid, bool>();

	public UnityEvent<SerializableGuid, bool> OnEquipmentInUseChanged = new UnityEvent<SerializableGuid, bool>();

	public UnityEvent<SerializableGuid, float, float> OnEquipmentCooldownChanged = new UnityEvent<SerializableGuid, float, float>();

	public UnityEvent<SerializableGuid, float> OnEquipmentResourceChanged = new UnityEvent<SerializableGuid, float>();

	public UnityEvent<SerializableGuid, UsableDefinition.UseState> OnEquipmentUseStateChanged = new UnityEvent<SerializableGuid, UsableDefinition.UseState>();

	public UnityEvent<AppearanceAnimator> OnNewAppearence = new UnityEvent<AppearanceAnimator>();

	[SerializeField]
	private float angularVelocitySmoothTime = 0.25f;

	[SerializeField]
	private float velocitySmoothTime = 0.25f;

	[SerializeField]
	private float skinWidthPositionOffset = -0.025f;

	[SerializeField]
	private bool useRelativeAimPoint = true;

	[SerializeField]
	private FootstepController footstepController;

	private AppearanceAnimator appearanceAnimator;

	private InterpolationRingBuffer<NetState> stateRingBuffer = new InterpolationRingBuffer<NetState>(30);

	private float animatorAngularVelocity;

	private float animatorVelocityX;

	private float animatorVelocityY;

	private float animatorVelocityZ;

	private float animatorAngularVelocityVelocity;

	private float animatorVelocityXVelocity;

	private float animatorVelocityYVelocity;

	private float animatorVelocityZVelocity;

	private AppearancePerspective perspective = AppearancePerspective.Server;

	private List<string> pastAppearanceFrameTriggerableStates = new List<string>();

	private List<string> thisAppearanceFrameTriggerableStates = new List<string>();

	private EquipmentEventSwitch slot0EventSwitch;

	private EquipmentEventSwitch slot1EventSwitch;

	private PlayerPhysicsTaker.PushState pushState;

	private float currentCharRotation;

	private UnityEngine.Vector3 currentCharPosition;

	private uint firstStateFrame;

	private bool positionInitialized;

	private uint latestState;

	private List<RendererManager> cachedRendererMangers = new List<RendererManager>();

	private UnityEngine.Vector3 initialPosition_client;

	private float initialRotation_client;

	private AppearanceAnimator latestCosmeticsSpawnHandleAppearanceAnimator;

	private bool teleporting;

	private TeleportType activeTeleportType;

	private EndlessVisuals endlessVisuals;

	private List<HealthComponent.HealthLostData> hitReactionQueue = new List<HealthComponent.HealthLostData>();

	public Transform VisualsTransform
	{
		get
		{
			if (!(appearanceAnimator != null))
			{
				return base.transform;
			}
			return appearanceAnimator.transform;
		}
	}

	public PlayerReferenceManager PlayerReferences { get; protected set; }

	public AppearanceAnimator AppearanceAnimator => appearanceAnimator;

	public bool UseRelativeAimPoint => useRelativeAimPoint;

	public float CurrentCharRotation => currentCharRotation;

	public bool GhostModeActive => stateRingBuffer.ActiveInterpolatedState.Ghost;

	private void Awake()
	{
		slot0EventSwitch = new EquipmentEventSwitch(this);
		slot1EventSwitch = new EquipmentEventSwitch(this);
		stateRingBuffer.OnStatesShifted.AddListener(HandleInterpolationStatesShifted);
	}

	public void RuntimeInit(PlayerReferenceManager playerReferences, AppearancePerspective perspective, UnityEngine.Vector3 initialPos, float initialRot)
	{
		PlayerReferences = playerReferences;
		this.perspective = perspective;
		base.transform.position = playerReferences.transform.position;
		AppearanceAnimator appearanceAnimator = Object.Instantiate(PlayerReferences.ApperanceBasePrefab, base.transform);
		GameObject cosmetics = Object.Instantiate(MonoBehaviourSingleton<DefaultContentManager>.Instance.DefaultCharacterCosmeticsGameObject, appearanceAnimator.transform);
		ApplyCosmeticsGameObject(cosmetics);
		UpdateCharacterCosmetics(playerReferences.CharacterCosmetics);
		if ((bool)playerReferences.HealthComponent)
		{
			playerReferences.HealthComponent.OnHealthLost.AddListener(HandleDamageReactionEvent);
		}
		PlayerReferences.OnCharacterCosmeticsChanged.AddListener(UpdateCharacterCosmetics);
		initialPosition_client = initialPos;
		initialRotation_client = initialRot;
	}

	private void UpdateCharacterCosmetics(CharacterCosmeticsDefinition cosmeticsDefinition)
	{
		latestCosmeticsSpawnHandleAppearanceAnimator = Object.Instantiate(PlayerReferences.ApperanceBasePrefab, base.transform);
		AsyncOperationHandle<GameObject> asyncOperationHandle = cosmeticsDefinition.Instantiate(latestCosmeticsSpawnHandleAppearanceAnimator.transform);
		asyncOperationHandle.Completed += HandleCosmeticInstantiation;
	}

	private void HandleCosmeticInstantiation(AsyncOperationHandle<GameObject> handle)
	{
		if (latestCosmeticsSpawnHandleAppearanceAnimator == null || latestCosmeticsSpawnHandleAppearanceAnimator.transform != handle.Result.transform.parent || PlayerReferences == null || PlayerReferences.EndlessVisuals == null)
		{
			if ((bool)handle.Result.transform.parent)
			{
				Object.Destroy(handle.Result.transform.parent.gameObject);
			}
			else
			{
				Object.Destroy(handle.Result.gameObject);
			}
		}
		else
		{
			ApplyCosmeticsGameObject(handle.Result);
		}
	}

	private void ApplyCosmeticsGameObject(GameObject cosmetics)
	{
		AppearanceAnimator component = cosmetics.transform.parent.GetComponent<AppearanceAnimator>();
		component.transform.localPosition = new UnityEngine.Vector3(0f, skinWidthPositionOffset, 0f);
		component.InitializeCosmetics();
		PlayerReferences.EndlessVisuals.UnmanageRenderers(cachedRendererMangers);
		if (positionInitialized)
		{
			component.InitRotation(stateRingBuffer.GetValue(latestState).CharacterRotation);
		}
		AppearanceAnimator appearanceAnimator = this.appearanceAnimator;
		this.appearanceAnimator = component;
		if ((bool)PlayerReferences && (bool)PlayerReferences.Inventory)
		{
			PlayerReferences.Inventory.HandleCharacterCosmeticsChanged();
		}
		if ((bool)PlayerReferences && (bool)PlayerReferences.PlayerEquipmentManager)
		{
			PlayerReferences.PlayerEquipmentManager.TransferEquipment(component);
		}
		Renderer[] componentsInChildren = this.appearanceAnimator.GetComponentsInChildren<Renderer>();
		Renderer[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			Material[] materials = array[i].materials;
			foreach (Material material in materials)
			{
				string text = ((material.shader.name == "Shader Graphs/Endless_Shader") ? "Shader Graphs/Endless_Shader_Character_NoFade" : material.shader.name);
				material.shader = Shader.Find(text);
			}
		}
		cachedRendererMangers = PlayerReferences.EndlessVisuals.ManageRenderers(componentsInChildren);
		OnNewAppearence.Invoke(component);
		PlayerReferences.EndlessVisuals.FadeIn();
		if (appearanceAnimator != null)
		{
			Object.Destroy(appearanceAnimator.gameObject);
		}
	}

	public void AddState(ref NetState state)
	{
		stateRingBuffer.UpdateValue(ref state);
		if (!positionInitialized)
		{
			base.transform.position = state.Position;
			positionInitialized = true;
			if (appearanceAnimator != null)
			{
				appearanceAnimator.InitRotation(state.CharacterRotation);
			}
		}
		latestState = state.NetFrame;
	}

	public void TriggerAnimationImmediate(string animationTriggerString)
	{
		if ((bool)AppearanceAnimator && !string.IsNullOrEmpty(animationTriggerString))
		{
			AppearanceAnimator.Animator.SetTrigger(animationTriggerString);
		}
	}

	private void HandleDamageReactionEvent(HealthComponent.HealthLostData data)
	{
		if (data.networked)
		{
			GameplayPlayerReferenceManager gameplayPlayerReferenceManager = PlayerReferenceManager.LocalInstance as GameplayPlayerReferenceManager;
			if (perspective == AppearancePerspective.Extrapolate || ((bool)gameplayPlayerReferenceManager && data.damageSource == gameplayPlayerReferenceManager.NetworkObject))
			{
				return;
			}
		}
		hitReactionQueue.Add(data);
	}

	private void HandleInterpolationStatesShifted(NetState prev, NetState next)
	{
		if (prev.JumpFrame > -1 && next.JumpFrame < 0)
		{
			thisAppearanceFrameTriggerableStates.Add("Jump");
		}
		if (!prev.Grounded && next.Grounded)
		{
			if (!MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying && prev.TeleportAtFrame == prev.NetFrame)
			{
				thisAppearanceFrameTriggerableStates.Add("Spawn");
			}
			else if (next.TeleportAtFrame != next.NetFrame || (next.TeleportAtFrame == next.NetFrame && next.TeleportStatus == TeleportComponent.TeleportStatusType.WorldFallOff))
			{
				thisAppearanceFrameTriggerableStates.Add("Landed");
			}
			if (prev.Downed && next.Downed)
			{
				thisAppearanceFrameTriggerableStates.Add("Downed");
			}
		}
		if (!prev.Downed && next.Downed)
		{
			thisAppearanceFrameTriggerableStates.Add("Downed");
		}
	}

	private void Update()
	{
		if (stateRingBuffer.NextInterpolationState.NetFrame < 1)
		{
			base.transform.position = initialPosition_client;
			appearanceAnimator.SnapRotation(initialRotation_client);
			return;
		}
		stateRingBuffer.ActiveInterpolationTime = ((perspective == AppearancePerspective.Server) ? NetClock.ServerAppearanceTime : ((perspective == AppearancePerspective.Extrapolate) ? NetClock.ClientExtrapolatedAppearanceTime : NetClock.ClientInterpolatedAppearanceTime));
		if (stateRingBuffer.PastInterpolationState.TeleportActive && stateRingBuffer.PastInterpolationState.TeleportAtFrame == stateRingBuffer.PastInterpolationState.NetFrame + 1)
		{
			stateRingBuffer.ActiveInterpolatedState.Position = stateRingBuffer.PastInterpolationState.TeleportPosition;
			stateRingBuffer.ActiveInterpolatedState.CharacterRotation = stateRingBuffer.PastInterpolationState.TeleportRotation;
			appearanceAnimator.SnapRotation(stateRingBuffer.ActiveInterpolatedState.CharacterRotation);
		}
		else
		{
			stateRingBuffer.ActiveInterpolatedState.Position = UnityEngine.Vector3.Lerp(stateRingBuffer.PastInterpolationState.CalculatedPostion + stateRingBuffer.PastInterpolationState.MovingColliderVisualsCorrection, stateRingBuffer.NextInterpolationState.CalculatedPostion + stateRingBuffer.NextInterpolationState.MovingColliderVisualsCorrection, stateRingBuffer.ActiveStateLerpTime);
			stateRingBuffer.ActiveInterpolatedState.CharacterRotation = Mathf.LerpAngle(stateRingBuffer.PastInterpolationState.CharacterRotation, stateRingBuffer.NextInterpolationState.CharacterRotation, stateRingBuffer.ActiveStateLerpTime);
		}
		stateRingBuffer.ActiveInterpolatedState.MotionX = Mathf.RoundToInt(Mathf.Lerp(stateRingBuffer.PastInterpolationState.MotionX, stateRingBuffer.NextInterpolationState.MotionX, stateRingBuffer.ActiveStateLerpTime));
		stateRingBuffer.ActiveInterpolatedState.MotionZ = Mathf.RoundToInt(Mathf.Lerp(stateRingBuffer.PastInterpolationState.MotionZ, stateRingBuffer.NextInterpolationState.MotionZ, stateRingBuffer.ActiveStateLerpTime));
		stateRingBuffer.ActiveInterpolatedState.JumpFrame = (short)Mathf.RoundToInt(Mathf.Lerp(stateRingBuffer.PastInterpolationState.JumpFrame, stateRingBuffer.NextInterpolationState.JumpFrame, stateRingBuffer.ActiveStateLerpTime));
		stateRingBuffer.ActiveInterpolatedState.Grounded = stateRingBuffer.PastInterpolationState.Grounded;
		stateRingBuffer.ActiveInterpolatedState.SlopeAngle = Mathf.Lerp(stateRingBuffer.PastInterpolationState.SlopeAngle, stateRingBuffer.NextInterpolationState.SlopeAngle, stateRingBuffer.ActiveStateLerpTime);
		stateRingBuffer.ActiveInterpolatedState.LastWorldPhysics = stateRingBuffer.PastInterpolationState.LastWorldPhysics;
		stateRingBuffer.ActiveInterpolatedState.CalculatedMotion = UnityEngine.Vector3.Lerp(stateRingBuffer.PastInterpolationState.CalculatedMotion, stateRingBuffer.NextInterpolationState.CalculatedMotion, stateRingBuffer.ActiveStateLerpTime);
		stateRingBuffer.ActiveInterpolatedState.Ghost = stateRingBuffer.PastInterpolationState.Ghost;
		stateRingBuffer.ActiveInterpolatedState.AimMovementScaleMultiplier = Mathf.Lerp(stateRingBuffer.PastInterpolationState.AimMovementScaleMultiplier, stateRingBuffer.NextInterpolationState.AimMovementScaleMultiplier, stateRingBuffer.ActiveStateLerpTime);
		if (stateRingBuffer.NextInterpolationState.AimState != CameraController.CameraType.Normal && stateRingBuffer.PastInterpolationState.AimState == CameraController.CameraType.Normal)
		{
			stateRingBuffer.ActiveInterpolatedState.AimRelativePoint = stateRingBuffer.NextInterpolationState.AimRelativePoint;
		}
		else
		{
			stateRingBuffer.ActiveInterpolatedState.AimRelativePoint = UnityEngine.Vector3.Slerp(stateRingBuffer.PastInterpolationState.AimRelativePoint, stateRingBuffer.NextInterpolationState.AimRelativePoint, stateRingBuffer.ActiveStateLerpTime);
		}
		if (PlayerReferences.IsOwner && MonoBehaviourSingleton<CameraController>.Instance.AimUsingADS && stateRingBuffer.NextInterpolationState.AimState != CameraController.CameraType.Normal)
		{
			stateRingBuffer.ActiveInterpolatedState.CharacterRotation = Mathf.LerpAngle(currentCharRotation, MonoBehaviourSingleton<CameraController>.Instance.AimYaw, 0.5f);
		}
		if (PlayerReferences.IsOwner && NetClock.CurrentFrame <= NetworkBehaviourSingleton<NetClock>.Instance.GameplayReadyFrame.Value)
		{
			MonoBehaviourSingleton<CameraController>.Instance.ResetCamera(stateRingBuffer.NextInterpolationState.CharacterRotation);
		}
		currentCharRotation = stateRingBuffer.ActiveInterpolatedState.CharacterRotation;
		currentCharPosition = stateRingBuffer.ActiveInterpolatedState.Position;
		if (stateRingBuffer.PastInterpolationState.TeleportActive && stateRingBuffer.PastInterpolationState.TeleportAtFrame == stateRingBuffer.PastInterpolationState.NetFrame + 1)
		{
			base.transform.position = currentCharPosition;
		}
		else
		{
			base.transform.position = UnityEngine.Vector3.Lerp(base.transform.position, currentCharPosition, 0.5f);
		}
		if (MonoBehaviourSingleton<StageManager>.Instance != null && MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary != null && (bool)PlayerReferences.Inventory)
		{
			slot0EventSwitch.FrameSetup(PlayerReferences.Inventory.GetEquippedId(0));
			slot1EventSwitch.FrameSetup(PlayerReferences.Inventory.GetEquippedId(1));
		}
		List<UsableDefinition.UseState> activeUseStates = stateRingBuffer.PastInterpolationState.ActiveUseStates;
		bool ads = stateRingBuffer.PastInterpolationState.AimState != CameraController.CameraType.Normal;
		int comboBookmark = 0;
		UsableDefinition usableDefinition = null;
		UnityEngine.Vector3 vector = Quaternion.Euler(0f, stateRingBuffer.ActiveInterpolatedState.CharacterRotation, 0f) * UnityEngine.Vector3.forward;
		UnityEngine.Vector3 to = Quaternion.Euler(0f, stateRingBuffer.NextInterpolationState.HorizontalCameraAim, 0f) * UnityEngine.Vector3.forward;
		float playerAngleDot = UnityEngine.Vector3.SignedAngle(vector, to, UnityEngine.Vector3.up) % 180f / 180f;
		if (activeUseStates != null && activeUseStates.Count > 0)
		{
			foreach (UsableDefinition.UseState item in activeUseStates)
			{
				InventoryUsableDefinition usableDefinition2 = RuntimeDatabase.GetUsableDefinition<InventoryUsableDefinition>(item.EquipmentGuid);
				thisAppearanceFrameTriggerableStates.Add(usableDefinition2.GetAnimationTrigger(item, stateRingBuffer.PastInterpolationState.NetFrame));
				slot0EventSwitch.CheckEUS(item);
				slot1EventSwitch.CheckEUS(item);
				if (item.GetType() == typeof(MeleeAttackUsableDefinition.MeleeAttackEquipmentUseState))
				{
					comboBookmark = ((MeleeAttackUsableDefinition.MeleeAttackEquipmentUseState)item).ComboIndex;
					usableDefinition = usableDefinition2;
				}
			}
		}
		if ((bool)PlayerReferences.PlayerEquipmentManager)
		{
			slot0EventSwitch.ProcessEvents(stateRingBuffer.PastInterpolationState, stateRingBuffer.ActiveInterpolationTime, out var equipmentShowPriority);
			slot1EventSwitch.ProcessEvents(stateRingBuffer.PastInterpolationState, stateRingBuffer.ActiveInterpolationTime, out var equipmentShowPriority2);
			PlayerReferences.PlayerEquipmentManager.SetAppearanceVisibility(equipmentShowPriority, equipmentShowPriority2);
		}
		bool moving = stateRingBuffer.ActiveInterpolatedState.MotionX != 0 || stateRingBuffer.ActiveInterpolatedState.MotionZ != 0;
		bool grounded = stateRingBuffer.ActiveInterpolatedState.Grounded;
		bool walking = !stateRingBuffer.NextInterpolationState.Run;
		float airTime = (float)stateRingBuffer.PastInterpolationState.AirborneFrames * NetClock.FixedDeltaTime;
		float fallTime = (float)stateRingBuffer.PastInterpolationState.FallFrames * NetClock.FixedDeltaTime;
		float target = Mathf.DeltaAngle(stateRingBuffer.PastInterpolationState.CharacterRotation, stateRingBuffer.NextInterpolationState.CharacterRotation) / NetClock.FixedDeltaTime / PlayerReferences.PlayerController.RotationSpeed;
		animatorAngularVelocity = Mathf.SmoothDamp(animatorAngularVelocity, target, ref animatorAngularVelocityVelocity, angularVelocitySmoothTime);
		UnityEngine.Vector3 vector2 = stateRingBuffer.NextInterpolationState.Position - stateRingBuffer.PastInterpolationState.Position;
		UnityEngine.Vector3 vector3 = VisualsTransform.InverseTransformDirection(vector2.normalized);
		float target2 = (stateRingBuffer.NextInterpolationState.Position.y - stateRingBuffer.PastInterpolationState.Position.y) / PlayerReferences.PlayerController.TerminalFallingVelocity;
		animatorVelocityX = Mathf.SmoothDamp(animatorVelocityX, vector3.x * stateRingBuffer.ActiveInterpolatedState.AimMovementScaleMultiplier, ref animatorVelocityXVelocity, velocitySmoothTime);
		animatorVelocityZ = Mathf.SmoothDamp(animatorVelocityZ, vector3.z * stateRingBuffer.ActiveInterpolatedState.AimMovementScaleMultiplier, ref animatorVelocityZVelocity, velocitySmoothTime);
		animatorVelocityY = Mathf.SmoothDamp(animatorVelocityY, target2, ref animatorVelocityYVelocity, velocitySmoothTime);
		PlayerController.AngleToForwardMotion(stateRingBuffer.ActiveInterpolatedState.CharacterRotation);
		float num = new Vector2(vector2.x, vector2.z).magnitude / (PlayerReferences.PlayerController.BaseSpeed * NetClock.FixedDeltaTime);
		if (grounded && !stateRingBuffer.PastInterpolationState.TeleportActive)
		{
			footstepController.UpdateFootsteps(num, walking);
		}
		string empty = string.Empty;
		if (thisAppearanceFrameTriggerableStates.Contains("Attack"))
		{
			bool flag = false;
			if ((bool)appearanceAnimator)
			{
				flag = appearanceAnimator.TriggerAttackCombo(comboBookmark);
			}
			if (flag && usableDefinition != null)
			{
				StartCoroutine(TriggerAttackVfxDelayed((MeleeAttackUsableDefinition)usableDefinition, comboBookmark));
			}
		}
		UnityEngine.Vector3 aimPoint = (PlayerReferences.IsOwner ? MonoBehaviourSingleton<CameraController>.Instance.LastAimPosition : (useRelativeAimPoint ? VisualsTransform.TransformPoint(stateRingBuffer.ActiveInterpolatedState.AimRelativePoint) : stateRingBuffer.ActiveInterpolatedState.AimRelativePoint));
		if ((bool)appearanceAnimator)
		{
			appearanceAnimator.SetAnimationState(stateRingBuffer.ActiveInterpolatedState.CharacterRotation, moving, walking, grounded, stateRingBuffer.ActiveInterpolatedState.SlopeAngle, airTime, fallTime, new Vector2(stateRingBuffer.ActiveInterpolatedState.MotionX, stateRingBuffer.ActiveInterpolatedState.MotionZ), animatorVelocityX, animatorVelocityY, animatorVelocityZ, animatorAngularVelocity, num, empty, comboBookmark, stateRingBuffer.ActiveInterpolatedState.Ghost, ads, playerAngleDot, aimPoint, stateRingBuffer.NextInterpolationState.UseIK);
		}
		foreach (string thisAppearanceFrameTriggerableState in thisAppearanceFrameTriggerableStates)
		{
			if (!pastAppearanceFrameTriggerableStates.Contains(thisAppearanceFrameTriggerableState) && !string.IsNullOrWhiteSpace(thisAppearanceFrameTriggerableState))
			{
				if ((bool)appearanceAnimator)
				{
					appearanceAnimator.TriggerAnimation(thisAppearanceFrameTriggerableState);
				}
				if (thisAppearanceFrameTriggerableState == "Attack" && usableDefinition != null)
				{
					StartCoroutine(TriggerAttackVfxDelayed((MeleeAttackUsableDefinition)usableDefinition, comboBookmark));
				}
			}
		}
		pastAppearanceFrameTriggerableStates.Clear();
		pastAppearanceFrameTriggerableStates.AddRange(thisAppearanceFrameTriggerableStates);
		thisAppearanceFrameTriggerableStates.Clear();
		int count = hitReactionQueue.Count;
		hitReactionQueue.RemoveAll((HealthComponent.HealthLostData hitReactionData) => hitReactionData.frame <= stateRingBuffer.PastInterpolationState.NetFrame);
		if (count > hitReactionQueue.Count)
		{
			PlayerReferences.DamageReaction.TriggerDamageReaction();
		}
		HandlePhysicsPushTriggers();
		if (teleporting && !stateRingBuffer.NextInterpolationState.GameplayTeleport)
		{
			teleporting = false;
			RuntimeDatabase.GetTeleportInfo(activeTeleportType).TeleportEnd(PlayerReferences.EndlessVisuals, appearanceAnimator?.Animator, base.transform.position);
			if (PlayerReferences.IsOwner && stateRingBuffer.PastInterpolationState.TeleportRotationSnapCamera)
			{
				MonoBehaviourSingleton<CameraController>.Instance.ResetCamera(stateRingBuffer.NextInterpolationState.TeleportRotation);
			}
		}
		else if (!teleporting && stateRingBuffer.NextInterpolationState.GameplayTeleport)
		{
			activeTeleportType = stateRingBuffer.NextInterpolationState.GameplayTeleportType;
			teleporting = true;
			RuntimeDatabase.GetTeleportInfo(activeTeleportType).TeleportStart(PlayerReferences.EndlessVisuals, appearanceAnimator?.Animator, base.transform.position);
		}
	}

	private void LateUpdate()
	{
		if (!(appearanceAnimator != null))
		{
			return;
		}
		UnityEngine.Vector3 vector;
		if (PlayerReferences.IsOwner)
		{
			vector = MonoBehaviourSingleton<CameraController>.Instance.CurrentAimPosition;
		}
		else
		{
			vector = stateRingBuffer.ActiveInterpolatedState.AimRelativePoint;
			if (useRelativeAimPoint)
			{
				vector = VisualsTransform.TransformPoint(vector);
			}
		}
		appearanceAnimator.UpdateIK(stateRingBuffer.NextInterpolationState.UseIK, stateRingBuffer.ActiveInterpolatedState.Grounded, stateRingBuffer.ActiveInterpolatedState.Ghost, vector, UnityEngine.Vector3.zero);
	}

	private void HandlePhysicsPushTriggers()
	{
		if (stateRingBuffer.NextInterpolationState.PhysicsPushState == pushState)
		{
			return;
		}
		if (pushState == PlayerPhysicsTaker.PushState.None)
		{
			if (stateRingBuffer.NextInterpolationState.PhysicsPushState == PlayerPhysicsTaker.PushState.Small)
			{
				appearanceAnimator.TriggerAnimation("SmallPush");
			}
			else
			{
				appearanceAnimator.TriggerAnimation("LargePush");
			}
		}
		else if (pushState == PlayerPhysicsTaker.PushState.Small)
		{
			if (stateRingBuffer.NextInterpolationState.PhysicsPushState == PlayerPhysicsTaker.PushState.None)
			{
				appearanceAnimator.TriggerAnimation("EndSmallPush");
			}
			else
			{
				appearanceAnimator.TriggerAnimation("LargePush");
			}
		}
		else
		{
			appearanceAnimator.TriggerAnimation("EndLargePush");
		}
		pushState = stateRingBuffer.NextInterpolationState.PhysicsPushState;
	}

	private IEnumerator TriggerAttackVfxDelayed(MeleeAttackUsableDefinition meleeAttackID, int comboBookmark)
	{
		yield return new WaitForSeconds(meleeAttackID.GetVisualEffectsDelay(comboBookmark));
		MeleeVfxPlayer visualEffect = meleeAttackID.GetVisualEffect(comboBookmark);
		if (visualEffect != null)
		{
			MeleeVfxPlayer meleeVfxPlayer = MeleeVfxPlayer.GetFromPool(visualEffect.gameObject);
			if (meleeVfxPlayer == null)
			{
				meleeVfxPlayer = Object.Instantiate(visualEffect, VisualsTransform.position, VisualsTransform.rotation);
			}
			meleeVfxPlayer.transform.position = VisualsTransform.position;
			meleeVfxPlayer.transform.rotation = VisualsTransform.rotation;
			meleeVfxPlayer.gameObject.SetActive(value: true);
			meleeVfxPlayer.PlayEffect(base.transform, VisualsTransform);
		}
	}

	public Transform GetBone(string boneName)
	{
		if ((bool)appearanceAnimator)
		{
			return appearanceAnimator.GetBone(boneName);
		}
		return null;
	}
}
