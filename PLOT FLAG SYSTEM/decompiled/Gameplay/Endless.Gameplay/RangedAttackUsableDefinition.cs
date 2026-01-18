using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class RangedAttackUsableDefinition : InventoryUsableDefinition
{
	public enum AirBehaviorOverride
	{
		Default,
		NoShooting,
		AllowSoftADS
	}

	public class RangedAttackEquipmentUseState : UseState
	{
	}

	private const int MIN_GROUNDED_FRAMES = 10;

	[Header("Shooting")]
	[SerializeField]
	private bool automatic;

	[SerializeField]
	private uint shotDelayFrames = 10u;

	[SerializeField]
	[Tooltip("The amount of frames that input can commit to the next shot before cooldown is finished.")]
	private uint commitToShotFrames = 5u;

	[SerializeField]
	private float rotationSpeedBeforeCameraLocked = 600f;

	[SerializeField]
	[Tooltip("Use -1 for infinite ammo.")]
	private int ammoCount = -1;

	[SerializeField]
	[Tooltip("Should this weapon automatically reload when fired with no ammo?")]
	private bool autoReload = true;

	[SerializeField]
	[Tooltip("Should this weapon automatically reload immediately after firing last ammo?")]
	private bool instantAutoReloadAfterLastShot;

	[SerializeField]
	[Tooltip("Time to reload")]
	private float reloadTime = 2f;

	[SerializeField]
	[Tooltip("Index of reload anim")]
	private int reloadAnimIndex;

	[SerializeField]
	[Tooltip("Disabled for slow reloading Heavy weapons")]
	private bool allowReloadWhenFullAmmo = true;

	[SerializeField]
	[Tooltip("Allow aiming and shooting while in the air? This controls the camera, aim animation, character rotation, and actual shooting")]
	private AirBehaviorOverride airBehaviorOverride;

	[Header("Recoil")]
	[SerializeField]
	[Tooltip("Bullet spread is multiplied by weapon strength")]
	private float weaponStrength = 1f;

	[SerializeField]
	[Tooltip("Bullet spread is divided by accuracy")]
	private float weaponAccuracy = 1f;

	[SerializeField]
	[Tooltip("How much shot angle varies with successive shots")]
	private float recoilAmountDegrees = 2f;

	[SerializeField]
	[Tooltip("Maximum shot angle can vary")]
	private float maxRecoilDegrees = 10f;

	[SerializeField]
	[Tooltip("Recoil settle in deg/sec (i.e. 10/5 = 2s to reset)")]
	private float recoilSettleAmountDegrees = 5f;

	[SerializeField]
	[Tooltip("Time in s after firing, before recoil starts to settle")]
	private float recoilSettleDelay = 0.05f;

	[SerializeField]
	[Tooltip("Settle curve is sampled inversely to the recoil amount and with each new shot,\n        so t=0 is the recoil at the time of the shot and t=1 at the reset moment.")]
	private AnimationCurve recoilSettleCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	[SerializeField]
	[Tooltip("Aim penalty in degrees applied while moving/jumping/falling.")]
	private float movementAimPenaltyDegrees = 2f;

	[SerializeField]
	[Tooltip("Camera climb per shot in degrees")]
	private float cameraClimbAmountDegrees = 2f;

	[SerializeField]
	[Tooltip("Max camera climb in degrees")]
	private float maxCameraClimbDegrees = 14f;

	[SerializeField]
	[Tooltip("Camera climb reset speed in degrees")]
	private float cameraClimbSettleAmountDegrees = 7f;

	[Header("Full ADS")]
	[SerializeField]
	private float adsMovementMultiplier = 0.4f;

	[SerializeField]
	private float adsLockedRotationSpeed = 1200f;

	[Header("Soft ADS")]
	[SerializeField]
	private float softADSMovementMultiplier = 0.6f;

	[SerializeField]
	private float softADSRotationSpeed = 600f;

	[Header("Cooldown")]
	[SerializeField]
	private uint aimOnlyCooldown = 10u;

	[SerializeField]
	private uint quickShotCooldown = 20u;

	[SerializeField]
	private uint fullCooldown = 30u;

	public int AmmoCount => ammoCount;

	public float ReloadTime => reloadTime;

	public int ReloadAnimIndex => reloadAnimIndex;

	public bool CanShootInAir => airBehaviorOverride != AirBehaviorOverride.NoShooting;

	public float WeaponStrength => weaponStrength;

	public float WeaponAccuracy => weaponAccuracy;

	public float RecoilAmount => recoilAmountDegrees;

	public float MaxRecoil => maxRecoilDegrees;

	public float RecoilSettleAmount => recoilSettleAmountDegrees;

	public AnimationCurve RecoilSettleCurve => recoilSettleCurve;

	public float RecoilSettleDelay => recoilSettleDelay;

	public float MovementAimPenalty => movementAimPenaltyDegrees;

	public float CameraClimbAmount => cameraClimbAmountDegrees;

	public float MaxCameraClimb => maxCameraClimbDegrees;

	public float CameraClimbSettleAmount => cameraClimbSettleAmountDegrees;

	public bool AllowReloadWhenFullAmmo => allowReloadWhenFullAmmo;

	public override UseState ProcessUseStart(ref NetState state, NetInput input, SerializableGuid guid, PlayerReferenceManager playerReference, Item item)
	{
		RangedAttackEquipmentUseState result = UseState.GetStateFromPool(guid, item) as RangedAttackEquipmentUseState;
		RangedWeaponItem rangedWeaponItem = item as RangedWeaponItem;
		ShootingState state2 = rangedWeaponItem.GetShootingState(input.NetFrame);
		if (input.NetFrame > state2.NetFrame)
		{
			state2.Clear();
			state2.NetFrame = input.NetFrame - 1;
			rangedWeaponItem.ShootingStateUpdated(input.NetFrame, ref state2);
		}
		return result;
	}

	public override bool ProcessUseFrame(ref NetState state, NetInput input, ref UseState equipmentUseState, PlayerReferenceManager playerReference, bool equipped, bool pressed)
	{
		if (state.IsStunned)
		{
			return false;
		}
		RangedWeaponItem rangedWeaponItem = equipmentUseState.Item as RangedWeaponItem;
		ProjectileShooter projectileShooter = rangedWeaponItem?.ProjectileShooter;
		if (rangedWeaponItem == null || projectileShooter == null)
		{
			return false;
		}
		bool flag = input.NetFrame == NetClock.CurrentFrame;
		bool flag2 = !state.Grounded;
		bool flag3 = rangedWeaponItem.ReloadRequested || rangedWeaponItem.ReloadStarted;
		bool flag4 = state.AnyEquipmentSwapActive || playerReference.ApperanceController.AppearanceAnimator.IsInHotswapState;
		ShootingState shootingState = rangedWeaponItem.GetShootingState(input.NetFrame - 1);
		uint num = 0u;
		state.BlockRotation = true;
		state.Strafing = true;
		if (equipped)
		{
			state.BlockItemInput = true;
		}
		bool flag5 = true;
		if (flag2 && !CanShootInAir)
		{
			flag5 = false;
		}
		if (input.PressingPrimary2 && (pressed || flag4))
		{
			CameraController.CameraType aimState = ((!flag2) ? CameraController.CameraType.FullADS : (airBehaviorOverride switch
			{
				AirBehaviorOverride.Default => CameraController.CameraType.FullADS, 
				AirBehaviorOverride.NoShooting => CameraController.CameraType.Normal, 
				AirBehaviorOverride.AllowSoftADS => CameraController.CameraType.SoftADS, 
				_ => CameraController.CameraType.FullADS, 
			}));
			shootingState.aimState = aimState;
			if (aimOnlyCooldown > num)
			{
				num = aimOnlyCooldown;
			}
		}
		else
		{
			CameraController.CameraType aimState = ((!flag2) ? CameraController.CameraType.SoftADS : (airBehaviorOverride switch
			{
				AirBehaviorOverride.Default => CameraController.CameraType.SoftADS, 
				AirBehaviorOverride.NoShooting => CameraController.CameraType.Normal, 
				_ => CameraController.CameraType.SoftADS, 
			}));
			shootingState.aimState = aimState;
		}
		if ((flag4 || (pressed && input.PressingPrimary1)) && aimOnlyCooldown > num)
		{
			num = aimOnlyCooldown;
		}
		float speed = ((shootingState.aimState == CameraController.CameraType.FullADS) ? adsLockedRotationSpeed : softADSRotationSpeed);
		if (!shootingState.cameraLocked)
		{
			speed = rotationSpeedBeforeCameraLocked;
		}
		bool flag6 = false;
		if (flag5)
		{
			flag6 = PlayerController.Rotate(ref state, MonoBehaviourSingleton<CameraController>.Instance.AimYaw, speed);
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
			if (pressed && input.PressingPrimary1 && (automatic || shootingState.shotReleased))
			{
				if (!flag6 && rangedWeaponItem.HasAmmo && !flag3)
				{
					shootingState.waitingForShot = true;
				}
				if (!shootingState.commitToShot && input.NetFrame - shootingState.lastShotFrame > (int)(shotDelayFrames - commitToShotFrames))
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
			uint num2 = ((shootingState.aimState == CameraController.CameraType.FullADS) ? fullCooldown : quickShotCooldown);
			if (num2 > num)
			{
				num = num2;
			}
		}
		if (equipped && flag5 && shootingState.commitToShot && flag6 && input.NetFrame - shootingState.lastShotFrame > shotDelayFrames)
		{
			uint num3 = ((shootingState.aimState == CameraController.CameraType.FullADS) ? fullCooldown : quickShotCooldown);
			if (num3 > num)
			{
				num = num3;
			}
			if (playerReference.IsOwner && !flag3)
			{
				if (rangedWeaponItem.HasAmmo)
				{
					if (flag)
					{
						float currentRecoilAccumulation = rangedWeaponItem.CurrentRecoilAccumulation;
						rangedWeaponItem.FireShot();
						Vector3 forward = projectileShooter.FirePoint.forward;
						Vector3 normalized = (MonoBehaviourSingleton<CameraController>.Instance.LastAimPosition - projectileShooter.FirePoint.position).normalized;
						Vector3 eulerAngles = Quaternion.LookRotation(normalized, Vector3.up).eulerAngles;
						float num4 = Vector3.Angle(forward, normalized);
						if (num4 > 10f)
						{
							Debug.LogWarning($"Angle between muzzle and aim is large: {num4}!");
						}
						Vector3 vector = eulerAngles + new Vector3(Random.Range(0f - currentRecoilAccumulation, currentRecoilAccumulation), Random.Range(0f - currentRecoilAccumulation, currentRecoilAccumulation), 0f);
						Vector3 position = projectileShooter.FirePoint.position + Quaternion.Euler(vector) * Vector3.forward * 0.1f;
						projectileShooter.UpdateBaseTeam(Team.Friendly);
						projectileShooter.ClientAuthoritativeShoot(position, vector, input.NetFrame, null, playerReference.WorldObject);
						if (instantAutoReloadAfterLastShot && !rangedWeaponItem.HasAmmo)
						{
							rangedWeaponItem.StartReload();
						}
					}
				}
				else if (autoReload)
				{
					rangedWeaponItem.StartReload();
				}
			}
			shootingState.commitToShot = false;
			shootingState.waitingForShot = false;
			shootingState.lastShotFrame = input.NetFrame;
		}
		if (num != 0)
		{
			ApplyFinishedFrame(ref shootingState, input.NetFrame + num);
		}
		state.XZInputMultiplierThisFrame = ((shootingState.aimState == CameraController.CameraType.FullADS) ? adsMovementMultiplier : softADSMovementMultiplier);
		state.AimMovementScaleMultiplier = ((shootingState.aimState == CameraController.CameraType.FullADS) ? (adsMovementMultiplier / softADSMovementMultiplier) : 1f);
		state.AimState = shootingState.aimState;
		if (flag)
		{
			shootingState.NetFrame = input.NetFrame;
			rangedWeaponItem.ShootingStateUpdated(input.NetFrame, ref shootingState);
			if (num != 0 && !NetworkManager.Singleton.IsServer)
			{
				SendFinishedFrame(rangedWeaponItem, input.NetFrame + num);
			}
		}
		if (!pressed)
		{
			uint num5 = ((NetworkManager.Singleton.IsServer && !playerReference.IsOwner) ? rangedWeaponItem.ServerFinishFrame : shootingState.finishedFrame);
			if (input.NetFrame > num5)
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

	private void SendFinishedFrame(RangedWeaponItem weapon, uint frame)
	{
		weapon.UpdateFinishFrame_ServerRpc(frame);
	}

	private void ApplyFinishedFrame(ref ShootingState shootingState, uint frame)
	{
		shootingState.finishedFrame = ((shootingState.finishedFrame < frame) ? frame : shootingState.finishedFrame);
	}

	public override string GetAnimationTrigger(UseState eus, uint currentVisualFrame)
	{
		return string.Empty;
	}

	public override void GetEventData(NetState state, UseState eus, double appearanceTime, ref EventData data)
	{
		RangedAttackEquipmentUseState rangedAttackEquipmentUseState = (RangedAttackEquipmentUseState)eus;
		CameraController.CameraType cameraType = CameraController.CameraType.Normal;
		if (rangedAttackEquipmentUseState != null && rangedAttackEquipmentUseState.Item is RangedWeaponItem rangedWeaponItem)
		{
			cameraType = rangedWeaponItem.GetShootingState(state.NetFrame).aimState;
		}
		data.Reset();
		data.InUse = rangedAttackEquipmentUseState != null && cameraType != CameraController.CameraType.Normal;
	}

	public override UseState GetNewUseState()
	{
		return new RangedAttackEquipmentUseState();
	}
}
