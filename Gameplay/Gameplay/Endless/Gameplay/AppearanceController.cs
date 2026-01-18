using System;
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

namespace Endless.Gameplay
{
	// Token: 0x0200005D RID: 93
	public class AppearanceController : MonoBehaviour
	{
		// Token: 0x1700003F RID: 63
		// (get) Token: 0x0600016D RID: 365 RVA: 0x000082F8 File Offset: 0x000064F8
		public Transform VisualsTransform
		{
			get
			{
				if (!(this.appearanceAnimator != null))
				{
					return base.transform;
				}
				return this.appearanceAnimator.transform;
			}
		}

		// Token: 0x17000040 RID: 64
		// (get) Token: 0x0600016E RID: 366 RVA: 0x0000831A File Offset: 0x0000651A
		// (set) Token: 0x0600016F RID: 367 RVA: 0x00008322 File Offset: 0x00006522
		public PlayerReferenceManager PlayerReferences { get; protected set; }

		// Token: 0x17000041 RID: 65
		// (get) Token: 0x06000170 RID: 368 RVA: 0x0000832B File Offset: 0x0000652B
		public AppearanceAnimator AppearanceAnimator
		{
			get
			{
				return this.appearanceAnimator;
			}
		}

		// Token: 0x17000042 RID: 66
		// (get) Token: 0x06000171 RID: 369 RVA: 0x00008333 File Offset: 0x00006533
		public bool UseRelativeAimPoint
		{
			get
			{
				return this.useRelativeAimPoint;
			}
		}

		// Token: 0x17000043 RID: 67
		// (get) Token: 0x06000172 RID: 370 RVA: 0x0000833B File Offset: 0x0000653B
		public float CurrentCharRotation
		{
			get
			{
				return this.currentCharRotation;
			}
		}

		// Token: 0x17000044 RID: 68
		// (get) Token: 0x06000173 RID: 371 RVA: 0x00008343 File Offset: 0x00006543
		public bool GhostModeActive
		{
			get
			{
				return this.stateRingBuffer.ActiveInterpolatedState.Ghost;
			}
		}

		// Token: 0x06000174 RID: 372 RVA: 0x00008355 File Offset: 0x00006555
		private void Awake()
		{
			this.slot0EventSwitch = new AppearanceController.EquipmentEventSwitch(this);
			this.slot1EventSwitch = new AppearanceController.EquipmentEventSwitch(this);
			this.stateRingBuffer.OnStatesShifted.AddListener(new UnityAction<NetState, NetState>(this.HandleInterpolationStatesShifted));
		}

		// Token: 0x06000175 RID: 373 RVA: 0x0000838C File Offset: 0x0000658C
		public void RuntimeInit(PlayerReferenceManager playerReferences, AppearanceController.AppearancePerspective perspective, global::UnityEngine.Vector3 initialPos, float initialRot)
		{
			this.PlayerReferences = playerReferences;
			this.perspective = perspective;
			base.transform.position = playerReferences.transform.position;
			AppearanceAnimator appearanceAnimator = global::UnityEngine.Object.Instantiate<AppearanceAnimator>(this.PlayerReferences.ApperanceBasePrefab, base.transform);
			GameObject gameObject = global::UnityEngine.Object.Instantiate<GameObject>(MonoBehaviourSingleton<DefaultContentManager>.Instance.DefaultCharacterCosmeticsGameObject, appearanceAnimator.transform);
			this.ApplyCosmeticsGameObject(gameObject);
			this.UpdateCharacterCosmetics(playerReferences.CharacterCosmetics);
			if (playerReferences.HealthComponent)
			{
				playerReferences.HealthComponent.OnHealthLost.AddListener(new UnityAction<HealthComponent.HealthLostData>(this.HandleDamageReactionEvent));
			}
			this.PlayerReferences.OnCharacterCosmeticsChanged.AddListener(new UnityAction<CharacterCosmeticsDefinition>(this.UpdateCharacterCosmetics));
			this.initialPosition_client = initialPos;
			this.initialRotation_client = initialRot;
		}

		// Token: 0x06000176 RID: 374 RVA: 0x00008454 File Offset: 0x00006654
		private void UpdateCharacterCosmetics(CharacterCosmeticsDefinition cosmeticsDefinition)
		{
			this.latestCosmeticsSpawnHandleAppearanceAnimator = global::UnityEngine.Object.Instantiate<AppearanceAnimator>(this.PlayerReferences.ApperanceBasePrefab, base.transform);
			cosmeticsDefinition.Instantiate(this.latestCosmeticsSpawnHandleAppearanceAnimator.transform, false).Completed += this.HandleCosmeticInstantiation;
		}

		// Token: 0x06000177 RID: 375 RVA: 0x000084A4 File Offset: 0x000066A4
		private void HandleCosmeticInstantiation(AsyncOperationHandle<GameObject> handle)
		{
			if (!(this.latestCosmeticsSpawnHandleAppearanceAnimator == null) && !(this.latestCosmeticsSpawnHandleAppearanceAnimator.transform != handle.Result.transform.parent) && !(this.PlayerReferences == null) && !(this.PlayerReferences.EndlessVisuals == null))
			{
				this.ApplyCosmeticsGameObject(handle.Result);
				return;
			}
			if (handle.Result.transform.parent)
			{
				global::UnityEngine.Object.Destroy(handle.Result.transform.parent.gameObject);
				return;
			}
			global::UnityEngine.Object.Destroy(handle.Result.gameObject);
		}

		// Token: 0x06000178 RID: 376 RVA: 0x00008558 File Offset: 0x00006758
		private void ApplyCosmeticsGameObject(GameObject cosmetics)
		{
			AppearanceAnimator component = cosmetics.transform.parent.GetComponent<AppearanceAnimator>();
			component.transform.localPosition = new global::UnityEngine.Vector3(0f, this.skinWidthPositionOffset, 0f);
			component.InitializeCosmetics();
			this.PlayerReferences.EndlessVisuals.UnmanageRenderers(this.cachedRendererMangers);
			if (this.positionInitialized)
			{
				component.InitRotation(this.stateRingBuffer.GetValue(this.latestState).CharacterRotation);
			}
			AppearanceAnimator appearanceAnimator = this.appearanceAnimator;
			this.appearanceAnimator = component;
			if (this.PlayerReferences && this.PlayerReferences.Inventory)
			{
				this.PlayerReferences.Inventory.HandleCharacterCosmeticsChanged();
			}
			if (this.PlayerReferences && this.PlayerReferences.PlayerEquipmentManager)
			{
				this.PlayerReferences.PlayerEquipmentManager.TransferEquipment(component);
			}
			Renderer[] componentsInChildren = this.appearanceAnimator.GetComponentsInChildren<Renderer>();
			Renderer[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				foreach (Material material in array[i].materials)
				{
					string text = ((material.shader.name == "Shader Graphs/Endless_Shader") ? "Shader Graphs/Endless_Shader_Character_NoFade" : material.shader.name);
					material.shader = Shader.Find(text);
				}
			}
			this.cachedRendererMangers = this.PlayerReferences.EndlessVisuals.ManageRenderers(componentsInChildren);
			this.OnNewAppearence.Invoke(component);
			this.PlayerReferences.EndlessVisuals.FadeIn();
			if (appearanceAnimator != null)
			{
				global::UnityEngine.Object.Destroy(appearanceAnimator.gameObject);
			}
		}

		// Token: 0x06000179 RID: 377 RVA: 0x0000870C File Offset: 0x0000690C
		public void AddState(ref NetState state)
		{
			this.stateRingBuffer.UpdateValue(ref state);
			if (!this.positionInitialized)
			{
				base.transform.position = state.Position;
				this.positionInitialized = true;
				if (this.appearanceAnimator != null)
				{
					this.appearanceAnimator.InitRotation(state.CharacterRotation);
				}
			}
			this.latestState = state.NetFrame;
		}

		// Token: 0x0600017A RID: 378 RVA: 0x00008770 File Offset: 0x00006970
		public void TriggerAnimationImmediate(string animationTriggerString)
		{
			if (this.AppearanceAnimator && !string.IsNullOrEmpty(animationTriggerString))
			{
				this.AppearanceAnimator.Animator.SetTrigger(animationTriggerString);
			}
		}

		// Token: 0x0600017B RID: 379 RVA: 0x00008798 File Offset: 0x00006998
		private void HandleDamageReactionEvent(HealthComponent.HealthLostData data)
		{
			if (data.networked)
			{
				GameplayPlayerReferenceManager gameplayPlayerReferenceManager = PlayerReferenceManager.LocalInstance as GameplayPlayerReferenceManager;
				if (this.perspective == AppearanceController.AppearancePerspective.Extrapolate || (gameplayPlayerReferenceManager && data.damageSource == gameplayPlayerReferenceManager.NetworkObject))
				{
					return;
				}
			}
			this.hitReactionQueue.Add(data);
		}

		// Token: 0x0600017C RID: 380 RVA: 0x000087E8 File Offset: 0x000069E8
		private void HandleInterpolationStatesShifted(NetState prev, NetState next)
		{
			if (prev.JumpFrame > -1 && next.JumpFrame < 0)
			{
				this.thisAppearanceFrameTriggerableStates.Add("Jump");
			}
			if (!prev.Grounded && next.Grounded)
			{
				if (!MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying && prev.TeleportAtFrame == prev.NetFrame)
				{
					this.thisAppearanceFrameTriggerableStates.Add("Spawn");
				}
				else if (next.TeleportAtFrame != next.NetFrame || (next.TeleportAtFrame == next.NetFrame && next.TeleportStatus == TeleportComponent.TeleportStatusType.WorldFallOff))
				{
					this.thisAppearanceFrameTriggerableStates.Add("Landed");
				}
				if (prev.Downed && next.Downed)
				{
					this.thisAppearanceFrameTriggerableStates.Add("Downed");
				}
			}
			if (!prev.Downed && next.Downed)
			{
				this.thisAppearanceFrameTriggerableStates.Add("Downed");
			}
		}

		// Token: 0x0600017D RID: 381 RVA: 0x000088D4 File Offset: 0x00006AD4
		private void Update()
		{
			if (this.stateRingBuffer.NextInterpolationState.NetFrame < 1U)
			{
				base.transform.position = this.initialPosition_client;
				this.appearanceAnimator.SnapRotation(this.initialRotation_client);
				return;
			}
			this.stateRingBuffer.ActiveInterpolationTime = ((this.perspective == AppearanceController.AppearancePerspective.Server) ? NetClock.ServerAppearanceTime : ((this.perspective == AppearanceController.AppearancePerspective.Extrapolate) ? NetClock.ClientExtrapolatedAppearanceTime : NetClock.ClientInterpolatedAppearanceTime));
			if (this.stateRingBuffer.PastInterpolationState.TeleportActive && this.stateRingBuffer.PastInterpolationState.TeleportAtFrame == this.stateRingBuffer.PastInterpolationState.NetFrame + 1U)
			{
				this.stateRingBuffer.ActiveInterpolatedState.Position = this.stateRingBuffer.PastInterpolationState.TeleportPosition;
				this.stateRingBuffer.ActiveInterpolatedState.CharacterRotation = this.stateRingBuffer.PastInterpolationState.TeleportRotation;
				this.appearanceAnimator.SnapRotation(this.stateRingBuffer.ActiveInterpolatedState.CharacterRotation);
			}
			else
			{
				this.stateRingBuffer.ActiveInterpolatedState.Position = global::UnityEngine.Vector3.Lerp(this.stateRingBuffer.PastInterpolationState.CalculatedPostion + this.stateRingBuffer.PastInterpolationState.MovingColliderVisualsCorrection, this.stateRingBuffer.NextInterpolationState.CalculatedPostion + this.stateRingBuffer.NextInterpolationState.MovingColliderVisualsCorrection, this.stateRingBuffer.ActiveStateLerpTime);
				this.stateRingBuffer.ActiveInterpolatedState.CharacterRotation = Mathf.LerpAngle(this.stateRingBuffer.PastInterpolationState.CharacterRotation, this.stateRingBuffer.NextInterpolationState.CharacterRotation, this.stateRingBuffer.ActiveStateLerpTime);
			}
			this.stateRingBuffer.ActiveInterpolatedState.MotionX = Mathf.RoundToInt(Mathf.Lerp((float)this.stateRingBuffer.PastInterpolationState.MotionX, (float)this.stateRingBuffer.NextInterpolationState.MotionX, this.stateRingBuffer.ActiveStateLerpTime));
			this.stateRingBuffer.ActiveInterpolatedState.MotionZ = Mathf.RoundToInt(Mathf.Lerp((float)this.stateRingBuffer.PastInterpolationState.MotionZ, (float)this.stateRingBuffer.NextInterpolationState.MotionZ, this.stateRingBuffer.ActiveStateLerpTime));
			this.stateRingBuffer.ActiveInterpolatedState.JumpFrame = (int)((short)Mathf.RoundToInt(Mathf.Lerp((float)this.stateRingBuffer.PastInterpolationState.JumpFrame, (float)this.stateRingBuffer.NextInterpolationState.JumpFrame, this.stateRingBuffer.ActiveStateLerpTime)));
			this.stateRingBuffer.ActiveInterpolatedState.Grounded = this.stateRingBuffer.PastInterpolationState.Grounded;
			this.stateRingBuffer.ActiveInterpolatedState.SlopeAngle = Mathf.Lerp(this.stateRingBuffer.PastInterpolationState.SlopeAngle, this.stateRingBuffer.NextInterpolationState.SlopeAngle, this.stateRingBuffer.ActiveStateLerpTime);
			this.stateRingBuffer.ActiveInterpolatedState.LastWorldPhysics = this.stateRingBuffer.PastInterpolationState.LastWorldPhysics;
			this.stateRingBuffer.ActiveInterpolatedState.CalculatedMotion = global::UnityEngine.Vector3.Lerp(this.stateRingBuffer.PastInterpolationState.CalculatedMotion, this.stateRingBuffer.NextInterpolationState.CalculatedMotion, this.stateRingBuffer.ActiveStateLerpTime);
			this.stateRingBuffer.ActiveInterpolatedState.Ghost = this.stateRingBuffer.PastInterpolationState.Ghost;
			this.stateRingBuffer.ActiveInterpolatedState.AimMovementScaleMultiplier = Mathf.Lerp(this.stateRingBuffer.PastInterpolationState.AimMovementScaleMultiplier, this.stateRingBuffer.NextInterpolationState.AimMovementScaleMultiplier, this.stateRingBuffer.ActiveStateLerpTime);
			if (this.stateRingBuffer.NextInterpolationState.AimState != CameraController.CameraType.Normal && this.stateRingBuffer.PastInterpolationState.AimState == CameraController.CameraType.Normal)
			{
				this.stateRingBuffer.ActiveInterpolatedState.AimRelativePoint = this.stateRingBuffer.NextInterpolationState.AimRelativePoint;
			}
			else
			{
				this.stateRingBuffer.ActiveInterpolatedState.AimRelativePoint = global::UnityEngine.Vector3.Slerp(this.stateRingBuffer.PastInterpolationState.AimRelativePoint, this.stateRingBuffer.NextInterpolationState.AimRelativePoint, this.stateRingBuffer.ActiveStateLerpTime);
			}
			if (this.PlayerReferences.IsOwner && MonoBehaviourSingleton<CameraController>.Instance.AimUsingADS && this.stateRingBuffer.NextInterpolationState.AimState != CameraController.CameraType.Normal)
			{
				this.stateRingBuffer.ActiveInterpolatedState.CharacterRotation = Mathf.LerpAngle(this.currentCharRotation, MonoBehaviourSingleton<CameraController>.Instance.AimYaw, 0.5f);
			}
			if (this.PlayerReferences.IsOwner && NetClock.CurrentFrame <= NetworkBehaviourSingleton<NetClock>.Instance.GameplayReadyFrame.Value)
			{
				MonoBehaviourSingleton<CameraController>.Instance.ResetCamera(this.stateRingBuffer.NextInterpolationState.CharacterRotation);
			}
			this.currentCharRotation = this.stateRingBuffer.ActiveInterpolatedState.CharacterRotation;
			this.currentCharPosition = this.stateRingBuffer.ActiveInterpolatedState.Position;
			if (this.stateRingBuffer.PastInterpolationState.TeleportActive && this.stateRingBuffer.PastInterpolationState.TeleportAtFrame == this.stateRingBuffer.PastInterpolationState.NetFrame + 1U)
			{
				base.transform.position = this.currentCharPosition;
			}
			else
			{
				base.transform.position = global::UnityEngine.Vector3.Lerp(base.transform.position, this.currentCharPosition, 0.5f);
			}
			if (MonoBehaviourSingleton<StageManager>.Instance != null && MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary != null && this.PlayerReferences.Inventory)
			{
				this.slot0EventSwitch.FrameSetup(this.PlayerReferences.Inventory.GetEquippedId(0));
				this.slot1EventSwitch.FrameSetup(this.PlayerReferences.Inventory.GetEquippedId(1));
			}
			List<UsableDefinition.UseState> activeUseStates = this.stateRingBuffer.PastInterpolationState.ActiveUseStates;
			bool flag = this.stateRingBuffer.PastInterpolationState.AimState > CameraController.CameraType.Normal;
			int num = 0;
			UsableDefinition usableDefinition = null;
			global::UnityEngine.Vector3 vector = Quaternion.Euler(0f, this.stateRingBuffer.ActiveInterpolatedState.CharacterRotation, 0f) * global::UnityEngine.Vector3.forward;
			global::UnityEngine.Vector3 vector2 = Quaternion.Euler(0f, this.stateRingBuffer.NextInterpolationState.HorizontalCameraAim, 0f) * global::UnityEngine.Vector3.forward;
			float num2 = global::UnityEngine.Vector3.SignedAngle(vector, vector2, global::UnityEngine.Vector3.up) % 180f / 180f;
			if (activeUseStates != null && activeUseStates.Count > 0)
			{
				foreach (UsableDefinition.UseState useState in activeUseStates)
				{
					InventoryUsableDefinition usableDefinition2 = RuntimeDatabase.GetUsableDefinition<InventoryUsableDefinition>(useState.EquipmentGuid);
					this.thisAppearanceFrameTriggerableStates.Add(usableDefinition2.GetAnimationTrigger(useState, this.stateRingBuffer.PastInterpolationState.NetFrame));
					this.slot0EventSwitch.CheckEUS(useState);
					this.slot1EventSwitch.CheckEUS(useState);
					if (useState.GetType() == typeof(MeleeAttackUsableDefinition.MeleeAttackEquipmentUseState))
					{
						num = ((MeleeAttackUsableDefinition.MeleeAttackEquipmentUseState)useState).ComboIndex;
						usableDefinition = usableDefinition2;
					}
				}
			}
			if (this.PlayerReferences.PlayerEquipmentManager)
			{
				InventoryUsableDefinition.EquipmentShowPriority equipmentShowPriority;
				this.slot0EventSwitch.ProcessEvents(this.stateRingBuffer.PastInterpolationState, this.stateRingBuffer.ActiveInterpolationTime, out equipmentShowPriority);
				InventoryUsableDefinition.EquipmentShowPriority equipmentShowPriority2;
				this.slot1EventSwitch.ProcessEvents(this.stateRingBuffer.PastInterpolationState, this.stateRingBuffer.ActiveInterpolationTime, out equipmentShowPriority2);
				this.PlayerReferences.PlayerEquipmentManager.SetAppearanceVisibility(equipmentShowPriority, equipmentShowPriority2);
			}
			bool flag2 = this.stateRingBuffer.ActiveInterpolatedState.MotionX != 0 || this.stateRingBuffer.ActiveInterpolatedState.MotionZ != 0;
			bool grounded = this.stateRingBuffer.ActiveInterpolatedState.Grounded;
			bool flag3 = !this.stateRingBuffer.NextInterpolationState.Run;
			float num3 = (float)this.stateRingBuffer.PastInterpolationState.AirborneFrames * NetClock.FixedDeltaTime;
			float num4 = (float)this.stateRingBuffer.PastInterpolationState.FallFrames * NetClock.FixedDeltaTime;
			float num5 = Mathf.DeltaAngle(this.stateRingBuffer.PastInterpolationState.CharacterRotation, this.stateRingBuffer.NextInterpolationState.CharacterRotation) / NetClock.FixedDeltaTime / this.PlayerReferences.PlayerController.RotationSpeed;
			this.animatorAngularVelocity = Mathf.SmoothDamp(this.animatorAngularVelocity, num5, ref this.animatorAngularVelocityVelocity, this.angularVelocitySmoothTime);
			global::UnityEngine.Vector3 vector3 = this.stateRingBuffer.NextInterpolationState.Position - this.stateRingBuffer.PastInterpolationState.Position;
			global::UnityEngine.Vector3 vector4 = this.VisualsTransform.InverseTransformDirection(vector3.normalized);
			float num6 = (this.stateRingBuffer.NextInterpolationState.Position.y - this.stateRingBuffer.PastInterpolationState.Position.y) / this.PlayerReferences.PlayerController.TerminalFallingVelocity;
			this.animatorVelocityX = Mathf.SmoothDamp(this.animatorVelocityX, vector4.x * this.stateRingBuffer.ActiveInterpolatedState.AimMovementScaleMultiplier, ref this.animatorVelocityXVelocity, this.velocitySmoothTime);
			this.animatorVelocityZ = Mathf.SmoothDamp(this.animatorVelocityZ, vector4.z * this.stateRingBuffer.ActiveInterpolatedState.AimMovementScaleMultiplier, ref this.animatorVelocityZVelocity, this.velocitySmoothTime);
			this.animatorVelocityY = Mathf.SmoothDamp(this.animatorVelocityY, num6, ref this.animatorVelocityYVelocity, this.velocitySmoothTime);
			PlayerController.AngleToForwardMotion(this.stateRingBuffer.ActiveInterpolatedState.CharacterRotation);
			Vector2 vector5 = new Vector2(vector3.x, vector3.z);
			float num7 = vector5.magnitude / (this.PlayerReferences.PlayerController.BaseSpeed * NetClock.FixedDeltaTime);
			if (grounded && !this.stateRingBuffer.PastInterpolationState.TeleportActive)
			{
				this.footstepController.UpdateFootsteps(num7, flag3);
			}
			string empty = string.Empty;
			if (this.thisAppearanceFrameTriggerableStates.Contains("Attack"))
			{
				bool flag4 = false;
				if (this.appearanceAnimator)
				{
					flag4 = this.appearanceAnimator.TriggerAttackCombo(num);
				}
				if (flag4 && usableDefinition != null)
				{
					base.StartCoroutine(this.TriggerAttackVfxDelayed((MeleeAttackUsableDefinition)usableDefinition, num));
				}
			}
			global::UnityEngine.Vector3 vector6 = (this.PlayerReferences.IsOwner ? MonoBehaviourSingleton<CameraController>.Instance.LastAimPosition : (this.useRelativeAimPoint ? this.VisualsTransform.TransformPoint(this.stateRingBuffer.ActiveInterpolatedState.AimRelativePoint) : this.stateRingBuffer.ActiveInterpolatedState.AimRelativePoint));
			if (this.appearanceAnimator)
			{
				this.appearanceAnimator.SetAnimationState(this.stateRingBuffer.ActiveInterpolatedState.CharacterRotation, flag2, flag3, grounded, this.stateRingBuffer.ActiveInterpolatedState.SlopeAngle, num3, num4, new Vector2((float)this.stateRingBuffer.ActiveInterpolatedState.MotionX, (float)this.stateRingBuffer.ActiveInterpolatedState.MotionZ), this.animatorVelocityX, this.animatorVelocityY, this.animatorVelocityZ, this.animatorAngularVelocity, num7, empty, num, this.stateRingBuffer.ActiveInterpolatedState.Ghost, flag, num2, vector6, this.stateRingBuffer.NextInterpolationState.UseIK);
			}
			foreach (string text in this.thisAppearanceFrameTriggerableStates)
			{
				if (!this.pastAppearanceFrameTriggerableStates.Contains(text) && !string.IsNullOrWhiteSpace(text))
				{
					if (this.appearanceAnimator)
					{
						this.appearanceAnimator.TriggerAnimation(text);
					}
					if (text == "Attack" && usableDefinition != null)
					{
						base.StartCoroutine(this.TriggerAttackVfxDelayed((MeleeAttackUsableDefinition)usableDefinition, num));
					}
				}
			}
			this.pastAppearanceFrameTriggerableStates.Clear();
			this.pastAppearanceFrameTriggerableStates.AddRange(this.thisAppearanceFrameTriggerableStates);
			this.thisAppearanceFrameTriggerableStates.Clear();
			int count = this.hitReactionQueue.Count;
			this.hitReactionQueue.RemoveAll((HealthComponent.HealthLostData hitReactionData) => hitReactionData.frame <= this.stateRingBuffer.PastInterpolationState.NetFrame);
			if (count > this.hitReactionQueue.Count)
			{
				this.PlayerReferences.DamageReaction.TriggerDamageReaction();
			}
			this.HandlePhysicsPushTriggers();
			if (this.teleporting && !this.stateRingBuffer.NextInterpolationState.GameplayTeleport)
			{
				this.teleporting = false;
				TeleportInfo teleportInfo = RuntimeDatabase.GetTeleportInfo(this.activeTeleportType);
				EndlessVisuals endlessVisuals = this.PlayerReferences.EndlessVisuals;
				AppearanceAnimator appearanceAnimator = this.appearanceAnimator;
				teleportInfo.TeleportEnd(endlessVisuals, (appearanceAnimator != null) ? appearanceAnimator.Animator : null, base.transform.position);
				if (this.PlayerReferences.IsOwner && this.stateRingBuffer.PastInterpolationState.TeleportRotationSnapCamera)
				{
					MonoBehaviourSingleton<CameraController>.Instance.ResetCamera(this.stateRingBuffer.NextInterpolationState.TeleportRotation);
					return;
				}
			}
			else if (!this.teleporting && this.stateRingBuffer.NextInterpolationState.GameplayTeleport)
			{
				this.activeTeleportType = this.stateRingBuffer.NextInterpolationState.GameplayTeleportType;
				this.teleporting = true;
				TeleportInfo teleportInfo2 = RuntimeDatabase.GetTeleportInfo(this.activeTeleportType);
				EndlessVisuals endlessVisuals2 = this.PlayerReferences.EndlessVisuals;
				AppearanceAnimator appearanceAnimator2 = this.appearanceAnimator;
				teleportInfo2.TeleportStart(endlessVisuals2, (appearanceAnimator2 != null) ? appearanceAnimator2.Animator : null, base.transform.position);
			}
		}

		// Token: 0x0600017E RID: 382 RVA: 0x00009640 File Offset: 0x00007840
		private void LateUpdate()
		{
			if (this.appearanceAnimator != null)
			{
				global::UnityEngine.Vector3 vector;
				if (this.PlayerReferences.IsOwner)
				{
					vector = MonoBehaviourSingleton<CameraController>.Instance.CurrentAimPosition;
				}
				else
				{
					vector = this.stateRingBuffer.ActiveInterpolatedState.AimRelativePoint;
					if (this.useRelativeAimPoint)
					{
						vector = this.VisualsTransform.TransformPoint(vector);
					}
				}
				this.appearanceAnimator.UpdateIK(this.stateRingBuffer.NextInterpolationState.UseIK, this.stateRingBuffer.ActiveInterpolatedState.Grounded, this.stateRingBuffer.ActiveInterpolatedState.Ghost, vector, global::UnityEngine.Vector3.zero);
			}
		}

		// Token: 0x0600017F RID: 383 RVA: 0x000096E0 File Offset: 0x000078E0
		private void HandlePhysicsPushTriggers()
		{
			if (this.stateRingBuffer.NextInterpolationState.PhysicsPushState != this.pushState)
			{
				if (this.pushState == PlayerPhysicsTaker.PushState.None)
				{
					if (this.stateRingBuffer.NextInterpolationState.PhysicsPushState == PlayerPhysicsTaker.PushState.Small)
					{
						this.appearanceAnimator.TriggerAnimation("SmallPush");
					}
					else
					{
						this.appearanceAnimator.TriggerAnimation("LargePush");
					}
				}
				else if (this.pushState == PlayerPhysicsTaker.PushState.Small)
				{
					if (this.stateRingBuffer.NextInterpolationState.PhysicsPushState == PlayerPhysicsTaker.PushState.None)
					{
						this.appearanceAnimator.TriggerAnimation("EndSmallPush");
					}
					else
					{
						this.appearanceAnimator.TriggerAnimation("LargePush");
					}
				}
				else
				{
					this.appearanceAnimator.TriggerAnimation("EndLargePush");
				}
				this.pushState = this.stateRingBuffer.NextInterpolationState.PhysicsPushState;
			}
		}

		// Token: 0x06000180 RID: 384 RVA: 0x000097AC File Offset: 0x000079AC
		private IEnumerator TriggerAttackVfxDelayed(MeleeAttackUsableDefinition meleeAttackID, int comboBookmark)
		{
			yield return new WaitForSeconds(meleeAttackID.GetVisualEffectsDelay(comboBookmark));
			MeleeVfxPlayer visualEffect = meleeAttackID.GetVisualEffect(comboBookmark);
			if (visualEffect != null)
			{
				MeleeVfxPlayer meleeVfxPlayer = MeleeVfxPlayer.GetFromPool(visualEffect.gameObject);
				if (meleeVfxPlayer == null)
				{
					meleeVfxPlayer = global::UnityEngine.Object.Instantiate<MeleeVfxPlayer>(visualEffect, this.VisualsTransform.position, this.VisualsTransform.rotation);
				}
				meleeVfxPlayer.transform.position = this.VisualsTransform.position;
				meleeVfxPlayer.transform.rotation = this.VisualsTransform.rotation;
				meleeVfxPlayer.gameObject.SetActive(true);
				meleeVfxPlayer.PlayEffect(base.transform, this.VisualsTransform);
			}
			yield break;
		}

		// Token: 0x06000181 RID: 385 RVA: 0x000097C9 File Offset: 0x000079C9
		public Transform GetBone(string boneName)
		{
			if (this.appearanceAnimator)
			{
				return this.appearanceAnimator.GetBone(boneName);
			}
			return null;
		}

		// Token: 0x04000137 RID: 311
		private const float POSITION_SMOOTHING_TIME = 0.05f;

		// Token: 0x04000138 RID: 312
		public UnityEvent<SerializableGuid, bool> OnEquipmentAvailableChanged = new UnityEvent<SerializableGuid, bool>();

		// Token: 0x04000139 RID: 313
		public UnityEvent<SerializableGuid, bool> OnEquipmentInUseChanged = new UnityEvent<SerializableGuid, bool>();

		// Token: 0x0400013A RID: 314
		public UnityEvent<SerializableGuid, float, float> OnEquipmentCooldownChanged = new UnityEvent<SerializableGuid, float, float>();

		// Token: 0x0400013B RID: 315
		public UnityEvent<SerializableGuid, float> OnEquipmentResourceChanged = new UnityEvent<SerializableGuid, float>();

		// Token: 0x0400013C RID: 316
		public UnityEvent<SerializableGuid, UsableDefinition.UseState> OnEquipmentUseStateChanged = new UnityEvent<SerializableGuid, UsableDefinition.UseState>();

		// Token: 0x0400013D RID: 317
		public UnityEvent<AppearanceAnimator> OnNewAppearence = new UnityEvent<AppearanceAnimator>();

		// Token: 0x0400013E RID: 318
		[SerializeField]
		private float angularVelocitySmoothTime = 0.25f;

		// Token: 0x0400013F RID: 319
		[SerializeField]
		private float velocitySmoothTime = 0.25f;

		// Token: 0x04000140 RID: 320
		[SerializeField]
		private float skinWidthPositionOffset = -0.025f;

		// Token: 0x04000141 RID: 321
		[SerializeField]
		private bool useRelativeAimPoint = true;

		// Token: 0x04000142 RID: 322
		[SerializeField]
		private FootstepController footstepController;

		// Token: 0x04000143 RID: 323
		private AppearanceAnimator appearanceAnimator;

		// Token: 0x04000144 RID: 324
		private InterpolationRingBuffer<NetState> stateRingBuffer = new InterpolationRingBuffer<NetState>(30);

		// Token: 0x04000145 RID: 325
		private float animatorAngularVelocity;

		// Token: 0x04000146 RID: 326
		private float animatorVelocityX;

		// Token: 0x04000147 RID: 327
		private float animatorVelocityY;

		// Token: 0x04000148 RID: 328
		private float animatorVelocityZ;

		// Token: 0x04000149 RID: 329
		private float animatorAngularVelocityVelocity;

		// Token: 0x0400014A RID: 330
		private float animatorVelocityXVelocity;

		// Token: 0x0400014B RID: 331
		private float animatorVelocityYVelocity;

		// Token: 0x0400014C RID: 332
		private float animatorVelocityZVelocity;

		// Token: 0x0400014D RID: 333
		private AppearanceController.AppearancePerspective perspective = AppearanceController.AppearancePerspective.Server;

		// Token: 0x0400014F RID: 335
		private List<string> pastAppearanceFrameTriggerableStates = new List<string>();

		// Token: 0x04000150 RID: 336
		private List<string> thisAppearanceFrameTriggerableStates = new List<string>();

		// Token: 0x04000151 RID: 337
		private AppearanceController.EquipmentEventSwitch slot0EventSwitch;

		// Token: 0x04000152 RID: 338
		private AppearanceController.EquipmentEventSwitch slot1EventSwitch;

		// Token: 0x04000153 RID: 339
		private PlayerPhysicsTaker.PushState pushState;

		// Token: 0x04000154 RID: 340
		private float currentCharRotation;

		// Token: 0x04000155 RID: 341
		private global::UnityEngine.Vector3 currentCharPosition;

		// Token: 0x04000156 RID: 342
		private uint firstStateFrame;

		// Token: 0x04000157 RID: 343
		private bool positionInitialized;

		// Token: 0x04000158 RID: 344
		private uint latestState;

		// Token: 0x04000159 RID: 345
		private List<RendererManager> cachedRendererMangers = new List<RendererManager>();

		// Token: 0x0400015A RID: 346
		private global::UnityEngine.Vector3 initialPosition_client;

		// Token: 0x0400015B RID: 347
		private float initialRotation_client;

		// Token: 0x0400015C RID: 348
		private AppearanceAnimator latestCosmeticsSpawnHandleAppearanceAnimator;

		// Token: 0x0400015D RID: 349
		private bool teleporting;

		// Token: 0x0400015E RID: 350
		private TeleportType activeTeleportType;

		// Token: 0x0400015F RID: 351
		private EndlessVisuals endlessVisuals;

		// Token: 0x04000160 RID: 352
		private List<HealthComponent.HealthLostData> hitReactionQueue = new List<HealthComponent.HealthLostData>();

		// Token: 0x0200005E RID: 94
		public enum AppearancePerspective
		{
			// Token: 0x04000162 RID: 354
			Extrapolate,
			// Token: 0x04000163 RID: 355
			Server,
			// Token: 0x04000164 RID: 356
			Interpolate
		}

		// Token: 0x0200005F RID: 95
		private class EquipmentEventSwitch
		{
			// Token: 0x06000184 RID: 388 RVA: 0x000098D3 File Offset: 0x00007AD3
			public EquipmentEventSwitch(AppearanceController target)
			{
				this.targetAppearanceController = target;
			}

			// Token: 0x06000185 RID: 389 RVA: 0x000098F8 File Offset: 0x00007AF8
			public void FrameSetup(SerializableGuid assetID)
			{
				this.slotAssedId = assetID;
				SerializableGuid usableDefinitionIDFromAssetID = RuntimeDatabase.GetUsableDefinitionIDFromAssetID(assetID);
				this.slotChanged = !this.slotGUID.Equals(usableDefinitionIDFromAssetID);
				this.slotGUID = usableDefinitionIDFromAssetID;
				this.previousSlotEus = this.slotEUS;
				this.slotEUS = null;
			}

			// Token: 0x06000186 RID: 390 RVA: 0x00009942 File Offset: 0x00007B42
			public void CheckEUS(UsableDefinition.UseState eus)
			{
				if (this.slotGUID.Equals(eus.EquipmentGuid))
				{
					this.slotEUS = eus;
				}
			}

			// Token: 0x06000187 RID: 391 RVA: 0x00009960 File Offset: 0x00007B60
			public void ProcessEvents(NetState state, double appearanceTime, out InventoryUsableDefinition.EquipmentShowPriority equipmentShowPriority)
			{
				if (!this.slotGUID.Equals(SerializableGuid.Empty))
				{
					InventoryUsableDefinition usableDefinition = RuntimeDatabase.GetUsableDefinition<InventoryUsableDefinition>(this.slotGUID);
					usableDefinition.GetEventData(state, this.slotEUS, appearanceTime, ref this.currentSlotData);
					equipmentShowPriority = usableDefinition.GetShowPriority(this.slotEUS);
				}
				else
				{
					equipmentShowPriority = InventoryUsableDefinition.EquipmentShowPriority.NotShown;
				}
				if (this.slotChanged)
				{
					this.targetAppearanceController.OnEquipmentUseStateChanged.Invoke(this.slotGUID, this.slotEUS);
					this.targetAppearanceController.OnEquipmentAvailableChanged.Invoke(this.slotGUID, this.currentSlotData.Available);
					this.targetAppearanceController.OnEquipmentInUseChanged.Invoke(this.slotGUID, this.currentSlotData.InUse);
					this.targetAppearanceController.OnEquipmentCooldownChanged.Invoke(this.slotGUID, this.currentSlotData.CooldownSecondsLeft, this.currentSlotData.CooldownSecondsTotal);
					this.targetAppearanceController.OnEquipmentResourceChanged.Invoke(this.slotGUID, this.currentSlotData.ResourcePercent);
				}
				else
				{
					if (this.slotEUS != null || this.slotEUS != this.previousSlotEus)
					{
						this.targetAppearanceController.OnEquipmentUseStateChanged.Invoke(this.slotGUID, this.slotEUS);
					}
					if (this.previousSlotData.Available != this.currentSlotData.Available)
					{
						this.targetAppearanceController.OnEquipmentAvailableChanged.Invoke(this.slotGUID, this.currentSlotData.Available);
					}
					if (this.previousSlotData.InUse != this.currentSlotData.InUse)
					{
						this.targetAppearanceController.OnEquipmentInUseChanged.Invoke(this.slotGUID, this.currentSlotData.InUse);
					}
					if (!Mathf.Approximately(this.previousSlotData.CooldownSecondsLeft, this.currentSlotData.CooldownSecondsLeft))
					{
						this.targetAppearanceController.OnEquipmentCooldownChanged.Invoke(this.slotGUID, this.currentSlotData.CooldownSecondsLeft, this.currentSlotData.CooldownSecondsTotal);
					}
					if (!Mathf.Approximately(this.previousSlotData.ResourcePercent, this.currentSlotData.ResourcePercent))
					{
						this.targetAppearanceController.OnEquipmentResourceChanged.Invoke(this.slotGUID, this.currentSlotData.ResourcePercent);
					}
				}
				this.currentSlotData.CopyTo(ref this.previousSlotData);
				this.currentSlotData.Reset();
			}

			// Token: 0x04000165 RID: 357
			private AppearanceController targetAppearanceController;

			// Token: 0x04000166 RID: 358
			private InventoryUsableDefinition.EventData previousSlotData;

			// Token: 0x04000167 RID: 359
			private InventoryUsableDefinition.EventData currentSlotData;

			// Token: 0x04000168 RID: 360
			private SerializableGuid slotGUID = SerializableGuid.Empty;

			// Token: 0x04000169 RID: 361
			private SerializableGuid slotAssedId = SerializableGuid.Empty;

			// Token: 0x0400016A RID: 362
			private bool slotChanged;

			// Token: 0x0400016B RID: 363
			private UsableDefinition.UseState previousSlotEus;

			// Token: 0x0400016C RID: 364
			private UsableDefinition.UseState slotEUS;
		}
	}
}
