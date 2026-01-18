using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class AirJumpUsableDefinition : InventoryUsableDefinition
{
	public class AirJumpEquipmentUseState : UseState
	{
		public short JumpFrame;

		public bool UseHorizontalForce;

		public float HorizontalForceAngle;

		public override void Serialize<T>(BufferSerializer<T> serializer)
		{
			base.Serialize(serializer);
			if (serializer.IsWriter)
			{
				Compression.SerializeFloatToUShort(serializer, HorizontalForceAngle, 0f, 360f);
				Compression.SerializeShort(serializer, JumpFrame);
				serializer.SerializeValue(ref UseHorizontalForce, default(FastBufferWriter.ForPrimitives));
			}
			else
			{
				HorizontalForceAngle = Compression.DeserializeFloatFromUShort(serializer, 0f, 360f);
				JumpFrame = Compression.DeserializeShort(serializer);
				serializer.SerializeValue(ref UseHorizontalForce, default(FastBufferWriter.ForPrimitives));
			}
		}
	}

	[SerializeField]
	private float[] airJumpForce;

	[SerializeField]
	private float airJumpHorizontalForce = 2f;

	[SerializeField]
	private AnimationCurve airJumpHorizontalForceCurve;

	[SerializeField]
	private AnimationCurve horizontalMotionCurve;

	[SerializeField]
	private bool resetsGravityOnUse = true;

	[SerializeField]
	private bool blocksGravityOnUse = true;

	private int airJumpFrames => airJumpForce.Length;

	public override UseState GetNewUseState()
	{
		return new AirJumpEquipmentUseState();
	}

	public override UseState ProcessUseStart(ref NetState state, NetInput input, SerializableGuid guid, PlayerReferenceManager playerReference, Item item)
	{
		state.AirborneFrames = 0;
		if (resetsGravityOnUse && state.TotalForce.y < 0f)
		{
			state.TotalForce.y = 0f;
		}
		AirJumpEquipmentUseState obj = UseState.GetStateFromPool(guid, item) as AirJumpEquipmentUseState;
		obj.JumpFrame = 0;
		obj.UseHorizontalForce = input.Horizontal != 0 || input.Vertical != 0;
		obj.HorizontalForceAngle = state.InputRotation;
		return obj;
	}

	public override bool ProcessUseFrame(ref NetState state, NetInput input, ref UseState equipmentUseState, PlayerReferenceManager playerReference, bool equipped, bool pressed)
	{
		if (!state.Grounded || state.AirborneFrames < 2)
		{
			AirJumpEquipmentUseState airJumpEquipmentUseState = equipmentUseState as AirJumpEquipmentUseState;
			if (airJumpEquipmentUseState.JumpFrame > state.AirborneFrames)
			{
				return false;
			}
			if (airJumpEquipmentUseState.JumpFrame >= airJumpFrames)
			{
				if (state.Grounded)
				{
					return false;
				}
				return true;
			}
			state.BlockRotation = true;
			float time = (float)airJumpEquipmentUseState.JumpFrame / (float)airJumpFrames;
			float num = airJumpForce[airJumpEquipmentUseState.JumpFrame];
			airJumpEquipmentUseState.JumpFrame++;
			state.BlockMotionY = false;
			state.BlockMotionY_Down = false;
			state.BlockJump = true;
			state.XZInputMultiplierThisFrame = horizontalMotionCurve.Evaluate(time);
			state.JumpFrame = 0;
			state.BlockItemInput = true;
			state.TotalForce += Vector3.up * num;
			state.BlockGravity = blocksGravityOnUse;
			if (airJumpEquipmentUseState.UseHorizontalForce)
			{
				state.CalculatedMotion += PlayerController.AngleToForwardMotion(airJumpEquipmentUseState.HorizontalForceAngle) * airJumpHorizontalForceCurve.Evaluate(time) * airJumpHorizontalForce;
			}
			return true;
		}
		return false;
	}

	public override void GetEventData(NetState state, UseState eus, double appearanceTime, ref EventData data)
	{
		base.GetEventData(state, eus, appearanceTime, ref data);
		if (eus != null)
		{
			data.Available = false;
			AirJumpEquipmentUseState airJumpEquipmentUseState = (AirJumpEquipmentUseState)eus;
			data.InUse = airJumpEquipmentUseState.JumpFrame <= airJumpFrames;
		}
	}

	public override EquipmentShowPriority GetShowPriority(UseState eus)
	{
		if (eus != null)
		{
			return EquipmentShowPriority.MinorInUse;
		}
		return EquipmentShowPriority.MinorOutOfUse;
	}
}
