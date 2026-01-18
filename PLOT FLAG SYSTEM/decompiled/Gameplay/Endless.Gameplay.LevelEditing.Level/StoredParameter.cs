using System;
using Endless.Gameplay.Serialization;
using Unity.Netcode;

namespace Endless.Gameplay.LevelEditing.Level;

[Serializable]
public class StoredParameter : INetworkSerializable
{
	public int DataType;

	public string JsonData;

	public StoredParameter Copy()
	{
		return new StoredParameter
		{
			DataType = DataType,
			JsonData = JsonData
		};
	}

	public Type GetReferencedType()
	{
		return Type.GetType(EndlessTypeMapping.Instance.GetAssemblyQualifiedTypeName(DataType));
	}

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref DataType, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref JsonData);
	}
}
