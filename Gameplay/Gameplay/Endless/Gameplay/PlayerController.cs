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

namespace Endless.Gameplay
{
	// Token: 0x0200026E RID: 622
	public class PlayerController : EndlessBehaviour, IStartSubscriber
	{
		// Token: 0x17000257 RID: 599
		// (get) Token: 0x06000CE4 RID: 3300 RVA: 0x00045364 File Offset: 0x00043564
		public NetState CurrentState
		{
			get
			{
				return this.currentState;
			}
		}

		// Token: 0x17000258 RID: 600
		// (get) Token: 0x06000CE5 RID: 3301 RVA: 0x0004536C File Offset: 0x0004356C
		public float BaseSpeed
		{
			get
			{
				return this.baseSpeed;
			}
		}

		// Token: 0x17000259 RID: 601
		// (get) Token: 0x06000CE6 RID: 3302 RVA: 0x00045374 File Offset: 0x00043574
		public float WalkSpeed
		{
			get
			{
				return this.walkSpeed;
			}
		}

		// Token: 0x1700025A RID: 602
		// (get) Token: 0x06000CE7 RID: 3303 RVA: 0x0004537C File Offset: 0x0004357C
		public float TerminalFallingVelocity
		{
			get
			{
				return this.jumpForce;
			}
		}

		// Token: 0x1700025B RID: 603
		// (get) Token: 0x06000CE8 RID: 3304 RVA: 0x00045384 File Offset: 0x00043584
		public float RotationSpeed
		{
			get
			{
				return this.rotationSpeed;
			}
		}

		// Token: 0x1700025C RID: 604
		// (get) Token: 0x06000CE9 RID: 3305 RVA: 0x0004538C File Offset: 0x0004358C
		public float GravityAccelerationRate
		{
			get
			{
				return this.gravityAccelerationRate;
			}
		}

		// Token: 0x1700025D RID: 605
		// (get) Token: 0x06000CEA RID: 3306 RVA: 0x00045394 File Offset: 0x00043594
		public float Mass
		{
			get
			{
				return this.mass;
			}
		}

		// Token: 0x1700025E RID: 606
		// (get) Token: 0x06000CEB RID: 3307 RVA: 0x0004539C File Offset: 0x0004359C
		public float Drag
		{
			get
			{
				return this.drag;
			}
		}

		// Token: 0x1700025F RID: 607
		// (get) Token: 0x06000CEC RID: 3308 RVA: 0x000453A4 File Offset: 0x000435A4
		public float AirborneDrag
		{
			get
			{
				return this.airborneDrag;
			}
		}

		// Token: 0x17000260 RID: 608
		// (get) Token: 0x06000CED RID: 3309 RVA: 0x000453AC File Offset: 0x000435AC
		private float GravityRate
		{
			get
			{
				return -this.gravityAccelerationRate * NetClock.FixedDeltaTime;
			}
		}

		// Token: 0x17000261 RID: 609
		// (get) Token: 0x06000CEE RID: 3310 RVA: 0x000453BB File Offset: 0x000435BB
		// (set) Token: 0x06000CEF RID: 3311 RVA: 0x000453C3 File Offset: 0x000435C3
		public float LastFrameMoveSpeedPercent { get; private set; }

		// Token: 0x17000262 RID: 610
		// (get) Token: 0x06000CF0 RID: 3312 RVA: 0x000453CC File Offset: 0x000435CC
		public bool UsingAimFix
		{
			get
			{
				return this.currentState.UsingAimFix;
			}
		}

		// Token: 0x06000CF1 RID: 3313 RVA: 0x000453D9 File Offset: 0x000435D9
		public void ServerSetInventorySwapBlockFrames(InventoryUsableDefinition.InventoryTypes type, uint frames)
		{
			if (type == InventoryUsableDefinition.InventoryTypes.Major)
			{
				this.currentState.PrimarySwapBlockingFrames = frames;
				return;
			}
			if (type == InventoryUsableDefinition.InventoryTypes.Minor)
			{
				this.currentState.SecondarySwapBlockingFrames = frames;
			}
		}

		// Token: 0x06000CF2 RID: 3314 RVA: 0x0001CAAE File Offset: 0x0001ACAE
		private void OnEnable()
		{
			NetClock.Register(this);
		}

		// Token: 0x06000CF3 RID: 3315 RVA: 0x0001CAB6 File Offset: 0x0001ACB6
		private void OnDisable()
		{
			NetClock.Unregister(this);
		}

		// Token: 0x06000CF4 RID: 3316 RVA: 0x000453FC File Offset: 0x000435FC
		public void SetInitialState(global::UnityEngine.Vector3 pos, float rot)
		{
			this.currentState = new NetState
			{
				Position = pos,
				CharacterRotation = rot
			};
			this.LevelChangeTeleportState = this.currentState;
		}

		// Token: 0x06000CF5 RID: 3317 RVA: 0x00045434 File Offset: 0x00043634
		public void SetState(NetState state)
		{
			base.transform.position = state.Position;
			state.CopyTo(ref this.currentState);
			if (!this.playerReferences.NetworkObject.IsOwner)
			{
				this.playerReferences.InteractableGameObject.SetActive(this.currentState.Downed);
			}
			if (!base.IsServer && state.serverVerifiedState && state.NetFrame < NetworkBehaviourSingleton<NetClock>.Instance.GameplayReadyFrame.Value)
			{
				this.SetInitialState(state.Position, state.CharacterRotation);
			}
		}

		// Token: 0x06000CF6 RID: 3318 RVA: 0x000454C6 File Offset: 0x000436C6
		public void HandleHealthChanged(int oldValue, int newValue)
		{
			if (NetworkManager.Singleton.IsServer && oldValue > newValue)
			{
				this.currentState.LastHitFrame = NetClock.CurrentFrame;
			}
		}

		// Token: 0x06000CF7 RID: 3319 RVA: 0x000454E8 File Offset: 0x000436E8
		public void HandleSpawnedAsOwner(float spawnRotation)
		{
			this.currentState.CharacterRotation = spawnRotation;
			MonoBehaviourSingleton<CameraController>.Instance.ResetCamera(this.currentState.CharacterRotation);
		}

		// Token: 0x06000CF8 RID: 3320 RVA: 0x0004550C File Offset: 0x0004370C
		public void TriggerLevelChangeTeleport(global::UnityEngine.Vector3 position, float rotation)
		{
			this.currentState.Position = position;
			this.currentState.CharacterRotation = rotation;
			this.LevelChangeTeleportState.Position = position;
			this.LevelChangeTeleportState.CharacterRotation = rotation;
			this.playerReferences.SafeGroundComponent.RegisterSafeGround(position);
		}

		// Token: 0x06000CF9 RID: 3321 RVA: 0x0004555A File Offset: 0x0004375A
		public void EndlessStart()
		{
			this.playerReferences.SafeGroundComponent.RegisterSafeGround(base.transform.position);
		}

		// Token: 0x06000CFA RID: 3322 RVA: 0x00045578 File Offset: 0x00043778
		public NetState HandleLoadingFrame(uint frame)
		{
			this.currentState.Position = this.LevelChangeTeleportState.Position;
			this.currentState.CharacterRotation = this.LevelChangeTeleportState.CharacterRotation;
			base.transform.position = this.LevelChangeTeleportState.Position;
			this.currentState.Grounded = true;
			this.currentState.FallFrames = 0;
			this.currentState.AirborneFrames = 0;
			this.currentState.TotalForce = global::UnityEngine.Vector3.zero;
			this.currentState.NetFrame = frame;
			return this.currentState;
		}

		// Token: 0x06000CFB RID: 3323 RVA: 0x0004560D File Offset: 0x0004380D
		public void MovingColliderCorrection(global::UnityEngine.Vector3 pushAmount)
		{
			this.movingColliderVisualsCorrectionList.Add(pushAmount);
		}

		// Token: 0x06000CFC RID: 3324 RVA: 0x0004561C File Offset: 0x0004381C
		public NetState SimulateNetFrameWithInput(NetInput input)
		{
			this.currentState.ResetTempFrameValues();
			this.currentState.NetFrame = input.NetFrame;
			if (!this.playerReferences.IsOwnedByServer && input.NetFrame < NetworkBehaviourSingleton<NetClock>.Instance.GameplayReadyFrame.Value + 4U)
			{
				return this.currentState;
			}
			this.currentState.CharacterRotation = input.CharacterRotation;
			this.currentState.Run = input.Run;
			if (this.currentState.GameplayTeleport && this.currentState.TeleportAtFrame - input.NetFrame < RuntimeDatabase.GetTeleportInfo(this.currentState.GameplayTeleportType).FramesToTeleport - 5U)
			{
				input.Clear();
			}
			if (base.IsServer && this.playerReferences.Inventory != null)
			{
				this.playerReferences.Inventory.HandleEquipmentQueue(this.currentState);
				if ((input.PrimaryEquipment != NetInput.PrimaryEquipmentInput.None && this.previousRawInput.PrimaryEquipment == NetInput.PrimaryEquipmentInput.None) || (input.SecondaryEquipment && !this.previousRawInput.SecondaryEquipment))
				{
					this.playerReferences.Inventory.CancelEquipmentSwapQueue();
				}
			}
			if (this.playerReferences.PlayerGhostController != null)
			{
				this.playerReferences.PlayerGhostController.SetGhostMode(input.Ghost);
				this.currentState.Ghost = input.Ghost;
			}
			if (this.playerReferences.PlayerDownedComponent)
			{
				this.currentState.Downed = this.playerReferences.PlayerDownedComponent.GetDowned(input.NetFrame);
				this.currentState.Reviving = this.playerReferences.PlayerDownedComponent.GetReviving(input.NetFrame);
			}
			else
			{
				this.currentState.Downed = false;
				this.currentState.Reviving = false;
			}
			if (!this.playerReferences.NetworkObject.IsOwner)
			{
				this.playerReferences.InteractableGameObject.SetActive(this.currentState.Downed);
			}
			if (this.currentState.Downed && this.currentState.Reviving)
			{
				this.currentState.Position = base.transform.position;
				this.currentState.NetFrame = input.NetFrame;
				return this.currentState;
			}
			this.currentState.CalculatedMotion = global::UnityEngine.Vector3.zero;
			if (this.currentState.TeleportStatus != TeleportComponent.TeleportStatusType.None && input.NetFrame == this.currentState.TeleportAtFrame)
			{
				if (this.currentState.TeleportStatus == TeleportComponent.TeleportStatusType.WorldFallOff)
				{
					if (this.playerReferences.HealthComponent)
					{
						this.playerReferences.HittableComponent.ModifyHealth(new HealthModificationArgs(-this.worldFallOffDamage, Context.StaticLevelContext, DamageType.Normal, HealthChangeType.WorldFallOff));
						this.playerReferences.PlayerStunComponent.ApplyStun(NetClock.CurrentSimulationFrame, this.crashLandingStunTime_Frames);
						this.currentState.FallFrames = 40;
						this.currentState.Grounded = false;
						this.currentState.TotalForce = global::UnityEngine.Vector3.zero;
					}
					else
					{
						this.playerReferences.PlayerStunComponent.ApplyStun(NetClock.CurrentSimulationFrame, this.spawnInStunTime_Frames);
					}
				}
				this.HandleTeleport(input);
				return this.currentState;
			}
			this.currentState.UseInputRotation = input.Horizontal != 0 || input.Vertical != 0;
			this.currentState.InputRotation = ((input.Horizontal != 0 || input.Vertical != 0) ? Mathf.Repeat(PlayerController.MotionInputToRotation(input.Horizontal, input.Vertical) + input.MotionRotation, 360f) : this.currentState.CharacterRotation);
			if (this.currentState.Downed)
			{
				input.Jump = false;
				input.PrimaryEquipment = NetInput.PrimaryEquipmentInput.None;
				input.SecondaryEquipment = false;
			}
			global::UnityEngine.Vector3 position = base.transform.position;
			if (this.currentState.Ghost)
			{
				this.ProcessGhost_NetFrame();
				this.ProcessStun_NetFrame(input.NetFrame);
				this.ProcessInput_NetFrame(input, this.ghostMovementRotationCurve);
				this.ProcessInputVerticalGhost_NetFrame(input);
				this.playerReferences.CharacterController.Move(this.currentState.CalculatedMotion * NetClock.FixedDeltaTime);
				this.ProcessRotation_NetFrame(position, (float)this.ghostRotationSpeed);
				if (input.UseIK)
				{
					this.ProcessAimRotation_NetFrame(ref this.currentState, input.FocusPoint, (float)this.ghostRotationSpeed);
				}
				this.currentState.Grounded = false;
			}
			else
			{
				this.ProcessStun_NetFrame(input.NetFrame);
				if (this.playerReferences.Inventory)
				{
					this.ProcessEquipment_NetFrame(input);
				}
				this.ProcessInput_NetFrame(input, this.currentState.Strafing ? this.strafingInputMovementRotationMultiplierCurve : this.inputMovementRotationMultiplierCurve);
				this.ProcessJump_NetFrame(input);
				this.ProcessPhysics_NetFrame(input);
				this.ProcessGroundCalculation_NetFrame();
				this.ProcessSlopeMovement_NetFrame();
				global::UnityEngine.Vector3 vector = (this.currentState.TotalForce + this.currentState.CalculatedMotion) * NetClock.FixedDeltaTime;
				this.playerReferences.CharacterController.Move(vector);
				global::UnityEngine.Vector3 vector2 = this.currentState.CalculatedMotion + this.currentState.TotalForce;
				vector2.x = vector2.x / this.currentState.XZInputMultiplierThisFrame * this.horizontalMoveAimMultiplier;
				vector2.y = (this.currentState.Grounded ? 0f : (this.baseSpeed * this.verticalMoveAimMultiplier));
				vector2.z = vector2.z / this.currentState.XZInputMultiplierThisFrame * this.horizontalMoveAimMultiplier;
				float magnitude = vector2.magnitude;
				if (magnitude > 0.05f)
				{
					float num = Mathf.Clamp(magnitude / this.baseSpeed, 0f, 1f);
					CrosshairUI.Instance.OnMoved(num);
					this.LastFrameMoveSpeedPercent = num;
				}
				else
				{
					this.LastFrameMoveSpeedPercent = 0f;
				}
				this.ProcessCollisionFromForce(vector, position);
				this.ProcessRotation_NetFrame(position, this.currentState.Downed ? this.downedRotationSpeed : this.rotationSpeed);
				if (input.UseIK)
				{
					this.ProcessAimRotation_NetFrame(ref this.currentState, input.FocusPoint, this.currentState.Downed ? this.downedRotationSpeed : this.rotationSpeed);
				}
				this.ProcessFallOffStage_NetFrame();
				this.ProcessSafePosition_NetFrame();
			}
			this.currentState.NetFrame = input.NetFrame;
			this.currentState.Position = base.transform.position;
			if (!this.currentState.Ghost)
			{
				if (this.currentState.Grounded)
				{
					this.currentState.FallFrames = 0;
				}
				else if (this.currentState.CalculatedMotion.y + this.currentState.TotalForce.y >= 0f)
				{
					this.currentState.FallFrames = 0;
				}
				else
				{
					this.currentState.FallFrames = this.currentState.FallFrames + 1;
				}
			}
			else
			{
				this.currentState.FallFrames = this.currentState.FallFrames + 1;
			}
			if ((NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost) && this.currentState.TeleportStatus == TeleportComponent.TeleportStatusType.OnCooldown && input.NetFrame >= this.playerReferences.TeleportComponent.TeleportReadyFrame)
			{
				this.currentState.TeleportStatus = TeleportComponent.TeleportStatusType.None;
			}
			this.currentState.PrimarySwapBlockingFrames = ((this.currentState.PrimarySwapBlockingFrames > 0U) ? (this.currentState.PrimarySwapBlockingFrames - 1U) : 0U);
			this.currentState.SecondarySwapBlockingFrames = ((this.currentState.SecondarySwapBlockingFrames > 0U) ? (this.currentState.SecondarySwapBlockingFrames - 1U) : 0U);
			this.currentState.UseIK = input.UseIK;
			return this.currentState;
		}

		// Token: 0x06000CFD RID: 3325 RVA: 0x00045DB4 File Offset: 0x00043FB4
		public NetState EndFrame()
		{
			if (this.movingColliderVisualsCorrectionList.Count > 0)
			{
				for (int i = 0; i < this.movingColliderVisualsCorrectionList.Count; i++)
				{
					this.currentState.MovingColliderVisualsCorrection = this.currentState.MovingColliderVisualsCorrection + this.movingColliderVisualsCorrectionList[i];
				}
				this.currentState.MovingColliderVisualsCorrection = this.currentState.MovingColliderVisualsCorrection / (float)this.movingColliderVisualsCorrectionList.Count;
				this.movingColliderVisualsCorrectionList.Clear();
			}
			this.playerReferences.PlayerStunComponent.EndFrame(ref this.currentState);
			return this.currentState;
		}

		// Token: 0x06000CFE RID: 3326 RVA: 0x00045E5A File Offset: 0x0004405A
		private void ProcessGhost_NetFrame()
		{
			this.currentState.TotalForce = global::UnityEngine.Vector3.zero;
			this.currentState.FallFrames = 0;
		}

		// Token: 0x06000CFF RID: 3327 RVA: 0x00045E78 File Offset: 0x00044078
		private void ProcessStun_NetFrame(uint frame)
		{
			if (this.currentState.Ghost)
			{
				this.currentState.StunFrame = 0U;
				return;
			}
			this.playerReferences.PlayerStunComponent.ProcessStun(frame, ref this.currentState);
		}

		// Token: 0x06000D00 RID: 3328 RVA: 0x00045EAC File Offset: 0x000440AC
		private void HandleTeleport(NetInput input)
		{
			base.transform.position = this.currentState.TeleportPosition;
			this.currentState.TeleportStatus = ((input.NetFrame >= this.playerReferences.TeleportComponent.TeleportReadyFrame) ? TeleportComponent.TeleportStatusType.None : TeleportComponent.TeleportStatusType.OnCooldown);
			this.currentState.Position = base.transform.position;
			this.currentState.NetFrame = input.NetFrame;
			this.currentState.GameplayTeleport = false;
			if (this.currentState.TeleportHasRotation)
			{
				this.currentState.CharacterRotation = this.currentState.TeleportRotation;
			}
		}

		// Token: 0x06000D01 RID: 3329 RVA: 0x00045F50 File Offset: 0x00044150
		private void ProcessEquipment_NetFrame(NetInput input)
		{
			SerializableGuid serializableGuid = SerializableGuid.Empty;
			Item item2 = null;
			if (this.currentState.ActiveUseStates == null)
			{
				this.currentState.ActiveUseStates = new List<UsableDefinition.UseState>();
			}
			if (this.currentState.SecondarySwapActive)
			{
				input.SecondaryEquipment = false;
			}
			bool flag = false;
			this.currentState.HorizontalCameraAim = input.MotionRotation;
			if (this.playerReferences.PlayerInteractor.CurrentInteractable != null && this.playerReferences.PlayerInteractor.CurrentInteractable.IsHeldInteraction)
			{
				this.currentState.BlockItemInput = true;
			}
			if (!this.currentState.BlockItemInput && !this.CurrentState.PrimarySwapActive)
			{
				if (input.PrimaryEquipment != NetInput.PrimaryEquipmentInput.None)
				{
					serializableGuid = RuntimeDatabase.GetUsableDefinitionIDFromAssetID(this.playerReferences.Inventory.GetEquippedId(0));
					item2 = this.playerReferences.Inventory.GetEquippedItem(0);
				}
				if (serializableGuid.Equals(SerializableGuid.Empty) && input.SecondaryEquipment)
				{
					serializableGuid = RuntimeDatabase.GetUsableDefinitionIDFromAssetID(this.playerReferences.Inventory.GetEquippedId(1));
					item2 = this.playerReferences.Inventory.GetEquippedItem(1);
				}
			}
			if (this.currentState.CurrentPressedEquipment.CompareTo(serializableGuid) == 1)
			{
				this.currentState.CurrentPressedEquipment = serializableGuid;
				this.currentState.EquipmentPressedDuration = 0;
			}
			if (this.currentState.ActiveWorldUseState != null && !RuntimeDatabase.GetUsableDefinition(this.currentState.ActiveWorldUseState.EquipmentGuid).ProcessUseFrame(ref this.currentState, input, ref this.currentState.ActiveWorldUseState, this.playerReferences, false, false))
			{
				this.currentState.ActiveWorldUseState = null;
			}
			if (this.WorldInteractableUseQueue != null && !this.currentState.IsStunned)
			{
				if (this.currentState.ActiveWorldUseState == null && this.WorldInteractableUseQueue.WorldUsableDefinition != null)
				{
					this.playerReferences.PlayerInteractor.MostRecentInteraction = this.WorldInteractableUseQueue;
					this.currentState.ActiveWorldUseState = this.WorldInteractableUseQueue.WorldUsableDefinition.ProcessUseStart(ref this.currentState, input, this.WorldInteractableUseQueue.WorldUsableDefinition.Guid, this.playerReferences, null);
				}
				this.WorldInteractableUseQueue = null;
			}
			for (int i = 0; i < this.currentState.ActiveUseStates.Count; i++)
			{
				UsableDefinition.UseState useState = this.currentState.ActiveUseStates[i];
				InventoryUsableDefinition inventoryUsableDefinition = (InventoryUsableDefinition)RuntimeDatabase.GetUsableDefinition(useState.EquipmentGuid);
				bool flag2 = inventoryUsableDefinition.Guid == RuntimeDatabase.GetUsableDefinitionIDFromAssetID(this.playerReferences.Inventory.GetEquippedId((inventoryUsableDefinition.InventoryType == InventoryUsableDefinition.InventoryTypes.Major) ? 0 : 1));
				bool flag3 = !this.currentState.IsStunned && serializableGuid.Equals(useState.EquipmentGuid);
				if (flag3 && useState.Item != item2)
				{
					useState.Item = item2;
				}
				if (inventoryUsableDefinition.ProcessUseFrame(ref this.currentState, input, ref useState, this.playerReferences, flag2, flag3))
				{
					this.currentState.ActiveUseStates[i] = useState;
				}
				else
				{
					this.currentState.ActiveUseStates[i] = null;
				}
				if (serializableGuid == useState.EquipmentGuid)
				{
					flag = true;
				}
			}
			this.currentState.ActiveUseStates.RemoveAll((UsableDefinition.UseState item) => item == null);
			if (!this.currentState.BlockItemInput && !this.currentState.IsStunned && !flag && serializableGuid.CompareTo(SerializableGuid.Empty) == 1 && (int)this.equipmentPressedBufferFrames >= this.currentState.EquipmentPressedDuration)
			{
				UsableDefinition usableDefinition = RuntimeDatabase.GetUsableDefinition(serializableGuid);
				UsableDefinition.UseState useState2 = usableDefinition.ProcessUseStart(ref this.currentState, input, serializableGuid, this.playerReferences, item2);
				if (useState2 != null && usableDefinition.ProcessUseFrame(ref this.currentState, input, ref useState2, this.playerReferences, true, true))
				{
					this.currentState.ActiveUseStates.Add(useState2);
				}
			}
			bool flag4 = MonoBehaviourSingleton<CameraController>.Instance.GameStateADS > CameraController.CameraType.Normal;
			if (!(flag4 ^ this.isUsingADS))
			{
				if (this.isUsingADS)
				{
					this.playerReferences.PlayerNetworkController.SetAimIKPosition(MonoBehaviourSingleton<CameraController>.Instance.LastAimPosition);
				}
				return;
			}
			this.isUsingADS = flag4;
			if (this.isUsingADS)
			{
				this.playerReferences.PlayerNetworkController.EnableAimIK(MonoBehaviourSingleton<CameraController>.Instance.LastAimPosition);
				return;
			}
			this.playerReferences.PlayerNetworkController.DisableAimIK();
		}

		// Token: 0x06000D02 RID: 3330 RVA: 0x000463CC File Offset: 0x000445CC
		public void ProcessWorldTriggerCheck_NetFrame(uint frame, bool onDestroy = false)
		{
			if (this.playerReferences.WorldCollidable != null)
			{
				float num = this.playerReferences.CharacterController.height * 0.5f - this.playerReferences.CharacterController.radius;
				global::UnityEngine.Vector3 vector = base.transform.position + this.playerReferences.CharacterController.center - global::UnityEngine.Vector3.up * num;
				global::UnityEngine.Vector3 vector2 = base.transform.position + this.playerReferences.CharacterController.center + global::UnityEngine.Vector3.up * num;
				int num2 = Physics.OverlapCapsuleNonAlloc(vector, vector2, this.playerReferences.CharacterController.radius, PlayerController.hitPool, this.worldEffectMask, QueryTriggerInteraction.Collide);
				for (int i = 0; i < num2; i++)
				{
					WorldTriggerCollider component = PlayerController.hitPool[i].GetComponent<WorldTriggerCollider>();
					if (component != null)
					{
						if (onDestroy)
						{
							component.WorldTrigger.DestroyOverlap(this.playerReferences.WorldCollidable, frame);
						}
						else
						{
							component.WorldTrigger.Overlapped(this.playerReferences.WorldCollidable, frame, true);
						}
					}
				}
			}
		}

		// Token: 0x06000D03 RID: 3331 RVA: 0x000464FC File Offset: 0x000446FC
		private void ProcessInput_NetFrame(NetInput input, AnimationCurve rotationMovementCurve)
		{
			if (this.currentState.IsStunned)
			{
				this.currentState.MotionX = 0;
				this.currentState.MotionZ = 0;
			}
			else
			{
				this.currentState.MotionX = PlayerController.MotorFromInput((float)input.Horizontal, this.currentState.MotionX, (int)this.motorFrames, 1);
				this.currentState.MotionZ = PlayerController.MotorFromInput((float)input.Vertical, this.currentState.MotionZ, (int)this.motorFrames, 1);
			}
			if (this.currentState.Ghost)
			{
				this.currentState.GhostVerticalMotorFrame = PlayerController.MotorFromInput((float)((input.Jump ? 1 : 0) + (input.Down ? (-1) : 0)), this.currentState.GhostVerticalMotorFrame, this.ghostVerticalMotorFrames, 3);
				this.currentState.JumpFrame = 0;
			}
			else
			{
				this.currentState.GhostVerticalMotorFrame = 0;
			}
			this.currentState.JumpPressedDuration = ((input.Jump && !this.currentState.IsStunned) ? (this.currentState.JumpPressedDuration + 1) : 0);
			this.currentState.EquipmentPressedDuration = ((input.PrimaryEquipment == NetInput.PrimaryEquipmentInput.None && !input.SecondaryEquipment) ? 0 : (this.currentState.EquipmentPressedDuration + 1));
			if (!this.currentState.BlockMotionXZ)
			{
				global::UnityEngine.Vector3 vector = new global::UnityEngine.Vector3(Mathf.Lerp(-1f, 1f, (float)((int)this.motorFrames + this.currentState.MotionX) / (float)(this.motorFrames * 2)), 0f, Mathf.Lerp(-1f, 1f, (float)((int)this.motorFrames + this.currentState.MotionZ) / (float)(this.motorFrames * 2)));
				vector = PlayerController.RotateMotionDirection(vector, input.MotionRotation);
				vector = global::UnityEngine.Vector3.ClampMagnitude(vector, 1f);
				vector *= (this.currentState.Downed ? this.downedSpeed : (input.Run ? this.baseSpeed : this.walkSpeed));
				vector *= this.currentState.XZInputMultiplierThisFrame;
				vector *= rotationMovementCurve.Evaluate(Mathf.Abs(Mathf.DeltaAngle(this.currentState.CharacterRotation, this.currentState.InputRotation) / 180f));
				if (this.currentState.Ghost)
				{
					vector *= this.ghostSpeedMultiplier;
				}
				float num = this.forceRecoveryCurve.Evaluate(this.currentState.PushForceControl);
				this.currentState.CalculatedMotion = this.currentState.CalculatedMotion + vector * num;
			}
		}

		// Token: 0x06000D04 RID: 3332 RVA: 0x00046798 File Offset: 0x00044998
		private void ProcessInputVerticalGhost_NetFrame(NetInput input)
		{
			float num = this.ghostVerticalSpeed * this.ghostVerticalMovementCurve.Evaluate((float)this.currentState.GhostVerticalMotorFrame / (float)this.ghostVerticalMotorFrames);
			this.currentState.CalculatedMotion = this.currentState.CalculatedMotion + new global::UnityEngine.Vector3(0f, num, 0f);
		}

		// Token: 0x06000D05 RID: 3333 RVA: 0x000467F8 File Offset: 0x000449F8
		private void ProcessJump_NetFrame(NetInput input)
		{
			if (this.currentState.Grounded && !this.currentState.BlockJump)
			{
				this.currentState.AirborneFrames = 0;
			}
			else
			{
				this.currentState.AirborneFrames = this.currentState.AirborneFrames + 1;
			}
			if (this.currentState.JumpFrame > -1 && !this.currentState.IsStunned && this.currentState.FramesSinceStableGround <= (int)this.coyoteTimeFrames && this.currentState.JumpPressedDuration > 0 && this.currentState.JumpPressedDuration <= (int)this.jumpBufferFrames && !this.currentState.BlockJump)
			{
				this.currentState.JumpFrame = (int)(-this.jumpFrames);
				this.currentState.JumpReleasedThisJump = false;
			}
			if (this.currentState.JumpFrame < 0)
			{
				if (this.ScanHeadBumped())
				{
					this.currentState.JumpReleasedThisJump = true;
					this.currentState.JumpFrame = 0;
				}
				else if (!this.currentState.JumpReleasedThisJump && (!input.Jump || this.currentState.IsStunned))
				{
					this.currentState.JumpReleasedThisJump = true;
				}
			}
			this.currentState.JumpFrame = Mathf.Min(this.currentState.JumpFrame + 1, 0);
			if (this.currentState.JumpFrame == 0 && this.currentState.LastWorldPhysics.magnitude > 0.02f)
			{
				this.currentState.JumpFrame = 0;
			}
			else if (this.currentState.JumpReleasedThisJump && this.currentState.JumpFrame < 0 && !this.currentState.BlockMotionY)
			{
				this.currentState.JumpFrame = (int)((short)(this.currentState.JumpFrame + 2));
			}
			if (this.currentState.JumpFrame < 1)
			{
				this.currentState.CalculatedMotion.y = (this.currentState.BlockMotionY ? this.currentState.CalculatedMotion.y : (this.jumpCurve.Evaluate((float)this.currentState.JumpFrame / (float)this.jumpFrames) * this.jumpForce));
			}
		}

		// Token: 0x06000D06 RID: 3334 RVA: 0x00046A0C File Offset: 0x00044C0C
		private bool ScanHeadBumped()
		{
			return Physics.Raycast(base.transform.position + global::UnityEngine.Vector3.up * this.playerReferences.CharacterController.height, global::UnityEngine.Vector3.up, 0.05f, this.groundedLayerMask);
		}

		// Token: 0x06000D07 RID: 3335 RVA: 0x00046A60 File Offset: 0x00044C60
		public void ProcessPhysics_NetFrame(NetInput input)
		{
			this.playerReferences.PhysicsTaker.GetFramePhysics(input.NetFrame, ref this.currentState);
			if (!this.currentState.BlockGravity)
			{
				this.currentState.TotalForce.y = Mathf.Max(this.currentState.TotalForce.y + this.GravityRate, -this.gravityTerminalVelocityMetersPerSecond);
			}
			global::UnityEngine.Vector3 vector = this.currentState.TotalForce * (1f - NetClock.FixedDeltaTime * (this.currentState.Grounded ? this.Drag : this.AirborneDrag));
			this.currentState.TotalForce.x = vector.x;
			this.currentState.TotalForce.z = vector.z;
		}

		// Token: 0x06000D08 RID: 3336 RVA: 0x00046B30 File Offset: 0x00044D30
		private void ProcessCollisionFromForce(global::UnityEngine.Vector3 expectedMovement, global::UnityEngine.Vector3 positionBeforeMove)
		{
			expectedMovement *= 0.9f;
			global::UnityEngine.Vector3 vector = base.transform.position - positionBeforeMove;
			if ((expectedMovement.y > 0f && vector.y < expectedMovement.y) || (expectedMovement.y < 0f && vector.y > expectedMovement.y))
			{
				if (this.currentState.TotalForce.y < 0f)
				{
					this.currentState.TotalForce.y = this.GravityRate;
				}
				else
				{
					this.currentState.TotalForce.y = 0f;
				}
			}
			if ((expectedMovement.x > 0f && vector.x < expectedMovement.x) || (expectedMovement.x < 0f && vector.x > expectedMovement.x))
			{
				this.currentState.TotalForce.x = 0f;
			}
			if ((expectedMovement.z > 0f && vector.z < expectedMovement.z) || (expectedMovement.z < 0f && vector.z > expectedMovement.z))
			{
				this.currentState.TotalForce.z = 0f;
			}
		}

		// Token: 0x06000D09 RID: 3337 RVA: 0x00046C6C File Offset: 0x00044E6C
		public void ProcessGroundCalculation_NetFrame()
		{
			bool flag = false;
			float num = this.playerReferences.CharacterController.radius + 0.15f + this.playerReferences.CharacterController.skinWidth;
			this.currentState.GroundNormal = global::UnityEngine.Vector3.zero;
			if (this.currentState.TotalForce.y + this.currentState.CalculatedMotion.y < 1f)
			{
				bool flag2 = false;
				global::UnityEngine.Vector3 vector = global::UnityEngine.Vector3.zero;
				int num2 = 0;
				float num3 = 0.02f + this.playerReferences.CharacterController.radius + this.playerReferences.CharacterController.skinWidth;
				for (int i = 0; i < 8; i++)
				{
					float num4 = (float)i * 3.1415927f * 2f / 8f;
					global::UnityEngine.Vector3 vector2 = new global::UnityEngine.Vector3(Mathf.Cos(num4) * this.playerReferences.CharacterController.radius, this.playerReferences.CharacterController.radius, Mathf.Sin(num4) * this.playerReferences.CharacterController.radius);
					RaycastHit raycastHit;
					if (this.GroundCalculationRay(vector2, num, out raycastHit, false))
					{
						this.currentState.GroundNormal = this.currentState.GroundNormal + raycastHit.normal;
						if (raycastHit.normal != global::UnityEngine.Vector3.up)
						{
							flag2 = true;
						}
						if (raycastHit.distance > num3)
						{
							vector += vector2;
						}
						else
						{
							num2++;
						}
					}
					else
					{
						vector += vector2;
					}
				}
				float num5 = this.playerReferences.CharacterController.radius + (flag2 ? 0.195f : 0.02f);
				RaycastHit raycastHit2;
				if (num2 > 3 || this.GroundCalculationRay(new global::UnityEngine.Vector3(0f, this.playerReferences.CharacterController.radius, 0f), num5 + this.playerReferences.CharacterController.skinWidth, out raycastHit2, true))
				{
					this.currentState.StableGround = true;
					this.currentState.FramesSinceStableGround = 0;
					flag = true;
				}
				else if (num2 > 0)
				{
					this.currentState.StableGround = false;
					this.currentState.FramesSinceStableGround = this.currentState.FramesSinceStableGround + 1;
					flag = true;
					if (!this.currentState.IsStunned)
					{
						vector.y = 0f;
						this.currentState.CalculatedMotion = this.currentState.CalculatedMotion + vector.normalized * this.EdgeSlideAmout;
					}
				}
				else if (this.CheckGroundingOnComplexGeometry())
				{
					this.currentState.StableGround = false;
					this.currentState.FramesSinceStableGround = this.currentState.FramesSinceStableGround + 1;
					flag = true;
				}
				else
				{
					this.currentState.FramesSinceStableGround = this.currentState.FramesSinceStableGround + 1;
				}
			}
			else
			{
				this.currentState.StableGround = false;
				this.currentState.FramesSinceStableGround = this.currentState.FramesSinceStableGround + 1;
			}
			if (flag)
			{
				this.currentState.TotalForce.y = this.GravityRate;
				if ((long)this.currentState.FallFrames > (long)((ulong)this.fallingFramesToCrashLanding) && !this.currentState.BlockGrounding && this.CheckIfCanTakeFallingDamageBasedOnTeleportState())
				{
					this.playerReferences.PlayerStunComponent.ApplyStun(NetClock.CurrentSimulationFrame, this.crashLandingStunTime_Frames);
					if (base.IsServer)
					{
						this.CheckAndCancelReload();
					}
					if ((NetworkManager.Singleton.IsServer || NetClock.CurrentSimulationFrame == NetClock.CurrentFrame) && this.playerReferences.HealthComponent)
					{
						this.playerReferences.HittableComponent.ModifyHealth(new HealthModificationArgs(-this.crashLandingDamage, Context.StaticLevelContext, DamageType.Normal, HealthChangeType.WorldFallOff));
					}
				}
			}
			this.currentState.GroundNormal.Normalize();
			global::UnityEngine.Vector3 vector3 = PlayerController.RotateMotionDirection(global::UnityEngine.Vector3.forward, this.currentState.CharacterRotation);
			float num6 = global::UnityEngine.Vector3.Angle(this.currentState.GroundNormal, vector3);
			this.currentState.SlopeAngle = ((!flag) ? 0f : ((num6 - 90f) / 45f));
			this.currentState.GroundSlope = global::UnityEngine.Vector3.Angle(this.currentState.GroundNormal, global::UnityEngine.Vector3.up);
			this.currentState.Grounded = flag;
			this.currentState.GroundedFrames = (flag ? (this.currentState.GroundedFrames + 1) : (-1));
		}

		// Token: 0x06000D0A RID: 3338 RVA: 0x000470A8 File Offset: 0x000452A8
		private void OnDrawGizmos()
		{
			Gizmos.DrawWireSphere(base.transform.position + global::UnityEngine.Vector3.up * (this.playerReferences.CharacterController.radius - 0.01f), this.playerReferences.CharacterController.radius);
		}

		// Token: 0x06000D0B RID: 3339 RVA: 0x000470FC File Offset: 0x000452FC
		private bool CheckIfCanTakeFallingDamageBasedOnTeleportState()
		{
			return this.currentState.TeleportStatus == TeleportComponent.TeleportStatusType.None || (this.currentState.TeleportStatus != TeleportComponent.TeleportStatusType.WorldFallOff && (this.currentState.TeleportStatus != TeleportComponent.TeleportStatusType.OnCooldown || this.currentState.NetFrame - 1U != this.currentState.TeleportAtFrame));
		}

		// Token: 0x06000D0C RID: 3340 RVA: 0x00047154 File Offset: 0x00045354
		private bool CheckGroundingOnComplexGeometry()
		{
			float num = this.playerReferences.CharacterController.radius + this.playerReferences.CharacterController.skinWidth;
			int num2 = Physics.SphereCastNonAlloc(base.transform.position + global::UnityEngine.Vector3.up * (num + 0.2f), this.playerReferences.CharacterController.radius, global::UnityEngine.Vector3.down, this.collisionCache, 0.225f + this.playerReferences.CharacterController.skinWidth, this.groundedLayerMask, QueryTriggerInteraction.Ignore);
			global::UnityEngine.Vector3 vector = global::UnityEngine.Vector3.zero;
			if (num2 < 1)
			{
				return false;
			}
			if (!this.currentState.IsStunned)
			{
				for (int i = 0; i < num2; i++)
				{
					global::UnityEngine.Vector3 vector2 = this.collisionCache[i].point - base.transform.position;
					vector += vector2 * this.EdgeSlideAmout * -1f;
				}
				vector.y = 0f;
				vector.Normalize();
				this.currentState.CalculatedMotion = this.currentState.CalculatedMotion + vector;
			}
			return true;
		}

		// Token: 0x06000D0D RID: 3341 RVA: 0x00047280 File Offset: 0x00045480
		private void ProcessSlopeMovement_NetFrame()
		{
			if (this.currentState.Grounded && this.currentState.CalculatedMotion.y + this.currentState.TotalForce.y <= 0f && !Mathf.Approximately(0f, global::UnityEngine.Vector3.Angle(this.currentState.GroundNormal, global::UnityEngine.Vector3.up)))
			{
				global::UnityEngine.Vector3 vector = new global::UnityEngine.Vector3(this.currentState.CalculatedMotion.x, 0f, this.currentState.CalculatedMotion.z);
				global::UnityEngine.Vector3 vector2 = Quaternion.FromToRotation(global::UnityEngine.Vector3.up, this.currentState.GroundNormal) * vector;
				if (vector2.y >= 0f || !this.currentState.BlockMotionY_Down)
				{
					this.currentState.CalculatedMotion = vector2;
				}
			}
		}

		// Token: 0x06000D0E RID: 3342 RVA: 0x00047358 File Offset: 0x00045558
		private void ProcessRotation_NetFrame(global::UnityEngine.Vector3 pastPosition, float speed)
		{
			if (!this.currentState.BlockRotation)
			{
				Vector2 vector = new Vector2(base.transform.position.x - pastPosition.x, base.transform.position.z - pastPosition.z);
				if (!Mathf.Approximately(vector.magnitude, 0f))
				{
					float num = (((base.transform.position - pastPosition).magnitude < 0.001f) ? 0f : Quaternion.LookRotation(base.transform.position - pastPosition).eulerAngles.y);
					PlayerController.Rotate(ref this.currentState, num, speed, true);
				}
			}
		}

		// Token: 0x06000D0F RID: 3343 RVA: 0x00047418 File Offset: 0x00045618
		private void ProcessAimRotation_NetFrame(ref NetState state, global::UnityEngine.Vector3 focusPoint, float speed)
		{
			Transform visualsTransform = this.playerReferences.ApperanceController.VisualsTransform;
			global::UnityEngine.Vector3 position = visualsTransform.position;
			float y = visualsTransform.eulerAngles.y;
			global::UnityEngine.Vector3 vector = focusPoint - position;
			float magnitude = vector.magnitude;
			vector /= magnitude;
			global::UnityEngine.Vector3 eulerAngles = Quaternion.LookRotation(vector, global::UnityEngine.Vector3.up).eulerAngles;
			float num = Mathf.DeltaAngle(0f, eulerAngles.x);
			float num2 = Mathf.MoveTowardsAngle(y, eulerAngles.y, this.playerReferences.ApperanceController.AppearanceAnimator.HorizontalAimLimit);
			global::UnityEngine.Vector3 vector2 = Quaternion.Euler(num, num2, 0f) * global::UnityEngine.Vector3.forward * magnitude + position;
			if (this.playerReferences.ApperanceController.UseRelativeAimPoint)
			{
				state.AimRelativePoint = visualsTransform.InverseTransformPoint(vector2);
				return;
			}
			state.AimRelativePoint = vector2;
		}

		// Token: 0x06000D10 RID: 3344 RVA: 0x000474FC File Offset: 0x000456FC
		private Quaternion ClampPitchAndYaw(Quaternion q, float maxPitch, float maxYaw)
		{
			q.x /= q.w;
			q.y /= q.w;
			q.z /= q.w;
			q.w = 1f;
			float num = 114.59156f * Mathf.Atan(q.x);
			num = Mathf.Clamp(num, -maxPitch, maxPitch);
			q.x = Mathf.Tan(0.008726646f * num);
			num = 114.59156f * Mathf.Atan(q.y);
			num = Mathf.Clamp(num, -maxYaw, maxYaw);
			q.y = Mathf.Tan(0.008726646f * num);
			return q.normalized;
		}

		// Token: 0x06000D11 RID: 3345 RVA: 0x000475B0 File Offset: 0x000457B0
		private void ProcessFallOffStage_NetFrame()
		{
			if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage && base.transform.position.y < MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.StageFallOffHeight)
			{
				this.playerReferences.TeleportComponent.WorldFallOffTriggered(ref this.currentState, this.playerReferences.SafeGroundComponent.LastSafePosition);
			}
		}

		// Token: 0x06000D12 RID: 3346 RVA: 0x00047618 File Offset: 0x00045818
		private void ProcessSafePosition_NetFrame()
		{
			if (this.currentState.Grounded && NetworkManager.Singleton.IsServer && MonoBehaviourSingleton<StageManager>.Instance.ActiveStage != null && base.transform.position.y > MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.StageFallOffHeight && NetClock.CurrentFrame % 10U == 0U && this.currentState.StableGround)
			{
				this.playerReferences.SafeGroundComponent.RegisterSafeGround(this.currentState.Position);
			}
		}

		// Token: 0x06000D13 RID: 3347 RVA: 0x000476A4 File Offset: 0x000458A4
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

		// Token: 0x06000D14 RID: 3348 RVA: 0x0004773C File Offset: 0x0004593C
		private bool GroundCalculationRay(global::UnityEngine.Vector3 relativePosition, float distance, out RaycastHit hit, bool draw = false)
		{
			global::UnityEngine.Vector3 vector = base.transform.position + relativePosition;
			if (!Physics.Raycast(vector, global::UnityEngine.Vector3.down, out hit, 1.7f, this.groundedLayerMask, QueryTriggerInteraction.Ignore))
			{
				if (draw)
				{
					Debug.DrawLine(vector, vector + global::UnityEngine.Vector3.down * distance, global::UnityEngine.Color.red);
				}
				return false;
			}
			if (hit.distance > distance)
			{
				if (draw)
				{
					Debug.DrawLine(vector, vector + global::UnityEngine.Vector3.down * distance, global::UnityEngine.Color.red);
				}
				return false;
			}
			if (draw)
			{
				Debug.DrawLine(vector, hit.point, global::UnityEngine.Color.green);
				Debug.DrawLine(hit.point, vector + global::UnityEngine.Vector3.down * distance, global::UnityEngine.Color.cyan);
			}
			return true;
		}

		// Token: 0x06000D15 RID: 3349 RVA: 0x00047800 File Offset: 0x00045A00
		public static int MotorFromInput(float input, int motor, int motorFrames, int degradeRate = 1)
		{
			if (input == 0f)
			{
				return ((motor > 0) ? 1 : (-1)) * Mathf.Max(0, Mathf.Abs(motor) - degradeRate);
			}
			if (motor == 0 || Mathf.Sign(input) == Mathf.Sign((float)motor))
			{
				if (!Mathf.Approximately((float)motor + input, (float)Mathf.Abs(motor) + Mathf.Abs(input)))
				{
					input *= (float)degradeRate;
				}
				return Mathf.Clamp(motor + ((input >= 0f) ? 1 : (-1)), -motorFrames, motorFrames);
			}
			return 0;
		}

		// Token: 0x06000D16 RID: 3350 RVA: 0x00047876 File Offset: 0x00045A76
		public static global::UnityEngine.Vector3 RotateMotionDirection(global::UnityEngine.Vector3 directionInput, float rotation)
		{
			return Quaternion.Euler(0f, rotation, 0f) * directionInput;
		}

		// Token: 0x06000D17 RID: 3351 RVA: 0x0004788E File Offset: 0x00045A8E
		public static float MotionInputToRotation(int horizontal, int vertical)
		{
			return Vector2.SignedAngle(new Vector2((float)horizontal, (float)vertical), Vector2.up);
		}

		// Token: 0x06000D18 RID: 3352 RVA: 0x000478A3 File Offset: 0x00045AA3
		public static float DirectionToAngle(global::UnityEngine.Vector3 direction)
		{
			return Vector2.SignedAngle(new Vector2(direction.x, direction.z), Vector2.up);
		}

		// Token: 0x06000D19 RID: 3353 RVA: 0x000478C0 File Offset: 0x00045AC0
		public static global::UnityEngine.Vector3 AngleToForwardMotion(float angle)
		{
			return Quaternion.AngleAxis(angle, global::UnityEngine.Vector3.up) * global::UnityEngine.Vector3.forward;
		}

		// Token: 0x06000D1A RID: 3354 RVA: 0x000478D8 File Offset: 0x00045AD8
		public void TriggerTeleport(global::UnityEngine.Vector3 position, float rotation, TeleportType teleportType, bool snapCamera, Context context = null)
		{
			if (base.IsServer)
			{
				PlayerInteractor playerInteractor = this.playerReferences.PlayerInteractor;
				if (playerInteractor != null)
				{
					InteractableBase currentInteractable = playerInteractor.CurrentInteractable;
					if (currentInteractable != null)
					{
						currentInteractable.InteractionStopped(this.playerReferences.PlayerInteractor);
					}
				}
			}
			this.playerReferences.TeleportComponent.TeleportTriggered(ref this.currentState, position, true, TeleportComponent.TeleportType.Regular, RuntimeDatabase.GetTeleportInfo(teleportType).FramesToTeleport, true, rotation, teleportType, snapCamera);
		}

		// Token: 0x06000D1B RID: 3355 RVA: 0x00047943 File Offset: 0x00045B43
		protected override void OnDestroy()
		{
			if (NetworkBehaviourSingleton<NetClock>.Instance)
			{
				this.ProcessWorldTriggerCheck_NetFrame(NetClock.CurrentFrame, true);
			}
			base.OnDestroy();
		}

		// Token: 0x06000D1C RID: 3356 RVA: 0x00047963 File Offset: 0x00045B63
		public void CheckAndCancelReload()
		{
			if (this.playerReferences.Inventory != null)
			{
				this.playerReferences.Inventory.CheckAndCancelReload();
			}
		}

		// Token: 0x04000BEA RID: 3050
		private const int GROUND_ANGLE_RAYS = 8;

		// Token: 0x04000BEB RID: 3051
		private const float UPWARDS_FORCE_GROUND_OVERRIDE_THRESHOLD = 1f;

		// Token: 0x04000BEC RID: 3052
		private const float NORMAL_CALC_GROUNDING_RAY_DISTANCE = 0.15f;

		// Token: 0x04000BED RID: 3053
		private const float FLAT_GROUNDING_RAY_DISTANCE = 0.02f;

		// Token: 0x04000BEE RID: 3054
		private const float EDGE_SLIDE_CHECK_OFFSET = 0.01f;

		// Token: 0x04000BEF RID: 3055
		private const float SLOPE_GROUNDING_RAY_DISTANCE = 0.195f;

		// Token: 0x04000BF0 RID: 3056
		private const float PITCH_CLAMP_UPPER = 50f;

		// Token: 0x04000BF1 RID: 3057
		private const float PITCH_CLAMP_LOWER = -50f;

		// Token: 0x04000BF2 RID: 3058
		private const float YAW_CLAMP_UPPER = 30f;

		// Token: 0x04000BF3 RID: 3059
		private const float YAW_CLAMP_LOWER = -30f;

		// Token: 0x04000BF4 RID: 3060
		private const int TELEPORT_INPUT_LOCK_FRAME_DELAY = 5;

		// Token: 0x04000BF5 RID: 3061
		private const uint CLIENT_SYNC_PERIOD = 4U;

		// Token: 0x04000BF6 RID: 3062
		private static Collider[] hitPool = new Collider[20];

		// Token: 0x04000BF7 RID: 3063
		private RaycastHit[] collisionCache = new RaycastHit[20];

		// Token: 0x04000BF8 RID: 3064
		[Header("References")]
		[SerializeField]
		private PlayerReferenceManager playerReferences;

		// Token: 0x04000BF9 RID: 3065
		[SerializeField]
		private LayerMask groundedLayerMask;

		// Token: 0x04000BFA RID: 3066
		[Header("Movement")]
		[SerializeField]
		private float baseSpeed = 4f;

		// Token: 0x04000BFB RID: 3067
		[SerializeField]
		private short motorFrames = 4;

		// Token: 0x04000BFC RID: 3068
		[SerializeField]
		private float rotationSpeed = 400f;

		// Token: 0x04000BFD RID: 3069
		[SerializeField]
		private AnimationCurve inputMovementRotationMultiplierCurve = new AnimationCurve();

		// Token: 0x04000BFE RID: 3070
		[SerializeField]
		private AnimationCurve strafingInputMovementRotationMultiplierCurve = new AnimationCurve();

		// Token: 0x04000BFF RID: 3071
		[SerializeField]
		private float horizontalMoveAimMultiplier;

		// Token: 0x04000C00 RID: 3072
		[SerializeField]
		private float verticalMoveAimMultiplier = 1f;

		// Token: 0x04000C01 RID: 3073
		[Header("Walk")]
		[SerializeField]
		private float walkSpeed = 1.3f;

		// Token: 0x04000C02 RID: 3074
		[Header("Gravity")]
		[SerializeField]
		private float gravityAccelerationRate = 9.81f;

		// Token: 0x04000C03 RID: 3075
		[SerializeField]
		[Min(1f)]
		private float gravityTerminalVelocityMetersPerSecond = 53f;

		// Token: 0x04000C04 RID: 3076
		[Header("Jump")]
		[SerializeField]
		private short jumpFrames = 15;

		// Token: 0x04000C05 RID: 3077
		[SerializeField]
		private float jumpForce = 8f;

		// Token: 0x04000C06 RID: 3078
		[SerializeField]
		private AnimationCurve jumpCurve = new AnimationCurve();

		// Token: 0x04000C07 RID: 3079
		[Header("Jump Helpers")]
		[SerializeField]
		private short jumpBufferFrames = 5;

		// Token: 0x04000C08 RID: 3080
		[SerializeField]
		private short coyoteTimeFrames = 4;

		// Token: 0x04000C09 RID: 3081
		[Header("Physics Calc")]
		[SerializeField]
		private float drag = 2f;

		// Token: 0x04000C0A RID: 3082
		[SerializeField]
		private float airborneDrag = 1f;

		// Token: 0x04000C0B RID: 3083
		[SerializeField]
		private float mass;

		// Token: 0x04000C0C RID: 3084
		[Header("Downed Movement")]
		[SerializeField]
		private float downedSpeed = 0.5f;

		// Token: 0x04000C0D RID: 3085
		[SerializeField]
		private float downedRotationSpeed = 80f;

		// Token: 0x04000C0E RID: 3086
		[Header("Equipment Helpers")]
		[SerializeField]
		private short equipmentPressedBufferFrames = 5;

		// Token: 0x04000C0F RID: 3087
		[Header("World Effect")]
		[SerializeField]
		private LayerMask worldEffectMask;

		// Token: 0x04000C10 RID: 3088
		[SerializeField]
		private LayerMask movingPlatformMask;

		// Token: 0x04000C11 RID: 3089
		[SerializeField]
		[Min(0f)]
		private int worldFallOffDamage;

		// Token: 0x04000C12 RID: 3090
		[Header("Grounding")]
		[SerializeField]
		private float EdgeSlideAmout = 0.25f;

		// Token: 0x04000C13 RID: 3091
		[Header("Ghost Movement")]
		[SerializeField]
		private float ghostSpeedMultiplier = 1.2f;

		// Token: 0x04000C14 RID: 3092
		[SerializeField]
		private int ghostRotationSpeed = 600;

		// Token: 0x04000C15 RID: 3093
		[SerializeField]
		private AnimationCurve ghostMovementRotationCurve;

		// Token: 0x04000C16 RID: 3094
		[SerializeField]
		private float ghostVerticalSpeed = 0.7f;

		// Token: 0x04000C17 RID: 3095
		[SerializeField]
		private int ghostVerticalMotorFrames = 10;

		// Token: 0x04000C18 RID: 3096
		[SerializeField]
		private AnimationCurve ghostVerticalMovementCurve;

		// Token: 0x04000C19 RID: 3097
		[Header("Force Recovery")]
		[SerializeField]
		private AnimationCurve forceRecoveryCurve;

		// Token: 0x04000C1A RID: 3098
		[Header("Crash Landing")]
		[SerializeField]
		private uint crashLandingStunTime_Frames = 40U;

		// Token: 0x04000C1B RID: 3099
		[SerializeField]
		private uint fallingFramesToCrashLanding = 30U;

		// Token: 0x04000C1C RID: 3100
		[SerializeField]
		[Min(0f)]
		private int crashLandingDamage = 2;

		// Token: 0x04000C1D RID: 3101
		[Header("Spawn In")]
		[SerializeField]
		private uint spawnInStunTime_Frames = 20U;

		// Token: 0x04000C1E RID: 3102
		private NetState currentState;

		// Token: 0x04000C20 RID: 3104
		public WorldUsableInteractable WorldInteractableUseQueue;

		// Token: 0x04000C21 RID: 3105
		private NetState LevelChangeTeleportState;

		// Token: 0x04000C22 RID: 3106
		private bool isUsingADS;

		// Token: 0x04000C23 RID: 3107
		private NetInput previousRawInput;

		// Token: 0x04000C24 RID: 3108
		private List<global::UnityEngine.Vector3> movingColliderVisualsCorrectionList = new List<global::UnityEngine.Vector3>();
	}
}
