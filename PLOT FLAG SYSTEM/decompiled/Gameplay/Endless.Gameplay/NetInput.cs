using Endless.Shared;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public struct NetInput : INetworkSerializable, IFrameInfo
{
	public enum InputEquipmentSlot : byte
	{
		None,
		E1,
		E2
	}

	public enum InputDirection : byte
	{
		L = 2,
		None,
		R,
		FL,
		F,
		FR,
		BL,
		B,
		BR
	}

	public enum PrimaryEquipmentInput : byte
	{
		None,
		P1,
		P2,
		Both
	}

	private uint netFrame;

	public int Vertical;

	public int Horizontal;

	public float MotionRotation;

	public float CharacterRotation;

	public float AimPitch;

	public float AimYaw;

	public bool Jump;

	public bool Ghost;

	public bool Down;

	public PrimaryEquipmentInput PrimaryEquipment;

	public bool SecondaryEquipment;

	public bool UseIK;

	public Vector3 FocusPoint;

	public bool Run;

	private InputDirection inputDirection;

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

	public bool PressingPrimary1
	{
		get
		{
			if (PrimaryEquipment != PrimaryEquipmentInput.P1)
			{
				return PrimaryEquipment == PrimaryEquipmentInput.Both;
			}
			return true;
		}
	}

	public bool PressingPrimary2
	{
		get
		{
			if (PrimaryEquipment != PrimaryEquipmentInput.P2)
			{
				return PrimaryEquipment == PrimaryEquipmentInput.Both;
			}
			return true;
		}
	}

	public void Clear()
	{
		Vertical = 0;
		Horizontal = 0;
		MotionRotation = 0f;
		CharacterRotation = 0f;
		AimPitch = 0f;
		AimYaw = 0f;
		Jump = false;
		Ghost = false;
		Down = false;
		PrimaryEquipment = PrimaryEquipmentInput.None;
		SecondaryEquipment = false;
		Run = false;
	}

	public void Initialize()
	{
	}

	public void NetworkSerialize<NetInput>(BufferSerializer<NetInput> serializer) where NetInput : IReaderWriter
	{
		if (serializer.IsWriter)
		{
			Compression.SerializeBoolsToByte(serializer, Jump, Ghost, Down, SecondaryEquipment, UseIK, Run);
			Compression.SerializeFloatToUShort(serializer, MotionRotation, 0f, 360f);
			serializer.SerializeValue(ref CharacterRotation, default(FastBufferWriter.ForPrimitives));
			inputDirection = (InputDirection)(((Vertical == 0) ? 3 : ((Vertical > 0) ? 6 : 9)) + Horizontal);
			serializer.SerializeValue(ref inputDirection, default(FastBufferWriter.ForEnums));
			serializer.SerializeValue(ref AimPitch, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref AimYaw, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref PrimaryEquipment, default(FastBufferWriter.ForEnums));
			serializer.SerializeValue(ref FocusPoint);
			return;
		}
		Compression.DeserializeBoolsFromByte(serializer, ref Jump, ref Ghost, ref Down, ref SecondaryEquipment, ref UseIK, ref Run);
		MotionRotation = Compression.DeserializeFloatFromUShort(serializer, 0f, 360f);
		serializer.SerializeValue(ref CharacterRotation, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref inputDirection, default(FastBufferWriter.ForEnums));
		int num = (int)inputDirection;
		Vertical = ((num >= 5) ? ((num <= 7) ? 1 : (-1)) : 0);
		Horizontal = num % 3;
		if (Horizontal > 1)
		{
			Horizontal = -1;
		}
		serializer.SerializeValue(ref AimPitch, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref AimYaw, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref PrimaryEquipment, default(FastBufferWriter.ForEnums));
		serializer.SerializeValue(ref FocusPoint);
	}

	public static void Send(NetInput input)
	{
		using FastBufferWriter fastBufferWriter = new FastBufferWriter(600, Allocator.Temp);
		BytePacker.WriteValueBitPacked(fastBufferWriter, input.netFrame);
		fastBufferWriter.WriteNetworkSerializable(in input);
		NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("NetInput", 0uL, fastBufferWriter, NetworkDelivery.Unreliable);
	}

	public static void Receive(ulong senderClientId, FastBufferReader reader)
	{
		PlayerReferenceManager playerObject = MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayerObject(senderClientId);
		if ((bool)playerObject)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out uint value);
			ref NetInput referenceFromBuffer = ref playerObject.PlayerNetworkController.ServerInputRingBuffer.GetReferenceFromBuffer(value);
			reader.ReadNetworkSerializable(out referenceFromBuffer);
			referenceFromBuffer.netFrame = value;
			playerObject.PlayerNetworkController.ServerInputRingBuffer.FrameUpdated(value);
		}
	}
}
