using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002DB RID: 731
	public class MeleeAttackUsableDefinition : InventoryUsableDefinition
	{
		// Token: 0x0600108B RID: 4235 RVA: 0x0005382C File Offset: 0x00051A2C
		public override UsableDefinition.UseState ProcessUseStart(ref NetState state, NetInput input, SerializableGuid guid, PlayerReferenceManager playerReference, Item item)
		{
			if (state.Grounded)
			{
				MeleeAttackUsableDefinition.MeleeAttackEquipmentUseState meleeAttackEquipmentUseState = UsableDefinition.UseState.GetStateFromPool(guid, item) as MeleeAttackUsableDefinition.MeleeAttackEquipmentUseState;
				MeleeAttackUsableDefinition.TrackingInfo trackingInfo = this.CalculateTracking(state, playerReference, this.Attacks[0]);
				meleeAttackEquipmentUseState.ComboStartFrame = input.NetFrame;
				meleeAttackEquipmentUseState.AttackFrame = 0U;
				meleeAttackEquipmentUseState.ComboIndex = 0;
				meleeAttackEquipmentUseState.ComboBuffered = false;
				meleeAttackEquipmentUseState.Released = false;
				meleeAttackEquipmentUseState.AttackAngle = trackingInfo.rotationResult;
				meleeAttackEquipmentUseState.EquipmentAnimID = this.AnimationItemID;
				meleeAttackEquipmentUseState.HitList_Server = new List<HittableComponent>();
				MeleeAttackUsableDefinition.HitList_Client.Clear();
				meleeAttackEquipmentUseState.DashFrames = trackingInfo.frames;
				meleeAttackEquipmentUseState.AttackActive = true;
				NpcEntity npcEntity;
				meleeAttackEquipmentUseState.UseAttackForce = trackingInfo.trackedTarget == null || trackingInfo.trackedTarget.WorldObject.TryGetUserComponent<NpcEntity>(out npcEntity);
				return meleeAttackEquipmentUseState;
			}
			return null;
		}

		// Token: 0x0600108C RID: 4236 RVA: 0x00053900 File Offset: 0x00051B00
		public override bool ProcessUseFrame(ref NetState state, NetInput input, ref UsableDefinition.UseState equipmentUseState, PlayerReferenceManager playerReference, bool equipped, bool pressed)
		{
			MeleeAttackUsableDefinition.MeleeAttackEquipmentUseState meleeAttackEquipmentUseState = equipmentUseState as MeleeAttackUsableDefinition.MeleeAttackEquipmentUseState;
			if (state.PrimarySwapActive)
			{
				meleeAttackEquipmentUseState.ComboBuffered = false;
			}
			if (meleeAttackEquipmentUseState.ComboIndex >= this.Attacks.Count)
			{
				return false;
			}
			if ((ulong)meleeAttackEquipmentUseState.AttackFrame >= (ulong)((long)(this.Attacks[meleeAttackEquipmentUseState.ComboIndex].TotalAttackFrameCount + meleeAttackEquipmentUseState.DashFrames)))
			{
				if (meleeAttackEquipmentUseState.ComboIndex + 1 >= this.Attacks.Count)
				{
					return false;
				}
				if ((pressed || meleeAttackEquipmentUseState.ComboBuffered) && state.Grounded)
				{
					meleeAttackEquipmentUseState.AttackFrame = 0U;
					meleeAttackEquipmentUseState.ComboIndex++;
					meleeAttackEquipmentUseState.ComboBuffered = false;
					meleeAttackEquipmentUseState.HitList_Server.Clear();
					MeleeAttackUsableDefinition.HitList_Client.Clear();
					MeleeAttackUsableDefinition.TrackingInfo trackingInfo = this.CalculateTracking(state, playerReference, this.Attacks[meleeAttackEquipmentUseState.ComboIndex]);
					meleeAttackEquipmentUseState.AttackAngle = trackingInfo.rotationResult;
					meleeAttackEquipmentUseState.DashFrames = trackingInfo.frames;
					meleeAttackEquipmentUseState.AttackActive = true;
				}
				else if ((ulong)meleeAttackEquipmentUseState.AttackFrame >= (ulong)((long)this.Attacks[meleeAttackEquipmentUseState.ComboIndex].TotalAttackFrameCount + (long)((ulong)this.ComboExpireFrames) + (long)meleeAttackEquipmentUseState.DashFrames))
				{
					return false;
				}
			}
			meleeAttackEquipmentUseState.AttackFrame += 1U;
			if (meleeAttackEquipmentUseState.AttackActive)
			{
				state.BlockSecondaryEquipmentInput = true;
			}
			if ((ulong)meleeAttackEquipmentUseState.AttackFrame - (ulong)((long)meleeAttackEquipmentUseState.DashFrames) < (ulong)((long)this.Attacks[meleeAttackEquipmentUseState.ComboIndex].TotalAttackFrameCount))
			{
				state.BlockRotation = true;
				state.BlockMotionXZ = true;
				state.BlockSecondaryEquipmentInput = true;
				if (PlayerController.Rotate(ref state, meleeAttackEquipmentUseState.AttackAngle, this.rotationSpeed, false))
				{
					uint num = (uint)((ulong)meleeAttackEquipmentUseState.AttackFrame - (ulong)((long)meleeAttackEquipmentUseState.DashFrames));
					if ((long)(this.Attacks[meleeAttackEquipmentUseState.ComboIndex].AttackAnimationFrameCount + meleeAttackEquipmentUseState.DashFrames) > (long)((ulong)meleeAttackEquipmentUseState.AttackFrame))
					{
						if ((long)meleeAttackEquipmentUseState.DashFrames < (long)((ulong)meleeAttackEquipmentUseState.AttackFrame))
						{
							MeleeAttackData runtimeInstance = MeleeAttackDataPool.GetRuntimeInstance(this.Attacks[meleeAttackEquipmentUseState.ComboIndex]);
							if (runtimeInstance != null)
							{
								foreach (HittableComponent hittableComponent in runtimeInstance.CheckCollisions(num, state.Position, state.CharacterRotation))
								{
									if (playerReference.Team.Damages(hittableComponent.Team))
									{
										if (!NetworkManager.Singleton.IsServer)
										{
											if (MeleeAttackUsableDefinition.HitList_Client.Contains(hittableComponent))
											{
												continue;
											}
											MeleeAttackUsableDefinition.HitList_Client.Add(hittableComponent);
										}
										else
										{
											if (meleeAttackEquipmentUseState.HitList_Server.Contains(hittableComponent))
											{
												continue;
											}
											meleeAttackEquipmentUseState.HitList_Server.Add(hittableComponent);
										}
										hittableComponent.ModifyHealth(new HealthModificationArgs(-runtimeInstance.HitDamage, playerReference.WorldObject.Context, DamageType.Normal, HealthChangeType.Damage));
										if (NetworkManager.Singleton.IsServer)
										{
											MeleeWeaponItem meleeWeaponItem = (MeleeWeaponItem)meleeAttackEquipmentUseState.Item;
											if (meleeWeaponItem)
											{
												meleeWeaponItem.Hit(hittableComponent);
											}
										}
										NpcEntity npcEntity;
										if (hittableComponent.WorldObject.TryGetUserComponent<NpcEntity>(out npcEntity))
										{
											Knockback knockback = new Knockback
											{
												StartFrame = state.NetFrame,
												Duration = this.Attacks[meleeAttackEquipmentUseState.ComboIndex].KnockbackFrames,
												Force = this.Attacks[meleeAttackEquipmentUseState.ComboIndex].KnockbackForce,
												Angle = PlayerController.DirectionToAngle(hittableComponent.transform.position - playerReference.transform.position)
											};
											npcEntity.Hit(knockback);
										}
									}
								}
							}
						}
						float num3;
						if ((ulong)meleeAttackEquipmentUseState.AttackFrame > (ulong)((long)meleeAttackEquipmentUseState.DashFrames))
						{
							float num2 = num / (float)this.Attacks[meleeAttackEquipmentUseState.ComboIndex].AttackAnimationFrameCount;
							num3 = this.Attacks[meleeAttackEquipmentUseState.ComboIndex].AttackMovementForceCurve.Evaluate(num2) * (float)this.Attacks[meleeAttackEquipmentUseState.ComboIndex].AttackMovementForce;
							if (!meleeAttackEquipmentUseState.UseAttackForce)
							{
								num3 *= 0.1f;
							}
						}
						else
						{
							num3 = this.Attacks[meleeAttackEquipmentUseState.ComboIndex].TrackingSpeed;
						}
						state.CalculatedMotion = PlayerController.AngleToForwardMotion(meleeAttackEquipmentUseState.AttackAngle) * num3;
					}
				}
			}
			if (meleeAttackEquipmentUseState.Released && pressed && (ulong)meleeAttackEquipmentUseState.AttackFrame - (ulong)((long)meleeAttackEquipmentUseState.DashFrames) > (ulong)((long)this.Attacks[meleeAttackEquipmentUseState.ComboIndex].TotalAttackFrameCount - (long)((ulong)this.ComboBufferFrames)))
			{
				meleeAttackEquipmentUseState.ComboBuffered = true;
			}
			else if (!pressed)
			{
				meleeAttackEquipmentUseState.Released = true;
			}
			if ((ulong)meleeAttackEquipmentUseState.AttackFrame < (ulong)((long)(this.Attacks[meleeAttackEquipmentUseState.ComboIndex].TotalAttackFrameCount + meleeAttackEquipmentUseState.DashFrames)))
			{
				state.BlockJump = true;
				state.BlockItemInput = true;
			}
			return true;
		}

		// Token: 0x0600108D RID: 4237 RVA: 0x00053DEC File Offset: 0x00051FEC
		private MeleeAttackUsableDefinition.TrackingInfo CalculateTracking(NetState state, PlayerReferenceManager playerReference, MeleeAttackData attackData)
		{
			int num = Physics.OverlapSphereNonAlloc(state.Position, attackData.MaxTrackingDistance, MeleeAttackUsableDefinition.hitsCache, 1 << LayerMask.NameToLayer("HittableColliders"), QueryTriggerInteraction.Ignore);
			float num2 = (state.UseInputRotation ? state.InputRotation : state.CharacterRotation);
			HittableComponent hittableComponent = null;
			float num3 = 0f;
			float num4 = 0f;
			MeleeAttackUsableDefinition.TrackingInfo trackingInfo = default(MeleeAttackUsableDefinition.TrackingInfo);
			for (int i = 0; i < num; i++)
			{
				HittableComponent hittableFromMap = MonoBehaviourSingleton<HittableMap>.Instance.GetHittableFromMap(MeleeAttackUsableDefinition.hitsCache[i]);
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
			trackingInfo.trackedTarget = hittableComponent;
			if (hittableComponent != null)
			{
				trackingInfo.rotationResult = ((hittableComponent != null) ? num4 : num2);
				trackingInfo.frames = this.GetTrackingFrameCount(Vector3.Distance(playerReference.transform.position, hittableComponent.transform.position) - attackData.TargetDistance, attackData.TrackingSpeed);
			}
			else
			{
				trackingInfo.rotationResult = num2;
			}
			return trackingInfo;
		}

		// Token: 0x0600108E RID: 4238 RVA: 0x00053F96 File Offset: 0x00052196
		private int GetTrackingFrameCount(float distance, float trackingSpeed)
		{
			return Mathf.RoundToInt(distance / (trackingSpeed * NetClock.FixedDeltaTime));
		}

		// Token: 0x0600108F RID: 4239 RVA: 0x00053FA6 File Offset: 0x000521A6
		public MeleeVfxPlayer GetVisualEffect(int comboIndex)
		{
			return this.Attacks[comboIndex].MeleeVFXPrefab;
		}

		// Token: 0x06001090 RID: 4240 RVA: 0x00053FB9 File Offset: 0x000521B9
		public float GetVisualEffectsDelay(int comboIndex)
		{
			return this.Attacks[comboIndex].VFXdelay;
		}

		// Token: 0x06001091 RID: 4241 RVA: 0x00053FCC File Offset: 0x000521CC
		public override string GetAnimationTrigger(UsableDefinition.UseState eus, uint currentVisualFrame)
		{
			MeleeAttackUsableDefinition.MeleeAttackEquipmentUseState meleeAttackEquipmentUseState = eus as MeleeAttackUsableDefinition.MeleeAttackEquipmentUseState;
			if (this.Attacks.Count > meleeAttackEquipmentUseState.ComboIndex && (ulong)meleeAttackEquipmentUseState.AttackFrame > (ulong)((long)meleeAttackEquipmentUseState.DashFrames))
			{
				return "Attack";
			}
			return string.Empty;
		}

		// Token: 0x06001092 RID: 4242 RVA: 0x00054010 File Offset: 0x00052210
		public override void GetEventData(NetState state, UsableDefinition.UseState eus, double appearanceTime, ref InventoryUsableDefinition.EventData data)
		{
			base.GetEventData(state, eus, appearanceTime, ref data);
			if (eus != null)
			{
				MeleeAttackUsableDefinition.MeleeAttackEquipmentUseState meleeAttackEquipmentUseState = (MeleeAttackUsableDefinition.MeleeAttackEquipmentUseState)eus;
				data.Available = true;
				data.InUse = true;
				data.UseFrame = NetClock.CurrentFrame - meleeAttackEquipmentUseState.AttackFrame;
			}
		}

		// Token: 0x06001093 RID: 4243 RVA: 0x00054055 File Offset: 0x00052255
		public override UsableDefinition.UseState GetNewUseState()
		{
			return new MeleeAttackUsableDefinition.MeleeAttackEquipmentUseState();
		}

		// Token: 0x06001094 RID: 4244 RVA: 0x0005405C File Offset: 0x0005225C
		public override uint GetItemSwapDelayFrames(UsableDefinition.UseState equipmentUseState)
		{
			MeleeAttackUsableDefinition.MeleeAttackEquipmentUseState meleeAttackEquipmentUseState = equipmentUseState as MeleeAttackUsableDefinition.MeleeAttackEquipmentUseState;
			if (!meleeAttackEquipmentUseState.AttackActive)
			{
				return 0U;
			}
			int num = this.Attacks[meleeAttackEquipmentUseState.ComboIndex].TotalAttackFrameCount + meleeAttackEquipmentUseState.DashFrames - (int)meleeAttackEquipmentUseState.AttackFrame;
			if (num <= 0)
			{
				return 0U;
			}
			return (uint)num;
		}

		// Token: 0x04000E29 RID: 3625
		[Header("Appearance")]
		[SerializeField]
		private int AnimationItemID = 1;

		// Token: 0x04000E2A RID: 3626
		[Header("Rotation")]
		[SerializeField]
		private float rotationSpeed = 800f;

		// Token: 0x04000E2B RID: 3627
		[Header("Combo")]
		[SerializeField]
		private uint ComboBufferFrames = 8U;

		// Token: 0x04000E2C RID: 3628
		[SerializeField]
		private uint ComboExpireFrames = 6U;

		// Token: 0x04000E2D RID: 3629
		[Header("Attack")]
		[SerializeField]
		private List<MeleeAttackData> Attacks = new List<MeleeAttackData>();

		// Token: 0x04000E2E RID: 3630
		protected static Collider[] hitsCache = new Collider[20];

		// Token: 0x04000E2F RID: 3631
		private static List<HittableComponent> HitList_Client = new List<HittableComponent>();

		// Token: 0x020002DC RID: 732
		public struct TrackingInfo
		{
			// Token: 0x04000E30 RID: 3632
			public HittableComponent trackedTarget;

			// Token: 0x04000E31 RID: 3633
			public float rotationResult;

			// Token: 0x04000E32 RID: 3634
			public int frames;
		}

		// Token: 0x020002DD RID: 733
		public class MeleeAttackEquipmentUseState : UsableDefinition.UseState
		{
			// Token: 0x06001097 RID: 4247 RVA: 0x000540F4 File Offset: 0x000522F4
			public override void Serialize<T>(BufferSerializer<T> serializer)
			{
				base.Serialize<T>(serializer);
				if (serializer.IsWriter)
				{
					Compression.SerializeUInt<T>(serializer, this.ComboStartFrame);
					Compression.SerializeIntToByteClamped<T>(serializer, this.EquipmentAnimID);
					Compression.SerializeIntToByteClamped<T>(serializer, this.ComboIndex);
					Compression.SerializeIntToByteClamped<T>(serializer, this.DashFrames);
					Compression.SerializeUIntToByteClamped<T>(serializer, this.AttackFrame);
					Compression.SerializeBoolsToByte<T>(serializer, this.ComboBuffered, this.Released, this.AttackActive, this.UseAttackForce, false, false, false, false);
					serializer.SerializeValue<float>(ref this.AttackAngle, default(FastBufferWriter.ForPrimitives));
					return;
				}
				this.ComboStartFrame = Compression.DeserializeUInt<T>(serializer);
				this.EquipmentAnimID = Compression.DeserializeIntFromByteClamped<T>(serializer);
				this.ComboIndex = Compression.DeserializeIntFromByteClamped<T>(serializer);
				this.DashFrames = Compression.DeserializeIntFromByteClamped<T>(serializer);
				this.AttackFrame = Compression.DeserializeUIntFromByteClamped<T>(serializer);
				Compression.DeserializeBoolsFromByte<T>(serializer, ref this.ComboBuffered, ref this.Released, ref this.AttackActive, ref this.UseAttackForce);
				serializer.SerializeValue<float>(ref this.AttackAngle, default(FastBufferWriter.ForPrimitives));
			}

			// Token: 0x04000E33 RID: 3635
			public uint ComboStartFrame;

			// Token: 0x04000E34 RID: 3636
			public int EquipmentAnimID;

			// Token: 0x04000E35 RID: 3637
			public int ComboIndex;

			// Token: 0x04000E36 RID: 3638
			public int DashFrames;

			// Token: 0x04000E37 RID: 3639
			public uint AttackFrame;

			// Token: 0x04000E38 RID: 3640
			public bool ComboBuffered;

			// Token: 0x04000E39 RID: 3641
			public bool Released;

			// Token: 0x04000E3A RID: 3642
			public bool AttackActive;

			// Token: 0x04000E3B RID: 3643
			public float AttackAngle;

			// Token: 0x04000E3C RID: 3644
			public bool UseAttackForce;

			// Token: 0x04000E3D RID: 3645
			public List<HittableComponent> HitList_Server = new List<HittableComponent>();
		}
	}
}
