using System;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002DF RID: 735
	public class RangedAttackUsableDefinition : InventoryUsableDefinition
	{
		// Token: 0x1700033C RID: 828
		// (get) Token: 0x0600109D RID: 4253 RVA: 0x0005425D File Offset: 0x0005245D
		public int AmmoCount
		{
			get
			{
				return this.ammoCount;
			}
		}

		// Token: 0x1700033D RID: 829
		// (get) Token: 0x0600109E RID: 4254 RVA: 0x00054265 File Offset: 0x00052465
		public float ReloadTime
		{
			get
			{
				return this.reloadTime;
			}
		}

		// Token: 0x1700033E RID: 830
		// (get) Token: 0x0600109F RID: 4255 RVA: 0x0005426D File Offset: 0x0005246D
		public int ReloadAnimIndex
		{
			get
			{
				return this.reloadAnimIndex;
			}
		}

		// Token: 0x1700033F RID: 831
		// (get) Token: 0x060010A0 RID: 4256 RVA: 0x00054275 File Offset: 0x00052475
		public bool CanShootInAir
		{
			get
			{
				return this.airBehaviorOverride != RangedAttackUsableDefinition.AirBehaviorOverride.NoShooting;
			}
		}

		// Token: 0x17000340 RID: 832
		// (get) Token: 0x060010A1 RID: 4257 RVA: 0x00054283 File Offset: 0x00052483
		public float WeaponStrength
		{
			get
			{
				return this.weaponStrength;
			}
		}

		// Token: 0x17000341 RID: 833
		// (get) Token: 0x060010A2 RID: 4258 RVA: 0x0005428B File Offset: 0x0005248B
		public float WeaponAccuracy
		{
			get
			{
				return this.weaponAccuracy;
			}
		}

		// Token: 0x17000342 RID: 834
		// (get) Token: 0x060010A3 RID: 4259 RVA: 0x00054293 File Offset: 0x00052493
		public float RecoilAmount
		{
			get
			{
				return this.recoilAmountDegrees;
			}
		}

		// Token: 0x17000343 RID: 835
		// (get) Token: 0x060010A4 RID: 4260 RVA: 0x0005429B File Offset: 0x0005249B
		public float MaxRecoil
		{
			get
			{
				return this.maxRecoilDegrees;
			}
		}

		// Token: 0x17000344 RID: 836
		// (get) Token: 0x060010A5 RID: 4261 RVA: 0x000542A3 File Offset: 0x000524A3
		public float RecoilSettleAmount
		{
			get
			{
				return this.recoilSettleAmountDegrees;
			}
		}

		// Token: 0x17000345 RID: 837
		// (get) Token: 0x060010A6 RID: 4262 RVA: 0x000542AB File Offset: 0x000524AB
		public AnimationCurve RecoilSettleCurve
		{
			get
			{
				return this.recoilSettleCurve;
			}
		}

		// Token: 0x17000346 RID: 838
		// (get) Token: 0x060010A7 RID: 4263 RVA: 0x000542B3 File Offset: 0x000524B3
		public float RecoilSettleDelay
		{
			get
			{
				return this.recoilSettleDelay;
			}
		}

		// Token: 0x17000347 RID: 839
		// (get) Token: 0x060010A8 RID: 4264 RVA: 0x000542BB File Offset: 0x000524BB
		public float MovementAimPenalty
		{
			get
			{
				return this.movementAimPenaltyDegrees;
			}
		}

		// Token: 0x17000348 RID: 840
		// (get) Token: 0x060010A9 RID: 4265 RVA: 0x000542C3 File Offset: 0x000524C3
		public float CameraClimbAmount
		{
			get
			{
				return this.cameraClimbAmountDegrees;
			}
		}

		// Token: 0x17000349 RID: 841
		// (get) Token: 0x060010AA RID: 4266 RVA: 0x000542CB File Offset: 0x000524CB
		public float MaxCameraClimb
		{
			get
			{
				return this.maxCameraClimbDegrees;
			}
		}

		// Token: 0x1700034A RID: 842
		// (get) Token: 0x060010AB RID: 4267 RVA: 0x000542D3 File Offset: 0x000524D3
		public float CameraClimbSettleAmount
		{
			get
			{
				return this.cameraClimbSettleAmountDegrees;
			}
		}

		// Token: 0x1700034B RID: 843
		// (get) Token: 0x060010AC RID: 4268 RVA: 0x000542DB File Offset: 0x000524DB
		public bool AllowReloadWhenFullAmmo
		{
			get
			{
				return this.allowReloadWhenFullAmmo;
			}
		}

		// Token: 0x060010AD RID: 4269 RVA: 0x000542E4 File Offset: 0x000524E4
		public override UsableDefinition.UseState ProcessUseStart(ref NetState state, NetInput input, SerializableGuid guid, PlayerReferenceManager playerReference, Item item)
		{
			UsableDefinition.UseState useState = UsableDefinition.UseState.GetStateFromPool(guid, item) as RangedAttackUsableDefinition.RangedAttackEquipmentUseState;
			RangedWeaponItem rangedWeaponItem = item as RangedWeaponItem;
			ShootingState shootingState = rangedWeaponItem.GetShootingState(input.NetFrame);
			if (input.NetFrame > shootingState.NetFrame)
			{
				shootingState.Clear();
				shootingState.NetFrame = input.NetFrame - 1U;
				rangedWeaponItem.ShootingStateUpdated(input.NetFrame, ref shootingState);
			}
			return useState;
		}

		// Token: 0x060010AE RID: 4270 RVA: 0x0005434C File Offset: 0x0005254C
		public override bool ProcessUseFrame(ref NetState state, NetInput input, ref UsableDefinition.UseState equipmentUseState, PlayerReferenceManager playerReference, bool equipped, bool pressed)
		{
			if (state.IsStunned)
			{
				return false;
			}
			RangedWeaponItem rangedWeaponItem = equipmentUseState.Item as RangedWeaponItem;
			ProjectileShooter projectileShooter = ((rangedWeaponItem != null) ? rangedWeaponItem.ProjectileShooter : null);
			if (rangedWeaponItem == null || projectileShooter == null)
			{
				return false;
			}
			bool flag = input.NetFrame == NetClock.CurrentFrame;
			bool flag2 = !state.Grounded;
			bool flag3 = rangedWeaponItem.ReloadRequested || rangedWeaponItem.ReloadStarted;
			bool flag4 = state.AnyEquipmentSwapActive || playerReference.ApperanceController.AppearanceAnimator.IsInHotswapState;
			ShootingState shootingState = rangedWeaponItem.GetShootingState(input.NetFrame - 1U);
			uint num = 0U;
			state.BlockRotation = true;
			state.Strafing = true;
			if (equipped)
			{
				state.BlockItemInput = true;
			}
			bool flag5 = true;
			if (flag2 && !this.CanShootInAir)
			{
				flag5 = false;
			}
			if (input.PressingPrimary2 && (pressed || flag4))
			{
				CameraController.CameraType cameraType2;
				if (flag2)
				{
					CameraController.CameraType cameraType;
					switch (this.airBehaviorOverride)
					{
					case RangedAttackUsableDefinition.AirBehaviorOverride.Default:
						cameraType = CameraController.CameraType.FullADS;
						break;
					case RangedAttackUsableDefinition.AirBehaviorOverride.NoShooting:
						cameraType = CameraController.CameraType.Normal;
						break;
					case RangedAttackUsableDefinition.AirBehaviorOverride.AllowSoftADS:
						cameraType = CameraController.CameraType.SoftADS;
						break;
					default:
						cameraType = CameraController.CameraType.FullADS;
						break;
					}
					cameraType2 = cameraType;
				}
				else
				{
					cameraType2 = CameraController.CameraType.FullADS;
				}
				shootingState.aimState = cameraType2;
				if (this.aimOnlyCooldown > num)
				{
					num = this.aimOnlyCooldown;
				}
			}
			else
			{
				CameraController.CameraType cameraType2;
				if (flag2)
				{
					RangedAttackUsableDefinition.AirBehaviorOverride airBehaviorOverride = this.airBehaviorOverride;
					CameraController.CameraType cameraType;
					if (airBehaviorOverride != RangedAttackUsableDefinition.AirBehaviorOverride.Default)
					{
						if (airBehaviorOverride != RangedAttackUsableDefinition.AirBehaviorOverride.NoShooting)
						{
							cameraType = CameraController.CameraType.SoftADS;
						}
						else
						{
							cameraType = CameraController.CameraType.Normal;
						}
					}
					else
					{
						cameraType = CameraController.CameraType.SoftADS;
					}
					cameraType2 = cameraType;
				}
				else
				{
					cameraType2 = CameraController.CameraType.SoftADS;
				}
				shootingState.aimState = cameraType2;
			}
			if ((flag4 || (pressed && input.PressingPrimary1)) && this.aimOnlyCooldown > num)
			{
				num = this.aimOnlyCooldown;
			}
			float num2 = ((shootingState.aimState == CameraController.CameraType.FullADS) ? this.adsLockedRotationSpeed : this.softADSRotationSpeed);
			if (!shootingState.cameraLocked)
			{
				num2 = this.rotationSpeedBeforeCameraLocked;
			}
			bool flag6 = false;
			if (flag5)
			{
				flag6 = PlayerController.Rotate(ref state, MonoBehaviourSingleton<CameraController>.Instance.AimYaw, num2, false);
				flag6 &= playerReference.ApperanceController.AppearanceAnimator.IsInAimState && !flag4;
			}
			if (flag6)
			{
				shootingState.cameraLocked = true;
			}
			if (equipped && flag5)
			{
				if (!shootingState.commitToShot && !input.PressingPrimary1)
				{
					shootingState.shotReleased = true;
				}
				if (pressed && input.PressingPrimary1 && (this.automatic || shootingState.shotReleased))
				{
					if (!flag6 && rangedWeaponItem.HasAmmo && !flag3)
					{
						shootingState.waitingForShot = true;
					}
					if (!shootingState.commitToShot && (ulong)(input.NetFrame - shootingState.lastShotFrame) > (ulong)((long)(this.shotDelayFrames - this.commitToShotFrames)))
					{
						shootingState.commitToShot = true;
						shootingState.shotReleased = false;
					}
				}
			}
			else
			{
				shootingState.cameraLocked = false;
				shootingState.waitingForShot = false;
				if (!input.PressingPrimary1)
				{
					shootingState.commitToShot = false;
				}
			}
			if (flag5 && (shootingState.commitToShot || shootingState.waitingForShot))
			{
				uint num3 = ((shootingState.aimState == CameraController.CameraType.FullADS) ? this.fullCooldown : this.quickShotCooldown);
				if (num3 > num)
				{
					num = num3;
				}
			}
			if (equipped && flag5 && shootingState.commitToShot && flag6 && input.NetFrame - shootingState.lastShotFrame > this.shotDelayFrames)
			{
				uint num4 = ((shootingState.aimState == CameraController.CameraType.FullADS) ? this.fullCooldown : this.quickShotCooldown);
				if (num4 > num)
				{
					num = num4;
				}
				if (playerReference.IsOwner && !flag3)
				{
					if (rangedWeaponItem.HasAmmo)
					{
						if (flag)
						{
							float currentRecoilAccumulation = rangedWeaponItem.CurrentRecoilAccumulation;
							rangedWeaponItem.FireShot(1);
							Vector3 forward = projectileShooter.FirePoint.forward;
							Vector3 normalized = (MonoBehaviourSingleton<CameraController>.Instance.LastAimPosition - projectileShooter.FirePoint.position).normalized;
							Vector3 eulerAngles = Quaternion.LookRotation(normalized, Vector3.up).eulerAngles;
							float num5 = Vector3.Angle(forward, normalized);
							if (num5 > 10f)
							{
								Debug.LogWarning(string.Format("Angle between muzzle and aim is large: {0}!", num5));
							}
							Vector3 vector = eulerAngles + new Vector3(global::UnityEngine.Random.Range(-currentRecoilAccumulation, currentRecoilAccumulation), global::UnityEngine.Random.Range(-currentRecoilAccumulation, currentRecoilAccumulation), 0f);
							Vector3 vector2 = projectileShooter.FirePoint.position + Quaternion.Euler(vector) * Vector3.forward * 0.1f;
							projectileShooter.UpdateBaseTeam(Team.Friendly);
							projectileShooter.ClientAuthoritativeShoot(vector2, vector, input.NetFrame, null, playerReference.WorldObject);
							if (this.instantAutoReloadAfterLastShot && !rangedWeaponItem.HasAmmo)
							{
								rangedWeaponItem.StartReload();
							}
						}
					}
					else if (this.autoReload)
					{
						rangedWeaponItem.StartReload();
					}
				}
				shootingState.commitToShot = false;
				shootingState.waitingForShot = false;
				shootingState.lastShotFrame = input.NetFrame;
			}
			if (num > 0U)
			{
				this.ApplyFinishedFrame(ref shootingState, input.NetFrame + num);
			}
			state.XZInputMultiplierThisFrame = ((shootingState.aimState == CameraController.CameraType.FullADS) ? this.adsMovementMultiplier : this.softADSMovementMultiplier);
			state.AimMovementScaleMultiplier = ((shootingState.aimState == CameraController.CameraType.FullADS) ? (this.adsMovementMultiplier / this.softADSMovementMultiplier) : 1f);
			state.AimState = shootingState.aimState;
			if (flag)
			{
				shootingState.NetFrame = input.NetFrame;
				rangedWeaponItem.ShootingStateUpdated(input.NetFrame, ref shootingState);
				if (num > 0U && !NetworkManager.Singleton.IsServer)
				{
					this.SendFinishedFrame(rangedWeaponItem, input.NetFrame + num);
				}
			}
			if (!pressed)
			{
				uint num6 = ((NetworkManager.Singleton.IsServer && !playerReference.IsOwner) ? rangedWeaponItem.ServerFinishFrame : shootingState.finishedFrame);
				if (input.NetFrame > num6)
				{
					return false;
				}
			}
			if (playerReference.IsOwner && shootingState.aimState != CameraController.CameraType.Normal)
			{
				state.UsingAimFix = true;
			}
			return true;
		}

		// Token: 0x060010AF RID: 4271 RVA: 0x000548EC File Offset: 0x00052AEC
		private void SendFinishedFrame(RangedWeaponItem weapon, uint frame)
		{
			weapon.UpdateFinishFrame_ServerRpc(frame, default(ServerRpcParams));
		}

		// Token: 0x060010B0 RID: 4272 RVA: 0x00054909 File Offset: 0x00052B09
		private void ApplyFinishedFrame(ref ShootingState shootingState, uint frame)
		{
			shootingState.finishedFrame = ((shootingState.finishedFrame < frame) ? frame : shootingState.finishedFrame);
		}

		// Token: 0x060010B1 RID: 4273 RVA: 0x00054923 File Offset: 0x00052B23
		public override string GetAnimationTrigger(UsableDefinition.UseState eus, uint currentVisualFrame)
		{
			return string.Empty;
		}

		// Token: 0x060010B2 RID: 4274 RVA: 0x0005492C File Offset: 0x00052B2C
		public override void GetEventData(NetState state, UsableDefinition.UseState eus, double appearanceTime, ref InventoryUsableDefinition.EventData data)
		{
			RangedAttackUsableDefinition.RangedAttackEquipmentUseState rangedAttackEquipmentUseState = (RangedAttackUsableDefinition.RangedAttackEquipmentUseState)eus;
			CameraController.CameraType cameraType = CameraController.CameraType.Normal;
			if (rangedAttackEquipmentUseState != null)
			{
				RangedWeaponItem rangedWeaponItem = rangedAttackEquipmentUseState.Item as RangedWeaponItem;
				if (rangedWeaponItem != null)
				{
					cameraType = rangedWeaponItem.GetShootingState(state.NetFrame).aimState;
				}
			}
			data.Reset();
			data.InUse = rangedAttackEquipmentUseState != null && cameraType > CameraController.CameraType.Normal;
		}

		// Token: 0x060010B3 RID: 4275 RVA: 0x0005497F File Offset: 0x00052B7F
		public override UsableDefinition.UseState GetNewUseState()
		{
			return new RangedAttackUsableDefinition.RangedAttackEquipmentUseState();
		}

		// Token: 0x04000E46 RID: 3654
		private const int MIN_GROUNDED_FRAMES = 10;

		// Token: 0x04000E47 RID: 3655
		[Header("Shooting")]
		[SerializeField]
		private bool automatic;

		// Token: 0x04000E48 RID: 3656
		[SerializeField]
		private uint shotDelayFrames = 10U;

		// Token: 0x04000E49 RID: 3657
		[SerializeField]
		[Tooltip("The amount of frames that input can commit to the next shot before cooldown is finished.")]
		private uint commitToShotFrames = 5U;

		// Token: 0x04000E4A RID: 3658
		[SerializeField]
		private float rotationSpeedBeforeCameraLocked = 600f;

		// Token: 0x04000E4B RID: 3659
		[SerializeField]
		[Tooltip("Use -1 for infinite ammo.")]
		private int ammoCount = -1;

		// Token: 0x04000E4C RID: 3660
		[SerializeField]
		[Tooltip("Should this weapon automatically reload when fired with no ammo?")]
		private bool autoReload = true;

		// Token: 0x04000E4D RID: 3661
		[SerializeField]
		[Tooltip("Should this weapon automatically reload immediately after firing last ammo?")]
		private bool instantAutoReloadAfterLastShot;

		// Token: 0x04000E4E RID: 3662
		[SerializeField]
		[Tooltip("Time to reload")]
		private float reloadTime = 2f;

		// Token: 0x04000E4F RID: 3663
		[SerializeField]
		[Tooltip("Index of reload anim")]
		private int reloadAnimIndex;

		// Token: 0x04000E50 RID: 3664
		[SerializeField]
		[Tooltip("Disabled for slow reloading Heavy weapons")]
		private bool allowReloadWhenFullAmmo = true;

		// Token: 0x04000E51 RID: 3665
		[SerializeField]
		[Tooltip("Allow aiming and shooting while in the air? This controls the camera, aim animation, character rotation, and actual shooting")]
		private RangedAttackUsableDefinition.AirBehaviorOverride airBehaviorOverride;

		// Token: 0x04000E52 RID: 3666
		[Header("Recoil")]
		[SerializeField]
		[Tooltip("Bullet spread is multiplied by weapon strength")]
		private float weaponStrength = 1f;

		// Token: 0x04000E53 RID: 3667
		[SerializeField]
		[Tooltip("Bullet spread is divided by accuracy")]
		private float weaponAccuracy = 1f;

		// Token: 0x04000E54 RID: 3668
		[SerializeField]
		[Tooltip("How much shot angle varies with successive shots")]
		private float recoilAmountDegrees = 2f;

		// Token: 0x04000E55 RID: 3669
		[SerializeField]
		[Tooltip("Maximum shot angle can vary")]
		private float maxRecoilDegrees = 10f;

		// Token: 0x04000E56 RID: 3670
		[SerializeField]
		[Tooltip("Recoil settle in deg/sec (i.e. 10/5 = 2s to reset)")]
		private float recoilSettleAmountDegrees = 5f;

		// Token: 0x04000E57 RID: 3671
		[SerializeField]
		[Tooltip("Time in s after firing, before recoil starts to settle")]
		private float recoilSettleDelay = 0.05f;

		// Token: 0x04000E58 RID: 3672
		[SerializeField]
		[Tooltip("Settle curve is sampled inversely to the recoil amount and with each new shot,\n        so t=0 is the recoil at the time of the shot and t=1 at the reset moment.")]
		private AnimationCurve recoilSettleCurve = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 0f),
			new Keyframe(1f, 1f)
		});

		// Token: 0x04000E59 RID: 3673
		[SerializeField]
		[Tooltip("Aim penalty in degrees applied while moving/jumping/falling.")]
		private float movementAimPenaltyDegrees = 2f;

		// Token: 0x04000E5A RID: 3674
		[SerializeField]
		[Tooltip("Camera climb per shot in degrees")]
		private float cameraClimbAmountDegrees = 2f;

		// Token: 0x04000E5B RID: 3675
		[SerializeField]
		[Tooltip("Max camera climb in degrees")]
		private float maxCameraClimbDegrees = 14f;

		// Token: 0x04000E5C RID: 3676
		[SerializeField]
		[Tooltip("Camera climb reset speed in degrees")]
		private float cameraClimbSettleAmountDegrees = 7f;

		// Token: 0x04000E5D RID: 3677
		[Header("Full ADS")]
		[SerializeField]
		private float adsMovementMultiplier = 0.4f;

		// Token: 0x04000E5E RID: 3678
		[SerializeField]
		private float adsLockedRotationSpeed = 1200f;

		// Token: 0x04000E5F RID: 3679
		[Header("Soft ADS")]
		[SerializeField]
		private float softADSMovementMultiplier = 0.6f;

		// Token: 0x04000E60 RID: 3680
		[SerializeField]
		private float softADSRotationSpeed = 600f;

		// Token: 0x04000E61 RID: 3681
		[Header("Cooldown")]
		[SerializeField]
		private uint aimOnlyCooldown = 10U;

		// Token: 0x04000E62 RID: 3682
		[SerializeField]
		private uint quickShotCooldown = 20U;

		// Token: 0x04000E63 RID: 3683
		[SerializeField]
		private uint fullCooldown = 30U;

		// Token: 0x020002E0 RID: 736
		public enum AirBehaviorOverride
		{
			// Token: 0x04000E65 RID: 3685
			Default,
			// Token: 0x04000E66 RID: 3686
			NoShooting,
			// Token: 0x04000E67 RID: 3687
			AllowSoftADS
		}

		// Token: 0x020002E1 RID: 737
		public class RangedAttackEquipmentUseState : UsableDefinition.UseState
		{
		}
	}
}
