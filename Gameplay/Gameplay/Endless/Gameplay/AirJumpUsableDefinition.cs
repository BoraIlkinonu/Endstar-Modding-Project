using System;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002D3 RID: 723
	public class AirJumpUsableDefinition : InventoryUsableDefinition
	{
		// Token: 0x17000338 RID: 824
		// (get) Token: 0x06001064 RID: 4196 RVA: 0x00052E90 File Offset: 0x00051090
		private int airJumpFrames
		{
			get
			{
				return this.airJumpForce.Length;
			}
		}

		// Token: 0x06001065 RID: 4197 RVA: 0x00052E9A File Offset: 0x0005109A
		public override UsableDefinition.UseState GetNewUseState()
		{
			return new AirJumpUsableDefinition.AirJumpEquipmentUseState();
		}

		// Token: 0x06001066 RID: 4198 RVA: 0x00052EA4 File Offset: 0x000510A4
		public override UsableDefinition.UseState ProcessUseStart(ref NetState state, NetInput input, SerializableGuid guid, PlayerReferenceManager playerReference, Item item)
		{
			state.AirborneFrames = 0;
			if (this.resetsGravityOnUse && state.TotalForce.y < 0f)
			{
				state.TotalForce.y = 0f;
			}
			AirJumpUsableDefinition.AirJumpEquipmentUseState airJumpEquipmentUseState = UsableDefinition.UseState.GetStateFromPool(guid, item) as AirJumpUsableDefinition.AirJumpEquipmentUseState;
			airJumpEquipmentUseState.JumpFrame = 0;
			airJumpEquipmentUseState.UseHorizontalForce = input.Horizontal != 0 || input.Vertical != 0;
			airJumpEquipmentUseState.HorizontalForceAngle = state.InputRotation;
			return airJumpEquipmentUseState;
		}

		// Token: 0x06001067 RID: 4199 RVA: 0x00052F1C File Offset: 0x0005111C
		public override bool ProcessUseFrame(ref NetState state, NetInput input, ref UsableDefinition.UseState equipmentUseState, PlayerReferenceManager playerReference, bool equipped, bool pressed)
		{
			if (state.Grounded && state.AirborneFrames >= 2)
			{
				return false;
			}
			AirJumpUsableDefinition.AirJumpEquipmentUseState airJumpEquipmentUseState = equipmentUseState as AirJumpUsableDefinition.AirJumpEquipmentUseState;
			if ((int)airJumpEquipmentUseState.JumpFrame > state.AirborneFrames)
			{
				return false;
			}
			if ((int)airJumpEquipmentUseState.JumpFrame >= this.airJumpFrames)
			{
				return !state.Grounded;
			}
			state.BlockRotation = true;
			float num = (float)airJumpEquipmentUseState.JumpFrame / (float)this.airJumpFrames;
			float num2 = this.airJumpForce[(int)airJumpEquipmentUseState.JumpFrame];
			AirJumpUsableDefinition.AirJumpEquipmentUseState airJumpEquipmentUseState2 = airJumpEquipmentUseState;
			airJumpEquipmentUseState2.JumpFrame += 1;
			state.BlockMotionY = false;
			state.BlockMotionY_Down = false;
			state.BlockJump = true;
			state.XZInputMultiplierThisFrame = this.horizontalMotionCurve.Evaluate(num);
			state.JumpFrame = 0;
			state.BlockItemInput = true;
			state.TotalForce += Vector3.up * num2;
			state.BlockGravity = this.blocksGravityOnUse;
			if (airJumpEquipmentUseState.UseHorizontalForce)
			{
				state.CalculatedMotion += PlayerController.AngleToForwardMotion(airJumpEquipmentUseState.HorizontalForceAngle) * this.airJumpHorizontalForceCurve.Evaluate(num) * this.airJumpHorizontalForce;
			}
			return true;
		}

		// Token: 0x06001068 RID: 4200 RVA: 0x00053050 File Offset: 0x00051250
		public override void GetEventData(NetState state, UsableDefinition.UseState eus, double appearanceTime, ref InventoryUsableDefinition.EventData data)
		{
			base.GetEventData(state, eus, appearanceTime, ref data);
			if (eus != null)
			{
				data.Available = false;
				AirJumpUsableDefinition.AirJumpEquipmentUseState airJumpEquipmentUseState = (AirJumpUsableDefinition.AirJumpEquipmentUseState)eus;
				data.InUse = (int)airJumpEquipmentUseState.JumpFrame <= this.airJumpFrames;
			}
		}

		// Token: 0x06001069 RID: 4201 RVA: 0x00053092 File Offset: 0x00051292
		public override InventoryUsableDefinition.EquipmentShowPriority GetShowPriority(UsableDefinition.UseState eus)
		{
			if (eus != null)
			{
				return InventoryUsableDefinition.EquipmentShowPriority.MinorInUse;
			}
			return InventoryUsableDefinition.EquipmentShowPriority.MinorOutOfUse;
		}

		// Token: 0x04000E0E RID: 3598
		[SerializeField]
		private float[] airJumpForce;

		// Token: 0x04000E0F RID: 3599
		[SerializeField]
		private float airJumpHorizontalForce = 2f;

		// Token: 0x04000E10 RID: 3600
		[SerializeField]
		private AnimationCurve airJumpHorizontalForceCurve;

		// Token: 0x04000E11 RID: 3601
		[SerializeField]
		private AnimationCurve horizontalMotionCurve;

		// Token: 0x04000E12 RID: 3602
		[SerializeField]
		private bool resetsGravityOnUse = true;

		// Token: 0x04000E13 RID: 3603
		[SerializeField]
		private bool blocksGravityOnUse = true;

		// Token: 0x020002D4 RID: 724
		public class AirJumpEquipmentUseState : UsableDefinition.UseState
		{
			// Token: 0x0600106B RID: 4203 RVA: 0x000530BC File Offset: 0x000512BC
			public override void Serialize<T>(BufferSerializer<T> serializer)
			{
				base.Serialize<T>(serializer);
				if (serializer.IsWriter)
				{
					Compression.SerializeFloatToUShort<T>(serializer, this.HorizontalForceAngle, 0f, 360f);
					Compression.SerializeShort<T>(serializer, this.JumpFrame);
					serializer.SerializeValue<bool>(ref this.UseHorizontalForce, default(FastBufferWriter.ForPrimitives));
					return;
				}
				this.HorizontalForceAngle = Compression.DeserializeFloatFromUShort<T>(serializer, 0f, 360f);
				this.JumpFrame = Compression.DeserializeShort<T>(serializer);
				serializer.SerializeValue<bool>(ref this.UseHorizontalForce, default(FastBufferWriter.ForPrimitives));
			}

			// Token: 0x04000E14 RID: 3604
			public short JumpFrame;

			// Token: 0x04000E15 RID: 3605
			public bool UseHorizontalForce;

			// Token: 0x04000E16 RID: 3606
			public float HorizontalForceAngle;
		}
	}
}
