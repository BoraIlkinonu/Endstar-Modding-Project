using System;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002E2 RID: 738
	public class ThrownConsumableUsableDefinition : ConsumableUsableDefinition
	{
		// Token: 0x060010B6 RID: 4278 RVA: 0x00054AC4 File Offset: 0x00052CC4
		public override UsableDefinition.UseState ProcessUseStart(ref NetState state, NetInput input, SerializableGuid guid, PlayerReferenceManager playerReference, Item item)
		{
			ThrownConsumableUsableDefinition.ThrownConsumableEquipmentUseState thrownConsumableEquipmentUseState = UsableDefinition.UseState.GetStateFromPool(guid, item) as ThrownConsumableUsableDefinition.ThrownConsumableEquipmentUseState;
			thrownConsumableEquipmentUseState.HoldFrames = 0U;
			thrownConsumableEquipmentUseState.ThrowCooldownFrames = 0U;
			thrownConsumableEquipmentUseState.ADS = false;
			thrownConsumableEquipmentUseState.ThrownThisFrame = false;
			return thrownConsumableEquipmentUseState;
		}

		// Token: 0x060010B7 RID: 4279 RVA: 0x00054AF0 File Offset: 0x00052CF0
		public override bool ProcessUseFrame(ref NetState state, NetInput input, ref UsableDefinition.UseState equipmentUseState, PlayerReferenceManager playerReference, bool equipped, bool pressed)
		{
			if (state.IsStunned || !equipped)
			{
				return false;
			}
			ThrownConsumableUsableDefinition.ThrownConsumableEquipmentUseState thrownConsumableEquipmentUseState = equipmentUseState as ThrownConsumableUsableDefinition.ThrownConsumableEquipmentUseState;
			if (thrownConsumableEquipmentUseState.ThrownThisFrame && NetworkManager.Singleton.IsServer)
			{
				StackableItem stackableItem = (StackableItem)equipmentUseState.Item;
				if (stackableItem)
				{
					stackableItem.ChangeStackCount(-1);
				}
			}
			thrownConsumableEquipmentUseState.ThrownThisFrame = false;
			if (thrownConsumableEquipmentUseState.ThrowCooldownFrames > 0U)
			{
				thrownConsumableEquipmentUseState.ThrowCooldownFrames -= 1U;
			}
			if (pressed)
			{
				thrownConsumableEquipmentUseState.HoldFrames += 1U;
				thrownConsumableEquipmentUseState.ADS = thrownConsumableEquipmentUseState.ADS || thrownConsumableEquipmentUseState.HoldFrames >= this.adsStartFrame;
				state.AimState = (thrownConsumableEquipmentUseState.ADS ? CameraController.CameraType.FullADS : CameraController.CameraType.Normal);
				if (thrownConsumableEquipmentUseState.ADS)
				{
					PlayerController.Rotate(ref state, input.MotionRotation, this.adsRotationSpeed, false);
					state.BlockRotation = true;
				}
				state.BlockItemInput = true;
			}
			else if (thrownConsumableEquipmentUseState.HoldFrames > 0U && thrownConsumableEquipmentUseState.ThrowCooldownFrames == 0U)
			{
				state.AimState = (thrownConsumableEquipmentUseState.ADS ? CameraController.CameraType.FullADS : CameraController.CameraType.Normal);
				thrownConsumableEquipmentUseState.HoldFrames = 0U;
				thrownConsumableEquipmentUseState.ThrowCooldownFrames = this.throwCooldownFrames;
				thrownConsumableEquipmentUseState.ThrownThisFrame = true;
				if (NetworkManager.Singleton.IsServer)
				{
					ThrownConsumableUsableDefinition.ThrowInfo throwInfo = this.GetThrowInfo(state, input);
					NetworkRigidbodyController networkRigidbodyController = global::UnityEngine.Object.Instantiate<NetworkRigidbodyController>(this.thrownObjectPrefab, throwInfo.position, Quaternion.identity);
					networkRigidbodyController.NetworkObject.Spawn(false);
					IThrowable component = networkRigidbodyController.GetComponent<IThrowable>();
					if (component != null)
					{
						component.InitiateThrow(thrownConsumableEquipmentUseState.ADS ? (this.adsThrownForce * throwInfo.forceMultiplier) : this.quickThrownForce, throwInfo.direction, input.NetFrame, playerReference.NetworkObject, equipmentUseState.Item);
					}
				}
				thrownConsumableEquipmentUseState.ThrowCooldownFrames = this.throwCooldownFrames;
			}
			if (thrownConsumableEquipmentUseState.HoldFrames == 0U)
			{
				if (thrownConsumableEquipmentUseState.ADS && thrownConsumableEquipmentUseState.ThrowCooldownFrames < this.throwCooldownFrames - this.adsKeepFrames)
				{
					thrownConsumableEquipmentUseState.ADS = true;
					state.AimState = CameraController.CameraType.FullADS;
				}
				else
				{
					thrownConsumableEquipmentUseState.ADS = false;
					state.AimState = CameraController.CameraType.Normal;
				}
			}
			if (playerReference.IsOwner)
			{
				if (thrownConsumableEquipmentUseState.ADS && pressed)
				{
					MonoBehaviourSingleton<TrajectoryDrawer>.Instance.Enabled = true;
					ThrownConsumableUsableDefinition.ThrowInfo throwInfo2 = this.GetThrowInfo(state, input);
					Vector3 vector = throwInfo2.direction * this.adsThrownForce * throwInfo2.forceMultiplier;
					MonoBehaviourSingleton<TrajectoryDrawer>.Instance.SetTrajectoryDrawerInfo(this.thrownObjectPrefab.GetComponent<Rigidbody>(), throwInfo2.position, vector, thrownConsumableEquipmentUseState.ThrowCooldownFrames > 0U, state.NetFrame);
				}
				else
				{
					MonoBehaviourSingleton<TrajectoryDrawer>.Instance.Enabled = false;
				}
			}
			return pressed || thrownConsumableEquipmentUseState.ThrowCooldownFrames != 0U || thrownConsumableEquipmentUseState.HoldFrames != 0U;
		}

		// Token: 0x060010B8 RID: 4280 RVA: 0x00054D8C File Offset: 0x00052F8C
		private ThrownConsumableUsableDefinition.ThrowInfo GetThrowInfo(NetState state, NetInput input)
		{
			float num = input.AimPitch + this.adsThrowPitchShift;
			bool flag = state.AimState > CameraController.CameraType.Normal;
			Vector3 vector = Quaternion.Euler(flag ? new Vector3(num, input.AimYaw, 0f) : new Vector3(this.quickThrowPitchShift, input.MotionRotation, 0f)) * Vector3.forward;
			float num2 = 1f;
			if (flag)
			{
				float num3 = Mathf.Abs(num - -40f);
				num2 = this.adsThrownForceAngleCurve.Evaluate(num3);
			}
			Vector3 vector2 = new Vector3(0f, this.throwPositionVerticalOffset, 0f) + state.Position;
			return new ThrownConsumableUsableDefinition.ThrowInfo
			{
				position = vector2,
				direction = vector,
				forceMultiplier = num2
			};
		}

		// Token: 0x060010B9 RID: 4281 RVA: 0x00054E53 File Offset: 0x00053053
		public override void GetEventData(NetState state, UsableDefinition.UseState eus, double appearanceTime, ref InventoryUsableDefinition.EventData data)
		{
			base.GetEventData(state, eus, appearanceTime, ref data);
			if (eus != null)
			{
				ThrownConsumableUsableDefinition.ThrownConsumableEquipmentUseState thrownConsumableEquipmentUseState = (ThrownConsumableUsableDefinition.ThrownConsumableEquipmentUseState)eus;
				data.Available = false;
				data.InUse = true;
				return;
			}
			data.Available = true;
		}

		// Token: 0x060010BA RID: 4282 RVA: 0x00053092 File Offset: 0x00051292
		public override InventoryUsableDefinition.EquipmentShowPriority GetShowPriority(UsableDefinition.UseState eus)
		{
			if (eus != null)
			{
				return InventoryUsableDefinition.EquipmentShowPriority.MinorInUse;
			}
			return InventoryUsableDefinition.EquipmentShowPriority.MinorOutOfUse;
		}

		// Token: 0x060010BB RID: 4283 RVA: 0x00054E83 File Offset: 0x00053083
		public override UsableDefinition.UseState GetNewUseState()
		{
			return new ThrownConsumableUsableDefinition.ThrownConsumableEquipmentUseState();
		}

		// Token: 0x060010BC RID: 4284 RVA: 0x00054E8A File Offset: 0x0005308A
		public override string GetAnimationTrigger(UsableDefinition.UseState eus, uint currentVisualFrame)
		{
			if ((eus as ThrownConsumableUsableDefinition.ThrownConsumableEquipmentUseState).ThrownThisFrame)
			{
				return "UseTool";
			}
			return string.Empty;
		}

		// Token: 0x04000E68 RID: 3688
		[SerializeField]
		private NetworkRigidbodyController thrownObjectPrefab;

		// Token: 0x04000E69 RID: 3689
		[SerializeField]
		private uint throwCooldownFrames = 30U;

		// Token: 0x04000E6A RID: 3690
		[SerializeField]
		private uint adsRotationSpeed;

		// Token: 0x04000E6B RID: 3691
		[Header("Quick throw")]
		[SerializeField]
		private float quickThrownForce;

		// Token: 0x04000E6C RID: 3692
		[SerializeField]
		private float quickThrowPitchShift;

		// Token: 0x04000E6D RID: 3693
		[Header("ADS throw")]
		[SerializeField]
		private float adsThrownForce;

		// Token: 0x04000E6E RID: 3694
		[SerializeField]
		private float adsThrowPitchShift;

		// Token: 0x04000E6F RID: 3695
		[SerializeField]
		private AnimationCurve adsThrownForceAngleCurve;

		// Token: 0x04000E70 RID: 3696
		[SerializeField]
		private float throwPositionVerticalOffset;

		// Token: 0x04000E71 RID: 3697
		[SerializeField]
		private float throwPositionForwardOffset;

		// Token: 0x04000E72 RID: 3698
		[Header("ADS timing")]
		[SerializeField]
		private uint adsStartFrame = 6U;

		// Token: 0x04000E73 RID: 3699
		[SerializeField]
		private uint adsKeepFrames = 6U;

		// Token: 0x020002E3 RID: 739
		private struct ThrowInfo
		{
			// Token: 0x04000E74 RID: 3700
			public Vector3 position;

			// Token: 0x04000E75 RID: 3701
			public Vector3 direction;

			// Token: 0x04000E76 RID: 3702
			public float forceMultiplier;
		}

		// Token: 0x020002E4 RID: 740
		public class ThrownConsumableEquipmentUseState : UsableDefinition.UseState
		{
			// Token: 0x060010BE RID: 4286 RVA: 0x00054EC4 File Offset: 0x000530C4
			public override void Serialize<T>(BufferSerializer<T> serializer)
			{
				base.Serialize<T>(serializer);
				if (serializer.IsWriter)
				{
					Compression.SerializeUInt<T>(serializer, this.ThrowCooldownFrames);
					Compression.SerializeUInt<T>(serializer, this.HoldFrames);
					Compression.SerializeBoolsToByte<T>(serializer, this.ADS, this.ThrownThisFrame, false, false, false, false, false, false);
					return;
				}
				this.ThrowCooldownFrames = Compression.DeserializeUInt<T>(serializer);
				this.HoldFrames = Compression.DeserializeUInt<T>(serializer);
				Compression.DeserializeBoolsFromByte<T>(serializer, ref this.ADS, ref this.ThrownThisFrame);
			}

			// Token: 0x04000E77 RID: 3703
			public uint ThrowCooldownFrames;

			// Token: 0x04000E78 RID: 3704
			public bool ADS;

			// Token: 0x04000E79 RID: 3705
			public uint HoldFrames;

			// Token: 0x04000E7A RID: 3706
			public bool ThrownThisFrame;
		}
	}
}
