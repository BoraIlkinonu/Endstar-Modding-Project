using System;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using Endless.Gameplay.UI;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class PlayerController : EndlessBehaviour, IStartSubscriber
{
	private const int GROUND_ANGLE_RAYS = 8;

	private const float UPWARDS_FORCE_GROUND_OVERRIDE_THRESHOLD = 1f;

	private const float NORMAL_CALC_GROUNDING_RAY_DISTANCE = 0.15f;

	private const float FLAT_GROUNDING_RAY_DISTANCE = 0.02f;

	private const float EDGE_SLIDE_CHECK_OFFSET = 0.01f;

	private const float SLOPE_GROUNDING_RAY_DISTANCE = 0.195f;

	private const float PITCH_CLAMP_UPPER = 50f;

	private const float PITCH_CLAMP_LOWER = -50f;

	private const float YAW_CLAMP_UPPER = 30f;

	private const float YAW_CLAMP_LOWER = -30f;

	private const int TELEPORT_INPUT_LOCK_FRAME_DELAY = 5;

	private const uint CLIENT_SYNC_PERIOD = 4u;

	private static Collider[] hitPool = new Collider[20];

	private RaycastHit[] collisionCache = new RaycastHit[20];

	[Header("References")]
	[SerializeField]
	private PlayerReferenceManager playerReferences;

	[SerializeField]
	private LayerMask groundedLayerMask;

	[Header("Movement")]
	[SerializeField]
	private float baseSpeed = 4f;

	[SerializeField]
	private short motorFrames = 4;

	[SerializeField]
	private float rotationSpeed = 400f;

	[SerializeField]
	private AnimationCurve inputMovementRotationMultiplierCurve = new AnimationCurve();

	[SerializeField]
	private AnimationCurve strafingInputMovementRotationMultiplierCurve = new AnimationCurve();

	[SerializeField]
	private float horizontalMoveAimMultiplier;

	[SerializeField]
	private float verticalMoveAimMultiplier = 1f;

	[Header("Walk")]
	[SerializeField]
	private float walkSpeed = 1.3f;

	[Header("Gravity")]
	[SerializeField]
	private float gravityAccelerationRate = 9.81f;

	[SerializeField]
	[Min(1f)]
	private float gravityTerminalVelocityMetersPerSecond = 53f;

	[Header("Jump")]
	[SerializeField]
	private short jumpFrames = 15;

	[SerializeField]
	private float jumpForce = 8f;

	[SerializeField]
	private AnimationCurve jumpCurve = new AnimationCurve();

	[Header("Jump Helpers")]
	[SerializeField]
	private short jumpBufferFrames = 5;

	[SerializeField]
	private short coyoteTimeFrames = 4;

	[Header("Physics Calc")]
	[SerializeField]
	private float drag = 2f;

	[SerializeField]
	private float airborneDrag = 1f;

	[SerializeField]
	private float mass;

	[Header("Downed Movement")]
	[SerializeField]
	private float downedSpeed = 0.5f;

	[SerializeField]
	private float downedRotationSpeed = 80f;

	[Header("Equipment Helpers")]
	[SerializeField]
	private short equipmentPressedBufferFrames = 5;

	[Header("World Effect")]
	[SerializeField]
	private LayerMask worldEffectMask;

	[SerializeField]
	private LayerMask movingPlatformMask;

	[SerializeField]
	[Min(0f)]
	private int worldFallOffDamage;

	[Header("Grounding")]
	[SerializeField]
	private float EdgeSlideAmout = 0.25f;

	[Header("Ghost Movement")]
	[SerializeField]
	private float ghostSpeedMultiplier = 1.2f;

	[SerializeField]
	private int ghostRotationSpeed = 600;

	[SerializeField]
	private AnimationCurve ghostMovementRotationCurve;

	[SerializeField]
	private float ghostVerticalSpeed = 0.7f;

	[SerializeField]
	private int ghostVerticalMotorFrames = 10;

	[SerializeField]
	private AnimationCurve ghostVerticalMovementCurve;

	[Header("Force Recovery")]
	[SerializeField]
	private AnimationCurve forceRecoveryCurve;

	[Header("Crash Landing")]
	[SerializeField]
	private uint crashLandingStunTime_Frames = 40u;

	[SerializeField]
	private uint fallingFramesToCrashLanding = 30u;

	[SerializeField]
	[Min(0f)]
	private int crashLandingDamage = 2;

	[Header("Spawn In")]
	[SerializeField]
	private uint spawnInStunTime_Frames = 20u;

	private NetState currentState;

	public WorldUsableInteractable WorldInteractableUseQueue;

	private NetState LevelChangeTeleportState;

	private bool isUsingADS;

	private NetInput previousRawInput;

	private List<UnityEngine.Vector3> movingColliderVisualsCorrectionList = new List<UnityEngine.Vector3>();

	public NetState CurrentState => currentState;

	public float BaseSpeed => baseSpeed;

	public float WalkSpeed => walkSpeed;

	public float TerminalFallingVelocity => jumpForce;

	public float RotationSpeed => rotationSpeed;

	public float GravityAccelerationRate => gravityAccelerationRate;

	public float Mass => mass;

	public float Drag => drag;

	public float AirborneDrag => airborneDrag;

	private float GravityRate => (0f - gravityAccelerationRate) * NetClock.FixedDeltaTime;

	public float LastFrameMoveSpeedPercent { get; private set; }

	public bool UsingAimFix => currentState.UsingAimFix;

	public void ServerSetInventorySwapBlockFrames(InventoryUsableDefinition.InventoryTypes type, uint frames)
	{
		switch (type)
		{
		case InventoryUsableDefinition.InventoryTypes.Major:
			currentState.PrimarySwapBlockingFrames = frames;
			break;
		case InventoryUsableDefinition.InventoryTypes.Minor:
			currentState.SecondarySwapBlockingFrames = frames;
			break;
		}
	}

	private void OnEnable()
	{
		NetClock.Register(this);
	}

	private void OnDisable()
	{
		NetClock.Unregister(this);
	}

	public void SetInitialState(UnityEngine.Vector3 pos, float rot)
	{
		currentState = new NetState
		{
			Position = pos,
			CharacterRotation = rot
		};
		LevelChangeTeleportState = currentState;
	}

	public void SetState(NetState state)
	{
		base.transform.position = state.Position;
		state.CopyTo(ref currentState);
		if (!playerReferences.NetworkObject.IsOwner)
		{
			playerReferences.InteractableGameObject.SetActive(currentState.Downed);
		}
		if (!base.IsServer && state.serverVerifiedState && state.NetFrame < NetworkBehaviourSingleton<NetClock>.Instance.GameplayReadyFrame.Value)
		{
			SetInitialState(state.Position, state.CharacterRotation);
		}
	}

	public void HandleHealthChanged(int oldValue, int newValue)
	{
		if (NetworkManager.Singleton.IsServer && oldValue > newValue)
		{
			currentState.LastHitFrame = NetClock.CurrentFrame;
		}
	}

	public void HandleSpawnedAsOwner(float spawnRotation)
	{
		currentState.CharacterRotation = spawnRotation;
		MonoBehaviourSingleton<CameraController>.Instance.ResetCamera(currentState.CharacterRotation);
	}

	public void TriggerLevelChangeTeleport(UnityEngine.Vector3 position, float rotation)
	{
		currentState.Position = position;
		currentState.CharacterRotation = rotation;
		LevelChangeTeleportState.Position = position;
		LevelChangeTeleportState.CharacterRotation = rotation;
		playerReferences.SafeGroundComponent.RegisterSafeGround(position);
	}

	public void EndlessStart()
	{
		playerReferences.SafeGroundComponent.RegisterSafeGround(base.transform.position);
	}

	public NetState HandleLoadingFrame(uint frame)
	{
		currentState.Position = LevelChangeTeleportState.Position;
		currentState.CharacterRotation = LevelChangeTeleportState.CharacterRotation;
		base.transform.position = LevelChangeTeleportState.Position;
		currentState.Grounded = true;
		currentState.FallFrames = 0;
		currentState.AirborneFrames = 0;
		currentState.TotalForce = UnityEngine.Vector3.zero;
		currentState.NetFrame = frame;
		return currentState;
	}

	public void MovingColliderCorrection(UnityEngine.Vector3 pushAmount)
	{
		movingColliderVisualsCorrectionList.Add(pushAmount);
	}

	public NetState SimulateNetFrameWithInput(NetInput input)
	{
		currentState.ResetTempFrameValues();
		currentState.NetFrame = input.NetFrame;
		if (!playerReferences.IsOwnedByServer && input.NetFrame < NetworkBehaviourSingleton<NetClock>.Instance.GameplayReadyFrame.Value + 4)
		{
			return currentState;
		}
		currentState.CharacterRotation = input.CharacterRotation;
		currentState.Run = input.Run;
		if (currentState.GameplayTeleport && currentState.TeleportAtFrame - input.NetFrame < RuntimeDatabase.GetTeleportInfo(currentState.GameplayTeleportType).FramesToTeleport - 5)
		{
			input.Clear();
		}
		if (base.IsServer && playerReferences.Inventory != null)
		{
			playerReferences.Inventory.HandleEquipmentQueue(currentState);
			if ((input.PrimaryEquipment != NetInput.PrimaryEquipmentInput.None && previousRawInput.PrimaryEquipment == NetInput.PrimaryEquipmentInput.None) || (input.SecondaryEquipment && !previousRawInput.SecondaryEquipment))
			{
				playerReferences.Inventory.CancelEquipmentSwapQueue();
			}
		}
		if (playerReferences.PlayerGhostController != null)
		{
			playerReferences.PlayerGhostController.SetGhostMode(input.Ghost);
			currentState.Ghost = input.Ghost;
		}
		if ((bool)playerReferences.PlayerDownedComponent)
		{
			currentState.Downed = playerReferences.PlayerDownedComponent.GetDowned(input.NetFrame);
			currentState.Reviving = playerReferences.PlayerDownedComponent.GetReviving(input.NetFrame);
		}
		else
		{
			currentState.Downed = false;
			currentState.Reviving = false;
		}
		if (!playerReferences.NetworkObject.IsOwner)
		{
			playerReferences.InteractableGameObject.SetActive(currentState.Downed);
		}
		if (currentState.Downed && currentState.Reviving)
		{
			currentState.Position = base.transform.position;
			currentState.NetFrame = input.NetFrame;
			return currentState;
		}
		currentState.CalculatedMotion = UnityEngine.Vector3.zero;
		if (currentState.TeleportStatus != TeleportComponent.TeleportStatusType.None && input.NetFrame == currentState.TeleportAtFrame)
		{
			if (currentState.TeleportStatus == TeleportComponent.TeleportStatusType.WorldFallOff)
			{
				if ((bool)playerReferences.HealthComponent)
				{
					playerReferences.HittableComponent.ModifyHealth(new HealthModificationArgs(-worldFallOffDamage, Context.StaticLevelContext, DamageType.Normal, HealthChangeType.WorldFallOff));
					playerReferences.PlayerStunComponent.ApplyStun(NetClock.CurrentSimulationFrame, crashLandingStunTime_Frames);
					currentState.FallFrames = 40;
					currentState.Grounded = false;
					currentState.TotalForce = UnityEngine.Vector3.zero;
				}
				else
				{
					playerReferences.PlayerStunComponent.ApplyStun(NetClock.CurrentSimulationFrame, spawnInStunTime_Frames);
				}
			}
			HandleTeleport(input);
			return currentState;
		}
		currentState.UseInputRotation = input.Horizontal != 0 || input.Vertical != 0;
		currentState.InputRotation = ((input.Horizontal != 0 || input.Vertical != 0) ? Mathf.Repeat(MotionInputToRotation(input.Horizontal, input.Vertical) + input.MotionRotation, 360f) : currentState.CharacterRotation);
		if (currentState.Downed)
		{
			input.Jump = false;
			input.PrimaryEquipment = NetInput.PrimaryEquipmentInput.None;
			input.SecondaryEquipment = false;
		}
		UnityEngine.Vector3 position = base.transform.position;
		if (currentState.Ghost)
		{
			ProcessGhost_NetFrame();
			ProcessStun_NetFrame(input.NetFrame);
			ProcessInput_NetFrame(input, ghostMovementRotationCurve);
			ProcessInputVerticalGhost_NetFrame(input);
			playerReferences.CharacterController.Move(currentState.CalculatedMotion * NetClock.FixedDeltaTime);
			ProcessRotation_NetFrame(position, ghostRotationSpeed);
			if (input.UseIK)
			{
				ProcessAimRotation_NetFrame(ref currentState, input.FocusPoint, ghostRotationSpeed);
			}
			currentState.Grounded = false;
		}
		else
		{
			ProcessStun_NetFrame(input.NetFrame);
			if ((bool)playerReferences.Inventory)
			{
				ProcessEquipment_NetFrame(input);
			}
			ProcessInput_NetFrame(input, currentState.Strafing ? strafingInputMovementRotationMultiplierCurve : inputMovementRotationMultiplierCurve);
			ProcessJump_NetFrame(input);
			ProcessPhysics_NetFrame(input);
			ProcessGroundCalculation_NetFrame();
			ProcessSlopeMovement_NetFrame();
			UnityEngine.Vector3 vector = (currentState.TotalForce + currentState.CalculatedMotion) * NetClock.FixedDeltaTime;
			playerReferences.CharacterController.Move(vector);
			UnityEngine.Vector3 vector2 = currentState.CalculatedMotion + currentState.TotalForce;
			vector2.x = vector2.x / currentState.XZInputMultiplierThisFrame * horizontalMoveAimMultiplier;
			vector2.y = (currentState.Grounded ? 0f : (baseSpeed * verticalMoveAimMultiplier));
			vector2.z = vector2.z / currentState.XZInputMultiplierThisFrame * horizontalMoveAimMultiplier;
			float magnitude = vector2.magnitude;
			if (magnitude > 0.05f)
			{
				float num = Mathf.Clamp(magnitude / baseSpeed, 0f, 1f);
				CrosshairUI.Instance.OnMoved(num);
				LastFrameMoveSpeedPercent = num;
			}
			else
			{
				LastFrameMoveSpeedPercent = 0f;
			}
			ProcessCollisionFromForce(vector, position);
			ProcessRotation_NetFrame(position, currentState.Downed ? downedRotationSpeed : rotationSpeed);
			if (input.UseIK)
			{
				ProcessAimRotation_NetFrame(ref currentState, input.FocusPoint, currentState.Downed ? downedRotationSpeed : rotationSpeed);
			}
			ProcessFallOffStage_NetFrame();
			ProcessSafePosition_NetFrame();
		}
		currentState.NetFrame = input.NetFrame;
		currentState.Position = base.transform.position;
		if (!currentState.Ghost)
		{
			if (currentState.Grounded)
			{
				currentState.FallFrames = 0;
			}
			else if (currentState.CalculatedMotion.y + currentState.TotalForce.y >= 0f)
			{
				currentState.FallFrames = 0;
			}
			else
			{
				currentState.FallFrames++;
			}
		}
		else
		{
			currentState.FallFrames++;
		}
		if ((NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost) && currentState.TeleportStatus == TeleportComponent.TeleportStatusType.OnCooldown && input.NetFrame >= playerReferences.TeleportComponent.TeleportReadyFrame)
		{
			currentState.TeleportStatus = TeleportComponent.TeleportStatusType.None;
		}
		currentState.PrimarySwapBlockingFrames = ((currentState.PrimarySwapBlockingFrames != 0) ? (currentState.PrimarySwapBlockingFrames - 1) : 0u);
		currentState.SecondarySwapBlockingFrames = ((currentState.SecondarySwapBlockingFrames != 0) ? (currentState.SecondarySwapBlockingFrames - 1) : 0u);
		currentState.UseIK = input.UseIK;
		return currentState;
	}

	public NetState EndFrame()
	{
		if (movingColliderVisualsCorrectionList.Count > 0)
		{
			for (int i = 0; i < movingColliderVisualsCorrectionList.Count; i++)
			{
				currentState.MovingColliderVisualsCorrection += movingColliderVisualsCorrectionList[i];
			}
			currentState.MovingColliderVisualsCorrection /= movingColliderVisualsCorrectionList.Count;
			movingColliderVisualsCorrectionList.Clear();
		}
		playerReferences.PlayerStunComponent.EndFrame(ref currentState);
		return currentState;
	}

	private void ProcessGhost_NetFrame()
	{
		currentState.TotalForce = UnityEngine.Vector3.zero;
		currentState.FallFrames = 0;
	}

	private void ProcessStun_NetFrame(uint frame)
	{
		if (currentState.Ghost)
		{
			currentState.StunFrame = 0u;
		}
		else
		{
			playerReferences.PlayerStunComponent.ProcessStun(frame, ref currentState);
		}
	}

	private void HandleTeleport(NetInput input)
	{
		base.transform.position = currentState.TeleportPosition;
		currentState.TeleportStatus = ((input.NetFrame < playerReferences.TeleportComponent.TeleportReadyFrame) ? TeleportComponent.TeleportStatusType.OnCooldown : TeleportComponent.TeleportStatusType.None);
		currentState.Position = base.transform.position;
		currentState.NetFrame = input.NetFrame;
		currentState.GameplayTeleport = false;
		if (currentState.TeleportHasRotation)
		{
			currentState.CharacterRotation = currentState.TeleportRotation;
		}
	}

	private void ProcessEquipment_NetFrame(NetInput input)
	{
		SerializableGuid serializableGuid = SerializableGuid.Empty;
		Item item = null;
		if (currentState.ActiveUseStates == null)
		{
			currentState.ActiveUseStates = new List<UsableDefinition.UseState>();
		}
		if (currentState.SecondarySwapActive)
		{
			input.SecondaryEquipment = false;
		}
		bool flag = false;
		currentState.HorizontalCameraAim = input.MotionRotation;
		if (playerReferences.PlayerInteractor.CurrentInteractable != null && playerReferences.PlayerInteractor.CurrentInteractable.IsHeldInteraction)
		{
			currentState.BlockItemInput = true;
		}
		if (!currentState.BlockItemInput && !CurrentState.PrimarySwapActive)
		{
			if (input.PrimaryEquipment != NetInput.PrimaryEquipmentInput.None)
			{
				serializableGuid = RuntimeDatabase.GetUsableDefinitionIDFromAssetID(playerReferences.Inventory.GetEquippedId(0));
				item = playerReferences.Inventory.GetEquippedItem(0);
			}
			if (serializableGuid.Equals(SerializableGuid.Empty) && input.SecondaryEquipment)
			{
				serializableGuid = RuntimeDatabase.GetUsableDefinitionIDFromAssetID(playerReferences.Inventory.GetEquippedId(1));
				item = playerReferences.Inventory.GetEquippedItem(1);
			}
		}
		if (currentState.CurrentPressedEquipment.CompareTo(serializableGuid) == 1)
		{
			currentState.CurrentPressedEquipment = serializableGuid;
			currentState.EquipmentPressedDuration = 0;
		}
		if (currentState.ActiveWorldUseState != null && !RuntimeDatabase.GetUsableDefinition(currentState.ActiveWorldUseState.EquipmentGuid).ProcessUseFrame(ref currentState, input, ref currentState.ActiveWorldUseState, playerReferences, equipped: false, pressed: false))
		{
			currentState.ActiveWorldUseState = null;
		}
		if (WorldInteractableUseQueue != null && !currentState.IsStunned)
		{
			if (currentState.ActiveWorldUseState == null && WorldInteractableUseQueue.WorldUsableDefinition != null)
			{
				playerReferences.PlayerInteractor.MostRecentInteraction = WorldInteractableUseQueue;
				currentState.ActiveWorldUseState = WorldInteractableUseQueue.WorldUsableDefinition.ProcessUseStart(ref currentState, input, WorldInteractableUseQueue.WorldUsableDefinition.Guid, playerReferences, null);
			}
			WorldInteractableUseQueue = null;
		}
		for (int i = 0; i < currentState.ActiveUseStates.Count; i++)
		{
			UsableDefinition.UseState useState = currentState.ActiveUseStates[i];
			InventoryUsableDefinition inventoryUsableDefinition = (InventoryUsableDefinition)RuntimeDatabase.GetUsableDefinition(useState.EquipmentGuid);
			bool equipped = inventoryUsableDefinition.Guid == RuntimeDatabase.GetUsableDefinitionIDFromAssetID(playerReferences.Inventory.GetEquippedId((inventoryUsableDefinition.InventoryType != InventoryUsableDefinition.InventoryTypes.Major) ? 1 : 0));
			bool flag2 = !currentState.IsStunned && serializableGuid.Equals(useState.EquipmentGuid);
			if (flag2 && useState.Item != item)
			{
				useState.Item = item;
			}
			if (inventoryUsableDefinition.ProcessUseFrame(ref currentState, input, ref useState, playerReferences, equipped, flag2))
			{
				currentState.ActiveUseStates[i] = useState;
			}
			else
			{
				currentState.ActiveUseStates[i] = null;
			}
			if (serializableGuid == useState.EquipmentGuid)
			{
				flag = true;
			}
		}
		currentState.ActiveUseStates.RemoveAll((UsableDefinition.UseState useState3) => useState3 == null);
		if (!currentState.BlockItemInput && !currentState.IsStunned && !flag && serializableGuid.CompareTo(SerializableGuid.Empty) == 1 && equipmentPressedBufferFrames >= currentState.EquipmentPressedDuration)
		{
			UsableDefinition usableDefinition = RuntimeDatabase.GetUsableDefinition(serializableGuid);
			UsableDefinition.UseState useState2 = usableDefinition.ProcessUseStart(ref currentState, input, serializableGuid, playerReferences, item);
			if (useState2 != null && usableDefinition.ProcessUseFrame(ref currentState, input, ref useState2, playerReferences, equipped: true, pressed: true))
			{
				currentState.ActiveUseStates.Add(useState2);
			}
		}
		bool flag3 = MonoBehaviourSingleton<CameraController>.Instance.GameStateADS != CameraController.CameraType.Normal;
		if (flag3 ^ isUsingADS)
		{
			isUsingADS = flag3;
			if (isUsingADS)
			{
				playerReferences.PlayerNetworkController.EnableAimIK(MonoBehaviourSingleton<CameraController>.Instance.LastAimPosition);
			}
			else
			{
				playerReferences.PlayerNetworkController.DisableAimIK();
			}
		}
		else if (isUsingADS)
		{
			playerReferences.PlayerNetworkController.SetAimIKPosition(MonoBehaviourSingleton<CameraController>.Instance.LastAimPosition);
		}
	}

	public void ProcessWorldTriggerCheck_NetFrame(uint frame, bool onDestroy = false)
	{
		if (!(playerReferences.WorldCollidable != null))
		{
			return;
		}
		float num = playerReferences.CharacterController.height * 0.5f - playerReferences.CharacterController.radius;
		UnityEngine.Vector3 point = base.transform.position + playerReferences.CharacterController.center - UnityEngine.Vector3.up * num;
		UnityEngine.Vector3 point2 = base.transform.position + playerReferences.CharacterController.center + UnityEngine.Vector3.up * num;
		int num2 = Physics.OverlapCapsuleNonAlloc(point, point2, playerReferences.CharacterController.radius, hitPool, worldEffectMask, QueryTriggerInteraction.Collide);
		for (int i = 0; i < num2; i++)
		{
			WorldTriggerCollider component = hitPool[i].GetComponent<WorldTriggerCollider>();
			if (component != null)
			{
				if (onDestroy)
				{
					component.WorldTrigger.DestroyOverlap(playerReferences.WorldCollidable, frame);
				}
				else
				{
					component.WorldTrigger.Overlapped(playerReferences.WorldCollidable, frame);
				}
			}
		}
	}

	private void ProcessInput_NetFrame(NetInput input, AnimationCurve rotationMovementCurve)
	{
		if (currentState.IsStunned)
		{
			currentState.MotionX = 0;
			currentState.MotionZ = 0;
		}
		else
		{
			currentState.MotionX = MotorFromInput(input.Horizontal, currentState.MotionX, motorFrames);
			currentState.MotionZ = MotorFromInput(input.Vertical, currentState.MotionZ, motorFrames);
		}
		if (currentState.Ghost)
		{
			currentState.GhostVerticalMotorFrame = MotorFromInput((input.Jump ? 1 : 0) + (input.Down ? (-1) : 0), currentState.GhostVerticalMotorFrame, ghostVerticalMotorFrames, 3);
			currentState.JumpFrame = 0;
		}
		else
		{
			currentState.GhostVerticalMotorFrame = 0;
		}
		currentState.JumpPressedDuration = ((input.Jump && !currentState.IsStunned) ? (currentState.JumpPressedDuration + 1) : 0);
		currentState.EquipmentPressedDuration = ((input.PrimaryEquipment != NetInput.PrimaryEquipmentInput.None || input.SecondaryEquipment) ? (currentState.EquipmentPressedDuration + 1) : 0);
		if (!currentState.BlockMotionXZ)
		{
			UnityEngine.Vector3 directionInput = new UnityEngine.Vector3(Mathf.Lerp(-1f, 1f, (float)(motorFrames + currentState.MotionX) / (float)(motorFrames * 2)), 0f, Mathf.Lerp(-1f, 1f, (float)(motorFrames + currentState.MotionZ) / (float)(motorFrames * 2)));
			directionInput = RotateMotionDirection(directionInput, input.MotionRotation);
			directionInput = UnityEngine.Vector3.ClampMagnitude(directionInput, 1f);
			directionInput *= (currentState.Downed ? downedSpeed : (input.Run ? baseSpeed : walkSpeed));
			directionInput *= currentState.XZInputMultiplierThisFrame;
			directionInput *= rotationMovementCurve.Evaluate(Mathf.Abs(Mathf.DeltaAngle(currentState.CharacterRotation, currentState.InputRotation) / 180f));
			if (currentState.Ghost)
			{
				directionInput *= ghostSpeedMultiplier;
			}
			float num = forceRecoveryCurve.Evaluate(currentState.PushForceControl);
			currentState.CalculatedMotion += directionInput * num;
		}
	}

	private void ProcessInputVerticalGhost_NetFrame(NetInput input)
	{
		float y = ghostVerticalSpeed * ghostVerticalMovementCurve.Evaluate((float)currentState.GhostVerticalMotorFrame / (float)ghostVerticalMotorFrames);
		currentState.CalculatedMotion += new UnityEngine.Vector3(0f, y, 0f);
	}

	private void ProcessJump_NetFrame(NetInput input)
	{
		if (currentState.Grounded && !currentState.BlockJump)
		{
			currentState.AirborneFrames = 0;
		}
		else
		{
			currentState.AirborneFrames++;
		}
		if (currentState.JumpFrame > -1 && !currentState.IsStunned && currentState.FramesSinceStableGround <= coyoteTimeFrames && currentState.JumpPressedDuration > 0 && currentState.JumpPressedDuration <= jumpBufferFrames && !currentState.BlockJump)
		{
			currentState.JumpFrame = (short)(-jumpFrames);
			currentState.JumpReleasedThisJump = false;
		}
		if (currentState.JumpFrame < 0)
		{
			if (ScanHeadBumped())
			{
				currentState.JumpReleasedThisJump = true;
				currentState.JumpFrame = 0;
			}
			else if (!currentState.JumpReleasedThisJump && (!input.Jump || currentState.IsStunned))
			{
				currentState.JumpReleasedThisJump = true;
			}
		}
		currentState.JumpFrame = Mathf.Min(currentState.JumpFrame + 1, 0);
		if (currentState.JumpFrame == 0 && currentState.LastWorldPhysics.magnitude > 0.02f)
		{
			currentState.JumpFrame = 0;
		}
		else if (currentState.JumpReleasedThisJump && currentState.JumpFrame < 0 && !currentState.BlockMotionY)
		{
			currentState.JumpFrame += 2;
		}
		if (currentState.JumpFrame < 1)
		{
			currentState.CalculatedMotion.y = (currentState.BlockMotionY ? currentState.CalculatedMotion.y : (jumpCurve.Evaluate((float)currentState.JumpFrame / (float)jumpFrames) * jumpForce));
		}
	}

	private bool ScanHeadBumped()
	{
		return Physics.Raycast(base.transform.position + UnityEngine.Vector3.up * playerReferences.CharacterController.height, UnityEngine.Vector3.up, 0.05f, groundedLayerMask);
	}

	public void ProcessPhysics_NetFrame(NetInput input)
	{
		playerReferences.PhysicsTaker.GetFramePhysics(input.NetFrame, ref currentState);
		if (!currentState.BlockGravity)
		{
			currentState.TotalForce.y = Mathf.Max(currentState.TotalForce.y + GravityRate, 0f - gravityTerminalVelocityMetersPerSecond);
		}
		UnityEngine.Vector3 vector = currentState.TotalForce * (1f - NetClock.FixedDeltaTime * (currentState.Grounded ? Drag : AirborneDrag));
		currentState.TotalForce.x = vector.x;
		currentState.TotalForce.z = vector.z;
	}

	private void ProcessCollisionFromForce(UnityEngine.Vector3 expectedMovement, UnityEngine.Vector3 positionBeforeMove)
	{
		expectedMovement *= 0.9f;
		UnityEngine.Vector3 vector = base.transform.position - positionBeforeMove;
		if ((expectedMovement.y > 0f && vector.y < expectedMovement.y) || (expectedMovement.y < 0f && vector.y > expectedMovement.y))
		{
			if (currentState.TotalForce.y < 0f)
			{
				currentState.TotalForce.y = GravityRate;
			}
			else
			{
				currentState.TotalForce.y = 0f;
			}
		}
		if ((expectedMovement.x > 0f && vector.x < expectedMovement.x) || (expectedMovement.x < 0f && vector.x > expectedMovement.x))
		{
			currentState.TotalForce.x = 0f;
		}
		if ((expectedMovement.z > 0f && vector.z < expectedMovement.z) || (expectedMovement.z < 0f && vector.z > expectedMovement.z))
		{
			currentState.TotalForce.z = 0f;
		}
	}

	public void ProcessGroundCalculation_NetFrame()
	{
		bool flag = false;
		float distance = playerReferences.CharacterController.radius + 0.15f + playerReferences.CharacterController.skinWidth;
		currentState.GroundNormal = UnityEngine.Vector3.zero;
		if (currentState.TotalForce.y + currentState.CalculatedMotion.y < 1f)
		{
			bool flag2 = false;
			UnityEngine.Vector3 zero = UnityEngine.Vector3.zero;
			int num = 0;
			float num2 = 0.02f + playerReferences.CharacterController.radius + playerReferences.CharacterController.skinWidth;
			for (int i = 0; i < 8; i++)
			{
				float f = (float)i * MathF.PI * 2f / 8f;
				UnityEngine.Vector3 vector = new UnityEngine.Vector3(Mathf.Cos(f) * playerReferences.CharacterController.radius, playerReferences.CharacterController.radius, Mathf.Sin(f) * playerReferences.CharacterController.radius);
				if (GroundCalculationRay(vector, distance, out var hit))
				{
					currentState.GroundNormal += hit.normal;
					if (hit.normal != UnityEngine.Vector3.up)
					{
						flag2 = true;
					}
					if (hit.distance > num2)
					{
						zero += vector;
					}
					else
					{
						num++;
					}
				}
				else
				{
					zero += vector;
				}
			}
			float num3 = playerReferences.CharacterController.radius + (flag2 ? 0.195f : 0.02f);
			if (num > 3 || GroundCalculationRay(new UnityEngine.Vector3(0f, playerReferences.CharacterController.radius, 0f), num3 + playerReferences.CharacterController.skinWidth, out var _, draw: true))
			{
				currentState.StableGround = true;
				currentState.FramesSinceStableGround = 0;
				flag = true;
			}
			else if (num > 0)
			{
				currentState.StableGround = false;
				currentState.FramesSinceStableGround++;
				flag = true;
				if (!currentState.IsStunned)
				{
					zero.y = 0f;
					currentState.CalculatedMotion += zero.normalized * EdgeSlideAmout;
				}
			}
			else if (CheckGroundingOnComplexGeometry())
			{
				currentState.StableGround = false;
				currentState.FramesSinceStableGround++;
				flag = true;
			}
			else
			{
				currentState.FramesSinceStableGround++;
			}
		}
		else
		{
			currentState.StableGround = false;
			currentState.FramesSinceStableGround++;
		}
		if (flag)
		{
			currentState.TotalForce.y = GravityRate;
			if (currentState.FallFrames > fallingFramesToCrashLanding && !currentState.BlockGrounding && CheckIfCanTakeFallingDamageBasedOnTeleportState())
			{
				playerReferences.PlayerStunComponent.ApplyStun(NetClock.CurrentSimulationFrame, crashLandingStunTime_Frames);
				if (base.IsServer)
				{
					CheckAndCancelReload();
				}
				if ((NetworkManager.Singleton.IsServer || NetClock.CurrentSimulationFrame == NetClock.CurrentFrame) && (bool)playerReferences.HealthComponent)
				{
					playerReferences.HittableComponent.ModifyHealth(new HealthModificationArgs(-crashLandingDamage, Context.StaticLevelContext, DamageType.Normal, HealthChangeType.WorldFallOff));
				}
			}
		}
		currentState.GroundNormal.Normalize();
		UnityEngine.Vector3 to = RotateMotionDirection(UnityEngine.Vector3.forward, currentState.CharacterRotation);
		float num4 = UnityEngine.Vector3.Angle(currentState.GroundNormal, to);
		currentState.SlopeAngle = ((!flag) ? 0f : ((num4 - 90f) / 45f));
		currentState.GroundSlope = UnityEngine.Vector3.Angle(currentState.GroundNormal, UnityEngine.Vector3.up);
		currentState.Grounded = flag;
		currentState.GroundedFrames = (flag ? (currentState.GroundedFrames + 1) : (-1));
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawWireSphere(base.transform.position + UnityEngine.Vector3.up * (playerReferences.CharacterController.radius - 0.01f), playerReferences.CharacterController.radius);
	}

	private bool CheckIfCanTakeFallingDamageBasedOnTeleportState()
	{
		if (currentState.TeleportStatus == TeleportComponent.TeleportStatusType.None)
		{
			return true;
		}
		if (currentState.TeleportStatus == TeleportComponent.TeleportStatusType.WorldFallOff)
		{
			return false;
		}
		if (currentState.TeleportStatus == TeleportComponent.TeleportStatusType.OnCooldown && currentState.NetFrame - 1 == currentState.TeleportAtFrame)
		{
			return false;
		}
		return true;
	}

	private bool CheckGroundingOnComplexGeometry()
	{
		float num = playerReferences.CharacterController.radius + playerReferences.CharacterController.skinWidth;
		int num2 = Physics.SphereCastNonAlloc(base.transform.position + UnityEngine.Vector3.up * (num + 0.2f), playerReferences.CharacterController.radius, UnityEngine.Vector3.down, collisionCache, 0.225f + playerReferences.CharacterController.skinWidth, groundedLayerMask, QueryTriggerInteraction.Ignore);
		UnityEngine.Vector3 zero = UnityEngine.Vector3.zero;
		if (num2 < 1)
		{
			return false;
		}
		if (!currentState.IsStunned)
		{
			for (int i = 0; i < num2; i++)
			{
				UnityEngine.Vector3 vector = collisionCache[i].point - base.transform.position;
				zero += vector * EdgeSlideAmout * -1f;
			}
			zero.y = 0f;
			zero.Normalize();
			currentState.CalculatedMotion += zero;
		}
		return true;
	}

	private void ProcessSlopeMovement_NetFrame()
	{
		if (currentState.Grounded && currentState.CalculatedMotion.y + currentState.TotalForce.y <= 0f && !Mathf.Approximately(0f, UnityEngine.Vector3.Angle(currentState.GroundNormal, UnityEngine.Vector3.up)))
		{
			UnityEngine.Vector3 vector = new UnityEngine.Vector3(currentState.CalculatedMotion.x, 0f, currentState.CalculatedMotion.z);
			UnityEngine.Vector3 calculatedMotion = Quaternion.FromToRotation(UnityEngine.Vector3.up, currentState.GroundNormal) * vector;
			if (calculatedMotion.y >= 0f || !currentState.BlockMotionY_Down)
			{
				currentState.CalculatedMotion = calculatedMotion;
			}
		}
	}

	private void ProcessRotation_NetFrame(UnityEngine.Vector3 pastPosition, float speed)
	{
		if (!currentState.BlockRotation && !Mathf.Approximately(new Vector2(base.transform.position.x - pastPosition.x, base.transform.position.z - pastPosition.z).magnitude, 0f))
		{
			float targetRotation = (((base.transform.position - pastPosition).magnitude < 0.001f) ? 0f : Quaternion.LookRotation(base.transform.position - pastPosition).eulerAngles.y);
			Rotate(ref currentState, targetRotation, speed, onlyRotateTowardsInput: true);
		}
	}

	private void ProcessAimRotation_NetFrame(ref NetState state, UnityEngine.Vector3 focusPoint, float speed)
	{
		Transform visualsTransform = playerReferences.ApperanceController.VisualsTransform;
		UnityEngine.Vector3 position = visualsTransform.position;
		float y = visualsTransform.eulerAngles.y;
		UnityEngine.Vector3 forward = focusPoint - position;
		float magnitude = forward.magnitude;
		forward /= magnitude;
		UnityEngine.Vector3 eulerAngles = Quaternion.LookRotation(forward, UnityEngine.Vector3.up).eulerAngles;
		float x = Mathf.DeltaAngle(0f, eulerAngles.x);
		float y2 = Mathf.MoveTowardsAngle(y, eulerAngles.y, playerReferences.ApperanceController.AppearanceAnimator.HorizontalAimLimit);
		UnityEngine.Vector3 vector = Quaternion.Euler(x, y2, 0f) * UnityEngine.Vector3.forward * magnitude + position;
		if (playerReferences.ApperanceController.UseRelativeAimPoint)
		{
			state.AimRelativePoint = visualsTransform.InverseTransformPoint(vector);
		}
		else
		{
			state.AimRelativePoint = vector;
		}
	}

	private Quaternion ClampPitchAndYaw(Quaternion q, float maxPitch, float maxYaw)
	{
		q.x /= q.w;
		q.y /= q.w;
		q.z /= q.w;
		q.w = 1f;
		float value = 114.59156f * Mathf.Atan(q.x);
		value = Mathf.Clamp(value, 0f - maxPitch, maxPitch);
		q.x = Mathf.Tan(MathF.PI / 360f * value);
		value = 114.59156f * Mathf.Atan(q.y);
		value = Mathf.Clamp(value, 0f - maxYaw, maxYaw);
		q.y = Mathf.Tan(MathF.PI / 360f * value);
		return q.normalized;
	}

	private void ProcessFallOffStage_NetFrame()
	{
		if ((bool)MonoBehaviourSingleton<StageManager>.Instance.ActiveStage && base.transform.position.y < MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.StageFallOffHeight)
		{
			playerReferences.TeleportComponent.WorldFallOffTriggered(ref currentState, playerReferences.SafeGroundComponent.LastSafePosition);
		}
	}

	private void ProcessSafePosition_NetFrame()
	{
		if (currentState.Grounded && NetworkManager.Singleton.IsServer && MonoBehaviourSingleton<StageManager>.Instance.ActiveStage != null && base.transform.position.y > MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.StageFallOffHeight && NetClock.CurrentFrame % 10 == 0 && currentState.StableGround)
		{
			playerReferences.SafeGroundComponent.RegisterSafeGround(currentState.Position);
		}
	}

	public static bool Rotate(ref NetState state, float targetRotation, float speed, bool onlyRotateTowardsInput = false)
	{
		if (onlyRotateTowardsInput && Mathf.Abs(Mathf.DeltaAngle(targetRotation, state.CharacterRotation)) > Mathf.Abs(Mathf.DeltaAngle(state.InputRotation, state.CharacterRotation)))
		{
			targetRotation = state.InputRotation;
		}
		if (Mathf.Abs(Mathf.DeltaAngle(targetRotation, state.CharacterRotation)) > 175f)
		{
			targetRotation = state.CharacterRotation + 175f;
		}
		state.CharacterRotation = Mathf.MoveTowardsAngle(state.CharacterRotation, targetRotation, speed * NetClock.FixedDeltaTime);
		return Mathf.Approximately(0f, Mathf.DeltaAngle(state.CharacterRotation, targetRotation));
	}

	private bool GroundCalculationRay(UnityEngine.Vector3 relativePosition, float distance, out RaycastHit hit, bool draw = false)
	{
		UnityEngine.Vector3 vector = base.transform.position + relativePosition;
		if (Physics.Raycast(vector, UnityEngine.Vector3.down, out hit, 1.7f, groundedLayerMask, QueryTriggerInteraction.Ignore))
		{
			if (hit.distance > distance)
			{
				if (draw)
				{
					Debug.DrawLine(vector, vector + UnityEngine.Vector3.down * distance, UnityEngine.Color.red);
				}
				return false;
			}
			if (draw)
			{
				Debug.DrawLine(vector, hit.point, UnityEngine.Color.green);
				Debug.DrawLine(hit.point, vector + UnityEngine.Vector3.down * distance, UnityEngine.Color.cyan);
			}
			return true;
		}
		if (draw)
		{
			Debug.DrawLine(vector, vector + UnityEngine.Vector3.down * distance, UnityEngine.Color.red);
		}
		return false;
	}

	public static int MotorFromInput(float input, int motor, int motorFrames, int degradeRate = 1)
	{
		if (input == 0f)
		{
			return ((motor > 0) ? 1 : (-1)) * Mathf.Max(0, Mathf.Abs(motor) - degradeRate);
		}
		if (motor == 0 || Mathf.Sign(input) == Mathf.Sign(motor))
		{
			if (!Mathf.Approximately((float)motor + input, (float)Mathf.Abs(motor) + Mathf.Abs(input)))
			{
				input *= (float)degradeRate;
			}
			return Mathf.Clamp(motor + ((input >= 0f) ? 1 : (-1)), -motorFrames, motorFrames);
		}
		return 0;
	}

	public static UnityEngine.Vector3 RotateMotionDirection(UnityEngine.Vector3 directionInput, float rotation)
	{
		return Quaternion.Euler(0f, rotation, 0f) * directionInput;
	}

	public static float MotionInputToRotation(int horizontal, int vertical)
	{
		return Vector2.SignedAngle(new Vector2(horizontal, vertical), Vector2.up);
	}

	public static float DirectionToAngle(UnityEngine.Vector3 direction)
	{
		return Vector2.SignedAngle(new Vector2(direction.x, direction.z), Vector2.up);
	}

	public static UnityEngine.Vector3 AngleToForwardMotion(float angle)
	{
		return Quaternion.AngleAxis(angle, UnityEngine.Vector3.up) * UnityEngine.Vector3.forward;
	}

	public void TriggerTeleport(UnityEngine.Vector3 position, float rotation, TeleportType teleportType, bool snapCamera, Context context = null)
	{
		if (base.IsServer)
		{
			playerReferences.PlayerInteractor?.CurrentInteractable?.InteractionStopped(playerReferences.PlayerInteractor);
		}
		playerReferences.TeleportComponent.TeleportTriggered(ref currentState, position, ignoreCooldown: true, TeleportComponent.TeleportType.Regular, RuntimeDatabase.GetTeleportInfo(teleportType).FramesToTeleport, overrideRotation: true, rotation, teleportType, snapCamera);
	}

	protected override void OnDestroy()
	{
		if ((bool)NetworkBehaviourSingleton<NetClock>.Instance)
		{
			ProcessWorldTriggerCheck_NetFrame(NetClock.CurrentFrame, onDestroy: true);
		}
		base.OnDestroy();
	}

	public void CheckAndCancelReload()
	{
		if (playerReferences.Inventory != null)
		{
			playerReferences.Inventory.CheckAndCancelReload();
		}
	}
}
