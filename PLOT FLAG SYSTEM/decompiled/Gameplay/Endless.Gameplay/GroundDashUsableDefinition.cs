using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class GroundDashUsableDefinition : InventoryUsableDefinition
{
	public class GroundDashEquipmentUseState : UseState
	{
		public float DashAngle;

		public short DashFrame;

		public uint DashAllowedFrame;

		public float DashAngleRelative;

		public override void Serialize<T>(BufferSerializer<T> serializer)
		{
			base.Serialize(serializer);
			if (serializer.IsWriter)
			{
				Compression.SerializeFloatToUShort(serializer, DashAngle, 0f, 360f);
				Compression.SerializeShort(serializer, DashFrame);
				Compression.SerializeUInt(serializer, DashAllowedFrame);
				serializer.SerializeValue(ref DashAngleRelative, default(FastBufferWriter.ForPrimitives));
			}
			else
			{
				DashAngle = Compression.DeserializeFloatFromUShort(serializer, 0f, 360f);
				DashFrame = Compression.DeserializeShort(serializer);
				DashAllowedFrame = Compression.DeserializeUInt(serializer);
				serializer.SerializeValue(ref DashAngleRelative, default(FastBufferWriter.ForPrimitives));
			}
		}
	}

	[SerializeField]
	private short dashCoyoteFrames = 4;

	[SerializeField]
	private short dashDirectionDelayFramesCount = 4;

	[SerializeField]
	private short dashFrameCount = 12;

	[SerializeField]
	private short dashAllowFallFrames = 4;

	[SerializeField]
	private uint dashCooldownFrameCount = 36u;

	[SerializeField]
	private float dashSpeed = 10f;

	[SerializeField]
	private AnimationCurve dashCurve = new AnimationCurve();

	[SerializeField]
	[Header("Dash Rotation")]
	private float dashRotationSpeed = 800f;

	[SerializeField]
	private AnimationCurve dashRotationCurve = new AnimationCurve();

	public short DashDirectionDelayFramesCount => dashDirectionDelayFramesCount;

	public short DashFrameCount => dashFrameCount;

	public override UseState ProcessUseStart(ref NetState state, NetInput input, SerializableGuid guid, PlayerReferenceManager playerReference, Item item)
	{
		GroundDashEquipmentUseState obj = UseState.GetStateFromPool(guid, item) as GroundDashEquipmentUseState;
		obj.DashFrame = 0;
		obj.DashAngle = state.CharacterRotation;
		obj.DashAllowedFrame = (uint)(input.NetFrame + dashFrameCount + dashCooldownFrameCount);
		if (state.TotalForce.y < 0f)
		{
			state.TotalForce.y = 0f;
		}
		state.CalculatedMotion.y = 0f;
		state.JumpFrame = 99;
		return obj;
	}

	public override bool ProcessUseFrame(ref NetState state, NetInput input, ref UseState equipmentUseState, PlayerReferenceManager playerReference, bool equipped, bool pressed)
	{
		GroundDashEquipmentUseState groundDashEquipmentUseState = equipmentUseState as GroundDashEquipmentUseState;
		groundDashEquipmentUseState.DashFrame++;
		if (groundDashEquipmentUseState.DashFrame <= dashDirectionDelayFramesCount)
		{
			if (state.UseInputRotation)
			{
				groundDashEquipmentUseState.DashAngle = state.InputRotation;
			}
			state.BlockJump = true;
			state.BlockRotation = true;
			state.BlockGravity = true;
			PlayerController.Rotate(ref state, groundDashEquipmentUseState.DashAngle, dashRotationSpeed * dashRotationCurve.Evaluate((float)groundDashEquipmentUseState.DashFrame / (float)dashFrameCount));
			groundDashEquipmentUseState.DashAngleRelative = (groundDashEquipmentUseState.DashAngle - state.CharacterRotation) / 360f;
			if (groundDashEquipmentUseState.DashAngleRelative < 0f)
			{
				groundDashEquipmentUseState.DashAngleRelative += 1f;
			}
			equipmentUseState = groundDashEquipmentUseState;
			state.BlockItemInput = true;
			return true;
		}
		if (groundDashEquipmentUseState.DashFrame <= dashFrameCount)
		{
			state.CalculatedMotion = PlayerController.AngleToForwardMotion(groundDashEquipmentUseState.DashAngle) * dashCurve.Evaluate((float)(groundDashEquipmentUseState.DashFrame - dashDirectionDelayFramesCount) / (float)(dashFrameCount - dashDirectionDelayFramesCount)) * dashSpeed;
			PlayerController.Rotate(ref state, groundDashEquipmentUseState.DashAngle, dashRotationSpeed * dashRotationCurve.Evaluate((float)groundDashEquipmentUseState.DashFrame / (float)dashFrameCount));
			if (groundDashEquipmentUseState.DashFrame > 0)
			{
				state.JumpFrame = (short)Mathf.Min(state.JumpFrame, 0);
			}
			if (groundDashEquipmentUseState.DashFrame < dashFrameCount - dashAllowFallFrames)
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
		if (groundDashEquipmentUseState.DashAllowedFrame > input.NetFrame || state.AirborneFrames > dashCooldownFrameCount)
		{
			return true;
		}
		return false;
	}

	public override string GetAnimationTrigger(UseState equipmentUseState, uint currentVisualFrame)
	{
		if ((equipmentUseState as GroundDashEquipmentUseState).DashFrame <= dashDirectionDelayFramesCount)
		{
			return string.Empty;
		}
		return base.AnimationTrigger;
	}

	public override void GetEventData(NetState state, UseState eus, double appearanceTime, ref EventData data)
	{
		base.GetEventData(state, eus, appearanceTime, ref data);
		if (eus != null)
		{
			GroundDashEquipmentUseState groundDashEquipmentUseState = (GroundDashEquipmentUseState)eus;
			data.Available = false;
			data.InUse = groundDashEquipmentUseState.DashFrame < dashFrameCount;
			data.CooldownSecondsLeft = (float)(NetClock.GetFrameTime(groundDashEquipmentUseState.DashAllowedFrame) - appearanceTime);
			data.CooldownSecondsTotal = (float)dashCooldownFrameCount * NetClock.FixedDeltaTime;
		}
		else
		{
			data.Available = state.Grounded;
		}
	}

	public override EquipmentShowPriority GetShowPriority(UseState eus)
	{
		return EquipmentShowPriority.MinorOutOfUse;
	}

	public override UseState GetNewUseState()
	{
		return new GroundDashEquipmentUseState();
	}
}
