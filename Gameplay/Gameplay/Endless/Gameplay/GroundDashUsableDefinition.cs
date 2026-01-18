using System;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002D9 RID: 729
	public class GroundDashUsableDefinition : InventoryUsableDefinition
	{
		// Token: 0x17000339 RID: 825
		// (get) Token: 0x06001080 RID: 4224 RVA: 0x0005340E File Offset: 0x0005160E
		public short DashDirectionDelayFramesCount
		{
			get
			{
				return this.dashDirectionDelayFramesCount;
			}
		}

		// Token: 0x1700033A RID: 826
		// (get) Token: 0x06001081 RID: 4225 RVA: 0x00053416 File Offset: 0x00051616
		public short DashFrameCount
		{
			get
			{
				return this.dashFrameCount;
			}
		}

		// Token: 0x06001082 RID: 4226 RVA: 0x00053420 File Offset: 0x00051620
		public override UsableDefinition.UseState ProcessUseStart(ref NetState state, NetInput input, SerializableGuid guid, PlayerReferenceManager playerReference, Item item)
		{
			GroundDashUsableDefinition.GroundDashEquipmentUseState groundDashEquipmentUseState = UsableDefinition.UseState.GetStateFromPool(guid, item) as GroundDashUsableDefinition.GroundDashEquipmentUseState;
			groundDashEquipmentUseState.DashFrame = 0;
			groundDashEquipmentUseState.DashAngle = state.CharacterRotation;
			groundDashEquipmentUseState.DashAllowedFrame = (uint)((ulong)input.NetFrame + (ulong)((long)this.dashFrameCount) + (ulong)this.dashCooldownFrameCount);
			if (state.TotalForce.y < 0f)
			{
				state.TotalForce.y = 0f;
			}
			state.CalculatedMotion.y = 0f;
			state.JumpFrame = 99;
			return groundDashEquipmentUseState;
		}

		// Token: 0x06001083 RID: 4227 RVA: 0x000534A8 File Offset: 0x000516A8
		public override bool ProcessUseFrame(ref NetState state, NetInput input, ref UsableDefinition.UseState equipmentUseState, PlayerReferenceManager playerReference, bool equipped, bool pressed)
		{
			GroundDashUsableDefinition.GroundDashEquipmentUseState groundDashEquipmentUseState = equipmentUseState as GroundDashUsableDefinition.GroundDashEquipmentUseState;
			GroundDashUsableDefinition.GroundDashEquipmentUseState groundDashEquipmentUseState2 = groundDashEquipmentUseState;
			groundDashEquipmentUseState2.DashFrame += 1;
			if (groundDashEquipmentUseState.DashFrame <= this.dashDirectionDelayFramesCount)
			{
				if (state.UseInputRotation)
				{
					groundDashEquipmentUseState.DashAngle = state.InputRotation;
				}
				state.BlockJump = true;
				state.BlockRotation = true;
				state.BlockGravity = true;
				PlayerController.Rotate(ref state, groundDashEquipmentUseState.DashAngle, this.dashRotationSpeed * this.dashRotationCurve.Evaluate((float)groundDashEquipmentUseState.DashFrame / (float)this.dashFrameCount), false);
				groundDashEquipmentUseState.DashAngleRelative = (groundDashEquipmentUseState.DashAngle - state.CharacterRotation) / 360f;
				if (groundDashEquipmentUseState.DashAngleRelative < 0f)
				{
					groundDashEquipmentUseState.DashAngleRelative += 1f;
				}
				equipmentUseState = groundDashEquipmentUseState;
				state.BlockItemInput = true;
				return true;
			}
			if (groundDashEquipmentUseState.DashFrame <= this.dashFrameCount)
			{
				state.CalculatedMotion = PlayerController.AngleToForwardMotion(groundDashEquipmentUseState.DashAngle) * this.dashCurve.Evaluate((float)(groundDashEquipmentUseState.DashFrame - this.dashDirectionDelayFramesCount) / (float)(this.dashFrameCount - this.dashDirectionDelayFramesCount)) * this.dashSpeed;
				PlayerController.Rotate(ref state, groundDashEquipmentUseState.DashAngle, this.dashRotationSpeed * this.dashRotationCurve.Evaluate((float)groundDashEquipmentUseState.DashFrame / (float)this.dashFrameCount), false);
				if (groundDashEquipmentUseState.DashFrame > 0)
				{
					state.JumpFrame = (int)((short)Mathf.Min(state.JumpFrame, 0));
				}
				if (groundDashEquipmentUseState.DashFrame < this.dashFrameCount - this.dashAllowFallFrames)
				{
					state.BlockGravity = true;
				}
				state.BlockMotionXZ = true;
				state.BlockJump = true;
				state.BlockRotation = true;
				equipmentUseState = groundDashEquipmentUseState;
				state.BlockItemInput = true;
				return true;
			}
			return groundDashEquipmentUseState.DashAllowedFrame > input.NetFrame || (long)state.AirborneFrames > (long)((ulong)this.dashCooldownFrameCount);
		}

		// Token: 0x06001084 RID: 4228 RVA: 0x0005367D File Offset: 0x0005187D
		public override string GetAnimationTrigger(UsableDefinition.UseState equipmentUseState, uint currentVisualFrame)
		{
			if ((equipmentUseState as GroundDashUsableDefinition.GroundDashEquipmentUseState).DashFrame <= this.dashDirectionDelayFramesCount)
			{
				return string.Empty;
			}
			return base.AnimationTrigger;
		}

		// Token: 0x06001085 RID: 4229 RVA: 0x000536A0 File Offset: 0x000518A0
		public override void GetEventData(NetState state, UsableDefinition.UseState eus, double appearanceTime, ref InventoryUsableDefinition.EventData data)
		{
			base.GetEventData(state, eus, appearanceTime, ref data);
			if (eus != null)
			{
				GroundDashUsableDefinition.GroundDashEquipmentUseState groundDashEquipmentUseState = (GroundDashUsableDefinition.GroundDashEquipmentUseState)eus;
				data.Available = false;
				data.InUse = groundDashEquipmentUseState.DashFrame < this.dashFrameCount;
				data.CooldownSecondsLeft = (float)(NetClock.GetFrameTime(groundDashEquipmentUseState.DashAllowedFrame) - appearanceTime);
				data.CooldownSecondsTotal = this.dashCooldownFrameCount * NetClock.FixedDeltaTime;
				return;
			}
			data.Available = state.Grounded;
		}

		// Token: 0x06001086 RID: 4230 RVA: 0x00017586 File Offset: 0x00015786
		public override InventoryUsableDefinition.EquipmentShowPriority GetShowPriority(UsableDefinition.UseState eus)
		{
			return InventoryUsableDefinition.EquipmentShowPriority.MinorOutOfUse;
		}

		// Token: 0x06001087 RID: 4231 RVA: 0x00053717 File Offset: 0x00051917
		public override UsableDefinition.UseState GetNewUseState()
		{
			return new GroundDashUsableDefinition.GroundDashEquipmentUseState();
		}

		// Token: 0x04000E1C RID: 3612
		[SerializeField]
		private short dashCoyoteFrames = 4;

		// Token: 0x04000E1D RID: 3613
		[SerializeField]
		private short dashDirectionDelayFramesCount = 4;

		// Token: 0x04000E1E RID: 3614
		[SerializeField]
		private short dashFrameCount = 12;

		// Token: 0x04000E1F RID: 3615
		[SerializeField]
		private short dashAllowFallFrames = 4;

		// Token: 0x04000E20 RID: 3616
		[SerializeField]
		private uint dashCooldownFrameCount = 36U;

		// Token: 0x04000E21 RID: 3617
		[SerializeField]
		private float dashSpeed = 10f;

		// Token: 0x04000E22 RID: 3618
		[SerializeField]
		private AnimationCurve dashCurve = new AnimationCurve();

		// Token: 0x04000E23 RID: 3619
		[SerializeField]
		[Header("Dash Rotation")]
		private float dashRotationSpeed = 800f;

		// Token: 0x04000E24 RID: 3620
		[SerializeField]
		private AnimationCurve dashRotationCurve = new AnimationCurve();

		// Token: 0x020002DA RID: 730
		public class GroundDashEquipmentUseState : UsableDefinition.UseState
		{
			// Token: 0x06001089 RID: 4233 RVA: 0x00053784 File Offset: 0x00051984
			public override void Serialize<T>(BufferSerializer<T> serializer)
			{
				base.Serialize<T>(serializer);
				if (serializer.IsWriter)
				{
					Compression.SerializeFloatToUShort<T>(serializer, this.DashAngle, 0f, 360f);
					Compression.SerializeShort<T>(serializer, this.DashFrame);
					Compression.SerializeUInt<T>(serializer, this.DashAllowedFrame);
					serializer.SerializeValue<float>(ref this.DashAngleRelative, default(FastBufferWriter.ForPrimitives));
					return;
				}
				this.DashAngle = Compression.DeserializeFloatFromUShort<T>(serializer, 0f, 360f);
				this.DashFrame = Compression.DeserializeShort<T>(serializer);
				this.DashAllowedFrame = Compression.DeserializeUInt<T>(serializer);
				serializer.SerializeValue<float>(ref this.DashAngleRelative, default(FastBufferWriter.ForPrimitives));
			}

			// Token: 0x04000E25 RID: 3621
			public float DashAngle;

			// Token: 0x04000E26 RID: 3622
			public short DashFrame;

			// Token: 0x04000E27 RID: 3623
			public uint DashAllowedFrame;

			// Token: 0x04000E28 RID: 3624
			public float DashAngleRelative;
		}
	}
}
