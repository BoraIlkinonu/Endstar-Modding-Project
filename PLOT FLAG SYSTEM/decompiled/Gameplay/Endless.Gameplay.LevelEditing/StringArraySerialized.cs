using Unity.Netcode;

namespace Endless.Gameplay.LevelEditing;

public struct StringArraySerialized : INetworkSerializable
{
	public string[] value;

	public static implicit operator string[](StringArraySerialized obj)
	{
		return obj.value;
	}

	public static implicit operator StringArraySerialized(string[] objValue)
	{
		return new StringArraySerialized
		{
			value = objValue
		};
	}

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		int num = (serializer.IsWriter ? value.Length : 0);
		serializer.SerializeValue(ref num, default(FastBufferWriter.ForPrimitives));
		if (serializer.IsReader)
		{
			value = new string[num];
		}
		for (int i = 0; i < num; i++)
		{
			serializer.SerializeValue(ref value[i]);
		}
	}
}
