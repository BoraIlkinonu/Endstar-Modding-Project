using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public struct RigidbodyState : INetworkSerializable, IFrameInfo
{
	private uint netFrame;

	public bool FullSync;

	public bool Sleeping;

	public Vector3 Position;

	public Vector3 Angles;

	public Vector3 Velocity;

	public Vector3 AngularVelocity;

	public bool Teleporting;

	public TeleportType TeleportType;

	public uint FramesUntilTeleport;

	public Vector3 TeleportPosition;

	public bool serverVerifiedState;

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
	}

	public void Initialize()
	{
	}

	public static void Send(NetworkRigidbodyController controller, RigidbodyState state)
	{
		using FastBufferWriter messageStream = new FastBufferWriter(600, Allocator.Temp);
		messageStream.WriteValueSafe<NetworkObjectReference>((NetworkObjectReference)controller.NetworkObject, default(FastBufferWriter.ForNetworkSerializable));
		messageStream.WriteNetworkSerializable(in state);
		NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("RigidBodyState", messageStream, NetworkDelivery.Unreliable);
	}

	public static void Receive(FastBufferReader reader)
	{
		reader.ReadValueSafe(out NetworkObjectReference value, default(FastBufferWriter.ForNetworkSerializable));
		if (value.TryGet(out var networkObject))
		{
			NetworkRigidbodyController componentInChildren = networkObject.GetComponentInChildren<NetworkRigidbodyController>();
			reader.ReadNetworkSerializable(out RigidbodyState value2);
			value2.serverVerifiedState = true;
			componentInChildren.WriteStateToBuffer(ref value2);
			componentInChildren.UpdatedState(value2.netFrame);
		}
	}

	public void NetworkSerialize<RigidbodyState>(BufferSerializer<RigidbodyState> serializer) where RigidbodyState : IReaderWriter
	{
		if (serializer.IsWriter)
		{
			Compression.SerializeBoolsToByte(serializer, FullSync, Sleeping, Teleporting);
			Compression.SerializeUInt(serializer, netFrame);
		}
		else
		{
			Compression.DeserializeBoolsFromByte(serializer, ref FullSync, ref Sleeping, ref Teleporting);
			netFrame = Compression.DeserializeUInt(serializer);
		}
		if (!FullSync)
		{
			return;
		}
		serializer.SerializeValue(ref Position);
		serializer.SerializeValue(ref Angles);
		serializer.SerializeValue(ref Velocity);
		serializer.SerializeValue(ref AngularVelocity);
		if (Teleporting)
		{
			if (serializer.IsWriter)
			{
				Compression.SerializeUInt(serializer, FramesUntilTeleport);
			}
			else
			{
				FramesUntilTeleport = Compression.DeserializeUInt(serializer);
			}
			serializer.SerializeValue(ref TeleportType, default(FastBufferWriter.ForEnums));
			serializer.SerializeValue(ref TeleportPosition);
		}
	}
}
