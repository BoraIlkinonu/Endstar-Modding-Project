using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public struct LateJoinAudioInfo : INetworkSerializable
{
	public int ActiveFileId;

	public bool Loop;

	public float Volume;

	public float TimeStamp;

	public float FadeTime;

	public float Duration;

	public AudioType AudioType;

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		if (serializer.IsWriter)
		{
			Compression.SerializeFloatToUShort(serializer, Volume, 0f, 1f);
			Compression.SerializeFloatDecimal1(serializer, FadeTime);
			Compression.SerializeFloatDecimal1(serializer, Duration);
			Compression.SerializeFloatDecimal1(serializer, TimeStamp);
		}
		else
		{
			Volume = Compression.DeserializeFloatFromUShort(serializer, 0f, 1f);
			FadeTime = Compression.DeserializeFloatDecimal1(serializer);
			Duration = Compression.DeserializeFloatDecimal1(serializer);
			TimeStamp = Compression.DeserializeFloatDecimal1(serializer);
		}
		serializer.SerializeValue(ref ActiveFileId, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref Loop, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref AudioType, default(FastBufferWriter.ForEnums));
	}
}
