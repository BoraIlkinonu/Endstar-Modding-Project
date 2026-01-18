using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public struct NetState : INetworkSerializable, IFrameInfo
{
	private uint netFrame;

	public Vector3 Position;

	public Vector3 CalculatedMotion;

	public int JumpFrame;

	public int MotionX;

	public int MotionZ;

	public uint StunFrame;

	public float CharacterRotation;

	public bool Grounded;

	public float SlopeAngle;

	public SerializableGuid CurrentPressedEquipment;

	public int EquipmentPressedDuration;

	public bool JumpReleasedThisJump;

	public int JumpPressedDuration;

	public int AirborneFrames;

	public int FallFrames;

	public Vector3 LastWorldPhysics;

	public uint LastHitFrame;

	public NetworkObjectReference ParentNetworkObject;

	public Vector3 RelativePositionToParent;

	public TeleportComponent.TeleportStatusType TeleportStatus;

	public bool GameplayTeleport;

	public TeleportType GameplayTeleportType;

	public uint TeleportAtFrame;

	public Vector3 TeleportPosition;

	public bool TeleportHasRotation;

	public float TeleportRotation;

	public bool TeleportRotationSnapCamera;

	public bool Downed;

	public bool Reviving;

	public bool Ghost;

	public bool Run;

	public bool InputSynced;

	public int GhostVerticalMotorFrame;

	public List<UsableDefinition.UseState> ActiveUseStates;

	private NetworkObjectReference interactableNetObjRef;

	public UsableDefinition.UseState ActiveWorldUseState;

	public List<PhysicsForceInfo> PhysicsForces;

	public Vector3 TotalForce;

	public PlayerPhysicsTaker.PushState PhysicsPushState;

	public uint PushFrame;

	public Vector3 AimRelativePoint;

	public bool UseIK;

	public int FramesSinceStableGround;

	public uint PrimarySwapBlockingFrames;

	public uint SecondarySwapBlockingFrames;

	public float HorizontalCameraAim;

	public CameraController.CameraType AimState;

	public float AimMovementScaleMultiplier;

	public bool BlockItemInput;

	public bool BlockMotionXZ;

	public bool BlockMotionY;

	public bool BlockMotionY_Down;

	public bool BlockPhysics;

	public bool BlockJump;

	public bool BlockGrounding;

	public bool BlockRotation;

	public float InputRotation;

	public bool UseInputRotation;

	public bool BlockSecondaryEquipmentInput;

	public float GroundSlope;

	public Vector3 GroundNormal;

	public bool Strafing;

	public bool HasHostileForce;

	public bool BlockGravity;

	public bool StableGround;

	public int GroundedFrames;

	public bool UsingAimFix;

	public Vector3 MovingColliderVisualsCorrection;

	public bool serverVerifiedState;

	public float PushForceControl;

	private float _xzMultiplierThisFrame;

	public uint NetFrame
	{
		get
		{
			return netFrame;
		}
		set
		{
			netFrame = value;
		}
	}

	public bool IsStunned => StunFrame != 0;

	public bool TeleportActive => TeleportStatus != TeleportComponent.TeleportStatusType.None;

	public bool PrimarySwapActive => PrimarySwapBlockingFrames != 0;

	public bool SecondarySwapActive => SecondarySwapBlockingFrames != 0;

	public bool AnyEquipmentSwapActive
	{
		get
		{
			if (!PrimarySwapActive)
			{
				return SecondarySwapActive;
			}
			return true;
		}
	}

	public float XZInputMultiplierThisFrame
	{
		get
		{
			return _xzMultiplierThisFrame;
		}
		set
		{
			if (value < _xzMultiplierThisFrame)
			{
				_xzMultiplierThisFrame = value;
			}
		}
	}

	public Vector3 CalculatedPostion
	{
		get
		{
			NetworkObject networkObject = ParentNetworkObject;
			if (networkObject == null)
			{
				return Position;
			}
			return networkObject.transform.position + RelativePositionToParent;
		}
	}

	public void CopyTo(ref NetState copyTarget)
	{
		using FastBufferWriter writer = new FastBufferWriter(600, Allocator.Temp);
		writer.WriteNetworkSerializable(in this);
		new FastBufferReader(writer, Allocator.Temp).ReadValue(out copyTarget, default(FastBufferWriter.ForNetworkSerializable));
	}

	public void BouncedThisFrame()
	{
		BlockGrounding = true;
		AirborneFrames = 0;
	}

	public void ResetTempFrameValues()
	{
		BlockItemInput = false;
		BlockMotionXZ = false;
		BlockMotionY = false;
		BlockMotionY_Down = false;
		BlockPhysics = false;
		BlockJump = false;
		BlockGrounding = false;
		BlockRotation = false;
		InputRotation = 0f;
		UseInputRotation = false;
		BlockGravity = false;
		BlockSecondaryEquipmentInput = false;
		GroundSlope = 0f;
		GroundNormal = Vector3.zero;
		_xzMultiplierThisFrame = 1f;
		Strafing = false;
		PushForceControl = 1f;
		AimState = CameraController.CameraType.Normal;
		AimMovementScaleMultiplier = 1f;
		HasHostileForce = false;
		StableGround = false;
		UsingAimFix = false;
		MovingColliderVisualsCorrection = Vector3.zero;
	}

	public static void Send(PlayerReferenceManager playerReference, NetState state)
	{
		using FastBufferWriter fastBufferWriter = new FastBufferWriter(600, Allocator.Temp);
		fastBufferWriter.WriteValueSafe<NetworkObjectReference>((NetworkObjectReference)playerReference.NetworkObject, default(FastBufferWriter.ForNetworkSerializable));
		BytePacker.WriteValueBitPacked(fastBufferWriter, state.netFrame);
		fastBufferWriter.WriteNetworkSerializable(in state);
		NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("NetState", fastBufferWriter, NetworkDelivery.Unreliable);
	}

	public static void Receive(FastBufferReader reader)
	{
		reader.ReadValueSafe(out NetworkObjectReference value, default(FastBufferWriter.ForNetworkSerializable));
		ByteUnpacker.ReadValueBitPacked(reader, out uint value2);
		if (value.TryGet(out var networkObject))
		{
			PlayerReferenceManager playerObject = MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayerObject(networkObject.OwnerClientId);
			ref NetState referenceFromBuffer = ref playerObject.PlayerNetworkController.ClientStateRingBuffer.GetReferenceFromBuffer(value2);
			reader.ReadNetworkSerializable(out referenceFromBuffer);
			referenceFromBuffer.netFrame = value2;
			referenceFromBuffer.serverVerifiedState = true;
			playerObject.PlayerNetworkController.UpdatedState(value2);
		}
	}

	public void NetworkSerialize<NetState>(BufferSerializer<NetState> serializer) where NetState : IReaderWriter
	{
		serializer.SerializeValue(ref Position);
		serializer.SerializeValue(ref CurrentPressedEquipment, default(FastBufferWriter.ForNetworkSerializable));
		serializer.SerializeValue(ref CharacterRotation, default(FastBufferWriter.ForPrimitives));
		bool b = ActiveWorldUseState != null;
		bool b2 = (NetworkObject)ParentNetworkObject != null;
		bool b3 = TeleportActive;
		if (serializer.IsWriter)
		{
			Compression.SerializeIntToByteClamped(serializer, JumpFrame);
			Compression.SerializeIntToByteClamped(serializer, MotionX);
			Compression.SerializeIntToByteClamped(serializer, MotionZ);
			Compression.SerializeUIntToByteClamped(serializer, StunFrame);
			Compression.SerializeIntToByteClamped(serializer, EquipmentPressedDuration);
			Compression.SerializeIntToByteClamped(serializer, JumpPressedDuration);
			Compression.SerializeIntToByteClamped(serializer, AirborneFrames);
			Compression.SerializeIntToByteClamped(serializer, FramesSinceStableGround);
			Compression.SerializeIntToByteClamped(serializer, FallFrames);
			Compression.SerializeFloatToUShort(serializer, SlopeAngle, -1f, 1f);
			Compression.SerializeFloatToUShort(serializer, CalculatedMotion.x, -10f, 10f);
			Compression.SerializeFloatToUShort(serializer, CalculatedMotion.y, -10f, 10f);
			Compression.SerializeFloatToUShort(serializer, CalculatedMotion.z, -10f, 10f);
			Compression.SerializeFloatToUShort(serializer, LastWorldPhysics.x, -1f, 1f);
			Compression.SerializeFloatToUShort(serializer, LastWorldPhysics.y, -1f, 1f);
			Compression.SerializeFloatToUShort(serializer, LastWorldPhysics.z, -1f, 1f);
			Compression.SerializeUInt(serializer, LastHitFrame);
			Compression.SerializeBoolsToByte(serializer, Grounded, JumpReleasedThisJump, b3, Downed, Reviving, Ghost, b2);
			Compression.SerializeBoolsToByte(serializer, b, InputSynced, UseIK, Run);
			Compression.SerializeFloatToUShort(serializer, HorizontalCameraAim, 0f, 360f);
			Compression.SerializeUInt(serializer, PrimarySwapBlockingFrames);
			Compression.SerializeUInt(serializer, SecondarySwapBlockingFrames);
			serializer.SerializeValue(ref AimState, default(FastBufferWriter.ForEnums));
			if (AimState != CameraController.CameraType.Normal)
			{
				Compression.SerializeFloatToUShort(serializer, AimMovementScaleMultiplier, 0f, 1f);
			}
			serializer.SerializeValue(ref AimRelativePoint);
		}
		else
		{
			JumpFrame = Compression.DeserializeIntFromByteClamped(serializer);
			MotionX = Compression.DeserializeIntFromByteClamped(serializer);
			MotionZ = Compression.DeserializeIntFromByteClamped(serializer);
			StunFrame = Compression.DeserializeUIntFromByteClamped(serializer);
			EquipmentPressedDuration = Compression.DeserializeIntFromByteClamped(serializer);
			JumpPressedDuration = Compression.DeserializeIntFromByteClamped(serializer);
			AirborneFrames = Compression.DeserializeIntFromByteClamped(serializer);
			FramesSinceStableGround = Compression.DeserializeIntFromByteClamped(serializer);
			FallFrames = Compression.DeserializeIntFromByteClamped(serializer);
			SlopeAngle = Compression.DeserializeFloatFromUShort(serializer, -1f, 1f);
			CalculatedMotion.x = Compression.DeserializeFloatFromUShort(serializer, -10f, 10f);
			CalculatedMotion.y = Compression.DeserializeFloatFromUShort(serializer, -10f, 10f);
			CalculatedMotion.z = Compression.DeserializeFloatFromUShort(serializer, -10f, 10f);
			LastWorldPhysics.x = Compression.DeserializeFloatFromUShort(serializer, -1f, 1f);
			LastWorldPhysics.y = Compression.DeserializeFloatFromUShort(serializer, -1f, 1f);
			LastWorldPhysics.z = Compression.DeserializeFloatFromUShort(serializer, -1f, 1f);
			LastHitFrame = Compression.DeserializeUInt(serializer);
			Compression.DeserializeBoolsFromByte(serializer, ref Grounded, ref JumpReleasedThisJump, ref b3, ref Downed, ref Reviving, ref Ghost, ref b2);
			Compression.DeserializeBoolsFromByte(serializer, ref b, ref InputSynced, ref UseIK, ref Run);
			HorizontalCameraAim = Compression.DeserializeFloatFromUShort(serializer, 0f, 360f);
			PrimarySwapBlockingFrames = Compression.DeserializeUInt(serializer);
			SecondarySwapBlockingFrames = Compression.DeserializeUInt(serializer);
			serializer.SerializeValue(ref AimState, default(FastBufferWriter.ForEnums));
			if (AimState != CameraController.CameraType.Normal)
			{
				AimMovementScaleMultiplier = Compression.DeserializeFloatFromUShort(serializer, 0f, 1f);
			}
			else
			{
				AimMovementScaleMultiplier = 1f;
			}
			serializer.SerializeValue(ref AimRelativePoint);
		}
		if (b3)
		{
			serializer.SerializeValue(ref TeleportStatus, default(FastBufferWriter.ForEnums));
			serializer.SerializeValue(ref TeleportAtFrame, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref TeleportPosition);
			serializer.SerializeValue(ref TeleportHasRotation, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref GameplayTeleport, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref GameplayTeleportType, default(FastBufferWriter.ForEnums));
			serializer.SerializeValue(ref TeleportRotationSnapCamera, default(FastBufferWriter.ForPrimitives));
			if (TeleportHasRotation)
			{
				serializer.SerializeValue(ref TeleportRotation, default(FastBufferWriter.ForPrimitives));
			}
		}
		if (b2)
		{
			serializer.SerializeValue(ref ParentNetworkObject, default(FastBufferWriter.ForNetworkSerializable));
			serializer.SerializeValue(ref RelativePositionToParent);
		}
		else if (serializer.IsReader)
		{
			ParentNetworkObject = default(NetworkObjectReference);
		}
		if (Ghost)
		{
			if (serializer.IsWriter)
			{
				Compression.SerializeIntToByteClamped(serializer, GhostVerticalMotorFrame);
			}
			else
			{
				GhostVerticalMotorFrame = Compression.DeserializeIntFromByteClamped(serializer);
			}
		}
		int value = 0;
		if (serializer.IsWriter)
		{
			value = ((PhysicsForces != null) ? PhysicsForces.Count : 0);
		}
		serializer.SerializeValue(ref value, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref PhysicsPushState, default(FastBufferWriter.ForEnums));
		if (serializer.IsWriter)
		{
			Compression.SerializeUInt(serializer, PushFrame);
		}
		else
		{
			PushFrame = Compression.DeserializeUInt(serializer);
		}
		if (serializer.IsReader)
		{
			PhysicsForces = new List<PhysicsForceInfo>();
			for (int i = 0; i < value; i++)
			{
				PhysicsForceInfo value2 = default(PhysicsForceInfo);
				serializer.SerializeValue(ref value2, default(FastBufferWriter.ForNetworkSerializable));
				PhysicsForces.Add(value2);
			}
		}
		else
		{
			for (int j = 0; j < value; j++)
			{
				PhysicsForceInfo value3 = PhysicsForces[j];
				serializer.SerializeValue(ref value3, default(FastBufferWriter.ForNetworkSerializable));
			}
		}
		serializer.SerializeValue(ref TotalForce);
		int value4 = 0;
		if (serializer.IsWriter)
		{
			value4 = ((ActiveUseStates != null) ? ActiveUseStates.Count : 0);
		}
		serializer.SerializeValue(ref value4, default(FastBufferWriter.ForPrimitives));
		if (serializer.IsReader)
		{
			ActiveUseStates = new List<UsableDefinition.UseState>();
			for (int k = 0; k < value4; k++)
			{
				UsableDefinition.UseState useState = null;
				UsableDefinition.SerializeEquipmentUseState(serializer, ref useState);
				ActiveUseStates.Add(useState);
			}
		}
		else
		{
			for (int l = 0; l < value4; l++)
			{
				UsableDefinition.UseState useState2 = ActiveUseStates[l];
				UsableDefinition.SerializeEquipmentUseState(serializer, ref useState2);
			}
		}
		if (b)
		{
			UsableDefinition.SerializeEquipmentUseState(serializer, ref ActiveWorldUseState);
		}
	}

	public void Clear()
	{
		if (ActiveUseStates != null)
		{
			for (int num = ActiveUseStates.Count - 1; num > -1; num--)
			{
				UsableDefinition.UseState.PoolState(ActiveUseStates[num]);
			}
			ActiveUseStates.Clear();
		}
		if (PhysicsForces != null)
		{
			PhysicsForces.Clear();
		}
	}

	public void Initialize()
	{
	}
}
