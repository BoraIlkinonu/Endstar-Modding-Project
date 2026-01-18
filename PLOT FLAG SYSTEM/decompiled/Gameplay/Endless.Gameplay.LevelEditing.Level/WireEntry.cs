using System;
using System.Linq;
using Endless.Gameplay.Serialization;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using Unity.Netcode;

namespace Endless.Gameplay.LevelEditing.Level;

[Serializable]
public class WireEntry : INetworkSerializable
{
	public SerializableGuid WireId = SerializableGuid.Empty;

	public int EmitterComponentTypeId;

	public int ReceiverComponentTypeId;

	public string EmitterMemberName;

	public string ReceiverMemberName;

	public StoredParameter[] StaticParameters = Array.Empty<StoredParameter>();

	public int WireColor;

	[JsonIgnore]
	public string EmitterComponentAssemblyQualifiedTypeName
	{
		get
		{
			if (EmitterComponentTypeId != -1)
			{
				return EndlessTypeMapping.Instance.GetAssemblyQualifiedTypeName(EmitterComponentTypeId);
			}
			return null;
		}
	}

	[JsonIgnore]
	public string ReceiverComponentAssemblyQualifiedTypeName
	{
		get
		{
			if (ReceiverComponentTypeId != -1)
			{
				return EndlessTypeMapping.Instance.GetAssemblyQualifiedTypeName(ReceiverComponentTypeId);
			}
			return null;
		}
	}

	public WireEntry Copy(bool generateNewUniqueIds)
	{
		return new WireEntry
		{
			WireId = (generateNewUniqueIds ? SerializableGuid.NewGuid() : WireId),
			EmitterComponentTypeId = EmitterComponentTypeId,
			ReceiverComponentTypeId = ReceiverComponentTypeId,
			EmitterMemberName = EmitterMemberName,
			ReceiverMemberName = ReceiverMemberName,
			StaticParameters = StaticParameters.Select((StoredParameter storedParam) => storedParam.Copy()).ToArray(),
			WireColor = WireColor
		};
	}

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref WireId, default(FastBufferWriter.ForNetworkSerializable));
		serializer.SerializeValue(ref EmitterComponentTypeId, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref ReceiverComponentTypeId, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref EmitterMemberName);
		serializer.SerializeValue(ref ReceiverMemberName);
		serializer.SerializeValue(ref WireColor, default(FastBufferWriter.ForPrimitives));
		int value = 0;
		if (serializer.IsWriter)
		{
			StoredParameter[] staticParameters = StaticParameters;
			value = ((staticParameters != null) ? staticParameters.Length : 0);
		}
		serializer.SerializeValue(ref value, default(FastBufferWriter.ForPrimitives));
		if (serializer.IsReader)
		{
			StaticParameters = new StoredParameter[value];
			for (int i = 0; i < StaticParameters.Length; i++)
			{
				StaticParameters[i] = new StoredParameter();
			}
		}
		for (int j = 0; j < value; j++)
		{
			serializer.SerializeValue(ref StaticParameters[j], default(FastBufferWriter.ForNetworkSerializable));
		}
	}

	public override string ToString()
	{
		return $"{EmitterMemberName} -> {ReceiverMemberName}, {StaticParameters.Length} static params.";
	}
}
