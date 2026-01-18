using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public struct NpcState : IFrameInfo, INetworkSerializable
{
	private uint netFrame;

	public Vector3 Position;

	public float Rotation;

	public int attack;

	public bool jumped;

	public int taunt;

	public int fidget;

	public bool isMoving;

	public bool isGrounded;

	public bool isAirborne;

	public float slopeAngle;

	public float airTime;

	public float fallTime;

	public bool zLock;

	public float VelX;

	public float VelY;

	public float VelZ;

	public float AngularVelocity;

	public float HorizVelMagnitude;

	public bool landed;

	public bool LargePush;

	public bool PhysicsForceExit;

	public bool LoopSmallPush;

	public bool SmallPush;

	public bool EndSmallPush;

	public bool ImminentlyAttacking;

	public bool IsAwaitingTeleport;

	public bool walking;

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

	public void Clear()
	{
		netFrame = 0u;
		Position = Vector3.zero;
		Rotation = 0f;
		attack = 0;
		isMoving = false;
		isAirborne = false;
		jumped = false;
		slopeAngle = 0f;
		VelX = 0f;
		VelY = 0f;
		VelZ = 0f;
		AngularVelocity = 0f;
		HorizVelMagnitude = 0f;
		airTime = 0f;
		fallTime = 0f;
		taunt = 0;
		fidget = 0;
		landed = false;
		isGrounded = false;
		LargePush = false;
		PhysicsForceExit = false;
		LoopSmallPush = false;
		SmallPush = false;
		EndSmallPush = false;
		ImminentlyAttacking = false;
		walking = false;
	}

	public void Initialize()
	{
	}

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref netFrame, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref Position);
		serializer.SerializeValue(ref Rotation, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref attack, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref taunt, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref fidget, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref jumped, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref fallTime, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref VelX, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref VelY, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref VelZ, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref isMoving, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref AngularVelocity, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref HorizVelMagnitude, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref LargePush, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref PhysicsForceExit, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref isAirborne, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref zLock, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref landed, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref isGrounded, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref LoopSmallPush, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref SmallPush, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref EndSmallPush, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref ImminentlyAttacking, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref walking, default(FastBufferWriter.ForPrimitives));
	}

	public static void Send(NpcState simulatedState, uint key)
	{
		using FastBufferWriter messageStream = new FastBufferWriter(600, Allocator.Temp);
		messageStream.WriteValueSafe(in key, default(FastBufferWriter.ForPrimitives));
		messageStream.WriteNetworkSerializable(in simulatedState);
		NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("AiState", messageStream, NetworkDelivery.Unreliable);
	}

	public static void Receive(FastBufferReader reader)
	{
		reader.ReadValueSafe(out uint value, default(FastBufferWriter.ForPrimitives));
		reader.ReadNetworkSerializable(out NpcState value2);
		IndividualStateUpdater.ReceiveAiStates(value, value2);
	}
}
