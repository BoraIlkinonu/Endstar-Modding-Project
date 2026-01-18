using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class MeleeAttackUsableDefinition : InventoryUsableDefinition
{
	public struct TrackingInfo
	{
		public HittableComponent trackedTarget;

		public float rotationResult;

		public int frames;
	}

	public class MeleeAttackEquipmentUseState : UseState
	{
		public uint ComboStartFrame;

		public int EquipmentAnimID;

		public int ComboIndex;

		public int DashFrames;

		public uint AttackFrame;

		public bool ComboBuffered;

		public bool Released;

		public bool AttackActive;

		public float AttackAngle;

		public bool UseAttackForce;

		public List<HittableComponent> HitList_Server = new List<HittableComponent>();

		public override void Serialize<T>(BufferSerializer<T> serializer)
		{
			base.Serialize(serializer);
			if (serializer.IsWriter)
			{
				Compression.SerializeUInt(serializer, ComboStartFrame);
				Compression.SerializeIntToByteClamped(serializer, EquipmentAnimID);
				Compression.SerializeIntToByteClamped(serializer, ComboIndex);
				Compression.SerializeIntToByteClamped(serializer, DashFrames);
				Compression.SerializeUIntToByteClamped(serializer, AttackFrame);
				Compression.SerializeBoolsToByte(serializer, ComboBuffered, Released, AttackActive, UseAttackForce);
				serializer.SerializeValue(ref AttackAngle, default(FastBufferWriter.ForPrimitives));
			}
			else
			{
				ComboStartFrame = Compression.DeserializeUInt(serializer);
				EquipmentAnimID = Compression.DeserializeIntFromByteClamped(serializer);
				ComboIndex = Compression.DeserializeIntFromByteClamped(serializer);
				DashFrames = Compression.DeserializeIntFromByteClamped(serializer);
				AttackFrame = Compression.DeserializeUIntFromByteClamped(serializer);
				Compression.DeserializeBoolsFromByte(serializer, ref ComboBuffered, ref Released, ref AttackActive, ref UseAttackForce);
				serializer.SerializeValue(ref AttackAngle, default(FastBufferWriter.ForPrimitives));
			}
		}
	}

	[Header("Appearance")]
	[SerializeField]
	private int AnimationItemID = 1;

	[Header("Rotation")]
	[SerializeField]
	private float rotationSpeed = 800f;

	[Header("Combo")]
	[SerializeField]
	private uint ComboBufferFrames = 8u;

	[SerializeField]
	private uint ComboExpireFrames = 6u;

	[Header("Attack")]
	[SerializeField]
	private List<MeleeAttackData> Attacks = new List<MeleeAttackData>();

	protected static Collider[] hitsCache = new Collider[20];

	private static List<HittableComponent> HitList_Client = new List<HittableComponent>();

	public override UseState ProcessUseStart(ref NetState state, NetInput input, SerializableGuid guid, PlayerReferenceManager playerReference, Item item)
	{
		if (state.Grounded)
		{
			MeleeAttackEquipmentUseState obj = UseState.GetStateFromPool(guid, item) as MeleeAttackEquipmentUseState;
			TrackingInfo trackingInfo = CalculateTracking(state, playerReference, Attacks[0]);
			obj.ComboStartFrame = input.NetFrame;
			obj.AttackFrame = 0u;
			obj.ComboIndex = 0;
			obj.ComboBuffered = false;
			obj.Released = false;
			obj.AttackAngle = trackingInfo.rotationResult;
			obj.EquipmentAnimID = AnimationItemID;
			obj.HitList_Server = new List<HittableComponent>();
			HitList_Client.Clear();
			obj.DashFrames = trackingInfo.frames;
			obj.AttackActive = true;
			obj.UseAttackForce = trackingInfo.trackedTarget == null || trackingInfo.trackedTarget.WorldObject.TryGetUserComponent<NpcEntity>(out var _);
			return obj;
		}
		return null;
	}

	public override bool ProcessUseFrame(ref NetState state, NetInput input, ref UseState equipmentUseState, PlayerReferenceManager playerReference, bool equipped, bool pressed)
	{
		MeleeAttackEquipmentUseState meleeAttackEquipmentUseState = equipmentUseState as MeleeAttackEquipmentUseState;
		if (state.PrimarySwapActive)
		{
			meleeAttackEquipmentUseState.ComboBuffered = false;
		}
		if (meleeAttackEquipmentUseState.ComboIndex >= Attacks.Count)
		{
			return false;
		}
		if (meleeAttackEquipmentUseState.AttackFrame >= Attacks[meleeAttackEquipmentUseState.ComboIndex].TotalAttackFrameCount + meleeAttackEquipmentUseState.DashFrames)
		{
			if (meleeAttackEquipmentUseState.ComboIndex + 1 >= Attacks.Count)
			{
				return false;
			}
			if ((pressed || meleeAttackEquipmentUseState.ComboBuffered) && state.Grounded)
			{
				meleeAttackEquipmentUseState.AttackFrame = 0u;
				meleeAttackEquipmentUseState.ComboIndex++;
				meleeAttackEquipmentUseState.ComboBuffered = false;
				meleeAttackEquipmentUseState.HitList_Server.Clear();
				HitList_Client.Clear();
				TrackingInfo trackingInfo = CalculateTracking(state, playerReference, Attacks[meleeAttackEquipmentUseState.ComboIndex]);
				meleeAttackEquipmentUseState.AttackAngle = trackingInfo.rotationResult;
				meleeAttackEquipmentUseState.DashFrames = trackingInfo.frames;
				meleeAttackEquipmentUseState.AttackActive = true;
			}
			else if (meleeAttackEquipmentUseState.AttackFrame >= Attacks[meleeAttackEquipmentUseState.ComboIndex].TotalAttackFrameCount + ComboExpireFrames + meleeAttackEquipmentUseState.DashFrames)
			{
				return false;
			}
		}
		meleeAttackEquipmentUseState.AttackFrame++;
		if (meleeAttackEquipmentUseState.AttackActive)
		{
			state.BlockSecondaryEquipmentInput = true;
		}
		if (meleeAttackEquipmentUseState.AttackFrame - meleeAttackEquipmentUseState.DashFrames < Attacks[meleeAttackEquipmentUseState.ComboIndex].TotalAttackFrameCount)
		{
			state.BlockRotation = true;
			state.BlockMotionXZ = true;
			state.BlockSecondaryEquipmentInput = true;
			if (PlayerController.Rotate(ref state, meleeAttackEquipmentUseState.AttackAngle, rotationSpeed))
			{
				uint num = (uint)(meleeAttackEquipmentUseState.AttackFrame - meleeAttackEquipmentUseState.DashFrames);
				if (Attacks[meleeAttackEquipmentUseState.ComboIndex].AttackAnimationFrameCount + meleeAttackEquipmentUseState.DashFrames > meleeAttackEquipmentUseState.AttackFrame)
				{
					if (meleeAttackEquipmentUseState.DashFrames < meleeAttackEquipmentUseState.AttackFrame)
					{
						MeleeAttackData runtimeInstance = MeleeAttackDataPool.GetRuntimeInstance(Attacks[meleeAttackEquipmentUseState.ComboIndex]);
						if (runtimeInstance != null)
						{
							foreach (HittableComponent item in runtimeInstance.CheckCollisions(num, state.Position, state.CharacterRotation))
							{
								if (!playerReference.Team.Damages(item.Team))
								{
									continue;
								}
								if (!NetworkManager.Singleton.IsServer)
								{
									if (HitList_Client.Contains(item))
									{
										continue;
									}
									HitList_Client.Add(item);
								}
								else
								{
									if (meleeAttackEquipmentUseState.HitList_Server.Contains(item))
									{
										continue;
									}
									meleeAttackEquipmentUseState.HitList_Server.Add(item);
								}
								item.ModifyHealth(new HealthModificationArgs(-runtimeInstance.HitDamage, playerReference.WorldObject.Context));
								if (NetworkManager.Singleton.IsServer)
								{
									MeleeWeaponItem meleeWeaponItem = (MeleeWeaponItem)meleeAttackEquipmentUseState.Item;
									if ((bool)meleeWeaponItem)
									{
										meleeWeaponItem.Hit(item);
									}
								}
								if (item.WorldObject.TryGetUserComponent<NpcEntity>(out var component))
								{
									Knockback knockback = new Knockback
									{
										StartFrame = state.NetFrame,
										Duration = Attacks[meleeAttackEquipmentUseState.ComboIndex].KnockbackFrames,
										Force = Attacks[meleeAttackEquipmentUseState.ComboIndex].KnockbackForce,
										Angle = PlayerController.DirectionToAngle(item.transform.position - playerReference.transform.position)
									};
									component.Hit(knockback);
								}
							}
						}
					}
					float num2;
					if (meleeAttackEquipmentUseState.AttackFrame > meleeAttackEquipmentUseState.DashFrames)
					{
						float time = (float)num / (float)Attacks[meleeAttackEquipmentUseState.ComboIndex].AttackAnimationFrameCount;
						num2 = Attacks[meleeAttackEquipmentUseState.ComboIndex].AttackMovementForceCurve.Evaluate(time) * (float)Attacks[meleeAttackEquipmentUseState.ComboIndex].AttackMovementForce;
						if (!meleeAttackEquipmentUseState.UseAttackForce)
						{
							num2 *= 0.1f;
						}
					}
					else
					{
						num2 = Attacks[meleeAttackEquipmentUseState.ComboIndex].TrackingSpeed;
					}
					state.CalculatedMotion = PlayerController.AngleToForwardMotion(meleeAttackEquipmentUseState.AttackAngle) * num2;
				}
			}
		}
		if (meleeAttackEquipmentUseState.Released && pressed && meleeAttackEquipmentUseState.AttackFrame - meleeAttackEquipmentUseState.DashFrames > Attacks[meleeAttackEquipmentUseState.ComboIndex].TotalAttackFrameCount - ComboBufferFrames)
		{
			meleeAttackEquipmentUseState.ComboBuffered = true;
		}
		else if (!pressed)
		{
			meleeAttackEquipmentUseState.Released = true;
		}
		if (meleeAttackEquipmentUseState.AttackFrame < Attacks[meleeAttackEquipmentUseState.ComboIndex].TotalAttackFrameCount + meleeAttackEquipmentUseState.DashFrames)
		{
			state.BlockJump = true;
			state.BlockItemInput = true;
		}
		return true;
	}

	private TrackingInfo CalculateTracking(NetState state, PlayerReferenceManager playerReference, MeleeAttackData attackData)
	{
		int num = Physics.OverlapSphereNonAlloc(state.Position, attackData.MaxTrackingDistance, hitsCache, 1 << LayerMask.NameToLayer("HittableColliders"), QueryTriggerInteraction.Ignore);
		float num2 = (state.UseInputRotation ? state.InputRotation : state.CharacterRotation);
		HittableComponent hittableComponent = null;
		float num3 = 0f;
		float num4 = 0f;
		TrackingInfo result = default(TrackingInfo);
		for (int i = 0; i < num; i++)
		{
			HittableComponent hittableFromMap = MonoBehaviourSingleton<HittableMap>.Instance.GetHittableFromMap(hitsCache[i]);
			if (!(hittableFromMap == null) && hittableFromMap.IsTargetable && playerReference.Team.Damages(hittableFromMap.Team))
			{
				Vector3 vector = hittableFromMap.transform.position - playerReference.transform.position;
				float num5 = Vector2.SignedAngle(new Vector2(vector.x, vector.z), Vector2.up);
				float num6 = Mathf.Abs(Mathf.DeltaAngle(num2, num5));
				float magnitude = vector.magnitude;
				if (num6 <= attackData.TrackingAngle && Mathf.Abs(vector.y) < attackData.TrackingHeight && (hittableComponent == null || magnitude < num3))
				{
					hittableComponent = hittableFromMap;
					num3 = magnitude;
					num4 = num5;
				}
			}
		}
		result.trackedTarget = hittableComponent;
		if (hittableComponent != null)
		{
			result.rotationResult = ((hittableComponent != null) ? num4 : num2);
			result.frames = GetTrackingFrameCount(Vector3.Distance(playerReference.transform.position, hittableComponent.transform.position) - attackData.TargetDistance, attackData.TrackingSpeed);
		}
		else
		{
			result.rotationResult = num2;
		}
		return result;
	}

	private int GetTrackingFrameCount(float distance, float trackingSpeed)
	{
		return Mathf.RoundToInt(distance / (trackingSpeed * NetClock.FixedDeltaTime));
	}

	public MeleeVfxPlayer GetVisualEffect(int comboIndex)
	{
		return Attacks[comboIndex].MeleeVFXPrefab;
	}

	public float GetVisualEffectsDelay(int comboIndex)
	{
		return Attacks[comboIndex].VFXdelay;
	}

	public override string GetAnimationTrigger(UseState eus, uint currentVisualFrame)
	{
		MeleeAttackEquipmentUseState meleeAttackEquipmentUseState = eus as MeleeAttackEquipmentUseState;
		if (Attacks.Count > meleeAttackEquipmentUseState.ComboIndex && meleeAttackEquipmentUseState.AttackFrame > meleeAttackEquipmentUseState.DashFrames)
		{
			return "Attack";
		}
		return string.Empty;
	}

	public override void GetEventData(NetState state, UseState eus, double appearanceTime, ref EventData data)
	{
		base.GetEventData(state, eus, appearanceTime, ref data);
		if (eus != null)
		{
			MeleeAttackEquipmentUseState meleeAttackEquipmentUseState = (MeleeAttackEquipmentUseState)eus;
			data.Available = true;
			data.InUse = true;
			data.UseFrame = NetClock.CurrentFrame - meleeAttackEquipmentUseState.AttackFrame;
		}
	}

	public override UseState GetNewUseState()
	{
		return new MeleeAttackEquipmentUseState();
	}

	public override uint GetItemSwapDelayFrames(UseState equipmentUseState)
	{
		MeleeAttackEquipmentUseState meleeAttackEquipmentUseState = equipmentUseState as MeleeAttackEquipmentUseState;
		if (meleeAttackEquipmentUseState.AttackActive)
		{
			int num = Attacks[meleeAttackEquipmentUseState.ComboIndex].TotalAttackFrameCount + meleeAttackEquipmentUseState.DashFrames - (int)meleeAttackEquipmentUseState.AttackFrame;
			if (num <= 0)
			{
				return 0u;
			}
			return (uint)num;
		}
		return 0u;
	}
}
