using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public struct PositionalAudioInfo : INetworkSerializable
{
	private enum SfxSource
	{
		NetworkId,
		InstanceId,
		Position
	}

	private const int MIN_PITCH = -3;

	private const int MAX_PITCH = 3;

	public int ActiveFileId;

	public AudioType AudioType;

	public bool Loop;

	public float Volume;

	public float TimeStamp;

	public float FadeTime;

	public float Duration;

	public float Pitch;

	public float MinDistance;

	public float MaxDistance;

	public Vector3 Position;

	public SerializableGuid InstanceId;

	public ulong NetworkId;

	public SerializableGuid TransformId;

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref ActiveFileId, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref AudioType, default(FastBufferWriter.ForEnums));
		serializer.SerializeValue(ref Loop, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref Duration, default(FastBufferWriter.ForPrimitives));
		if (serializer.IsWriter)
		{
			Compression.SerializeFloatToUShort(serializer, Volume, 0f, 1f);
			Compression.SerializeFloatDecimal1(serializer, FadeTime);
			Compression.SerializeFloatDecimal1(serializer, TimeStamp);
			Compression.SerializeFloatToUShort(serializer, Pitch, -3f, 3f);
			Compression.SerializeFloatDecimal1(serializer, MinDistance);
			Compression.SerializeFloatDecimal1(serializer, MaxDistance);
			if (NetworkId != 0L)
			{
				SfxSource value = SfxSource.NetworkId;
				serializer.SerializeValue(ref value, default(FastBufferWriter.ForEnums));
				serializer.SerializeValue(ref NetworkId, default(FastBufferWriter.ForPrimitives));
				serializer.SerializeValue(ref TransformId, default(FastBufferWriter.ForNetworkSerializable));
			}
			else if (!InstanceId.IsEmpty)
			{
				SfxSource value = SfxSource.InstanceId;
				serializer.SerializeValue(ref value, default(FastBufferWriter.ForEnums));
				serializer.SerializeValue(ref InstanceId, default(FastBufferWriter.ForNetworkSerializable));
				serializer.SerializeValue(ref TransformId, default(FastBufferWriter.ForNetworkSerializable));
			}
			else
			{
				SfxSource value = SfxSource.Position;
				serializer.SerializeValue(ref value, default(FastBufferWriter.ForEnums));
				Compression.SerializeFloatDecimal1(serializer, Position.x);
				Compression.SerializeFloatDecimal1(serializer, Position.y);
				Compression.SerializeFloatDecimal1(serializer, Position.z);
			}
		}
		else
		{
			Volume = Compression.DeserializeFloatFromUShort(serializer, 0f, 1f);
			FadeTime = Compression.DeserializeFloatDecimal1(serializer);
			TimeStamp = Compression.DeserializeFloatDecimal1(serializer);
			Pitch = Compression.DeserializeFloatFromUShort(serializer, -3f, 3f);
			MinDistance = Compression.DeserializeFloatDecimal1(serializer);
			MaxDistance = Compression.DeserializeFloatDecimal1(serializer);
			SfxSource value2 = SfxSource.NetworkId;
			serializer.SerializeValue(ref value2, default(FastBufferWriter.ForEnums));
			switch (value2)
			{
			case SfxSource.Position:
				Position = new Vector3(Compression.DeserializeFloatDecimal1(serializer), Compression.DeserializeFloatDecimal1(serializer), Compression.DeserializeFloatDecimal1(serializer));
				break;
			case SfxSource.InstanceId:
				serializer.SerializeValue(ref InstanceId, default(FastBufferWriter.ForNetworkSerializable));
				serializer.SerializeValue(ref TransformId, default(FastBufferWriter.ForNetworkSerializable));
				break;
			default:
				serializer.SerializeValue(ref NetworkId, default(FastBufferWriter.ForPrimitives));
				serializer.SerializeValue(ref TransformId, default(FastBufferWriter.ForNetworkSerializable));
				break;
			}
		}
	}
}
